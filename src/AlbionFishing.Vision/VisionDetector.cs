using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;
using AlbionFishing.Vision.Services;

namespace AlbionFishing.Vision;

/// <summary>
/// Detector visual para o bobber do minigame de pesca usando template matching
/// </summary>
public class VisionDetector
{
    private readonly Rectangle _region;
    private readonly double _confidenceThreshold;
    private readonly string _templatePath;
    private readonly TemplateMatchModes _matchMethod;
    private readonly bool _enableDebugWindow; // ✅ NOVO: Controle de debug visual

    private bool _enablePreMatchColorFilter; // ✅ NOVO: Controle do pré-filtro de cor
    private DateTime _lastLogTime;
    private Scalar? _expectedTemplateHsv; // ✅ NOVO: HSV médio esperado do template
    private double _hsvTolerance = 15.0; // ✅ NOVO: Tolerância para diferença HSV
    private double _hsvToleranceH = 25.0; // ✅ NOVO: Tolerância específica para Hue
    private double _hsvToleranceS = 50.0; // ✅ NOVO: Tolerância específica para Saturation  
    private double _hsvToleranceV = 60.0; // ✅ NOVO: Tolerância específica para Value
    private double _hsvScoreThreshold = 0.6; // ✅ NOVO: Threshold mínimo para score HSV (ajustado para melhor detecção)
    private bool _enableHsvSoftMode = true; // ✅ NOVO: Modo suave para HSV
    
    // ✅ NOVO: Configurações para análise local em blocos
    private bool _enableLocalBlockAnalysis = true;
    private int _localBlockSize = 20;
    private double _localBlockThreshold = 0.4;
    private int _minMatchingBlocks = 2;
    
    // ✅ NOVO: Configurações para máscara de exclusão de cores
    private bool _enableColorMaskExclusion = true;
    private Scalar _exclusionMaskLower = new Scalar(15, 50, 50); // Tons "arenosos" - H: 15-40, S: 50-150, V: 50-150
    private Scalar _exclusionMaskUpper = new Scalar(40, 150, 150);
    private double _maxExcludedPixelRatio = 0.7; // Máximo 70% de pixels excluídos
    
    // ✅ NOVO: Configurações para fallback mode
    private bool _allowFallbackTemplateMatching = true;
    private bool _logFallbackDecisions = true;
    
    // ✅ NOVO: Estatísticas para análise
    private int _totalFramesProcessed = 0;
    private int _hsvFilterRejections = 0;
    private int _fallbackExecutions = 0;
    private List<double> _hsvScores = new List<double>();

    private float _positionBoober = 0;

    // ✅ NOVO: Serviço de filtro de cor para detecção robusta de cor do bobber
    private readonly ColorFilterService _colorFilterService;

    /// <summary>
    /// Construtor principal do VisionDetector
    /// </summary>
    /// <param name="region">Região de detecção</param>
    /// <param name="confidenceThreshold">Threshold de confiança (0.0 a 1.0)</param>
    /// <param name="templatePath">Caminho para o template</param>
    /// <param name="matchMethod">Método de template matching</param>
    /// <param name="enableDebugWindow">Ativar janela de debug visual automaticamente</param>
    /// <param name="enableColorFilter">Ativar filtro de cor para reduzir falsos positivos</param>
    /// <param name="enablePreMatchColorFilter">Ativar pré-filtro de cor antes do template matching</param>
    public VisionDetector(Rectangle region, double confidenceThreshold = 0.5, string templatePath = "data/images/bobber.png", TemplateMatchModes matchMethod = TemplateMatchModes.CCoeffNormed, bool enableDebugWindow = false, bool enablePreMatchColorFilter = true)
    {
        _region = region;
        _confidenceThreshold = confidenceThreshold;
        _templatePath = templatePath;
        _matchMethod = matchMethod;
        _enableDebugWindow = enableDebugWindow; // ✅ NOVO: Armazenar configuração de debug
        _enablePreMatchColorFilter = enablePreMatchColorFilter; // ✅ NOVO: Armazenar configuração do pré-filtro
        _lastLogTime = DateTime.Now;

        // ✅ NOVO: Inicializar ColorFilterService com configurações otimizadas
        _colorFilterService = new ColorFilterService(
            redRatioThreshold: 0.05,
            whiteRatioThreshold: 0.1,
            greenRejectionThreshold: 0.7,
            enableDebug: enableDebugWindow,
            minBobberColorRatio: 0.01, // 1% de pixels com cor do bobber
            bobberHsvToleranceH: 25.0,
            bobberHsvToleranceS: 50.0,
            bobberHsvToleranceV: 60.0
        );

        // ✅ NOVO: Calcular HSV médio esperado do template
        _expectedTemplateHsv = CalculateExpectedTemplateHsv();

        // ✅ NOVO: Configurar HSV esperado do bobber no ColorFilterService
        if (_expectedTemplateHsv.HasValue)
        {
            _colorFilterService.SetExpectedBobberHsv(_expectedTemplateHsv.Value);
        }

        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚀 Inicializado");
        Console.WriteLine($"   - Região: {_region}");
        Console.WriteLine($"   - Threshold: {_confidenceThreshold:F4}");
        Console.WriteLine($"   - Template: {_templatePath}");
        Console.WriteLine($"   - Match Method: {_matchMethod}");
        Console.WriteLine($"   - Debug Window: {_enableDebugWindow}");
        Console.WriteLine($"   - Pre-Match Color Filter: {_enablePreMatchColorFilter}");
        Console.WriteLine($"   - HSV Score Threshold: {_hsvScoreThreshold:F3}");
        Console.WriteLine($"   - Fallback Mode: {_allowFallbackTemplateMatching}");
        if (_expectedTemplateHsv.HasValue)
        {
            Console.WriteLine($"   - Expected HSV: H={_expectedTemplateHsv.Value.Val0:F1}, S={_expectedTemplateHsv.Value.Val1:F1}, V={_expectedTemplateHsv.Value.Val2:F1}");
        }
        System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚀 Inicializado - Região: {_region}, Threshold: {_confidenceThreshold:F4}");
    }

    /// <summary>
    /// Construtor alternativo com parâmetros individuais
    /// </summary>
    /// <param name="x">Posição X da região</param>
    /// <param name="y">Posição Y da região</param>
    /// <param name="width">Largura da região</param>
    /// <param name="height">Altura da região</param>
    /// <param name="confidenceThreshold">Threshold de confiança</param>
    /// <param name="templatePath">Caminho para o template</param>
    /// <param name="matchMethod">Método de template matching</param>
    /// <param name="enableDebugWindow">Ativar janela de debug visual automaticamente</param>
    /// <param name="enableColorFilter">Ativar filtro de cor para reduzir falsos positivos</param>
    /// <param name="enablePreMatchColorFilter">Ativar pré-filtro de cor antes do template matching</param>
    public VisionDetector(int x, int y, int width, int height, double confidenceThreshold = 0.3, string templatePath = "data/images/bobber.png", TemplateMatchModes matchMethod = TemplateMatchModes.CCoeffNormed, bool enableDebugWindow = false, bool enablePreMatchColorFilter = true)
        : this(new Rectangle(x, y, width, height), confidenceThreshold, templatePath, matchMethod, enableDebugWindow, enablePreMatchColorFilter)
    {
    }

    /// <summary>
    /// Construtor avançado com configurações HSV personalizadas
    /// </summary>
    /// <param name="region">Região de detecção</param>
    /// <param name="confidenceThreshold">Threshold de confiança</param>
    /// <param name="templatePath">Caminho para o template</param>
    /// <param name="matchMethod">Método de template matching</param>
    /// <param name="enableDebugWindow">Ativar janela de debug visual</param>
    /// <param name="enableColorFilter">Ativar filtro de cor</param>
    /// <param name="enablePreMatchColorFilter">Ativar pré-filtro de cor</param>
    /// <param name="hsvToleranceH">Tolerância para Hue (0-180)</param>
    /// <param name="hsvToleranceS">Tolerância para Saturation (0-255)</param>
    /// <param name="hsvToleranceV">Tolerância para Value (0-255)</param>
    /// <param name="hsvScoreThreshold">Threshold mínimo para score HSV (0.0-1.0)</param>
    /// <param name="enableHsvSoftMode">Ativar modo suave para HSV</param>
    public VisionDetector(Rectangle region, double confidenceThreshold, string templatePath, TemplateMatchModes matchMethod, 
        bool enableDebugWindow, bool enablePreMatchColorFilter,
        double hsvToleranceH = 25.0, double hsvToleranceS = 50.0, double hsvToleranceV = 60.0, 
        double hsvScoreThreshold = 0.3, bool enableHsvSoftMode = true)
    {
        _region = region;
        _confidenceThreshold = confidenceThreshold;
        _templatePath = templatePath;
        _matchMethod = matchMethod;
        _enableDebugWindow = enableDebugWindow;
        _enablePreMatchColorFilter = enablePreMatchColorFilter;
        _hsvToleranceH = hsvToleranceH;
        _hsvToleranceS = hsvToleranceS;
        _hsvToleranceV = hsvToleranceV;
        _hsvScoreThreshold = hsvScoreThreshold;
        _enableHsvSoftMode = enableHsvSoftMode;
        
        _lastLogTime = DateTime.Now;

        // Calcular HSV médio esperado do template
        _expectedTemplateHsv = CalculateExpectedTemplateHsv();

        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚀 Inicializado (Avançado)");
        Console.WriteLine($"   - Região: {_region}");
        Console.WriteLine($"   - Threshold: {_confidenceThreshold:F4}");
        Console.WriteLine($"   - Template: {_templatePath}");
        Console.WriteLine($"   - Match Method: {_matchMethod}");
        Console.WriteLine($"   - Debug Window: {_enableDebugWindow}");

        Console.WriteLine($"   - Pre-Match Color Filter: {_enablePreMatchColorFilter}");
        Console.WriteLine($"   - HSV Tolerances: H={_hsvToleranceH:F1}, S={_hsvToleranceS:F1}, V={_hsvToleranceV:F1}");
        Console.WriteLine($"   - HSV Score Threshold: {_hsvScoreThreshold:F3}");
        Console.WriteLine($"   - HSV Soft Mode: {_enableHsvSoftMode}");
        if (_expectedTemplateHsv.HasValue)
        {
            Console.WriteLine($"   - Expected HSV: H={_expectedTemplateHsv.Value.Val0:F1}, S={_expectedTemplateHsv.Value.Val1:F1}, V={_expectedTemplateHsv.Value.Val2:F1}");
        }
        System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚀 Inicializado (Avançado) - Região: {_region}, Threshold: {_confidenceThreshold:F4}");
    }

    /// <summary>
    /// Detecta o bobber na região configurada
    /// </summary>
    /// <returns>Tupla (detected, score, posX) com o resultado da detecção e posição horizontal</returns>
    public (bool detected, double score, float posX) Detect()
    {
        try
        {
            using var bitmap = ScreenCaptureHelper.CaptureRegion(_region);
            var result = AnalyzeTemplateMatchingWithPosition(bitmap);
            
            // Log a cada 3 segundos para debug
            var now = DateTime.Now;
            if ((now - _lastLogTime).TotalSeconds >= 3)
            {
                Console.WriteLine($"[{now:HH:mm:ss}] 👁️ VisionDetector: 🔍 Detecção - Score: {result.score:F4}, Detected: {result.detected}, PosX: {result.posX:F1}, Threshold: {_confidenceThreshold:F4}");
                System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] 👁️ VisionDetector: 🔍 Detecção - Score: {result.score:F4}, Detected: {result.detected}, PosX: {result.posX:F1}, Threshold: {_confidenceThreshold:F4}");
                _lastLogTime = now;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            var now = DateTime.Now;
            Console.WriteLine($"[{now:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na detecção: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na detecção: {ex.Message}");
            return (false, 0.0, 0.0f);
        }
    }

    /// <summary>
    /// Método original Detect() mantido para compatibilidade
    /// </summary>
    /// <returns>Tupla (detected, score) com o resultado da análise</returns>
    public (bool detected, double score) DetectOriginal()
    {
        var result = Detect();
        return (result.detected, result.score);
    }

    /// <summary>
    /// Analisa uma imagem usando template matching com OpenCvSharp e retorna posição
    /// </summary>
    /// <param name="bitmap">Bitmap da região capturada</param>
    /// <returns>Tupla (detected, score, posX) com o resultado da análise e posição horizontal</returns>
    private (bool detected, double score, float posX) AnalyzeTemplateMatchingWithPosition(Bitmap bitmap)
    {
        try
        {
            //TODO: Aplicar com melhoria
            // ✅ Ajuste: Pré-filtro HSV em modo "soft" quando habilitado (_enableHsvSoftMode)
            if (_enablePreMatchColorFilter)
            {
                var isValidColor = IsScreenColorValid(bitmap);
                if (!isValidColor)
                {
                    var logTime = DateTime.Now;
                    Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚫 Pré-filtro HSV sinalizou rejeição de cor");
                    System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚫 Pré-filtro HSV sinalizou rejeição de cor");
                    if (!_enableHsvSoftMode)
                    {
                        return (false, 0.0, 0.0f);
                    }
                    // Soft mode: continua o processamento como fallback
                }
            }

            // 1. Converter Bitmap para Mat
            using var screenMat = BitmapConverter.ToMat(bitmap);
            
            // 2. Ajustar canal de cor: RGB -> BGR (OpenCV usa BGR por padrão)
            Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.RGB2BGR);
            
            // Verificar se a conversão foi bem-sucedida
            if (screenMat.Empty())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na conversão de cor - Mat vazio após conversão");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na conversão de cor - Mat vazio após conversão");
                return (false, 0.0, 0.0f);
            }
            
            // ✅ NOVO: Normalizar canais de cor para HSV para análise consistente
            using var screenHsv = new Mat();
            Cv2.CvtColor(screenMat, screenHsv, ColorConversionCodes.BGR2HSV);
            
            // ✅ NOVO: Calcular valores médios HSV da tela para debug
            Scalar meanScreenHsv = Cv2.Mean(screenHsv);
            var logTimeHsv = DateTime.Now;
            Console.WriteLine($"[{logTimeHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio da Tela - H={meanScreenHsv.Val0:F1}, S={meanScreenHsv.Val1:F1}, V={meanScreenHsv.Val2:F1}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio da Tela - H={meanScreenHsv.Val0:F1}, S={meanScreenHsv.Val1:F1}, V={meanScreenHsv.Val2:F1}");
            
            // 3. Carregar template do bobber
            if (!File.Exists(_templatePath))
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template não encontrado: {_templatePath}");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template não encontrado: {_templatePath}");
                return (false, 0.0, 0.0f);
            }

            using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template inválido ou vazio: {_templatePath}");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template inválido ou vazio: {_templatePath}");
                return (false, 0.0, 0.0f);
            }

            // 4. Forçar conversão para 3 canais BGR se necessário
            if (templateMat.Channels() == 4) // RGBA
            {
                Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.RGBA2BGR);
            }
            // Nota: Grayscale será tratado automaticamente pelo OpenCV
            
            // ✅ NOVO: Normalizar template para HSV e analisar valores médios
            using var templateHsv = new Mat();
            Cv2.CvtColor(templateMat, templateHsv, ColorConversionCodes.BGR2HSV);
            
            // ✅ NOVO: Calcular valores médios HSV do template para debug
            Scalar meanTemplateHsv = Cv2.Mean(templateHsv);
            var logTimeTemplateHsv = DateTime.Now;
            Console.WriteLine($"[{logTimeTemplateHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio do Template - H={meanTemplateHsv.Val0:F1}, S={meanTemplateHsv.Val1:F1}, V={meanTemplateHsv.Val2:F1}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeTemplateHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio do Template - H={meanTemplateHsv.Val0:F1}, S={meanTemplateHsv.Val1:F1}, V={meanTemplateHsv.Val2:F1}");
            
            // ✅ NOVO: Salvar template convertido para HSV como debug
            try
            {
                Cv2.ImWrite("debug_template_hsv.png", templateHsv);
                Cv2.ImWrite("debug_screen_hsv.png", screenHsv);
                Console.WriteLine($"[{logTimeTemplateHsv:HH:mm:ss}] 👁️ VisionDetector: 💾 Imagens HSV de debug salvas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{logTimeTemplateHsv:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Erro ao salvar HSV debug: {ex.Message}");
            }

            // 5. Verificar compatibilidade de canais (mitigado): converter automaticamente o template para BGR se necessário
            if (screenMat.Channels() != templateMat.Channels())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Ajustando canais - Screen: {screenMat.Channels()}, Template: {templateMat.Channels()}");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Ajustando canais - Screen: {screenMat.Channels()}, Template: {templateMat.Channels()}");
                if (templateMat.Channels() == 4)
                {
                    Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.RGBA2BGR);
                }
                else if (templateMat.Channels() == 1)
                {
                    Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.GRAY2BGR);
                }
            }

            // 6. Forçar redimensionamento do template se for maior que o frame
            if (templateMat.Width > screenMat.Width || templateMat.Height > screenMat.Height)
            {
                var scaleX = screenMat.Width / (double)templateMat.Width;
                var scaleY = screenMat.Height / (double)templateMat.Height;
                var scale = Math.Min(scaleX, scaleY) * 0.9; // Reduzir um pouco para margem
                
                var newWidth = (int)(templateMat.Width * scale);
                var newHeight = (int)(templateMat.Height * scale);
                
                Cv2.Resize(templateMat, templateMat, new OpenCvSharp.Size(newWidth, newHeight));
                
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Template redimensionado - Original: {templateMat.Width}x{templateMat.Height}, Novo: {newWidth}x{newHeight}, Scale: {scale:F3}");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Template redimensionado - Original: {templateMat.Width}x{templateMat.Height}, Novo: {newWidth}x{newHeight}, Scale: {scale:F3}");
            }

            // 7. Log detalhado de debug
            var logTimeDebug = DateTime.Now;
            Console.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: 🔍 Debug Detalhado");
            Console.WriteLine($"   - Screen: {screenMat.Width}x{screenMat.Height}, {screenMat.Channels()}ch, Type: {screenMat.Type()}");
            Console.WriteLine($"   - Template: {templateMat.Width}x{templateMat.Height}, {templateMat.Channels()}ch, Type: {templateMat.Type()}");
            Console.WriteLine($"   - Match Method: {_matchMethod}, Threshold: {_confidenceThreshold:F4}");
            
            // ✅ NOVO: Análise comparativa HSV entre template e tela (log reduzido)
            Console.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: 🎨 Comparação HSV (log reduzido)");
            
            // ✅ NOVO: Calcular diferenças HSV para diagnóstico
            double hDiff = Math.Abs(meanTemplateHsv.Val0 - meanScreenHsv.Val0);
            double sDiff = Math.Abs(meanTemplateHsv.Val1 - meanScreenHsv.Val1);
            double vDiff = Math.Abs(meanTemplateHsv.Val2 - meanScreenHsv.Val2);
            Console.WriteLine($"   - Diferenças HSV: ΔH={hDiff:F1}, ΔS={sDiff:F1}, ΔV={vDiff:F1}");
            
            // ✅ NOVO: Alertar se diferenças são significativas
            if (!_enableHsvSoftMode && (hDiff > 10 || sDiff > 30 || vDiff > 30))
            {
                Console.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: ⚠️ ATENÇÃO - Diferenças significativas de cor detectadas!");
                System.Diagnostics.Trace.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Diferenças significativas de cor - ΔH={hDiff:F1}, ΔS={sDiff:F1}, ΔV={vDiff:F1}");
            }
            
            System.Diagnostics.Trace.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: 🔍 Debug Detalhado - Screen: {screenMat.Width}x{screenMat.Height}, Template: {templateMat.Width}x{templateMat.Height}");

            // 8. Salvar imagens de debug (apenas na primeira execução)
            bool debugImagesSaved = false;
            if (!debugImagesSaved)
            {
                try
                {
                    Cv2.ImWrite("debug_screen.png", screenMat);
                    Cv2.ImWrite("debug_template.png", templateMat);
                    Console.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: 💾 Imagens de debug salvas");
                    debugImagesSaved = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{logTimeDebug:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Erro ao salvar debug: {ex.Message}");
                }
            }

            // 9. Executar template matching
            using var result = new Mat();
            Cv2.MatchTemplate(screenMat, templateMat, result, _matchMethod);

            // 10. Encontrar o melhor match
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

            // 11. Salvar resultado de debug (omitido por padrão para reduzir IO)

            bool detected = maxVal >= _confidenceThreshold;
            
            // ✅ NOVO: Calcular posição X do bobber (centro do template). Mitigação: aceitar detecções fracas sob Soft Mode
            float posX = 0.0f;
            if (detected)
            {
                posX = maxLoc.X + (templateMat.Width / 2.0f);
                _positionBoober = posX; // Atualizar posição armazenada
            }
            else if (_enableHsvSoftMode && maxVal >= Math.Max(0.5 * _confidenceThreshold, 0.1))
            {
                // fallback: se o score for próximo do threshold, considerar como candidato fraco (não altera 'detected', só fornece posX)
                posX = maxLoc.X + (templateMat.Width / 2.0f);
            }
            
            // 12. Log detalhado para debug
            var logTime2 = DateTime.Now;
            Console.WriteLine($"[{logTime2:HH:mm:ss}] 👁️ VisionDetector: 🎯 Template Match - MaxVal: {maxVal:F4}, MinVal: {minVal:F4}, Threshold: {_confidenceThreshold:F4}, Detected: {detected}");
            Console.WriteLine($"   - Match Location: ({maxLoc.X}, {maxLoc.Y})");
            if (detected)
            {
                Console.WriteLine($"   - Bobber Position X: {posX:F1}");
            }
            System.Diagnostics.Trace.WriteLine($"[{logTime2:HH:mm:ss}] 👁️ VisionDetector: 🎯 Template Match - MaxVal: {maxVal:F4}, Threshold: {_confidenceThreshold:F4}, Detected: {detected}");

            // ✅ REMOVIDO: Filtro pós-match por cor - mantendo apenas o pré-filtro HSV (em modo soft por padrão)
            bool finalDetected = detected;
            double finalScore = maxVal;

            // 13. Debug visual (ativado automaticamente se configurado)
            if (_enableDebugWindow)
            {
                Cv2.Rectangle(screenMat, new Rect(maxLoc.X, maxLoc.Y, templateMat.Width, templateMat.Height), new Scalar(0, 255, 0), 2);
                Cv2.PutText(screenMat, $"Score: {finalScore:F3}", new Point(maxLoc.X, maxLoc.Y - 10), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 0), 1);
                if (detected)
                {
                    Cv2.PutText(screenMat, $"PosX: {posX:F1}", new Point(maxLoc.X, maxLoc.Y + templateMat.Height + 15), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 0), 1);
                }
                Cv2.ImShow("DEBUG: Bobber Match", screenMat);
                Cv2.WaitKey(1);
                
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🖼️ Janela de debug visual aberta");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🖼️ Janela de debug visual aberta");
            }

            return (finalDetected, finalScore, posX);
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no template matching: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no template matching: {ex.Message}");
            return (false, 0.0, 0.0f);
        }
    }

    /// <summary>
    /// Analisa uma imagem usando template matching com OpenCvSharp
    /// </summary>
    /// <param name="bitmap">Bitmap da região capturada</param>
    /// <returns>Tupla (detected, score) com o resultado da análise</returns>
    private (bool detected, double score) AnalyzeTemplateMatching(Bitmap bitmap)
    {
        var result = AnalyzeTemplateMatchingWithPosition(bitmap);
        return (result.detected, result.score);
    }

    /// <summary>
    /// Método reutilizável para executar template matching em uma imagem
    /// </summary>
    /// <param name="screenMat">Mat da tela</param>
    /// <param name="templateMat">Mat do template</param>
    /// <param name="matchMethod">Método de matching</param>
    /// <returns>Tupla (detected, score, maxLoc, templateWidth) com resultado e informações de posição</returns>
    private (bool detected, double score, Point maxLoc, int templateWidth) ExecuteTemplateMatching(Mat screenMat, Mat templateMat, TemplateMatchModes matchMethod)
    {
        try
        {
            // Executar template matching
            using var result = new Mat();
            Cv2.MatchTemplate(screenMat, templateMat, result, matchMethod);

            // Encontrar o melhor match
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

            bool detected = maxVal >= _confidenceThreshold;
            double finalScore = maxVal;

            return (detected, finalScore, maxLoc, templateMat.Width);
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no template matching: {ex.Message}");
            return (false, 0.0, new Point(0, 0), 0);
        }
    }

    /// <summary>
    /// Método reutilizável para calcular a posição X do bobber
    /// </summary>
    /// <param name="maxLoc">Localização do melhor match</param>
    /// <param name="templateWidth">Largura do template</param>
    /// <returns>Posição X do centro do bobber</returns>
    private float CalculateBobberPositionX(Point maxLoc, int templateWidth)
    {
        return maxLoc.X + (templateWidth / 2.0f);
    }

    /// <summary>
    /// Método reutilizável para validar e converter bitmap para Mat
    /// </summary>
    /// <param name="bitmap">Bitmap para converter</param>
    /// <returns>Mat convertido ou null se falhar</returns>
    private Mat? ConvertBitmapToMat(Bitmap bitmap)
    {
        try
        {
            // Converter Bitmap para Mat
            using var screenMat = BitmapConverter.ToMat(bitmap);
            
            // Ajustar canal de cor: RGB -> BGR (OpenCV usa BGR por padrão)
            Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.RGB2BGR);
            
            // Verificar se a conversão foi bem-sucedida
            if (screenMat.Empty())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na conversão de cor - Mat vazio após conversão");
                return null;
            }

            return screenMat.Clone(); // Retornar clone para evitar problemas de disposição
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na conversão de bitmap: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Método reutilizável para carregar template
    /// </summary>
    /// <param name="templatePath">Caminho do template</param>
    /// <returns>Mat do template ou null se falhar</returns>
    private Mat? LoadTemplate(string templatePath)
    {
        try
        {
            if (!File.Exists(templatePath))
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template não encontrado: {templatePath}");
                return null;
            }

            using var templateMat = Cv2.ImRead(templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template inválido ou vazio: {templatePath}");
                return null;
            }

            // Forçar conversão para 3 canais BGR se necessário
            if (templateMat.Channels() == 4) // RGBA
            {
                Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.RGBA2BGR);
            }

            return templateMat.Clone(); // Retornar clone para evitar problemas de disposição
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro ao carregar template: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Método reutilizável para redimensionar template se necessário
    /// </summary>
    /// <param name="templateMat">Mat do template</param>
    /// <param name="screenMat">Mat da tela</param>
    /// <returns>True se redimensionou, false se não foi necessário</returns>
    private bool ResizeTemplateIfNeeded(Mat templateMat, Mat screenMat)
    {
        try
        {
            // Verificar se o template é maior que o frame
            if (templateMat.Width > screenMat.Width || templateMat.Height > screenMat.Height)
            {
                var scaleX = screenMat.Width / (double)templateMat.Width;
                var scaleY = screenMat.Height / (double)templateMat.Height;
                var scale = Math.Min(scaleX, scaleY) * 0.9; // Reduzir um pouco para margem
                
                var newWidth = (int)(templateMat.Width * scale);
                var newHeight = (int)(templateMat.Height * scale);
                
                Cv2.Resize(templateMat, templateMat, new OpenCvSharp.Size(newWidth, newHeight));
                
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Template redimensionado - Original: {templateMat.Width}x{templateMat.Height}, Novo: {newWidth}x{newHeight}, Scale: {scale:F3}");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro ao redimensionar template: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Método reutilizável para detectar bobber com posição usando componentes modulares
    /// </summary>
    /// <param name="bitmap">Bitmap da região capturada</param>
    /// <returns>Tupla (detected, score, posX) com resultado da detecção</returns>
    public (bool detected, double score, float posX) DetectModular(Bitmap bitmap)
    {
        try
        {
            // Aplicar pré-filtro HSV se habilitado
            if (_enablePreMatchColorFilter && !IsScreenColorValid(bitmap))
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🚫 Pré-filtro HSV bloqueou processamento");
                return (false, 0.0, 0.0f);
            }

            // Converter bitmap para Mat
            using var screenMat = ConvertBitmapToMat(bitmap);
            if (screenMat == null)
            {
                return (false, 0.0, 0.0f);
            }

            // Carregar template
            using var templateMat = LoadTemplate(_templatePath);
            if (templateMat == null)
            {
                return (false, 0.0, 0.0f);
            }

            // Verificar compatibilidade de canais
            if (screenMat.Channels() != templateMat.Channels())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Incompatibilidade de canais - Screen: {screenMat.Channels()}, Template: {templateMat.Channels()}");
                return (false, 0.0, 0.0f);
            }

            // Redimensionar template se necessário
            ResizeTemplateIfNeeded(templateMat, screenMat);

            // Executar template matching
            var (detected, score, maxLoc, templateWidth) = ExecuteTemplateMatching(screenMat, templateMat, _matchMethod);

            // Calcular posição X se detectado
            float posX = 0.0f;
            if (detected)
            {
                posX = CalculateBobberPositionX(maxLoc, templateWidth);
                _positionBoober = posX; // Atualizar posição armazenada
            }

            return (detected, score, posX);
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na detecção modular: {ex.Message}");
            return (false, 0.0, 0.0f);
        }
    }

    /// <summary>
    /// Método reutilizável para detectar bobber em uma região específica
    /// </summary>
    /// <param name="region">Região para capturar</param>
    /// <param name="templatePath">Caminho do template</param>
    /// <param name="confidenceThreshold">Threshold de confiança</param>
    /// <returns>Tupla (detected, score, posX) com resultado da detecção</returns>
    public static (bool detected, double score, float posX) DetectBobberInRegion(Rectangle region, string templatePath, double confidenceThreshold = 0.5)
    {
        try
        {
            // Capturar região
            using var bitmap = ScreenCaptureHelper.CaptureRegion(region);
            if (bitmap == null)
            {
                return (false, 0.0, 0.0f);
            }

            // Criar detector temporário
            var detector = new VisionDetector(region, confidenceThreshold, templatePath);
            
            // Usar método modular
            return detector.DetectModular(bitmap);
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na detecção estática: {ex.Message}");
            return (false, 0.0, 0.0f);
        }
    }

    /// <summary>
    /// Verifica se a cor da tela está dentro dos padrões esperados para o bobber
    /// </summary>
    /// <param name="bitmap">Bitmap da região capturada</param>
    /// <returns>True se a cor está dentro dos padrões esperados</returns>
    private bool IsScreenColorValid(Bitmap bitmap)
    {
        // ✅ NOVO: Usar o ColorFilterService para detecção robusta de cor do bobber
        // Este método é muito mais robusto que comparar médias HSV, pois verifica a presença real de pixels com cor do bobber
        return _colorFilterService.HasSufficientBobberColorPresence(bitmap, _expectedTemplateHsv);
    }

    /// <summary>
    /// Testa múltiplos métodos de template matching para encontrar o melhor
    /// </summary>
    /// <param name="bitmap">Bitmap para análise</param>
    /// <returns>Tupla (detected, score, method) com o melhor resultado</returns>
    public (bool detected, double score, string method) TestMultipleMethods(Bitmap bitmap)
    {
        var methods = new[]
        {
            (TemplateMatchModes.CCoeffNormed, "CCoeffNormed"),
            (TemplateMatchModes.CCorrNormed, "CCorrNormed"),
            (TemplateMatchModes.SqDiffNormed, "SqDiffNormed"),
            (TemplateMatchModes.CCoeff, "CCoeff"),
            (TemplateMatchModes.CCorr, "CCorr"),
            (TemplateMatchModes.SqDiff, "SqDiff")
        };

        double bestScore = 0.0;
        string bestMethod = "";
        bool detected = false;

        foreach (var (method, methodName) in methods)
        {
            try
            {
                using var screenMat = BitmapConverter.ToMat(bitmap);
                Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.RGB2BGR);
                
                if (!File.Exists(_templatePath)) continue;
                using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
                if (templateMat.Empty()) continue;

                // Converter RGBA se necessário
                if (templateMat.Channels() == 4)
                {
                    Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.RGBA2BGR);
                }

                if (screenMat.Channels() != templateMat.Channels()) continue;

                // Redimensionar template se necessário
                if (templateMat.Width > screenMat.Width || templateMat.Height > screenMat.Height)
                {
                    var scaleX = screenMat.Width / (double)templateMat.Width;
                    var scaleY = screenMat.Height / (double)templateMat.Height;
                    var scale = Math.Min(scaleX, scaleY) * 0.9;
                    var newWidth = (int)(templateMat.Width * scale);
                    var newHeight = (int)(templateMat.Height * scale);
                    Cv2.Resize(templateMat, templateMat, new OpenCvSharp.Size(newWidth, newHeight));
                }

                using var result = new Mat();
                Cv2.MatchTemplate(screenMat, templateMat, result, method);
                Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                var score = method == TemplateMatchModes.SqDiff || method == TemplateMatchModes.SqDiffNormed ? 1.0 - maxVal : maxVal;
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMethod = methodName;
                    detected = score >= _confidenceThreshold;
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: 🧪 Teste {methodName} - Score: {score:F4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no teste {methodName}: {ex.Message}");
            }
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: 🏆 Melhor método: {bestMethod} - Score: {bestScore:F4}");
        return (detected, bestScore, bestMethod);
    }

    /// <summary>
    /// Analisa o template para extrair ranges ideais de HSV automaticamente
    /// </summary>
    /// <returns>Tupla com ranges HSV (hue, saturation, value)</returns>
    public ((double minH, double maxH), (double minS, double maxS), (double minV, double maxV)) AnalyzeTemplateHSV()
    {
        try
        {
            if (!File.Exists(_templatePath))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template não encontrado para análise HSV: {_templatePath}");
                return ((0, 180), (0, 255), (0, 255)); // Ranges padrão
            }

            using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template vazio para análise HSV");
                return ((0, 180), (0, 255), (0, 255)); // Ranges padrão
            }

            // Converter para HSV
            using var templateHsv = new Mat();
            Cv2.CvtColor(templateMat, templateHsv, ColorConversionCodes.BGR2HSV);

            // Separar canais HSV
            var hsvChannels = Cv2.Split(templateHsv);
            var hChannel = hsvChannels[0];
            var sChannel = hsvChannels[1];
            var vChannel = hsvChannels[2];

            // Calcular ranges (min/max) para cada canal
            Cv2.MinMaxLoc(hChannel, out double minH, out double maxH);
            Cv2.MinMaxLoc(sChannel, out double minS, out double maxS);
            Cv2.MinMaxLoc(vChannel, out double minV, out double maxV);

            // Calcular percentis para ranges mais robustos (5% e 95%)
            var hHist = new Mat();
            var sHist = new Mat();
            var vHist = new Mat();
            
            Cv2.CalcHist(new[] { hChannel }, new[] { 0 }, null, hHist, 1, new[] { 180 }, new[] { new Rangef(0f, 180f) });
            Cv2.CalcHist(new[] { sChannel }, new[] { 0 }, null, sHist, 1, new[] { 256 }, new[] { new Rangef(0f, 256f) });
            Cv2.CalcHist(new[] { vChannel }, new[] { 0 }, null, vHist, 1, new[] { 256 }, new[] { new Rangef(0f, 256f) });

            // Encontrar percentis 5% e 95%
            var hRange = FindPercentileRange(hHist, 0.05, 0.95, 180);
            var sRange = FindPercentileRange(sHist, 0.05, 0.95, 256);
            var vRange = FindPercentileRange(vHist, 0.05, 0.95, 256);

            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎨 Análise HSV do Template");
            Console.WriteLine($"   - Hue Range: {hRange.min:F1}-{hRange.max:F1} (min/max: {minH:F1}-{maxH:F1})");
            Console.WriteLine($"   - Saturation Range: {sRange.min:F1}-{sRange.max:F1} (min/max: {minS:F1}-{maxS:F1})");
            Console.WriteLine($"   - Value Range: {vRange.min:F1}-{vRange.max:F1} (min/max: {minV:F1}-{maxV:F1})");
            System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎨 Análise HSV do Template - H:{hRange.min:F1}-{hRange.max:F1}, S:{sRange.min:F1}-{sRange.max:F1}, V:{vRange.min:F1}-{vRange.max:F1}");

            return (hRange, sRange, vRange);
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na análise HSV do template: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na análise HSV do template: {ex.Message}");
            return ((0, 180), (0, 255), (0, 255)); // Ranges padrão em caso de erro
        }
    }

    /// <summary>
    /// Encontra os ranges de percentil para um histograma
    /// </summary>
    /// <param name="histogram">Histograma do canal</param>
    /// <param name="lowerPercentile">Percentil inferior (0.0-1.0)</param>
    /// <param name="upperPercentile">Percentil superior (0.0-1.0)</param>
    /// <param name="maxValue">Valor máximo do canal</param>
    /// <returns>Tupla (min, max) com os ranges encontrados</returns>
    private (double min, double max) FindPercentileRange(Mat histogram, double lowerPercentile, double upperPercentile, int maxValue)
    {
        double totalPixels = Cv2.Sum(histogram)[0];
        double lowerThreshold = totalPixels * lowerPercentile;
        double upperThreshold = totalPixels * upperPercentile;

        double cumulativeSum = 0;
        int lowerIndex = 0;
        int upperIndex = maxValue - 1;

        // Encontrar índice do percentil inferior
        for (int i = 0; i < maxValue; i++)
        {
            cumulativeSum += histogram.At<float>(i);
            if (cumulativeSum >= lowerThreshold)
            {
                lowerIndex = i;
                break;
            }
        }

        // Encontrar índice do percentil superior
        cumulativeSum = 0;
        for (int i = 0; i < maxValue; i++)
        {
            cumulativeSum += histogram.At<float>(i);
            if (cumulativeSum >= upperThreshold)
            {
                upperIndex = i;
                break;
            }
        }

        return (lowerIndex, upperIndex);
    }

    /// <summary>
    /// Calcula o HSV médio esperado do template
    /// </summary>
    /// <returns>Scalar com valores HSV médios ou null se não conseguir carregar</returns>
    private Scalar? CalculateExpectedTemplateHsv()
    {
        try
        {
            if (!File.Exists(_templatePath))
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template não encontrado para cálculo HSV: {_templatePath}");
                return null;
            }

            using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Template vazio para cálculo HSV");
                return null;
            }

            // Converter para HSV
            using var templateHsv = new Mat();
            Cv2.CvtColor(templateMat, templateHsv, ColorConversionCodes.BGR2HSV);

            // Calcular média HSV
            var meanHsv = Cv2.Mean(templateHsv);
            
            var logTimeHsv = DateTime.Now;
            Console.WriteLine($"[{logTimeHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio do Template Calculado - H={meanHsv.Val0:F1}, S={meanHsv.Val1:F1}, V={meanHsv.Val2:F1}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeHsv:HH:mm:ss}] 👁️ VisionDetector: 🎨 HSV Médio do Template - H={meanHsv.Val0:F1}, S={meanHsv.Val1:F1}, V={meanHsv.Val2:F1}");

            return meanHsv;
        }
        catch (Exception ex)
        {
            var logTimeError = DateTime.Now;
            Console.WriteLine($"[{logTimeError:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro ao calcular HSV do template: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeError:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro ao calcular HSV do template: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Calcula um score de similaridade HSV entre a tela e o template esperado
    /// </summary>
    /// <param name="actualHsv">HSV atual da região</param>
    /// <param name="expectedHsv">HSV esperado do template</param>
    /// <returns>Score de similaridade (0.0-1.0) onde 1.0 = idêntico</returns>
    private double CalculateHsvSimilarityScore(Scalar actualHsv, Scalar expectedHsv)
    {
        // ✅ MELHORADO: Calcular diferenças normalizadas para cada canal
        double hDiff = Math.Abs(actualHsv.Val0 - expectedHsv.Val0);
        double sDiff = Math.Abs(actualHsv.Val1 - expectedHsv.Val1);
        double vDiff = Math.Abs(actualHsv.Val2 - expectedHsv.Val2);

        // ✅ MELHORADO: Para Hue, considerar que 0 e 180 são próximos (círculo de cores)
        if (hDiff > 90) hDiff = 180 - hDiff;

        // ✅ MELHORADO: Normalizar diferenças usando tolerâncias específicas
        double hScore = Math.Max(0, 1.0 - (hDiff / _hsvToleranceH));
        double sScore = Math.Max(0, 1.0 - (sDiff / _hsvToleranceS));
        double vScore = Math.Max(0, 1.0 - (vDiff / _hsvToleranceV));

        // ✅ MELHORADO: Calcular score final como média ponderada
        // Hue tem peso menor pois é mais variável, Value tem peso maior pois é mais estável
        double finalScore = (hScore * 0.2) + (sScore * 0.3) + (vScore * 0.5);

        if (_enableDebugWindow)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎨 Score HSV Detalhado");
            Console.WriteLine($"   - Diferenças: ΔH={hDiff:F1}, ΔS={sDiff:F1}, ΔV={vDiff:F1}");
            Console.WriteLine($"   - Scores: H={hScore:F3}, S={sScore:F3}, V={vScore:F3}");
            Console.WriteLine($"   - Score Final: {finalScore:F3}");
        }

        return finalScore;
    }

    /// <summary>
    /// ✅ NOVO: Análise local em blocos para evitar falsos positivos de áreas grandes
    /// </summary>
    /// <param name="screenHsv">Mat HSV da tela</param>
    /// <param name="expectedHsv">HSV esperado do template</param>
    /// <param name="threshold">Threshold mínimo para score</param>
    /// <param name="blockSize">Tamanho dos blocos para análise</param>
    /// <returns>True se encontrar blocos suficientes com cores compatíveis</returns>
    private bool ContainsMatchingColorBlocks(Mat screenHsv, Scalar expectedHsv, double threshold, int blockSize = 20)
    {
        if (!_enableLocalBlockAnalysis)
            return true; // Se desabilitado, aceitar sempre

        int matchingBlocks = 0;
        int totalBlocks = 0;
        var matchingBlockPositions = new List<(int x, int y, double score)>();

        for (int y = 0; y < screenHsv.Rows; y += blockSize)
        {
            for (int x = 0; x < screenHsv.Cols; x += blockSize)
            {
                totalBlocks++;
                
                // Calcular região do bloco
                var rect = new Rect(x, y, 
                    Math.Min(blockSize, screenHsv.Cols - x), 
                    Math.Min(blockSize, screenHsv.Rows - y));
                
                if (rect.Width <= 0 || rect.Height <= 0) continue;
                
                // Extrair bloco
                using var block = new Mat(screenHsv, rect);
                var blockMean = Cv2.Mean(block);
                
                // Calcular score do bloco
                var blockScore = CalculateHsvSimilarityScore(blockMean, expectedHsv);
                
                if (blockScore >= threshold)
                {
                    matchingBlocks++;
                    matchingBlockPositions.Add((x, y, blockScore));
                }
            }
        }

        bool isAccepted = matchingBlocks >= _minMatchingBlocks;
        
        if (_enableDebugWindow)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔍 Análise Local em Blocos");
            Console.WriteLine($"   - Blocos compatíveis: {matchingBlocks}/{totalBlocks}");
            Console.WriteLine($"   - Threshold: {threshold:F3}, Mínimo: {_minMatchingBlocks}");
            Console.WriteLine($"   - Aceito: {isAccepted}");
            
            if (matchingBlockPositions.Count > 0)
            {
                Console.WriteLine($"   - Melhores blocos:");
                foreach (var (x, y, score) in matchingBlockPositions.OrderByDescending(p => p.score).Take(3))
                {
                    Console.WriteLine($"     * Posição ({x}, {y}): Score {score:F3}");
                }
            }
        }

        return isAccepted;
    }

    /// <summary>
    /// ✅ NOVO: Filtro por máscara de exclusão de cores (ex: tons "arenosos")
    /// </summary>
    /// <param name="screenHsv">Mat HSV da tela</param>
    /// <returns>True se a proporção de pixels excluídos for aceitável</returns>
    private bool IsColorMaskExclusionValid(Mat screenHsv)
    {
        if (!_enableColorMaskExclusion)
            return true; // Se desabilitado, aceitar sempre

        try
        {
            // Criar máscara para cores excluídas
            using var exclusionMask = new Mat();
            Cv2.InRange(screenHsv, _exclusionMaskLower, _exclusionMaskUpper, exclusionMask);
            
            // Contar pixels excluídos
            int excludedPixels = Cv2.CountNonZero(exclusionMask);
            int totalPixels = screenHsv.Rows * screenHsv.Cols;
            double excludedRatio = (double)excludedPixels / totalPixels;
            
            bool isAccepted = excludedRatio <= _maxExcludedPixelRatio;
            
            if (_enableDebugWindow)
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎨 Máscara de Exclusão de Cores");
                Console.WriteLine($"   - Pixels excluídos: {excludedPixels}/{totalPixels} ({excludedRatio:P1})");
                Console.WriteLine($"   - Máximo permitido: {_maxExcludedPixelRatio:P1}");
                Console.WriteLine($"   - Aceito: {isAccepted}");
            }
            
            return isAccepted;
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro na máscara de exclusão: {ex.Message}");
            return true; // Em caso de erro, aceitar
        }
    }

    /// <summary>
    /// ✅ NOVO: Método unificado de filtragem HSV avançada
    /// </summary>
    /// <param name="bitmap">Bitmap para análise</param>
    /// <returns>True se passar em todos os filtros ou fallback ativado</returns>
    private bool IsAdvancedHsvFilterValid(Bitmap bitmap)
    {
        if (!_expectedTemplateHsv.HasValue)
            return true; // Se não temos HSV esperado, não podemos filtrar

        try
        {
            _totalFramesProcessed++;
            
            // Converter bitmap para Mat e depois para HSV
            using var screenMat = BitmapConverter.ToMat(bitmap);
            using var screenHsv = new Mat();
            Cv2.CvtColor(screenMat, screenHsv, ColorConversionCodes.RGB2HSV);

            // ✅ PASSO 1: Análise local em blocos
            bool localBlocksValid = ContainsMatchingColorBlocks(
                screenHsv, 
                _expectedTemplateHsv.Value, 
                _localBlockThreshold, 
                _localBlockSize
            );

            // ✅ PASSO 2: Máscara de exclusão de cores
            bool colorMaskValid = IsColorMaskExclusionValid(screenHsv);

            // ✅ PASSO 3: Score HSV global (método original como backup)
            Scalar meanScreenHsv = Cv2.Mean(screenHsv);
            var globalHsvScore = CalculateHsvSimilarityScore(meanScreenHsv, _expectedTemplateHsv.Value);
            bool globalHsvValid = globalHsvScore >= _hsvScoreThreshold;

            // ✅ PASSO 4: Decisão final
            bool allFiltersPassed = localBlocksValid && colorMaskValid && globalHsvValid;
            
            // ✅ PASSO 5: Fallback mode
            bool shouldExecuteFallback = !allFiltersPassed && _allowFallbackTemplateMatching;
            
            // ✅ NOVO: Log explícito de rejeição HSV
            if (!allFiltersPassed && !shouldExecuteFallback)
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] ❌ VisionDetector: Template matching BLOQUEADO pelo filtro HSV!");
                Console.WriteLine($"   - Blocos locais: {localBlocksValid}");
                Console.WriteLine($"   - Máscara de cores: {colorMaskValid}");
                Console.WriteLine($"   - Score global: {globalHsvValid} ({globalHsvScore:F3})");
                Console.WriteLine($"   - Threshold: {_hsvScoreThreshold:F3}");
                Console.WriteLine($"   - Fallback habilitado: {_allowFallbackTemplateMatching}");
            }
            
            if (shouldExecuteFallback)
            {
                _fallbackExecutions++;
                if (_logFallbackDecisions)
                {
                    var logTime = DateTime.Now;
                    Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ⚠️ Filtros HSV falharam, mas executando template matching como fallback");
                    Console.WriteLine($"   - Blocos locais: {localBlocksValid}");
                    Console.WriteLine($"   - Máscara de cores: {colorMaskValid}");
                    Console.WriteLine($"   - Score global: {globalHsvValid} ({globalHsvScore:F3})");
                }
            }

            if (!allFiltersPassed)
                _hsvFilterRejections++;

            _hsvScores.Add(globalHsvScore);

            // Log detalhado
            var logTimeAdvanced = DateTime.Now;
            Console.WriteLine($"[{logTimeAdvanced:HH:mm:ss}] 👁️ VisionDetector: 🎨 Filtro HSV Avançado");
            Console.WriteLine($"   - Blocos locais: {localBlocksValid}");
            Console.WriteLine($"   - Máscara de cores: {colorMaskValid}");
            Console.WriteLine($"   - Score global: {globalHsvScore:F3} (threshold: {_hsvScoreThreshold:F3})");
            Console.WriteLine($"   - Todos os filtros: {allFiltersPassed}");
            Console.WriteLine($"   - Fallback ativado: {shouldExecuteFallback}");

            // Retornar true se todos os filtros passaram OU se fallback está ativado
            return allFiltersPassed || shouldExecuteFallback;
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no filtro HSV avançado: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no filtro HSV avançado: {ex.Message}");
            return true; // Em caso de erro, processar normalmente
        }
    }

    /// <summary>
    /// ✅ NOVO: Obter estatísticas dos filtros HSV
    /// </summary>
    /// <returns>Estatísticas detalhadas</returns>
    public (int totalFrames, int rejections, int fallbacks, double avgScore, double minScore, double maxScore) GetHsvFilterStatistics()
    {
        double avgScore = _hsvScores.Count > 0 ? _hsvScores.Average() : 0.0;
        double minScore = _hsvScores.Count > 0 ? _hsvScores.Min() : 0.0;
        double maxScore = _hsvScores.Count > 0 ? _hsvScores.Max() : 0.0;
        
        return (_totalFramesProcessed, _hsvFilterRejections, _fallbackExecutions, avgScore, minScore, maxScore);
    }

    /// <summary>
    /// ✅ NOVO: Configurar parâmetros do filtro HSV avançado
    /// </summary>
    public void ConfigureAdvancedHsvFilter(
        bool enableLocalBlocks = true, int blockSize = 20, double blockThreshold = 0.4, int minBlocks = 2,
        bool enableColorMask = true, Scalar? exclusionLower = null, Scalar? exclusionUpper = null, double maxExcludedRatio = 0.7,
        bool allowFallback = true, bool logFallback = true, double hsvScoreThreshold = 0.6)
    {
        _enableLocalBlockAnalysis = enableLocalBlocks;
        _localBlockSize = blockSize;
        _localBlockThreshold = blockThreshold;
        _minMatchingBlocks = minBlocks;
        _hsvScoreThreshold = hsvScoreThreshold; // ✅ NOVO: Permitir ajuste do threshold
        
        _enableColorMaskExclusion = enableColorMask;
        if (exclusionLower.HasValue) _exclusionMaskLower = exclusionLower.Value;
        if (exclusionUpper.HasValue) _exclusionMaskUpper = exclusionUpper.Value;
        _maxExcludedPixelRatio = maxExcludedRatio;
        
        _allowFallbackTemplateMatching = allowFallback;
        _logFallbackDecisions = logFallback;
        
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Filtro HSV Avançado Configurado");
        Console.WriteLine($"   - Análise local: {_enableLocalBlockAnalysis} (blocos: {_localBlockSize}, threshold: {_localBlockThreshold:F3})");
        Console.WriteLine($"   - Máscara de cores: {_enableColorMaskExclusion} (máx. excluídos: {_maxExcludedPixelRatio:P1})");
        Console.WriteLine($"   - Fallback: {_allowFallbackTemplateMatching}");
    }

    /// <summary>
    /// ✅ NOVO: Ativar/desativar recursos do filtro HSV avançado
    /// </summary>
    /// <param name="enableLocalBlocks">Ativar análise local em blocos</param>
    /// <param name="enableColorMask">Ativar máscara de exclusão de cores</param>
    /// <param name="enableFallback">Ativar modo fallback</param>
    public void SetAdvancedHsvFeatures(bool enableLocalBlocks = true, bool enableColorMask = true, bool enableFallback = true)
    {
        _enableLocalBlockAnalysis = enableLocalBlocks;
        _enableColorMaskExclusion = enableColorMask;
        _allowFallbackTemplateMatching = enableFallback;
        
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Recursos HSV Avançado Configurados");
        Console.WriteLine($"   - Análise local em blocos: {_enableLocalBlockAnalysis}");
        Console.WriteLine($"   - Máscara de exclusão de cores: {_enableColorMaskExclusion}");
        Console.WriteLine($"   - Modo fallback: {_allowFallbackTemplateMatching}");
    }

    /// <summary>
    /// Verifica se os valores HSV estão dentro da tolerância esperada (método legado)
    /// </summary>
    /// <param name="actualHsv">HSV atual da região</param>
    /// <param name="expectedHsv">HSV esperado do template</param>
    /// <param name="tolerance">Tolerância para diferença</param>
    /// <returns>True se os valores estão próximos</returns>
    private bool IsHsvWithinTolerance(Scalar actualHsv, Scalar expectedHsv, double tolerance)
    {
        double hDiff = Math.Abs(actualHsv.Val0 - expectedHsv.Val0);
        double sDiff = Math.Abs(actualHsv.Val1 - expectedHsv.Val1);
        double vDiff = Math.Abs(actualHsv.Val2 - expectedHsv.Val2);

        // Para Hue, considerar que 0 e 180 são próximos (círculo de cores)
        if (hDiff > 90) hDiff = 180 - hDiff;

        bool isWithinTolerance = hDiff <= tolerance && sDiff <= tolerance && vDiff <= tolerance;

        if (_enableDebugWindow)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎨 Pré-filtro HSV - Atual: H={actualHsv.Val0:F1}, S={actualHsv.Val1:F1}, V={actualHsv.Val2:F1}");
            Console.WriteLine($"   - Esperado: H={expectedHsv.Val0:F1}, S={expectedHsv.Val1:F1}, V={expectedHsv.Val2:F1}");
            Console.WriteLine($"   - Diferenças: ΔH={hDiff:F1}, ΔS={sDiff:F1}, ΔV={vDiff:F1}, Tolerância: {tolerance:F1}");
            Console.WriteLine($"   - Dentro da tolerância: {isWithinTolerance}");
        }

        return isWithinTolerance;
    }



    /// <summary>
    /// Ativa modo de debug que desativa todos os filtros para análise
    /// </summary>
    /// <param name="enable">True para ativar modo debug, False para voltar ao normal</param>
    public void SetDebugMode(bool enable)
    {
        if (enable)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: 🔧 MODO DEBUG ATIVADO - Todos os filtros desabilitados");
            _enablePreMatchColorFilter = false;
            _hsvScoreThreshold = 0.0; // Aceitar qualquer score HSV
        }
        else
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: 🔧 MODO DEBUG DESATIVADO - Filtros restaurados");
            _enablePreMatchColorFilter = true;
            _hsvScoreThreshold = 0.3; // Restaurar threshold normal
        }
    }

    /// <summary>
    /// Ajusta as tolerâncias HSV dinamicamente
    /// </summary>
    /// <param name="toleranceH">Tolerância para Hue</param>
    /// <param name="toleranceS">Tolerância para Saturation</param>
    /// <param name="toleranceV">Tolerância para Value</param>
    /// <param name="scoreThreshold">Threshold do score HSV</param>
    public void AdjustHsvTolerances(double toleranceH, double toleranceS, double toleranceV, double scoreThreshold)
    {
        _hsvToleranceH = toleranceH;
        _hsvToleranceS = toleranceS;
        _hsvToleranceV = toleranceV;
        _hsvScoreThreshold = scoreThreshold;
        
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🔧 Tolerâncias HSV Ajustadas");
        Console.WriteLine($"   - H: {_hsvToleranceH:F1}, S: {_hsvToleranceS:F1}, V: {_hsvToleranceV:F1}");
        Console.WriteLine($"   - Score Threshold: {_hsvScoreThreshold:F3}");
    }

    /// <summary>
    /// Método de tracking do bobber com template matching real
    /// Retorna a posição X do bobber e o score de confiança
    /// </summary>
    /// <param name="posX">Posição X do bobber (out parameter)</param>
    /// <param name="matchScore">Score de matching (out parameter)</param>
    /// <returns>True se o bobber foi detectado, False caso contrário</returns>
    public bool TrackBobber(out float posX, out float matchScore)
    {
        posX = 0.0f;
        matchScore = 0.0f;
        
        try
        {
            // Capturar região da tela
            using var screenBitmap = ScreenCaptureHelper.CaptureRegion(_region);
            if (screenBitmap == null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Falha ao capturar tela");
                return false;
            }
            
            // Carregar template
            if (!File.Exists(_templatePath))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Template não encontrado: {_templatePath}");
                return false;
            }
            
            using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Falha ao carregar template");
                return false;
            }
            
            // Converter bitmap para Mat
            using var screenMat = BitmapConverter.ToMat(screenBitmap);
            
            // Aplicar pré-filtro HSV se habilitado
            if (_enablePreMatchColorFilter)
            {
                if (!IsScreenColorValid(screenBitmap))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: 🚫 Pré-filtro HSV bloqueou tracking");
                    return false;
                }
            }
            
            // Realizar template matching
            using var result = new Mat();
            Cv2.MatchTemplate(screenMat, templateMat, result, _matchMethod);
            
            // Encontrar melhor match
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);
            
            double matchValue = _matchMethod == TemplateMatchModes.SqDiff || _matchMethod == TemplateMatchModes.SqDiffNormed ? minVal : maxVal;
            
            // Verificar se o match é válido
            if (matchValue >= _confidenceThreshold)
            {
                // Calcular posição X do bobber (centro do template)
                posX = maxLoc.X + (templateMat.Width / 2.0f);
                _positionBoober = posX;
                matchScore = (float)matchValue;
                
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 👁️ VisionDetector: 🎣 Bobber detectado!");
                Console.WriteLine($"   - Posição X: {posX:F1}");
                Console.WriteLine($"   - Score: {matchScore:F4}");
                Console.WriteLine($"   - Match Location: ({maxLoc.X}, {maxLoc.Y})");
                Console.WriteLine($"   - Template Size: {templateMat.Width}x{templateMat.Height}");
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ✅ Bobber detectado - Pos: {posX:F1}, Score: {matchScore:F3}");
                return true;
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Bobber não detectado - Score: {matchValue:F3} < {_confidenceThreshold:F3}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionDetector: ❌ Erro no tracking: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ✅ NOVO: Configura as tolerâncias HSV do ColorFilterService
    /// </summary>
    /// <param name="toleranceH">Tolerância para Hue</param>
    /// <param name="toleranceS">Tolerância para Saturation</param>
    /// <param name="toleranceV">Tolerância para Value</param>
    public void ConfigureColorFilterHsvTolerances(double toleranceH, double toleranceS, double toleranceV)
    {
        _colorFilterService.SetBobberHsvTolerances(toleranceH, toleranceS, toleranceV);
    }

    /// <summary>
    /// ✅ NOVO: Configura a porcentagem mínima de pixels com cor do bobber
    /// </summary>
    /// <param name="minRatio">Porcentagem mínima (0.0-1.0)</param>
    public void ConfigureColorFilterMinRatio(double minRatio)
    {
        _colorFilterService.SetMinBobberColorRatio(minRatio);
    }

    /// <summary>
    /// ✅ NOVO: Atualiza o HSV esperado do bobber no ColorFilterService
    /// </summary>
    /// <param name="hsv">HSV médio do bobber</param>
    public void UpdateColorFilterExpectedHsv(Scalar hsv)
    {
        _colorFilterService.SetExpectedBobberHsv(hsv);
        _expectedTemplateHsv = hsv; // Atualizar também no VisionDetector
    }

    /// <summary>
    /// ✅ NOVO: Testa o ColorFilterService com uma imagem específica
    /// </summary>
    /// <param name="bitmap">Bitmap para teste</param>
    /// <returns>True se o ColorFilterService aceita a imagem</returns>
    public bool TestColorFilterService(System.Drawing.Bitmap bitmap)
    {
        return _colorFilterService.HasSufficientBobberColorPresence(bitmap, _expectedTemplateHsv);
    }
} 