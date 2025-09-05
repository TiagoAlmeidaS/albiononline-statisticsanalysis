using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCvSharp;
using AlbionFishing.Vision.Reporting;
using System.IO;
using OpenCvSharp.Extensions;

namespace AlbionFishing.Vision;

/// <summary>
/// Detector independente para localizar o anzol/bobber NA ÁGUA dentro de uma ROI de pesca
/// sem impactar o fluxo e métodos existentes de VisionDetector.
/// </summary>
public class BobberInWaterDetector
{
    private const string DefaultTemplatePath = "data/images/bobber_in_water.png";
    private readonly TemplateMatchModes _matchMethod;
    public string TemplatePath { get; private set; }
    private IBobberVisionReporter? _reporter;

    // --- Estado para splash/jerk (não interfere no fluxo básico) ---
    private Mat? _prevGrayROI;
    private DateTime _lastKinematicsTime;
    private bool _kinematicsInitialized;
    private float _lastY;
    private float _vy;
    private float _ay;
    private DateTime _lastGateOn;
    private const int GateDurationMs = 450;

    // --- Micro-motion state ---
    private Mat? _prevStabilizedROI;
    private readonly Queue<double> _lastMicromoveScores = new();
    private double _emaRipple = 0, _emaBobberDy = 0;
    private const double EmaAlpha = 0.15; // baseline adaptativo

    // Parâmetros tunáveis
    private const int MicroRoiHalf = 18;              // janela ao redor do bobber (36x36)
    private const int AnnulusR0 = 20;                 // r interno da “rosquinha”
    private const int AnnulusR1 = 44;                 // r externo
    private const double RippleZThresholdOn  = 2.2;   // z-score para ligar
    private const double RippleZThresholdOff = 1.2;   // z-score para desligar (histerese)
    private const double DyZThresholdOn      = 2.0;   // z-score micro deslocamento do bobber
    private const double DyZThresholdOff     = 1.0;

    private bool _microGateOn;
    private DateTime _lastMicroFire = DateTime.MinValue;
    private const int MicroCooldownMs = 600;
    
    // --- Controle de filtros de cor (temporário, por solicitação)
    private bool _colorFiltersEnabled = false; // default: desabilitado

    // --- Módulos configuráveis (para isolar responsabilidades)
    // Template-only por padrão para depuração do Estágio 1
    private bool _templateOnlyMode = true;
    private bool _useEqualization = false;
    private bool _useMask = false;
    private bool _useGradientChannel = false;
    private bool _useMultiScale = false;
    private bool _useColorGateModule = false;
    private bool _useKinematics = false;
    private bool _useMicroMotionModule = false;
    private bool _useHeuristicHookModule = false;

    public BobberInWaterDetector(TemplateMatchModes matchMethod = TemplateMatchModes.CCoeffNormed)
    {
        _matchMethod = matchMethod;
        // Preferir explicitamente o arquivo fornecido pelo usuário quando existir
        // Ordem de preferência:
        // 1) data/images/bobber_in_water.png
        // 2) data/images/bobber_inwater.png (fallback alternativo, caso nome esteja sem underscore)
        // 3) resources/bobber_in_water.png (fallback recursos)
        // 4) DefaultTemplatePath

        var candidates = new[]
        {
            "data/images/bobber_in_water.png",
            "data/images/bobber_inwater.png",
            "resources/bobber_in_water.png"
        };

        foreach (var candidate in candidates)
        {
            var resolved = ResolvePath(candidate);
            if (System.IO.File.Exists(resolved))
            {
                TemplatePath = resolved;
                return;
            }
        }

        TemplatePath = DefaultTemplatePath;
    }

    public BobberInWaterDetector(string templatePath, TemplateMatchModes matchMethod = TemplateMatchModes.CCoeffNormed)
    {
        _matchMethod = matchMethod;
        TemplatePath = string.IsNullOrWhiteSpace(templatePath) ? DefaultTemplatePath : templatePath;
    }

    public void SetTemplatePath(string templatePath)
    {
        TemplatePath = string.IsNullOrWhiteSpace(templatePath) ? DefaultTemplatePath : templatePath;
    }

    public void SetReporter(IBobberVisionReporter reporter, string? sessionName = null, Rectangle? detectionRegion = null, double? confidenceThreshold = null)
    {
        _reporter = reporter;
        _reporter.StartSession(sessionName, TemplatePath, detectionRegion, confidenceThreshold);
        try
        {
            System.Diagnostics.Trace.WriteLine($"[Biw] Reporter started. Template='{TemplatePath}', ROI='{detectionRegion?.ToString() ?? "(unset)"}', Threshold={confidenceThreshold?.ToString("F2") ?? "(unset)"}");
        }
        catch { }
    }
    
    /// <summary>
    /// Habilita/desabilita filtros baseados em cor e máscaras derivadas de cor/alpha.
    /// Quando desabilitado, usa máscara cheia (sem filtragem por cor) e ignora ColorGate.
    /// </summary>
    public void EnableColorFilters(bool enabled) => _colorFiltersEnabled = enabled;

    /// <summary>
    /// Ativa modo template-only (matching puro do template, sem filtros/canais extras).
    /// </summary>
    public void EnableTemplateOnlyMode(bool enable)
    {
        _templateOnlyMode = enable;
        if (enable)
        {
            _useEqualization = false;
            _useMask = false;
            _useGradientChannel = false;
            _useMultiScale = false;
            _useColorGateModule = false;
            _useKinematics = false;
            _useMicroMotionModule = false;
            _useHeuristicHookModule = false;
        }
    }

    /// <summary>
    /// Configura módulos individualmente (desativa template-only automaticamente).
    /// </summary>
    public void ConfigureMatchingModules(
        bool useEqualization,
        bool useMask,
        bool useGradientChannel,
        bool useMultiScale,
        bool useColorGate,
        bool useKinematics,
        bool useMicroMotion,
        bool useHeuristicHook)
    {
        _templateOnlyMode = false;
        _useEqualization = useEqualization;
        _useMask = useMask;
        _useGradientChannel = useGradientChannel;
        _useMultiScale = useMultiScale;
        _useColorGateModule = useColorGate;
        _useKinematics = useKinematics;
        _useMicroMotionModule = useMicroMotion;
        _useHeuristicHookModule = useHeuristicHook;
    }

    public void EndReporterSession()
    {
        _reporter?.EndSession();
    }

    private (double bestVal, OpenCvSharp.Point bestLoc, OpenCvSharp.Size bestSize, double bestScale, double bestV1, double bestV2, double bestMaskOnRatio)
        DetectCore(Mat screenBgr, Mat templateBgr, Mat mask)
    {
        // Equalização leve de iluminação (condicional)
        using var ycrcbGlobal = new Mat();
        using var screenEqGlobal = new Mat();
        if (_useEqualization && !_templateOnlyMode)
        {
            Cv2.CvtColor(screenBgr, ycrcbGlobal, ColorConversionCodes.BGR2YCrCb);
            var chGlobal = ycrcbGlobal.Split();
            using (var claheG = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8)))
            {
                claheG.Apply(chGlobal[0], chGlobal[0]);
            }
            Cv2.Merge(chGlobal, ycrcbGlobal);
            foreach (var c in chGlobal) c.Dispose();
            Cv2.CvtColor(ycrcbGlobal, screenEqGlobal, ColorConversionCodes.YCrCb2BGR);
        }

        double bestVal = -1.0;
        OpenCvSharp.Point bestLoc = default;
        OpenCvSharp.Size bestSize = default;
        double bestScale = 1.0;
        double bestV1 = 0.0;
        double bestV2 = 0.0;
        double bestMaskOnRatio = 0.0;

        // Para foco estrito no template da foto, use escala 1.0 sempre
        var scales = new double[] { 1.00 };
        foreach (var scale in scales)
        {
            using var tpl = new Mat();
            Cv2.Resize(templateBgr, tpl, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Cubic);
            using var mskResized = new Mat();
            // Sempre use a máscara derivada do template
            Cv2.Resize(mask, mskResized, tpl.Size(), 0, 0, InterpolationFlags.Nearest);

            // Corrigir máscara muito magra nesta escala
            var maskOnRatio = mskResized.Total() > 0 ? Cv2.CountNonZero(mskResized) / mskResized.Total() : 0.0;
            if (maskOnRatio < 0.08)
            {
                Cv2.MorphologyEx(mskResized, mskResized, MorphTypes.Dilate,
                    Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
                maskOnRatio = mskResized.Total() > 0 ? Cv2.CountNonZero(mskResized) / mskResized.Total() : 0.0;
            }

            // Selecionar imagem base (equalizada ou original)
            var screenBase = (_useEqualization && !_templateOnlyMode) ? screenEqGlobal : screenBgr;

            // Matching com máscara (intensidade)
            using var r1 = new Mat();
            Cv2.MatchTemplate(screenBase, tpl, r1, TemplateMatchModes.CCorrNormed, mskResized);
            Cv2.MinMaxLoc(r1, out _, out var v1, out _, out var p1);

            double v = v1; OpenCvSharp.Point p = p1; double v2 = 0;
            if (_useGradientChannel && !_templateOnlyMode)
            {
                // Matching no gradiente (invariância à cor)
            v2 = 0; // Desativa canal de gradiente para foco estrito no template
            p = p1;
            }
            if (v > bestVal)
            {
                bestVal = v;
                bestLoc = p;
                bestSize = tpl.Size();
                bestScale = scale;
                bestV1 = v1;
                bestV2 = v2;
                bestMaskOnRatio = maskOnRatio;
            }
        }

        return (bestVal, bestLoc, bestSize, bestScale, bestV1, bestV2, bestMaskOnRatio);
    }

    private void BuildTemplateAndMaskFromRaw(Mat tplRaw, Mat templateMat, Mat mask)
    {
        if (tplRaw.Channels() == 4)
        {
            // Sempre respeitar alpha como máscara do template (foco estrito no recorte da foto)
            var split = tplRaw.Split();
            Cv2.CvtColor(tplRaw, templateMat, ColorConversionCodes.BGRA2BGR);
            split[3].CopyTo(mask);
            foreach (var m in split) m.Dispose();
        }
        else
        {
            // Sem alpha: usar remoção de fundo preto para gerar máscara do template
            tplRaw.CopyTo(templateMat);
            using var gray = new Mat();
            Cv2.CvtColor(templateMat, gray, ColorConversionCodes.BGR2GRAY);
            // Pixels muito escuros ~fundo preto ficam 0 na máscara
            Cv2.Threshold(gray, mask, 15, 255, ThresholdTypes.Binary);
            // Limpeza leve
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
        }
    }

    private bool? EvaluateColorGateAt(Mat screenMat, OpenCvSharp.Point bestLoc, OpenCvSharp.Size bestSize)
    {
        var winRect = new OpenCvSharp.Rect(
            Math.Max(0, bestLoc.X),
            Math.Max(0, bestLoc.Y),
            Math.Min(bestSize.Width, screenMat.Width - Math.Max(0, bestLoc.X)),
            Math.Min(bestSize.Height, screenMat.Height - Math.Max(0, bestLoc.Y))
        );
        if (winRect.Width <= 3 || winRect.Height <= 3) return null;

        using var winBgr = new Mat(screenMat, winRect);
        using var winHsv = new Mat();
        Cv2.CvtColor(winBgr, winHsv, ColorConversionCodes.BGR2HSV);
        var chs = winHsv.Split();
        try
        {
            using var red1 = new Mat();
            using var red2 = new Mat();
            Cv2.InRange(winHsv, new Scalar(0, 60, 40), new Scalar(10, 255, 255), red1);
            Cv2.InRange(winHsv, new Scalar(170, 60, 40), new Scalar(180, 255, 255), red2);
            using var reds = new Mat();
            Cv2.BitwiseOr(red1, red2, reds);

            using var nonWater = new Mat();
            Cv2.Threshold(chs[1], nonWater, 50, 255, ThresholdTypes.Binary);

            double total = winRect.Width * winRect.Height;
            double redsPct = total > 0 ? Cv2.CountNonZero(reds) / total : 0.0;
            double nonWaterPct = total > 0 ? Cv2.CountNonZero(nonWater) / total : 0.0;
            return (redsPct >= 0.05) && (nonWaterPct >= 0.25);
        }
        finally
        {
            foreach (var c in chs) c.Dispose();
        }
    }

    private static bool EvaluateHookHeuristic(int votes, bool micro, double zDy, double zRip, double dyPix, double ripE, bool gateOn)
    {
        bool strongMicro = micro || (zDy > 2.0) || (zRip > 1.5) || (Math.Abs(dyPix) >= 2.0);
        bool rippleSpike = ripE > 0.65;
        return (votes >= 2) || strongMicro || rippleSpike || gateOn;
    }

    /// <summary>
    /// Executa uma detecção simples por template matching dentro da área de pesca informada.
    /// </summary>
    /// <param name="fishingArea">Área de pesca (ROI) na tela.</param>
    /// <param name="confidenceThreshold">Limiar mínimo de confiança (0..1).</param>
    /// <param name="templatePath">Caminho do template do bobber/anzol.</param>
    /// <returns>Resultado com detectado, score e posição do centro (posX,posY) em coordenadas da ROI.</returns>
    public BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5, string? templatePath = null)
    {
        var startTime = DateTime.UtcNow;
        var debugPrefix = $"[BiW-{startTime:HH:mm:ss.fff}]";
        
        try
        {
            // 🔍 LOG: Parâmetros de entrada
            Console.WriteLine($"{debugPrefix} 🎯 DetectInArea iniciado - Região: {fishingArea}, Threshold: {confidenceThreshold:F3}, Template: {templatePath ?? "(default)"}");
            System.Diagnostics.Trace.WriteLine($"{debugPrefix} DetectInArea iniciado - Região: {fishingArea}, Threshold: {confidenceThreshold:F3}");
            
            // 🔍 LOG: Tentativa de captura
            Console.WriteLine($"{debugPrefix} 📸 Capturando região da tela...");
            using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
            if (bitmap == null)
            {
                var errorMsg = "ScreenCapture.CaptureRegion retornou null";
                Console.WriteLine($"{debugPrefix} ❌ ERRO: {errorMsg}");
                System.Diagnostics.Trace.WriteLine($"{debugPrefix} ERRO: {errorMsg}");
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, Reason = $"CaptureRegion null (basic), ROI={fishingArea}" });
                return BobberInWaterDetectionResult.Failed("CaptureRegion returned null");
            }
            
            // 🔍 LOG: Sucesso na captura
            Console.WriteLine($"{debugPrefix} ✅ Captura bem-sucedida - Dimensões: {bitmap.Width}x{bitmap.Height}, Formato: {bitmap.PixelFormat}");

            // Escolher caminho do template: parâmetro explícito tem precedência; senão usa TemplatePath interno
            var pathToUse = ResolvePath(string.IsNullOrWhiteSpace(templatePath) ? (TemplatePath ?? DefaultTemplatePath) : templatePath);
            Console.WriteLine($"{debugPrefix} 📋 Template selecionado: {pathToUse}");

            // 🔍 LOG: Verificação de template
            if (!System.IO.File.Exists(pathToUse))
            {
                var errorMsg = $"Template não encontrado: {pathToUse}";
                Console.WriteLine($"{debugPrefix} ❌ ERRO: {errorMsg}");
                System.Diagnostics.Trace.WriteLine($"{debugPrefix} ERRO: {errorMsg}");
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = $"Template not found (basic): {pathToUse}" });
                return BobberInWaterDetectionResult.Failed($"Template not found: {pathToUse}");
            }
            Console.WriteLine($"{debugPrefix} ✅ Template existe e é acessível");

            // 🔍 LOG: Conversão para OpenCV Mat
            Console.WriteLine($"{debugPrefix} 🔄 Convertendo bitmap para OpenCV Mat...");
            using var screenMat = BitmapConverter.ToMat(bitmap);
            if (screenMat.Empty())
            {
                var errorMsg = "Conversão para OpenCV Mat resultou em Mat vazio";
                Console.WriteLine($"{debugPrefix} ❌ ERRO: {errorMsg}");
                System.Diagnostics.Trace.WriteLine($"{debugPrefix} ERRO: {errorMsg}");
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = "Empty screenMat (basic)" });
                return BobberInWaterDetectionResult.Failed("Empty screenMat after conversion");
            }
            Console.WriteLine($"{debugPrefix} ✅ Conversão bem-sucedida - Mat: {screenMat.Width}x{screenMat.Height}, Canais: {screenMat.Channels()}, Tipo: {screenMat.Type()}");

            // Normalização de canais: BitmapConverter.ToMat já entrega BGR/BGRA
            if (screenMat.Channels() == 4)
            {
                Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.BGRA2BGR);
            }

            // Cinza completo para análises dinâmicas
            using var fullGray = new Mat();
            Cv2.CvtColor(screenMat, fullGray, ColorConversionCodes.BGR2GRAY);

            // 🔍 LOG: Carregamento do template
            Console.WriteLine($"{debugPrefix} 📖 Carregando template do disco...");
            using var tplRaw = Cv2.ImRead(pathToUse, ImreadModes.Unchanged);
            if (tplRaw.Empty())
            {
                var errorMsg = $"Falha ao carregar template de {pathToUse} - OpenCV retornou Mat vazio";
                Console.WriteLine($"{debugPrefix} ❌ ERRO: {errorMsg}");
                System.Diagnostics.Trace.WriteLine($"{debugPrefix} ERRO: {errorMsg}");
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = "Empty templateMat (basic)" });
                return BobberInWaterDetectionResult.Failed("Empty templateMat");
            }
            Console.WriteLine($"{debugPrefix} ✅ Template carregado - Dimensões: {tplRaw.Width}x{tplRaw.Height}, Canais: {tplRaw.Channels()}");
            using var templateMat = new Mat();
            using var mask = new Mat();
            BuildTemplateAndMaskFromRaw(tplRaw, templateMat, mask);
            if (_colorFiltersEnabled)
            {
                Cv2.MorphologyEx(mask, mask, MorphTypes.Open, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
                Cv2.MorphologyEx(mask, mask, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5)));
            }

            // Redimensionar template se maior que ROI
            if (templateMat.Width > screenMat.Width || templateMat.Height > screenMat.Height)
            {
                var scaleX = screenMat.Width / (double)templateMat.Width;
                var scaleY = screenMat.Height / (double)templateMat.Height;
                var scale = Math.Min(scaleX, scaleY) * 0.9; // margem
                var newSize = new OpenCvSharp.Size((int)(templateMat.Width * scale), (int)(templateMat.Height * scale));
                Cv2.Resize(templateMat, templateMat, newSize);
            }

            // 🔍 LOG: Início da detecção multi-escala
            Console.WriteLine($"{debugPrefix} 🎯 Iniciando detecção multi-escala...");

            double bestVal = -1.0;
            OpenCvSharp.Point bestLoc = default;
            OpenCvSharp.Size bestSize = default;
            double bestScale = 1.0;
            double bestV1 = 0.0;
            double bestV2 = 0.0;
            double bestMaskOnRatio = 0.0;

            // Equalização leve de iluminação (uma vez por frame)
            using var ycrcbGlobal = new Mat();
            Cv2.CvtColor(screenMat, ycrcbGlobal, ColorConversionCodes.BGR2YCrCb);
            var chGlobal = ycrcbGlobal.Split();
            using (var claheG = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8)))
            {
                claheG.Apply(chGlobal[0], chGlobal[0]);
            }
            Cv2.Merge(chGlobal, ycrcbGlobal);
            foreach (var c in chGlobal) c.Dispose();
            using var screenEqGlobal = new Mat();
            Cv2.CvtColor(ycrcbGlobal, screenEqGlobal, ColorConversionCodes.YCrCb2BGR);

            foreach (var scale in new double[] { 0.60, 0.70, 0.80, 0.90, 1.00, 1.10, 1.20 })
            {
                using var tpl = new Mat();
                Cv2.Resize(templateMat, tpl, new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Cubic);
                using var mskResized = new Mat();
                Cv2.Resize(mask, mskResized, tpl.Size(), 0, 0, InterpolationFlags.Nearest);
                var maskOnRatio = mskResized.Total() > 0 ? Cv2.CountNonZero(mskResized) / mskResized.Total() : 0.0;
                if (maskOnRatio < 0.08)
                {
                    // Afrouxa máscara para esta escala
                    Cv2.MorphologyEx(mskResized, mskResized, MorphTypes.Dilate,
                        Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
                    maskOnRatio = mskResized.Total() > 0 ? Cv2.CountNonZero(mskResized) / mskResized.Total() : 0.0;
                }

                // Equalização leve de iluminação no frame (CLAHE no Y do YCrCb)
                using var screenEq = screenEqGlobal; // usar equalização global

                // Matching com máscara (intensidade)
                using var r1 = new Mat();
                Cv2.MatchTemplate(screenEq, tpl, r1, TemplateMatchModes.CCorrNormed, mskResized);
                Cv2.MinMaxLoc(r1, out _, out var v1, out _, out var p1);

                // Matching no gradiente (invariância à cor)
                using var tplG = new Mat();
                using var scrG = new Mat();
                Cv2.Canny(tpl, tplG, 60, 120);
                Cv2.Canny(screenEq, scrG, 60, 120);
                using var r2 = new Mat();
                Cv2.MatchTemplate(scrG, tplG, r2, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(r2, out _, out var v2, out _, out var p2);

                var v = 0.6 * v1 + 0.4 * v2;
                var p = (v1 >= v2) ? p1 : p2;
                if (v > bestVal || (bestVal < 0.3 && v2 >= 0.62 && v1 < 0.35)) // aceitar gradiente forte como fallback
                {
                    bestVal = v;
                    bestLoc = p;
                    bestSize = tpl.Size();
                    bestScale = scale;
                    bestV1 = v1;
                    bestV2 = v2;
                    bestMaskOnRatio = maskOnRatio;
                }
            }

            var detected = bestVal >= confidenceThreshold;
            float posX = 0f, posY = 0f;
            bool jerk=false, flow=false, diff=false;
            bool micro=false; double zDy=0, zRip=0, dyPix=0, ripE=0; int votes=0;
            bool? colorGateOk = null;
            
            // 🔍 LOG: Resultado da detecção
            Console.WriteLine($"{debugPrefix} 📊 Detecção concluída - Score: {bestVal:F3}, Threshold: {confidenceThreshold:F3}, Detectado: {detected}");
            if (detected && _useKinematics && !_templateOnlyMode)
            {
                posX = bestLoc.X + (bestSize.Width / 2.0f);
                posY = bestLoc.Y + (bestSize.Height / 2.0f);
                Console.WriteLine($"{debugPrefix} 🎯 Posição detectada: ({posX:F1}, {posY:F1})");
            }

            // --- Análise dinâmica (splash/jerk) sem alterar contrato ---
            if (detected)
            {
                // raios do "donut" (anel) ao redor do bobber
                int r0 = (int)(Math.Max(bestSize.Width, bestSize.Height) * 0.55);
                int r1 = r0 + 24;
                var res = AnalyzeSplashAndJerk(fullGray, new OpenCvSharp.Point2f(posX, posY), r0, r1);
                jerk = res.jerk; flow = res.flow; diff = res.diff;
                votes = 0;
                if (jerk) votes++;
                if (flow) votes++;
                if (diff) votes++;

                // Gate simples temporário: console log + memoriza último ON (integração com áudio pode ler esse timestamp)
                if (votes >= 2)
                {
                    _lastGateOn = DateTime.UtcNow;
                    System.Diagnostics.Trace.WriteLine($"🌊 WaterSplashDetected @ ({posX:F1},{posY:F1}) — gate ON 450ms");
                }

            // --- Micro-movimento (dy do bobber + ondulação local) ---
            if (_useMicroMotionModule && !_templateOnlyMode)
            {
                var microRes = AnalyzeMicroMotion(fullGray, new OpenCvSharp.Point2f(posX, posY));
                micro = microRes.microMove; zDy = microRes.zDy; zRip = microRes.zRipple; dyPix = microRes.dyPixels; ripE = microRes.rippleEnergy;
                if (micro)
                {
                    _lastGateOn = DateTime.UtcNow; // pode reutilizar o gate visual
                    System.Diagnostics.Trace.WriteLine($"🫧 MicroMove: zDy={zDy:F2}, zRipple={zRip:F2}, dy={dyPix:F2}px, ripple={ripE:F3}");
                }
            }

            // --- ColorGate (opcional, apenas logging) ---
            if (_colorFiltersEnabled && _useColorGateModule && !_templateOnlyMode)
            {
                colorGateOk = EvaluateColorGateAt(screenMat, bestLoc, bestSize);
            }
            }

            // Reporter hook: log por frame
            // Estágio 2: heurística de fisgada (hook)
            bool hook_basic = EvaluateHookHeuristic(votes, micro, zDy, zRip, dyPix, ripE, IsGateOpen());

            // calcula desvio padrão do patch matched para avaliar textura/plana
            double patchStd = 0;
            try
            {
                var rect = new OpenCvSharp.Rect(Math.Max(0, bestLoc.X), Math.Max(0, bestLoc.Y), Math.Min(bestSize.Width, screenMat.Width - Math.Max(0, bestLoc.X)), Math.Min(bestSize.Height, screenMat.Height - Math.Max(0, bestLoc.Y)));
                if (rect.Width > 2 && rect.Height > 2)
                {
                    using var patch = screenMat[rect];
                    using var patchGray = new Mat();
                    Cv2.CvtColor(patch, patchGray, ColorConversionCodes.BGR2GRAY);
                    Cv2.MeanStdDev(patchGray, out _, out var stddev);
                    patchStd = stddev[0];
                }
            } catch {}

            _reporter?.LogFrame(new BobberVisionFrame
            {
                TimestampUtc = DateTime.UtcNow,
                Detected = detected,
                Score = bestVal,
                PosX = posX,
                PosY = posY,
                PatchW = bestSize.Width,
                PatchH = bestSize.Height,
                BBoxX = bestLoc.X,
                BBoxY = bestLoc.Y,
                TemplateW = bestSize.Width,
                TemplateH = bestSize.Height,
                PatchStdDev = patchStd,
                TemplateOnly = true,
                ZDy = zDy,
                ZRipple = zRip,
                DyPixels = dyPix,
                RippleEnergy = ripE,
                Jerk = jerk,
                Flow = flow,
                Diff = diff,
                Votes = votes,
                MicroMove = micro,
                HookDetected = hook_basic,
                ScaleUsed = bestScale,
                TemplateId = Path.GetFileName(pathToUse),
                ColorGateOk = colorGateOk,
                MatchV1 = bestV1,
                MatchV2 = bestV2,
                MaskOnRatio = bestMaskOnRatio,
                Method = "CCorrNormed+mask (strict-1.0)",
                UsedMask = true
            });

            // 🔍 LOG: Finalização com sucesso
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Console.WriteLine($"{debugPrefix} ✅ DetectInArea concluído em {processingTime:F1}ms - Resultado: {(detected ? "SUCESSO" : "NÃO DETECTADO")}");
            System.Diagnostics.Trace.WriteLine($"{debugPrefix} DetectInArea concluído em {processingTime:F1}ms");
            
            return new BobberInWaterDetectionResult(detected, bestVal, posX, posY, fishingArea);
        }
        catch (Exception ex)
        {
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var errorMsg = $"Exceção em DetectInArea após {processingTime:F1}ms: {ex.Message}";
            Console.WriteLine($"{debugPrefix} 💥 EXCEÇÃO: {errorMsg}");
            Console.WriteLine($"{debugPrefix} 📍 Stack Trace: {ex.StackTrace}");
            System.Diagnostics.Trace.WriteLine($"{debugPrefix} EXCEÇÃO: {errorMsg}");
            System.Diagnostics.Trace.WriteLine($"{debugPrefix} Stack Trace: {ex.StackTrace}");
            
            _reporter?.LogFrame(new BobberVisionFrame { 
                TimestampUtc = DateTime.UtcNow, 
                Detected = false, 
                Score = 0, 
                Reason = $"Exception: {ex.Message}" 
            });
            
            return BobberInWaterDetectionResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Variante que usa o TemplatePath interno (ou o default) sem precisar passar caminho toda chamada.
    /// </summary>
    public BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        return DetectInArea(fishingArea, confidenceThreshold, TemplatePath);
    }

    /// <summary>
    /// Sobrecarga prática que recebe coordenadas inteiras da ROI de pesca.
    /// </summary>
    public BobberInWaterDetectionResult DetectInArea(int x, int y, int width, int height, double confidenceThreshold = 0.5, string? templatePath = null)
    {
        return DetectInArea(new Rectangle(x, y, width, height), confidenceThreshold, templatePath);
    }

    // --- Implementação dos sinais dinâmicos (jerk, flow, diff) ---
    private (bool jerk, bool flow, bool diff) AnalyzeSplashAndJerk(Mat fullGray, OpenCvSharp.Point2f pos, int r0, int r1)
    {
        // 1) Cinemática (jerk)
        var now = DateTime.UtcNow;
        double dt = 0.016; // fallback 60 FPS
        if (_kinematicsInitialized)
        {
            dt = Math.Max(1, (now - _lastKinematicsTime).TotalMilliseconds) / 1000.0;
        }
        var vy = (_kinematicsInitialized ? (pos.Y - _lastY) / (float)dt : 0f);
        var ay = (_kinematicsInitialized ? (vy - _vy) / (float)dt : 0f);
        var jy = (_kinematicsInitialized ? (ay - _ay) / (float)dt : 0f);
        bool jerk = Math.Abs(ay) > 220f || Math.Abs(jy) > 5000f;
        _lastKinematicsTime = now; _lastY = pos.Y; _vy = vy; _ay = ay; _kinematicsInitialized = true;

        // 2) ROI anular
        var roiRect = new OpenCvSharp.Rect(
            (int)(pos.X - r1), (int)(pos.Y - r1),
            Math.Max(1, 2 * r1), Math.Max(1, 2 * r1));
        roiRect = roiRect & new OpenCvSharp.Rect(0, 0, fullGray.Cols, fullGray.Rows);
        if (roiRect.Width <= 0 || roiRect.Height <= 0)
            return (jerk, false, false);

        using var roiNow = new Mat(fullGray, roiRect);
        bool flow = false, diff = false;

        // 2a) Optical Flow radial
        if (_prevGrayROI != null && !_prevGrayROI.Empty() && _prevGrayROI.Size() == roiNow.Size())
        {
            using var flowMat = new Mat();
            Cv2.CalcOpticalFlowFarneback(_prevGrayROI, roiNow, flowMat,
                0.5, 2, 15, 3, 5, 1.2, 0);

            var center = new OpenCvSharp.Point2f(roiNow.Cols / 2f, roiNow.Rows / 2f);
            double radialAbsSum = 0; int count = 0;

            for (int y = 0; y < roiNow.Rows; y++)
            for (int x = 0; x < roiNow.Cols; x++)
            {
                var dx = x - center.X; var dy = y - center.Y;
                var norm = Math.Sqrt(dx * dx + dy * dy);
                if (norm < r0 || norm > r1) continue;
                var vec = flowMat.At<Vec2f>(y, x);
                var rx = dx / (norm + 1e-6); var ry = dy / (norm + 1e-6);
                var radial = vec.Item0 * rx + vec.Item1 * ry;
                radialAbsSum += Math.Abs(radial); count++;
            }
            var radialMeanAbs = count > 0 ? radialAbsSum / count : 0;
            flow = radialMeanAbs > 0.55; // ajuste em campo
        }

        // 2b) Frame diff no anel
        using var roiBlur = roiNow.GaussianBlur(new OpenCvSharp.Size(5, 5), 0);
        if (_prevGrayROI != null && !_prevGrayROI.Empty() && _prevGrayROI.Size() == roiNow.Size())
        {
            using var diffMat = new Mat();
            Cv2.Absdiff(roiBlur, _prevGrayROI, diffMat);
            Cv2.Threshold(diffMat, diffMat, 24, 255, ThresholdTypes.Binary);
            var active = Cv2.CountNonZero(diffMat);
            diff = active > (0.10 * roiNow.Total());
        }

        _prevGrayROI?.Dispose();
        _prevGrayROI = roiNow.Clone();

        return (jerk, flow, diff);
    }

    private (bool microMove, double zDy, double zRipple, double dyPixels, double rippleEnergy)
        AnalyzeMicroMotion(Mat fullGray, OpenCvSharp.Point2f pos)
    {
        var x0 = Math.Max(0, (int)pos.X - MicroRoiHalf);
        var y0 = Math.Max(0, (int)pos.Y - MicroRoiHalf);
        var w  = Math.Min(fullGray.Cols - x0, 2 * MicroRoiHalf);
        var h  = Math.Min(fullGray.Rows - y0, 2 * MicroRoiHalf);
        if (w <= 6 || h <= 6) return (false, 0, 0, 0, 0);

        using var roiNow = new Mat(fullGray, new OpenCvSharp.Rect(x0, y0, w, h)).Clone();
        Cv2.GaussianBlur(roiNow, roiNow, new OpenCvSharp.Size(3, 3), 0);

        double dy = 0;
        if (_prevStabilizedROI != null && !_prevStabilizedROI.Empty() && _prevStabilizedROI.Size() == roiNow.Size())
        {
            int maxShift = Math.Min(4, roiNow.Rows / 8);
            double best = double.NegativeInfinity;
            int bestLag = 0;
            for (int lag = -maxShift; lag <= maxShift; lag++)
            {
                int rowStartNow = Math.Max(0, lag);
                int rowEndNow = Math.Min(roiNow.Rows, roiNow.Rows + lag);
                int rowStartPrev = Math.Max(0, -lag);
                int rowEndPrev = Math.Min(_prevStabilizedROI.Rows, _prevStabilizedROI.Rows - lag);
                if (rowEndNow - rowStartNow <= 4 || rowEndPrev - rowStartPrev <= 4) continue;
                using var shifted = roiNow.RowRange(rowStartNow, rowEndNow);
                using var baseR = _prevStabilizedROI.RowRange(rowStartPrev, rowEndPrev);
                using var tm = new Mat();
                Cv2.MatchTemplate(shifted, baseR, tm, TemplateMatchModes.CCorrNormed);
                double corr = tm.At<float>(0, 0);
                if (corr > best) { best = corr; bestLag = lag; }
            }
            dy = bestLag;
        }

        // Energia de ondulação radial no anel global
        int r0 = AnnulusR0, r1 = AnnulusR1;
        var ringRect = new OpenCvSharp.Rect(
            Math.Max(0, (int)pos.X - r1),
            Math.Max(0, (int)pos.Y - r1),
            Math.Min(fullGray.Cols - Math.Max(0, (int)pos.X - r1), 2 * r1),
            Math.Min(fullGray.Rows - Math.Max(0, (int)pos.Y - r1), 2 * r1));
        double rippleEnergy = 0;
        if (ringRect.Width > 8 && ringRect.Height > 8 && _prevGrayROI != null && !_prevGrayROI.Empty())
        {
            using var ringNow = new Mat(fullGray, ringRect);
            using var flowMat = new Mat();
            Cv2.CalcOpticalFlowFarneback(_prevGrayROI, ringNow, flowMat, 0.5, 2, 13, 2, 5, 1.1, 0);

            var center = new OpenCvSharp.Point2f(ringNow.Cols / 2f, ringNow.Rows / 2f);
            double radialAbsSum = 0; int count = 0;
            for (int y = 0; y < ringNow.Rows; y++)
            for (int x = 0; x < ringNow.Cols; x++)
            {
                var dx = x - center.X; var dyv = y - center.Y;
                var norm = Math.Sqrt(dx * dx + dyv * dyv);
                if (norm < r0 || norm > r1) continue;
                var vec = flowMat.At<Vec2f>(y, x);
                var rx = dx / (norm + 1e-6); var ry = dyv / (norm + 1e-6);
                var radial = vec.Item0 * rx + vec.Item1 * ry;
                radialAbsSum += Math.Abs(radial);
                count++;
            }
            rippleEnergy = count > 0 ? radialAbsSum / count : 0;
        }

        // Atualiza baselines e z-scores
        _emaBobberDy = (1 - EmaAlpha) * _emaBobberDy + EmaAlpha * Math.Abs(dy);
        _emaRipple   = (1 - EmaAlpha) * _emaRipple   + EmaAlpha * rippleEnergy;

        _lastMicromoveScores.Enqueue(Math.Abs(dy));
        if (_lastMicromoveScores.Count > 60) _lastMicromoveScores.Dequeue();
        var mean = _lastMicromoveScores.Any() ? _lastMicromoveScores.Average() : 0.0;
        var variance = _lastMicromoveScores.Any()
            ? _lastMicromoveScores.Select(v => (v - mean) * (v - mean)).Average()
            : 0.0;
        var std = Math.Sqrt(variance);
        double zDy = std > 1e-6 ? (Math.Abs(dy) - mean) / std : 0.0;
        double zRipple = (_emaRipple > 1e-6) ? (rippleEnergy / _emaRipple) - 1.0 : 0.0;

        var now = DateTime.UtcNow;
        var inCd = (now - _lastMicroFire).TotalMilliseconds < MicroCooldownMs;
        bool dyGate = _microGateOn ? (zDy >= DyZThresholdOff) : (zDy >= DyZThresholdOn);
        bool rpGate = _microGateOn ? (zRipple >= RippleZThresholdOff) : (zRipple >= RippleZThresholdOn);

        int votes = 0;
        if (dyGate) votes++;
        if (rpGate) votes++;

        bool fire = votes >= 1 && !inCd;
        if (fire) { _microGateOn = true; _lastMicroFire = now; }
        else if (!dyGate && !rpGate) { _microGateOn = false; }

        _prevStabilizedROI?.Dispose();
        _prevStabilizedROI = roiNow.Clone();

        return (fire, zDy, zRipple, dy, rippleEnergy);
    }

    public (bool microMove, double zDy, double zRipple, double dyPixels, double rippleEnergy, bool gateOn, DateTime until)
        DetectMicroMotionVerbose(Rectangle fishingArea, double confidenceThreshold = 0.5, string? templatePath = null)
    {
        var r = DetectInAreaVerbose(fishingArea, confidenceThreshold, templatePath);
        if (!r.Detected) return (false, 0, 0, 0, 0, false, DateTime.MinValue);

        using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
        using var screenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            // Corrigir apenas BGRA -> BGR
            if (screenMat.Channels() == 4)
            {
                Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.BGRA2BGR);
            }
        using var fullGray = new Mat();
        Cv2.CvtColor(screenMat, fullGray, ColorConversionCodes.BGR2GRAY);

        var (micro, zDy, zRip, dyPix, ripE) = AnalyzeMicroMotion(fullGray, new OpenCvSharp.Point2f(r.PositionX, r.PositionY));
        var gateOn = _microGateOn;
        var until  = _lastMicroFire.AddMilliseconds(MicroCooldownMs);
        return (micro, zDy, zRip, dyPix, ripE, gateOn, until);
    }
    /// <summary>
    /// Versão verbosa para depuração na GUI: devolve também os sinais (jerk/flow/diff), votos e estado do gate.
    /// Não altera o método básico.
    /// </summary>
    public BobberInWaterDetectionVerboseResult DetectInAreaVerbose(Rectangle fishingArea, double confidenceThreshold = 0.5, string? templatePath = null)
    {
        // Repete o fluxo de DetectInArea para manter o método básico estável
        try
        {
            using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
            if (bitmap == null)
            {
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, Reason = $"CaptureRegion null (verbose), ROI={fishingArea}" });
                return BobberInWaterDetectionVerboseResult.Failed("CaptureRegion returned null");
            }

            var pathToUse = ResolvePath(string.IsNullOrWhiteSpace(templatePath) ? (TemplatePath ?? DefaultTemplatePath) : templatePath);
            if (!System.IO.File.Exists(pathToUse))
            {
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = $"Template not found (verbose): {pathToUse}" });
                return BobberInWaterDetectionVerboseResult.Failed($"Template not found: {pathToUse}");
            }

            using var screenMat = BitmapConverter.ToMat(bitmap);
            if (screenMat.Empty())
            {
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = "Empty screenMat (verbose)" });
                return BobberInWaterDetectionVerboseResult.Failed("Empty screenMat after conversion");
            }

            // Corrigir apenas BGRA -> BGR
            if (screenMat.Channels() == 4)
            {
                Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.BGRA2BGR);
            }
            using var fullGray = new Mat();
            Cv2.CvtColor(screenMat, fullGray, ColorConversionCodes.BGR2GRAY);

            using var tplRaw = Cv2.ImRead(pathToUse, ImreadModes.Unchanged);
            if (tplRaw.Empty())
            {
                _reporter?.LogFrame(new BobberVisionFrame { TimestampUtc = DateTime.UtcNow, Detected = false, Score = 0, TemplateId = Path.GetFileName(pathToUse), Reason = "Empty templateMat (verbose)" });
                return BobberInWaterDetectionVerboseResult.Failed("Empty templateMat");
            }
            using var templateMat = new Mat();
            using var mask = new Mat();
            if (tplRaw.Channels() == 4)
            {
                var split = tplRaw.Split();
                split[3].CopyTo(mask);
                Cv2.CvtColor(tplRaw, templateMat, ColorConversionCodes.BGRA2BGR);
                foreach (var m in split) m.Dispose();
            }
            else
            {
                tplRaw.CopyTo(templateMat);
                using var tmpHsvV = new Mat();
                Cv2.CvtColor(templateMat, tmpHsvV, ColorConversionCodes.BGR2HSV);
                var hsvV = tmpHsvV.Split();
                Cv2.Threshold(hsvV[1], mask, 50, 255, ThresholdTypes.Binary);
                foreach (var m in hsvV) m.Dispose();
                Cv2.MorphologyEx(mask, mask, MorphTypes.Open, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3)));
                Cv2.MorphologyEx(mask, mask, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5)));
            }

            if (templateMat.Width > screenMat.Width || templateMat.Height > screenMat.Height)
            {
                var scaleX = screenMat.Width / (double)templateMat.Width;
                var scaleY = screenMat.Height / (double)templateMat.Height;
                var scale = Math.Min(scaleX, scaleY) * 0.9;
                var newSize = new OpenCvSharp.Size((int)(templateMat.Width * scale), (int)(templateMat.Height * scale));
                Cv2.Resize(templateMat, templateMat, newSize);
            }

            // Usar núcleo unificado também no Verbose
            var core = DetectCore(screenMat, templateMat, mask);
            double maxVal = core.bestVal;
            OpenCvSharp.Point maxLoc = core.bestLoc;
            var bestSize = core.bestSize;

            var detected = maxVal >= confidenceThreshold;
            float posX = 0f, posY = 0f;
            bool jerk = false, flow = false, diff = false;
            int votes = 0;
            bool micro = false; double zDy = 0, zRipple = 0, dyPix = 0, ripE = 0;

            // Sempre compute o centro da melhor correspondência para permitir overlay/telemetria,
            // mesmo quando o score estiver abaixo do threshold (detected == false).
            posX = maxLoc.X + (bestSize.Width / 2.0f);
            posY = maxLoc.Y + (bestSize.Height / 2.0f);

            if (detected)
            {
                int r0 = (int)(Math.Max(bestSize.Width, bestSize.Height) * 0.55);
                int r1 = r0 + 24;
                (jerk, flow, diff) = AnalyzeSplashAndJerk(fullGray, new OpenCvSharp.Point2f(posX, posY), r0, r1);
                if (jerk) votes++;
                if (flow) votes++;
                if (diff) votes++;
                if (votes >= 2)
                {
                    _lastGateOn = DateTime.UtcNow;
                }
                // Micro-movimento para logging
                var mm = AnalyzeMicroMotion(fullGray, new OpenCvSharp.Point2f(posX, posY));
                micro = mm.microMove; zDy = mm.zDy; zRipple = mm.zRipple; dyPix = mm.dyPixels; ripE = mm.rippleEnergy;
                if (micro)
                {
                    _lastGateOn = DateTime.UtcNow;
                }
            }

            var gateOn = (DateTime.UtcNow - _lastGateOn).TotalMilliseconds <= GateDurationMs;
            var gateExpiresAt = _lastGateOn.AddMilliseconds(GateDurationMs);

            // Reporter hook (verboso)
            // Estágio 2 (verboso): heurística de fisgada
            bool strongMicro_verbose = micro || (zDy > 2.0) || (zRipple > 1.5) || (Math.Abs(dyPix) >= 2.0);
            bool rippleSpike_verbose = ripE > 0.65;
            bool hook_verbose = (votes >= 2) || strongMicro_verbose || rippleSpike_verbose || ((DateTime.UtcNow - _lastGateOn).TotalMilliseconds <= GateDurationMs);

            // calcula desvio padrão do patch matched no verbose também
            double patchStdV = 0;
            try
            {
                var rectV = new OpenCvSharp.Rect(Math.Max(0, maxLoc.X), Math.Max(0, maxLoc.Y), Math.Min(bestSize.Width, screenMat.Width - Math.Max(0, maxLoc.X)), Math.Min(bestSize.Height, screenMat.Height - Math.Max(0, maxLoc.Y)));
                if (rectV.Width > 2 && rectV.Height > 2)
                {
                    using var patch = screenMat[rectV];
                    using var patchGray = new Mat();
                    Cv2.CvtColor(patch, patchGray, ColorConversionCodes.BGR2GRAY);
                    Cv2.MeanStdDev(patchGray, out _, out var stddev);
                    patchStdV = stddev[0];
                }
            } catch {}

            _reporter?.LogFrame(new BobberVisionFrame
            {
                TimestampUtc = DateTime.UtcNow,
                Detected = detected,
                Score = maxVal,
                PosX = posX,
                PosY = posY,
                PatchW = templateMat.Width,
                PatchH = templateMat.Height,
                BBoxX = maxLoc.X,
                BBoxY = maxLoc.Y,
                TemplateW = templateMat.Width,
                TemplateH = templateMat.Height,
                PatchStdDev = patchStdV,
                TemplateOnly = true,
                ZDy = zDy,
                ZRipple = zRipple,
                DyPixels = dyPix,
                RippleEnergy = ripE,
                Jerk = jerk,
                Flow = flow,
                Diff = diff,
                Votes = votes,
                MicroMove = micro,
                HookDetected = hook_verbose,
                ScaleUsed = null,
                TemplateId = System.IO.Path.GetFileName(pathToUse),
                ColorGateOk = null,
                MatchV1 = null,
                MatchV2 = null
            });
            return new BobberInWaterDetectionVerboseResult(
                detected,
                maxVal,
                posX,
                posY,
                fishingArea,
                jerk,
                flow,
                diff,
                votes,
                gateOn,
                gateExpiresAt,
                bestSize.Width,
                bestSize.Height);
        }
        catch (Exception ex)
        {
            // Garantir logging de frame também em exceções, para diagnóstico
            try
            {
                _reporter?.LogFrame(new BobberVisionFrame
                {
                    TimestampUtc = DateTime.UtcNow,
                    Detected = false,
                    Score = 0,
                    PosX = 0,
                    PosY = 0,
                    PatchW = 0,
                    PatchH = 0,
                    ZDy = 0,
                    ZRipple = 0,
                    DyPixels = 0,
                    RippleEnergy = 0,
                    Jerk = false,
                    Flow = false,
                    Diff = false,
                    Votes = 0,
                    MicroMove = false,
                    HookDetected = false,
                    ScaleUsed = null,
                    TemplateId = null,
                    ColorGateOk = null,
                    MatchV1 = null,
                    MatchV2 = null,
                    Reason = $"Exception (verbose): {ex.Message}"
                });
            }
            catch { }
            return BobberInWaterDetectionVerboseResult.Failed(ex.Message);
        }
    }

    public bool IsGateOpen() => (DateTime.UtcNow - _lastGateOn).TotalMilliseconds <= GateDurationMs;
    
    /// <summary>
    /// Método de diagnóstico isolado para testar um único frame com máximo detalhe.
    /// Salva imagens intermediárias se debugMode = true e retorna diagnóstico completo.
    /// </summary>
    public BobberDetectionDiagnostics DiagnoseFrame(Rectangle fishingArea, double confidenceThreshold = 0.5, bool saveImages = false)
    {
        var diagnosis = new BobberDetectionDiagnostics
        {
            TestTimestamp = DateTime.UtcNow,
            Region = fishingArea,
            ConfidenceThreshold = confidenceThreshold
        };

        try
        {
            diagnosis.AddStep("Iniciando diagnóstico", true);

            // 1. Teste de captura
            using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
            if (bitmap == null)
            {
                diagnosis.AddStep("ScreenCapture.CaptureRegion", false, "Retornou null");
                return diagnosis;
            }
            diagnosis.AddStep($"ScreenCapture ({bitmap.Width}x{bitmap.Height}, {bitmap.PixelFormat})", true);

            // 2. Teste de template
            var pathToUse = ResolvePath(TemplatePath ?? DefaultTemplatePath);
            if (!System.IO.File.Exists(pathToUse))
            {
                diagnosis.AddStep("Template existe", false, $"Não encontrado: {pathToUse}");
                return diagnosis;
            }
            diagnosis.AddStep($"Template existe ({pathToUse})", true);

            // 3. Teste de conversão OpenCV
            using var screenMat = BitmapConverter.ToMat(bitmap);
            if (screenMat.Empty())
            {
                diagnosis.AddStep("Conversão para Mat", false, "Mat resultante está vazio");
                return diagnosis;
            }
            diagnosis.AddStep($"Conversão para Mat ({screenMat.Width}x{screenMat.Height}, {screenMat.Channels()} canais)", true);

            // 4. Teste de carregamento do template
            using var templateRaw = Cv2.ImRead(pathToUse, ImreadModes.Unchanged);
            if (templateRaw.Empty())
            {
                diagnosis.AddStep("Carregamento template", false, "Template Mat está vazio");
                return diagnosis;
            }
            diagnosis.AddStep($"Template carregado ({templateRaw.Width}x{templateRaw.Height}, {templateRaw.Channels()} canais)", true);

            // 5. Salvar imagens se solicitado
            if (saveImages)
            {
                var debugDir = Path.Combine(AppContext.BaseDirectory, "debug_output");
                Directory.CreateDirectory(debugDir);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                bitmap.Save(Path.Combine(debugDir, $"captured_{timestamp}.png"));
                diagnosis.AddStep("Screenshot salvo", true);
                
                var templateBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templateRaw);
                templateBitmap.Save(Path.Combine(debugDir, $"template_{timestamp}.png"));
                templateBitmap.Dispose();
                diagnosis.AddStep("Template salvo", true);
            }

            // 6. Teste de detecção real
            var result = DetectInArea(fishingArea, confidenceThreshold, TemplatePath);
            diagnosis.AddStep($"Detecção executada (Score: {result.Score:F3}, Detectado: {result.Detected})", true);
            
            diagnosis.DetectionResult = result;
            diagnosis.Success = true;

        }
        catch (Exception ex)
        {
            diagnosis.AddStep($"EXCEÇÃO: {ex.Message}", false, ex.StackTrace);
        }

        return diagnosis;
    }

    private static string ResolvePath(string path)
    {
        if (System.IO.Path.IsPathRooted(path)) return path;
        var normalized = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
        var baseDir = AppContext.BaseDirectory;
        string[] probes = new[]
        {
            System.IO.Path.Combine(baseDir, normalized),
            System.IO.Path.Combine(baseDir, "win-x64", normalized),
            System.IO.Path.Combine(System.IO.Directory.GetParent(baseDir)?.FullName ?? baseDir, normalized)
        };
        foreach (var p in probes)
        {
            if (System.IO.File.Exists(p)) return p;
        }
        return path;
    }
}

public class BobberInWaterDetectionResult
{
    public bool Detected { get; }
    public double Score { get; }
    public float PositionX { get; }
    public float PositionY { get; }
    public Rectangle Region { get; }
    public string? Error { get; }

    public BobberInWaterDetectionResult(bool detected, double score, float positionX, float positionY, Rectangle region, string? error = null)
    {
        Detected = detected;
        Score = score;
        PositionX = positionX;
        PositionY = positionY;
        Region = region;
        Error = error;
    }

    public static BobberInWaterDetectionResult Failed(string error) =>
        new BobberInWaterDetectionResult(false, 0.0, 0f, 0f, Rectangle.Empty, error);
}

public class BobberInWaterDetectionVerboseResult : BobberInWaterDetectionResult
{
    public bool Jerk { get; }
    public bool Flow { get; }
    public bool Diff { get; }
    public int Votes { get; }
    public bool GateOn { get; }
    public DateTime GateExpiresAt { get; }
    public int PatchW { get; }
    public int PatchH { get; }
    public string? VerboseError { get; }

    public BobberInWaterDetectionVerboseResult(
        bool detected,
        double score,
        float positionX,
        float positionY,
        Rectangle region,
        bool jerk,
        bool flow,
        bool diff,
        int votes,
        bool gateOn,
        DateTime gateExpiresAt,
        int patchW,
        int patchH,
        string? error = null)
        : base(detected, score, positionX, positionY, region, error)
    {
        Jerk = jerk;
        Flow = flow;
        Diff = diff;
        Votes = votes;
        GateOn = gateOn;
        GateExpiresAt = gateExpiresAt;
        PatchW = patchW;
        PatchH = patchH;
        VerboseError = error;
    }

    public new static BobberInWaterDetectionVerboseResult Failed(string error) =>
        new BobberInWaterDetectionVerboseResult(
            false, 0.0, 0f, 0f, Rectangle.Empty,
            false, false, false, 0, false, DateTime.MinValue,
            0, 0,
            error);
}

/// <summary>
/// Resultado detalhado de diagnóstico para debugging do detector de bobber.
/// </summary>
public class BobberDetectionDiagnostics
{
    public DateTime TestTimestamp { get; set; }
    public Rectangle Region { get; set; }
    public double ConfidenceThreshold { get; set; }
    public bool Success { get; set; }
    public List<DiagnosticStep> Steps { get; set; } = new();
    public BobberInWaterDetectionResult? DetectionResult { get; set; }

    public void AddStep(string description, bool success, string? details = null)
    {
        Steps.Add(new DiagnosticStep
        {
            Timestamp = DateTime.UtcNow,
            Description = description,
            Success = success,
            Details = details
        });
    }

    public string GetFullReport()
    {
        var report = new StringBuilder();
        report.AppendLine($"=== DIAGNÓSTICO DE DETECÇÃO DE BOBBER ===");
        report.AppendLine($"Teste executado em: {TestTimestamp:yyyy-MM-dd HH:mm:ss.fff}");
        report.AppendLine($"Região: {Region}");
        report.AppendLine($"Threshold: {ConfidenceThreshold:F3}");
        report.AppendLine($"Resultado geral: {(Success ? "✅ SUCESSO" : "❌ FALHA")}");
        report.AppendLine();

        report.AppendLine("Passos executados:");
        foreach (var step in Steps)
        {
            var icon = step.Success ? "✅" : "❌";
            report.AppendLine($"  {icon} [{step.Timestamp:HH:mm:ss.fff}] {step.Description}");
            if (!string.IsNullOrWhiteSpace(step.Details))
            {
                report.AppendLine($"     Detalhes: {step.Details}");
            }
        }

        if (DetectionResult != null)
        {
            report.AppendLine();
            report.AppendLine("Resultado da detecção:");
            report.AppendLine($"  Detectado: {DetectionResult.Detected}");
            report.AppendLine($"  Score: {DetectionResult.Score:F3}");
            if (DetectionResult.Detected)
            {
                report.AppendLine($"  Posição: ({DetectionResult.PositionX:F1}, {DetectionResult.PositionY:F1})");
            }
            if (!string.IsNullOrWhiteSpace(DetectionResult.Error))
            {
                report.AppendLine($"  Erro: {DetectionResult.Error}");
            }
        }

        return report.ToString();
    }
}

/// <summary>
/// Representa um passo individual no processo de diagnóstico.
/// </summary>
public class DiagnosticStep
{
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = "";
    public bool Success { get; set; }
    public string? Details { get; set; }
}


