using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;

namespace AlbionFishing.Vision.Services;

/// <summary>
/// Serviço para filtrar detecções de bobber baseado na distribuição de cores
/// </summary>
public class ColorFilterService
{
    private readonly double _redRatioThreshold;
    private readonly double _whiteRatioThreshold;
    private readonly double _greenRejectionThreshold;
    private readonly bool _enableDebug;

    // ✅ NOVO: Configurações para detecção de presença de cor do bobber
    private double _minBobberColorRatio;
    private double _bobberHsvToleranceH;
    private double _bobberHsvToleranceS;
    private double _bobberHsvToleranceV;
    private Scalar? _expectedBobberHsv;

    /// <summary>
    /// Construtor do ColorFilterService
    /// </summary>
    /// <param name="redRatioThreshold">Threshold mínimo para pixels vermelhos (0.0-1.0)</param>
    /// <param name="whiteRatioThreshold">Threshold mínimo para pixels brancos (0.0-1.0)</param>
    /// <param name="greenRejectionThreshold">Threshold para rejeitar regiões muito verdes (0.0-1.0)</param>
    /// <param name="enableDebug">Ativar logs de debug</param>
    /// <param name="minBobberColorRatio">Porcentagem mínima de pixels com cor do bobber (0.0-1.0)</param>
    /// <param name="bobberHsvToleranceH">Tolerância HSV H para cor do bobber</param>
    /// <param name="bobberHsvToleranceS">Tolerância HSV S para cor do bobber</param>
    /// <param name="bobberHsvToleranceV">Tolerância HSV V para cor do bobber</param>
    public ColorFilterService(double redRatioThreshold = 0.05, double whiteRatioThreshold = 0.1, double greenRejectionThreshold = 0.7, bool enableDebug = false,
        double minBobberColorRatio = 0.01, double bobberHsvToleranceH = 25.0, double bobberHsvToleranceS = 50.0, double bobberHsvToleranceV = 60.0)
    {
        _redRatioThreshold = redRatioThreshold;
        _whiteRatioThreshold = whiteRatioThreshold;
        _greenRejectionThreshold = greenRejectionThreshold;
        _enableDebug = enableDebug;
        _minBobberColorRatio = minBobberColorRatio;
        _bobberHsvToleranceH = bobberHsvToleranceH;
        _bobberHsvToleranceS = bobberHsvToleranceS;
        _bobberHsvToleranceV = bobberHsvToleranceV;

        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 🚀 Inicializado");
        Console.WriteLine($"   - Red Threshold: {_redRatioThreshold:F3}");
        Console.WriteLine($"   - White Threshold: {_whiteRatioThreshold:F3}");
        Console.WriteLine($"   - Green Rejection: {_greenRejectionThreshold:F3}");
        Console.WriteLine($"   - Debug: {_enableDebug}");
        Console.WriteLine($"   - Min Bobber Color Ratio: {_minBobberColorRatio:F3}");
        Console.WriteLine($"   - Bobber HSV Tolerance: H={_bobberHsvToleranceH:F1}, S={_bobberHsvToleranceS:F1}, V={_bobberHsvToleranceV:F1}");
        System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 🚀 Inicializado");
    }

    /// <summary>
    /// ✅ NOVO: Verifica se há uma porcentagem mínima de pixels com cor similar ao bobber
    /// Este método é muito mais robusto que comparar médias HSV, pois ignora ruídos de fundo
    /// </summary>
    /// <param name="bitmap">Bitmap da tela capturada</param>
    /// <param name="expectedBobberHsv">HSV médio esperado do bobber (opcional)</param>
    /// <param name="minRatio">Porcentagem mínima de pixels similares (padrão: 0.01 = 1%)</param>
    /// <returns>True se há pixels suficientes com cor do bobber</returns>
    public bool HasSufficientBobberColorPresence(System.Drawing.Bitmap bitmap, Scalar? expectedBobberHsv = null, double? minRatio = null)
    {
        try
        {
            // Usar HSV fornecido ou o padrão configurado
            var targetHsv = expectedBobberHsv ?? _expectedBobberHsv;
            if (!targetHsv.HasValue)
            {
                var logTimeNow = DateTime.Now;
                Console.WriteLine($"[{logTimeNow:HH:mm:ss}] 🎨 ColorFilterService: ⚠️ HSV do bobber não definido - usando valores padrão");
                // Valores padrão baseados no bobber vermelho/branco típico
                targetHsv = new Scalar(2.0, 166.0, 247.0); // H=2 (vermelho), S=166, V=247 (brilhante)
            }

            using var screenMat = BitmapConverter.ToMat(bitmap);
            using var hsv = new Mat();
            Cv2.CvtColor(screenMat, hsv, ColorConversionCodes.BGR2HSV);

            // Definir range HSV para pixels similares ao bobber
            var lower = new Scalar(
                Math.Max(targetHsv.Value.Val0 - _bobberHsvToleranceH, 0),
                Math.Max(targetHsv.Value.Val1 - _bobberHsvToleranceS, 0),
                Math.Max(targetHsv.Value.Val2 - _bobberHsvToleranceV, 0)
            );

            var upper = new Scalar(
                Math.Min(targetHsv.Value.Val0 + _bobberHsvToleranceH, 180),
                Math.Min(targetHsv.Value.Val1 + _bobberHsvToleranceS, 255),
                Math.Min(targetHsv.Value.Val2 + _bobberHsvToleranceV, 255)
            );

            // Criar máscara para pixels similares ao bobber
            using var mask = new Mat();
            Cv2.InRange(hsv, lower, upper, mask);

            // Calcular porcentagem de pixels similares
            int totalPixels = hsv.Rows * hsv.Cols;
            int matchingPixels = Cv2.CountNonZero(mask);
            double ratio = (double)matchingPixels / totalPixels;
            double threshold = minRatio ?? _minBobberColorRatio;

            var logTimeDetection = DateTime.Now;
            Console.WriteLine($"[{logTimeDetection:HH:mm:ss}] 🎨 ColorFilterService: 🎯 Detecção de cor bobber");
            Console.WriteLine($"   - Pixels similares: {matchingPixels:N0} / {totalPixels:N0} ({ratio:P2})");
            Console.WriteLine($"   - Threshold: {threshold:P2}");
            Console.WriteLine($"   - HSV Target: H={targetHsv.Value.Val0:F1}, S={targetHsv.Value.Val1:F1}, V={targetHsv.Value.Val2:F1}");
            Console.WriteLine($"   - HSV Range: H={lower.Val0:F1}-{upper.Val0:F1}, S={lower.Val1:F1}-{upper.Val1:F1}, V={lower.Val2:F1}-{upper.Val2:F1}");
            Console.WriteLine($"   - Resultado: {(ratio >= threshold ? "✅ VÁLIDO" : "❌ INSUFICIENTE")}");

            System.Diagnostics.Trace.WriteLine($"[{logTimeDetection:HH:mm:ss}] 🎨 ColorFilterService: 🎯 Bobber color presence - {matchingPixels}/{totalPixels} ({ratio:P2}) >= {threshold:P2} = {ratio >= threshold}");

            return ratio >= threshold;
        }
        catch (Exception ex)
        {
            var logTimeError = DateTime.Now;
            Console.WriteLine($"[{logTimeError:HH:mm:ss}] 🎨 ColorFilterService: ❌ Erro na detecção de cor bobber: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeError:HH:mm:ss}] 🎨 ColorFilterService: ❌ Erro na detecção de cor bobber: {ex.Message}");
            return true; // Em caso de erro, permitir processamento
        }
    }

    /// <summary>
    /// ✅ NOVO: Define o HSV médio esperado do bobber para comparação
    /// </summary>
    /// <param name="hsv">HSV médio do bobber</param>
    public void SetExpectedBobberHsv(Scalar hsv)
    {
        _expectedBobberHsv = hsv;
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 🎯 HSV do bobber definido - H={hsv.Val0:F1}, S={hsv.Val1:F1}, V={hsv.Val2:F1}");
    }

    /// <summary>
    /// ✅ NOVO: Ajusta as tolerâncias HSV para detecção de cor do bobber
    /// </summary>
    /// <param name="toleranceH">Tolerância para Hue</param>
    /// <param name="toleranceS">Tolerância para Saturation</param>
    /// <param name="toleranceV">Tolerância para Value</param>
    public void SetBobberHsvTolerances(double toleranceH, double toleranceS, double toleranceV)
    {
        _bobberHsvToleranceH = toleranceH;
        _bobberHsvToleranceS = toleranceS;
        _bobberHsvToleranceV = toleranceV;
        
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ⚙️ Tolerâncias HSV ajustadas - H={toleranceH:F1}, S={toleranceS:F1}, V={toleranceV:F1}");
    }

    /// <summary>
    /// ✅ NOVO: Ajusta a porcentagem mínima de pixels com cor do bobber
    /// </summary>
    /// <param name="minRatio">Porcentagem mínima (0.0-1.0)</param>
    public void SetMinBobberColorRatio(double minRatio)
    {
        _minBobberColorRatio = minRatio;
        var logTime = DateTime.Now;
        Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ⚙️ Min bobber color ratio ajustado: {minRatio:P2}");
    }

    /// <summary>
    /// Analisa a distribuição de cores na região detectada
    /// </summary>
    /// <param name="screenMat">Mat da tela capturada</param>
    /// <param name="matchLocation">Localização do match (Point)</param>
    /// <param name="templateSize">Tamanho do template (Size)</param>
    /// <returns>Resultado da análise de cor com detalhes</returns>
    public ColorAnalysisResult AnalyzeColorDistribution(Mat screenMat, Point matchLocation, Size templateSize)
    {
        try
        {
            // 1. Recortar a região que corresponde ao match
            var matchRect = new Rect(matchLocation.X, matchLocation.Y, templateSize.Width, templateSize.Height);
            
            // Verificar se a região está dentro dos limites da imagem
            if (matchRect.X < 0 || matchRect.Y < 0 || 
                matchRect.X + matchRect.Width > screenMat.Width || 
                matchRect.Y + matchRect.Height > screenMat.Height)
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ⚠️ Região fora dos limites - Match: ({matchLocation.X}, {matchLocation.Y}), Size: {templateSize}, Screen: {screenMat.Width}x{screenMat.Height}");
                return new ColorAnalysisResult(false, 0.0, 0.0, 0.0, "Região fora dos limites");
            }

            using var matchRegion = new Mat(screenMat, matchRect);
            
            // ✅ NOVO: Garantir conversão para HSV mesmo que a imagem já tenha sido convertida
            using var hsvRegion = new Mat();
            Cv2.CvtColor(matchRegion, hsvRegion, ColorConversionCodes.BGR2HSV);
            
            // ✅ NOVO: Log dos valores HSV médios da região para debug
            var meanRegionHsv = Cv2.Mean(hsvRegion);
            var logTimeHsv = DateTime.Now;
            Console.WriteLine($"[{logTimeHsv:HH:mm:ss}] 🎨 ColorFilterService: 🎨 HSV Médio da Região - H={meanRegionHsv.Val0:F1}, S={meanRegionHsv.Val1:F1}, V={meanRegionHsv.Val2:F1}");
            System.Diagnostics.Trace.WriteLine($"[{logTimeHsv:HH:mm:ss}] 🎨 ColorFilterService: 🎨 HSV Médio da Região - H={meanRegionHsv.Val0:F1}, S={meanRegionHsv.Val1:F1}, V={meanRegionHsv.Val2:F1}");

            // 3. Criar máscaras para diferentes cores
            var (redRatio, whiteRatio, greenRatio) = CalculateColorRatios(hsvRegion);

            // 4. Aplicar filtros de validação
            bool isValidMatch = ValidateColorDistribution(redRatio, whiteRatio, greenRatio, out string reason);

            // 5. Calcular score de confiança baseado na distribuição de cores
            double colorScore = CalculateColorScore(redRatio, whiteRatio, greenRatio);

            var result = new ColorAnalysisResult(isValidMatch, colorScore, redRatio, whiteRatio, reason);

            // 6. Log de debug se habilitado
            if (_enableDebug)
            {
                var logTime = DateTime.Now;
                Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 🔍 Análise de Cor");
                Console.WriteLine($"   - Red Ratio: {redRatio:F3} (threshold: {_redRatioThreshold:F3})");
                Console.WriteLine($"   - White Ratio: {whiteRatio:F3} (threshold: {_whiteRatioThreshold:F3})");
                Console.WriteLine($"   - Green Ratio: {greenRatio:F3} (rejection: {_greenRejectionThreshold:F3})");
                Console.WriteLine($"   - Color Score: {colorScore:F3}");
                Console.WriteLine($"   - Valid Match: {isValidMatch} - Reason: {reason}");
                System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 🔍 Análise - Red: {redRatio:F3}, White: {whiteRatio:F3}, Green: {greenRatio:F3}, Valid: {isValidMatch}");
            }

            return result;
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ❌ Erro na análise de cor: {ex.Message}");
            System.Diagnostics.Trace.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ❌ Erro na análise de cor: {ex.Message}");
            return new ColorAnalysisResult(false, 0.0, 0.0, 0.0, $"Erro: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula as proporções de cores vermelha, branca e verde
    /// </summary>
    /// <param name="hsvRegion">Região em HSV</param>
    /// <returns>Tupla com as proporções (red, white, green)</returns>
    private (double redRatio, double whiteRatio, double greenRatio) CalculateColorRatios(Mat hsvRegion)
    {
        int totalPixels = hsvRegion.Rows * hsvRegion.Cols;

        // Máscara para vermelho (Hue: 0-10 e 160-180)
        var lowerRed1 = new Scalar(0, 70, 50);
        var upperRed1 = new Scalar(10, 255, 255);
        var lowerRed2 = new Scalar(160, 70, 50);
        var upperRed2 = new Scalar(180, 255, 255);
        
        using var maskRed1 = new Mat();
        using var maskRed2 = new Mat();
        using var redMask = new Mat();
        
        Cv2.InRange(hsvRegion, lowerRed1, upperRed1, maskRed1);
        Cv2.InRange(hsvRegion, lowerRed2, upperRed2, maskRed2);
        Cv2.BitwiseOr(maskRed1, maskRed2, redMask);
        
        int redPixels = Cv2.CountNonZero(redMask);
        double redRatio = (double)redPixels / totalPixels;

        // Máscara para branco (Saturação baixa, Value alto)
        var lowerWhite = new Scalar(0, 0, 200);
        var upperWhite = new Scalar(180, 40, 255);
        
        using var whiteMask = new Mat();
        Cv2.InRange(hsvRegion, lowerWhite, upperWhite, whiteMask);
        
        int whitePixels = Cv2.CountNonZero(whiteMask);
        double whiteRatio = (double)whitePixels / totalPixels;

        // Máscara para verde (Hue: 60-90)
        var lowerGreen = new Scalar(60, 50, 50);
        var upperGreen = new Scalar(90, 255, 255);
        
        using var greenMask = new Mat();
        Cv2.InRange(hsvRegion, lowerGreen, upperGreen, greenMask);
        
        int greenPixels = Cv2.CountNonZero(greenMask);
        double greenRatio = (double)greenPixels / totalPixels;

        return (redRatio, whiteRatio, greenRatio);
    }

    /// <summary>
    /// Valida se a distribuição de cores corresponde ao bobber esperado
    /// </summary>
    /// <param name="redRatio">Proporção de pixels vermelhos</param>
    /// <param name="whiteRatio">Proporção de pixels brancos</param>
    /// <param name="greenRatio">Proporção de pixels verdes</param>
    /// <param name="reason">Razão da validação</param>
    /// <returns>True se a distribuição é válida</returns>
    private bool ValidateColorDistribution(double redRatio, double whiteRatio, double greenRatio, out string reason)
    {
        // Verificar se há vermelho suficiente (faixa superior do bobber)
        if (redRatio < _redRatioThreshold)
        {
            reason = $"Red ratio too low: {redRatio:F3} < {_redRatioThreshold:F3}";
            return false;
        }

        // Verificar se há branco suficiente (parte central do bobber)
        if (whiteRatio < _whiteRatioThreshold)
        {
            reason = $"White ratio too low: {whiteRatio:F3} < {_whiteRatioThreshold:F3}";
            return false;
        }

        // Rejeitar se a região for muito verde (provavelmente fundo)
        if (greenRatio > _greenRejectionThreshold)
        {
            reason = $"Too much green (background): {greenRatio:F3} > {_greenRejectionThreshold:F3}";
            return false;
        }

        // Verificar se há uma distribuição balanceada (não pode ser só uma cor)
        double totalColorRatio = redRatio + whiteRatio + greenRatio;
        if (totalColorRatio > 0.95) // Se mais de 95% é uma das cores principais, pode ser suspeito
        {
            reason = $"Too homogeneous: total color ratio {totalColorRatio:F3} > 0.95";
            return false;
        }

        reason = "Valid color distribution";
        return true;
    }

    /// <summary>
    /// Calcula um score de confiança baseado na distribuição de cores
    /// </summary>
    /// <param name="redRatio">Proporção de pixels vermelhos</param>
    /// <param name="whiteRatio">Proporção de pixels brancos</param>
    /// <param name="greenRatio">Proporção de pixels verdes</param>
    /// <returns>Score de 0.0 a 1.0</returns>
    private double CalculateColorScore(double redRatio, double whiteRatio, double greenRatio)
    {
        // Score baseado na presença das cores esperadas do bobber
        double redScore = Math.Min(redRatio / _redRatioThreshold, 1.0);
        double whiteScore = Math.Min(whiteRatio / _whiteRatioThreshold, 1.0);
        
        // Penalizar excesso de verde
        double greenPenalty = Math.Max(0, greenRatio / _greenRejectionThreshold);
        
        // Score final: média ponderada das cores positivas menos penalidade do verde
        double colorScore = (redScore * 0.5 + whiteScore * 0.5) - greenPenalty * 0.3;
        
        return Math.Max(0.0, Math.Min(1.0, colorScore));
    }

    /// <summary>
    /// Salva imagens de debug da análise de cor
    /// </summary>
    /// <param name="matchRegion">Região do match</param>
    /// <param name="hsvRegion">Região em HSV</param>
    /// <param name="redMask">Máscara vermelha</param>
    /// <param name="whiteMask">Máscara branca</param>
    /// <param name="greenMask">Máscara verde</param>
    public void SaveDebugImages(Mat matchRegion, Mat hsvRegion, Mat redMask, Mat whiteMask, Mat greenMask)
    {
        if (!_enableDebug) return;

        try
        {
            var timestamp = DateTime.Now.ToString("HHmmss");
            Cv2.ImWrite($"debug_color_region_{timestamp}.png", matchRegion);
            Cv2.ImWrite($"debug_color_hsv_{timestamp}.png", hsvRegion);
            Cv2.ImWrite($"debug_color_red_mask_{timestamp}.png", redMask);
            Cv2.ImWrite($"debug_color_white_mask_{timestamp}.png", whiteMask);
            Cv2.ImWrite($"debug_color_green_mask_{timestamp}.png", greenMask);
            
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: 💾 Imagens de debug salvas");
        }
        catch (Exception ex)
        {
            var logTime = DateTime.Now;
            Console.WriteLine($"[{logTime:HH:mm:ss}] 🎨 ColorFilterService: ⚠️ Erro ao salvar debug: {ex.Message}");
        }
    }
}

/// <summary>
/// Resultado da análise de cor
/// </summary>
public class ColorAnalysisResult
{
    /// <summary>
    /// Se a distribuição de cores é válida para um bobber
    /// </summary>
    public bool IsValidMatch { get; }

    /// <summary>
    /// Score de confiança baseado na distribuição de cores (0.0-1.0)
    /// </summary>
    public double ColorScore { get; }

    /// <summary>
    /// Proporção de pixels vermelhos
    /// </summary>
    public double RedRatio { get; }

    /// <summary>
    /// Proporção de pixels brancos
    /// </summary>
    public double WhiteRatio { get; }

    /// <summary>
    /// Razão da validação ou rejeição
    /// </summary>
    public string Reason { get; }

    public ColorAnalysisResult(bool isValidMatch, double colorScore, double redRatio, double whiteRatio, string reason)
    {
        IsValidMatch = isValidMatch;
        ColorScore = colorScore;
        RedRatio = redRatio;
        WhiteRatio = whiteRatio;
        Reason = reason;
    }
} 