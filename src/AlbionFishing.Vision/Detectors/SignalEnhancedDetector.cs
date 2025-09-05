using System.Drawing;
using AlbionFishing.Vision.Models;

namespace AlbionFishing.Vision.Detectors;

/// <summary>
/// Detector avançado com análise de sinais dinâmicos (jerk, flow, diff, micro-movimento)
/// Baseado no BobberInWaterDetector existente
/// </summary>
public class SignalEnhancedDetector : IBobberDetector
{
    private readonly BobberInWaterDetector _innerDetector;
    private readonly DetectorPerformanceMetrics _metrics;
    private DateTime _lastLogTime = DateTime.Now;

    public string DetectorName => "Signal Enhanced Detector";
    public string Version => "1.0.0";

    public SignalEnhancedDetector(string templatePath = "data/images/bobber_in_water.png")
    {
        _innerDetector = new BobberInWaterDetector(templatePath);
        _metrics = new DetectorPerformanceMetrics
        {
            StartTime = DateTime.UtcNow
        };

        // Configurar módulos avançados do BobberInWaterDetector
        _innerDetector.ConfigureMatchingModules(
            useEqualization: true,       // CLAHE para equalização
            useMask: true,               // Máscara derivada do alpha/cor
            useGradientChannel: true,    // Canal de gradiente (Canny)
            useMultiScale: true,         // Detecção multi-escala
            useColorGate: true,          // Validação por cor
            useKinematics: true,         // Análise de jerk/flow/diff
            useMicroMotion: true,        // Micro-movimento
            useHeuristicHook: true       // Heurística de fisgada
        );

        // Habilitar filtros de cor para melhor robustez
        _innerDetector.EnableColorFilters(true);

        LogInfo($"SignalEnhancedDetector inicializado - Template: {templatePath}");
    }

    public BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Usar o detector interno que já implementa toda a lógica avançada
            var result = _innerDetector.DetectInArea(fishingArea, confidenceThreshold);
            
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(result.Detected, result.Score, processingTime, result.Error);

            LogDetectionResult(result, processingTime);

            return result;
        }
        catch (Exception ex)
        {
            var error = $"Exception in signal enhanced detection: {ex.Message}";
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
            // Usar versão verbosa do detector interno
            var verboseResult = _innerDetector.DetectInAreaVerbose(fishingArea, confidenceThreshold);
            
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(verboseResult.Detected, verboseResult.Score, processingTime, verboseResult.Error);

            // Retornar diretamente o resultado do detector interno
            LogDetectionResult(verboseResult, processingTime);

            return verboseResult;
        }
        catch (Exception ex)
        {
            var error = $"Exception in signal enhanced detection (verbose): {ex.Message}";
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(false, 0.0, processingTime, error);
            LogError(error);
            return BobberInWaterDetectionVerboseResult.Failed(error);
        }
    }

    public void SetTemplatePath(string templatePath)
    {
        _innerDetector.SetTemplatePath(templatePath);
        LogInfo($"Template path updated: {templatePath}");
    }

    /// <summary>
    /// Configurar módulos de matching do detector interno
    /// </summary>
    public void ConfigureMatchingModules(
        bool useEqualization = true,
        bool useMask = true,
        bool useGradientChannel = true,
        bool useMultiScale = true,
        bool useColorGate = true,
        bool useKinematics = true,
        bool useMicroMotion = true,
        bool useHeuristicHook = true)
    {
        _innerDetector.ConfigureMatchingModules(
            useEqualization,
            useMask,
            useGradientChannel,
            useMultiScale,
            useColorGate,
            useKinematics,
            useMicroMotion,
            useHeuristicHook
        );

        LogInfo("Matching modules reconfigured");
    }

    /// <summary>
    /// Habilitar/desabilitar filtros de cor
    /// </summary>
    public void EnableColorFilters(bool enabled)
    {
        _innerDetector.EnableColorFilters(enabled);
        LogInfo($"Color filters {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Ativar modo template-only (sem análises avançadas)
    /// </summary>
    public void EnableTemplateOnlyMode(bool enable)
    {
        _innerDetector.EnableTemplateOnlyMode(enable);
        LogInfo($"Template-only mode {(enable ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Verificar se o gate de splash está ativo
    /// </summary>
    public bool IsGateOpen()
    {
        return _innerDetector.IsGateOpen();
    }

    /// <summary>
    /// Executar diagnóstico detalhado de um frame
    /// </summary>
    public BobberDetectionDiagnostics DiagnoseFrame(Rectangle fishingArea, double confidenceThreshold = 0.5, bool saveImages = false)
    {
        return _innerDetector.DiagnoseFrame(fishingArea, confidenceThreshold, saveImages);
    }

    /// <summary>
    /// Detecção específica de micro-movimento com informações verbosas
    /// </summary>
    public (bool microMove, double zDy, double zRipple, double dyPixels, double rippleEnergy, bool gateOn, DateTime until)
        DetectMicroMotionVerbose(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        return _innerDetector.DetectMicroMotionVerbose(fishingArea, confidenceThreshold);
    }

    /// <summary>
    /// Obter métricas de performance
    /// </summary>
    public DetectorPerformanceMetrics GetMetrics()
    {
        _metrics.Finish();
        return _metrics;
    }

    /// <summary>
    /// Finalizar sessão do reporter interno se ativo
    /// </summary>
    public void EndReporterSession()
    {
        _innerDetector.EndReporterSession();
    }

    private void LogInfo(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] 🔬 {DetectorName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {DetectorName}: {message}");
    }

    private void LogError(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] ❌ {DetectorName}: {message}");
        System.Diagnostics.Trace.WriteLine($"[{now:HH:mm:ss}] {DetectorName} ERROR: {message}");
    }

    private void LogDetectionResult(BobberInWaterDetectionResult result, double processingTimeMs)
    {
        var now = DateTime.Now;
        if ((now - _lastLogTime).TotalSeconds >= 3) // Log a cada 3 segundos
        {
            var status = result.Detected ? "DETECTED" : "NOT DETECTED";
            Console.WriteLine($"[{now:HH:mm:ss}] 🔬 {DetectorName}: {status} - Score: {result.Score:F3}, Time: {processingTimeMs:F1}ms");
            
            if (result.Detected)
            {
                Console.WriteLine($"   Position: ({result.PositionX:F1}, {result.PositionY:F1})");
                
                // Mostrar informações específicas de sinais se for verbose result
                if (result is BobberInWaterDetectionVerboseResult verboseResult)
                {
                    Console.WriteLine($"   Signals: Jerk={verboseResult.Jerk}, Flow={verboseResult.Flow}, Diff={verboseResult.Diff}, Votes={verboseResult.Votes}");
                    Console.WriteLine($"   Gate: {(verboseResult.GateOn ? "ON" : "OFF")}, Expires: {verboseResult.GateExpiresAt:HH:mm:ss.fff}");
                }
            }

            _lastLogTime = now;
        }
    }
}
