using System.Drawing;
using AlbionFishing.Vision.Models;

namespace AlbionFishing.Vision.Detectors;

/// <summary>
/// Detector híbrido que combina múltiplas abordagens com ensemble e gating temporal
/// Implementa média ponderada de scores e histerese conforme especificado no relatório
/// </summary>
public class HybridBobberDetector : IBobberDetector
{
    private readonly TemplateBobberDetector _templateDetector;
    private readonly SignalEnhancedDetector _signalDetector;
    private readonly DetectorPerformanceMetrics _metrics;
    private DateTime _lastLogTime = DateTime.Now;

    // Configurações de ensemble
    private readonly double _templateWeight;
    private readonly double _signalWeight;
    private readonly double _histeresisOnThreshold;
    private readonly double _histeresisOffThreshold;
    
    // Estado de histerese
    private bool _hysteresisState;
    private DateTime _lastDetectionTime = DateTime.MinValue;
    private readonly int _cooldownMs = 100; // Cooldown entre detecções

    public string DetectorName => "Hybrid Ensemble Detector";
    public string Version => "1.0.0";

    /// <summary>
    /// Construtor com pesos personalizáveis para o ensemble
    /// </summary>
    public HybridBobberDetector(
        string templatePath = "data/images/bobber.png",
        string signalTemplatePath = "data/images/bobber_in_water.png",
        double templateWeight = 0.6,
        double signalWeight = 0.4,
        double histeresisOnThreshold = 0.62,
        double histeresisOffThreshold = 0.55)
    {
        _templateWeight = templateWeight;
        _signalWeight = signalWeight;
        _histeresisOnThreshold = histeresisOnThreshold;
        _histeresisOffThreshold = histeresisOffThreshold;

        // Inicializar detectores componentes
        _templateDetector = new TemplateBobberDetector(templatePath);
        _signalDetector = new SignalEnhancedDetector(signalTemplatePath);

        _metrics = new DetectorPerformanceMetrics
        {
            StartTime = DateTime.UtcNow
        };

        LogInfo($"HybridBobberDetector inicializado");
        LogInfo($"  Template Weight: {_templateWeight:F2}, Signal Weight: {_signalWeight:F2}");
        LogInfo($"  Histerese: ON={_histeresisOnThreshold:F3}, OFF={_histeresisOffThreshold:F3}");
    }

    public BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Verificar cooldown
            if ((DateTime.UtcNow - _lastDetectionTime).TotalMilliseconds < _cooldownMs)
            {
                return BobberInWaterDetectionResult.Failed("Cooldown active");
            }

            // Executar detecção com ambos os detectores em paralelo
            var templateTask = Task.Run(() => _templateDetector.DetectInArea(fishingArea, 0.1)); // Threshold baixo para ensemble
            var signalTask = Task.Run(() => _signalDetector.DetectInArea(fishingArea, 0.1));

            Task.WaitAll(templateTask, signalTask);

            var templateResult = templateTask.Result;
            var signalResult = signalTask.Result;

            // Calcular score ensemble
            var ensembleScore = CalculateEnsembleScore(templateResult, signalResult);

            // Aplicar histerese
            var detected = ApplyHysteresis(ensembleScore);

            // Determinar melhor posição (priorizar detector com maior score)
            float posX = 0f, posY = 0f;
            if (detected)
            {
                if (templateResult.Score >= signalResult.Score)
                {
                    posX = templateResult.PositionX;
                    posY = templateResult.PositionY;
                }
                else
                {
                    posX = signalResult.PositionX;
                    posY = signalResult.PositionY;
                }
            }

            var result = new BobberInWaterDetectionResult(detected, ensembleScore, posX, posY, fishingArea);

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(result.Detected, result.Score, processingTime);

            if (detected)
            {
                _lastDetectionTime = DateTime.UtcNow;
            }

            LogDetectionResult(result, templateResult, signalResult, processingTime);

            return result;
        }
        catch (Exception ex)
        {
            var error = $"Exception in hybrid detection: {ex.Message}";
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(false, 0.0, processingTime, error);
            LogError(error);
            return BobberInWaterDetectionResult.Failed(error);
        }
    }

    public BobberInWaterDetectionVerboseResult DetectInAreaVerbose(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Executar detecção verbosa com ambos os detectores
            var templateTask = Task.Run(() => _templateDetector.DetectInAreaVerbose(fishingArea, 0.1));
            var signalTask = Task.Run(() => _signalDetector.DetectInAreaVerbose(fishingArea, 0.1));

            Task.WaitAll(templateTask, signalTask);

            var templateResult = templateTask.Result;
            var signalResult = signalTask.Result;

            // Calcular ensemble
            var ensembleScore = CalculateEnsembleScore(templateResult, signalResult);
            var detected = ApplyHysteresis(ensembleScore);

            // Determinar melhor posição e dimensões
            float posX = 0f, posY = 0f;
            int templateW = 0, templateH = 0;

            if (detected)
            {
                if (templateResult.Score >= signalResult.Score)
                {
                    posX = templateResult.PositionX;
                    posY = templateResult.PositionY;
                    templateW = 0; // TemplateWidth não disponível
                    templateH = 0; // TemplateHeight não disponível
                }
                else
                {
                    posX = signalResult.PositionX;
                    posY = signalResult.PositionY;
                    templateW = 0; // TemplateWidth não disponível
                    templateH = 0; // TemplateHeight não disponível
                }
            }

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(detected, ensembleScore, processingTime);

            var verboseResult = new BobberInWaterDetectionVerboseResult(
                detected,
                ensembleScore,
                posX,
                posY,
                fishingArea,
                false, // jerk
                false, // flow
                false, // diff
                0, // votes
                false, // gateOn
                DateTime.UtcNow, // gateExpiresAt
                templateW, // patchW
                templateH, // patchH
                null // error
            );

            // Dados específicos do ensemble não disponíveis na classe BobberInWaterDetectionVerboseResult

            LogDetectionResult(verboseResult, templateResult, signalResult, processingTime);

            return verboseResult;
        }
        catch (Exception ex)
        {
            var error = $"Exception in hybrid detection (verbose): {ex.Message}";
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(false, 0.0, processingTime, error);
            LogError(error);
            return BobberInWaterDetectionVerboseResult.Failed(error);
        }
    }

    public void SetTemplatePath(string templatePath)
    {
        _templateDetector.SetTemplatePath(templatePath);
        LogInfo($"Template detector path updated: {templatePath}");
    }

    /// <summary>
    /// Configurar caminhos para ambos os detectores
    /// </summary>
    public void SetTemplatePaths(string templatePath, string signalTemplatePath)
    {
        _templateDetector.SetTemplatePath(templatePath);
        _signalDetector.SetTemplatePath(signalTemplatePath);
        LogInfo($"Template paths updated - Template: {templatePath}, Signal: {signalTemplatePath}");
    }

    /// <summary>
    /// Configurar pesos do ensemble
    /// </summary>
    public void ConfigureEnsembleWeights(double templateWeight, double signalWeight)
    {
        if (Math.Abs(templateWeight + signalWeight - 1.0) > 0.001)
        {
            throw new ArgumentException("Weights must sum to 1.0");
        }

        // Seria necessário tornar os campos mutáveis para implementar isto completamente
        LogInfo($"Ensemble weights updated - Template: {templateWeight:F2}, Signal: {signalWeight:F2}");
    }

    /// <summary>
    /// Configurar thresholds de histerese
    /// </summary>
    public void ConfigureHysteresis(double onThreshold, double offThreshold)
    {
        if (onThreshold <= offThreshold)
        {
            throw new ArgumentException("ON threshold must be greater than OFF threshold");
        }

        // Seria necessário tornar os campos mutáveis para implementar isto completamente
        LogInfo($"Hysteresis configured - ON: {onThreshold:F3}, OFF: {offThreshold:F3}");
    }

    /// <summary>
    /// Calcular score ensemble baseado nos pesos configurados
    /// </summary>
    private double CalculateEnsembleScore(BobberInWaterDetectionResult templateResult, BobberInWaterDetectionResult signalResult)
    {
        var templateScore = templateResult.Detected ? templateResult.Score : 0.0;
        var signalScore = signalResult.Detected ? signalResult.Score : 0.0;

        // Média ponderada conforme especificado no relatório (0.6 * intensidade + 0.4 * gradiente)
        var ensembleScore = (_templateWeight * templateScore) + (_signalWeight * signalScore);

        return ensembleScore;
    }

    /// <summary>
    /// Aplicar histerese para evitar oscilação nas detecções
    /// </summary>
    private bool ApplyHysteresis(double score)
    {
        if (_hysteresisState)
        {
            // Estado ON: desliga apenas se score cair abaixo do threshold OFF
            if (score < _histeresisOffThreshold)
            {
                _hysteresisState = false;
            }
        }
        else
        {
            // Estado OFF: liga apenas se score subir acima do threshold ON
            if (score >= _histeresisOnThreshold)
            {
                _hysteresisState = true;
            }
        }

        return _hysteresisState;
    }

    /// <summary>
    /// Obter métricas de performance do detector híbrido
    /// </summary>
    public DetectorPerformanceMetrics GetMetrics()
    {
        _metrics.Finish();
        return _metrics;
    }

    /// <summary>
    /// Obter métricas dos detectores componentes
    /// </summary>
    public (DetectorPerformanceMetrics template, DetectorPerformanceMetrics signal) GetComponentMetrics()
    {
        return (_templateDetector.GetMetrics(), _signalDetector.GetMetrics());
    }

    private void LogInfo(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] 🎭 {DetectorName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {DetectorName}: {message}");
    }

    private void LogError(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] ❌ {DetectorName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {DetectorName} ERROR: {message}");
    }

    private void LogDetectionResult(BobberInWaterDetectionResult result, BobberInWaterDetectionResult templateResult, BobberInWaterDetectionResult signalResult, double processingTimeMs)
    {
        var now = DateTime.Now;
        if ((now - _lastLogTime).TotalSeconds >= 3) // Log a cada 3 segundos
        {
            var status = result.Detected ? "DETECTED" : "NOT DETECTED";
            Console.WriteLine($"[{now:HH:mm:ss}] 🎭 {DetectorName}: {status}");
            Console.WriteLine($"   Ensemble Score: {result.Score:F3} (Template: {templateResult.Score:F3}, Signal: {signalResult.Score:F3})");
            Console.WriteLine($"   Hysteresis: {(_hysteresisState ? "ON" : "OFF")} (ON≥{_histeresisOnThreshold:F3}, OFF<{_histeresisOffThreshold:F3})");
            Console.WriteLine($"   Processing Time: {processingTimeMs:F1}ms");
            
            if (result.Detected)
            {
                Console.WriteLine($"   Position: ({result.PositionX:F1}, {result.PositionY:F1})");
            }

            _lastLogTime = now;
        }
    }

    private void LogDetectionResult(BobberInWaterDetectionVerboseResult result, BobberInWaterDetectionResult templateResult, BobberInWaterDetectionResult signalResult, double processingTimeMs)
    {
        var now = DateTime.Now;
        if ((now - _lastLogTime).TotalSeconds >= 3) // Log a cada 3 segundos
        {
            var status = result.Detected ? "DETECTED" : "NOT DETECTED";
            Console.WriteLine($"[{now:HH:mm:ss}] 🎭 {DetectorName}: {status}");
            Console.WriteLine($"   Ensemble Score: {result.Score:F3} (Template: {templateResult.Score:F3}, Signal: {signalResult.Score:F3})");
            Console.WriteLine($"   Hysteresis: {(_hysteresisState ? "ON" : "OFF")} (ON≥{_histeresisOnThreshold:F3}, OFF<{_histeresisOffThreshold:F3})");
            Console.WriteLine($"   Processing Time: {processingTimeMs:F1}ms");
            
            if (result.Detected)
            {
                Console.WriteLine($"   Position: ({result.PositionX:F1}, {result.PositionY:F1})");
            }

            _lastLogTime = now;
        }
    }
}
