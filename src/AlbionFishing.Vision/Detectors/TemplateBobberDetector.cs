using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using AlbionFishing.Vision.Models;

namespace AlbionFishing.Vision.Detectors;

/// <summary>
/// Detector baseado em Template Matching básico
/// Adaptado do VisionDetector existente para implementar IBobberDetector
/// </summary>
public class TemplateBobberDetector : IBobberDetector
{
    private readonly TemplateMatchModes _matchMethod;
    private readonly bool _enableDebugWindow;
    private readonly DetectorPerformanceMetrics _metrics;
    
    private string _templatePath;
    private DateTime _lastLogTime = DateTime.Now;

    public string DetectorName => "Template Matcher";
    public string Version => "1.0.0";

    public TemplateBobberDetector(
        string templatePath = "data/images/bobber.png",
        TemplateMatchModes matchMethod = TemplateMatchModes.CCoeffNormed,
        bool enableDebugWindow = false)
    {
        _templatePath = templatePath;
        _matchMethod = matchMethod;
        _enableDebugWindow = enableDebugWindow;
        _metrics = new DetectorPerformanceMetrics
        {
            StartTime = DateTime.UtcNow
        };

        LogInfo($"TemplateBobberDetector inicializado - Template: {_templatePath}, Método: {_matchMethod}");
    }

    public BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
            if (bitmap == null)
            {
                var error = "Screen capture returned null";
                _metrics.RecordFrame(false, 0.0, (DateTime.UtcNow - startTime).TotalMilliseconds, error);
                return BobberInWaterDetectionResult.Failed(error);
            }

            var result = DetectInBitmap(bitmap, fishingArea, confidenceThreshold);
            
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(result.Detected, result.Score, processingTime);
            
            LogDetectionResult(result, processingTime);
            
            return result;
        }
        catch (Exception ex)
        {
            var error = $"Exception in template detection: {ex.Message}";
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
            using var bitmap = ScreenCaptureHelper.CaptureRegion(fishingArea);
            if (bitmap == null)
            {
                var error = "Screen capture returned null";
                var errorProcessingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _metrics.RecordFrame(false, 0.0, errorProcessingTime, error);
                return BobberInWaterDetectionVerboseResult.Failed(error);
            }

            var basicResult = DetectInBitmap(bitmap, fishingArea, confidenceThreshold);
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _metrics.RecordFrame(basicResult.Detected, basicResult.Score, processingTime);

            // Obter informações do template
            var (templateWidth, templateHeight) = GetTemplateInfo();

            var verboseResult = new BobberInWaterDetectionVerboseResult(
                basicResult.Detected,
                basicResult.Score,
                basicResult.PositionX,
                basicResult.PositionY,
                fishingArea,
                false, false, false, 0, false, DateTime.MinValue,
                templateWidth,
                templateHeight,
                basicResult.Error
            );

            LogDetectionResult(basicResult, processingTime);

            return verboseResult;
        }
        catch (Exception ex)
        {
            var error = $"Exception in template detection (verbose): {ex.Message}";
            var exceptionProcessingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _metrics.RecordFrame(false, 0.0, exceptionProcessingTime, error);
            LogError(error);
            return BobberInWaterDetectionVerboseResult.Failed(error);
        }
    }

    public void SetTemplatePath(string templatePath)
    {
        _templatePath = templatePath;
        LogInfo($"Template path updated: {_templatePath}");
    }

    /// <summary>
    /// Método principal de detecção usando template matching
    /// </summary>
    private BobberInWaterDetectionResult DetectInBitmap(Bitmap bitmap, Rectangle region, double confidenceThreshold)
    {
        try
        {
            // Converter bitmap para Mat
            using var screenMat = BitmapConverter.ToMat(bitmap);
            if (screenMat.Empty())
            {
                return BobberInWaterDetectionResult.Failed("Empty screen Mat after conversion");
            }

            // Ajustar canais de cor
            if (screenMat.Channels() == 4)
            {
                Cv2.CvtColor(screenMat, screenMat, ColorConversionCodes.BGRA2BGR);
            }

            // Carregar template
            if (!File.Exists(_templatePath))
            {
                return BobberInWaterDetectionResult.Failed($"Template not found: {_templatePath}");
            }

            using var templateMat = Cv2.ImRead(_templatePath, ImreadModes.Color);
            if (templateMat.Empty())
            {
                return BobberInWaterDetectionResult.Failed("Empty template Mat");
            }

            // Ajustar canais do template
            if (templateMat.Channels() == 4)
            {
                Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.RGBA2BGR);
            }

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

            // Executar template matching
            using var result = new Mat();
            Cv2.MatchTemplate(screenMat, templateMat, result, _matchMethod);

            // Encontrar melhor match
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

            // Determinar score e posição baseado no método
            double score;
            OpenCvSharp.Point bestLoc;

            if (_matchMethod == TemplateMatchModes.SqDiff || _matchMethod == TemplateMatchModes.SqDiffNormed)
            {
                score = 1.0 - minVal; // Para SqDiff, menor é melhor
                bestLoc = minLoc;
            }
            else
            {
                score = maxVal;
                bestLoc = maxLoc;
            }

            bool detected = score >= confidenceThreshold;
            float posX = 0f, posY = 0f;

            if (detected)
            {
                posX = bestLoc.X + (templateMat.Width / 2.0f);
                posY = bestLoc.Y + (templateMat.Height / 2.0f);
            }

            // Debug visual se habilitado
            if (_enableDebugWindow && detected)
            {
                ShowDebugWindow(screenMat, templateMat, bestLoc, score);
            }

            return new BobberInWaterDetectionResult(detected, score, posX, posY, region);
        }
        catch (Exception ex)
        {
            return BobberInWaterDetectionResult.Failed($"Template matching error: {ex.Message}");
        }
    }

    /// <summary>
    /// Mostra janela de debug com resultado da detecção
    /// </summary>
    private void ShowDebugWindow(Mat screenMat, Mat templateMat, OpenCvSharp.Point bestLoc, double score)
    {
        try
        {
            using var debugMat = screenMat.Clone();
            
            // Desenhar retângulo ao redor da detecção
            var rect = new Rect(bestLoc.X, bestLoc.Y, templateMat.Width, templateMat.Height);
            Cv2.Rectangle(debugMat, rect, new Scalar(0, 255, 0), 2);
            
            // Adicionar texto com score
            var text = $"Score: {score:F3}";
            Cv2.PutText(debugMat, text, new OpenCvSharp.Point(bestLoc.X, bestLoc.Y - 10), 
                HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 255, 0), 1);

            // Mostrar janela
            Cv2.ImShow($"DEBUG: {DetectorName}", debugMat);
            Cv2.WaitKey(1);
        }
        catch (Exception ex)
        {
            LogError($"Error showing debug window: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém informações sobre o template
    /// </summary>
    private (int width, int height) GetTemplateInfo()
    {
        try
        {
            if (File.Exists(_templatePath))
            {
                using var template = new Bitmap(_templatePath);
                return (template.Width, template.Height);
            }
        }
        catch (Exception ex)
        {
            LogError($"Error getting template info: {ex.Message}");
        }

        return (0, 0);
    }

    /// <summary>
    /// Obter métricas de performance
    /// </summary>
    public DetectorPerformanceMetrics GetMetrics()
    {
        _metrics.Finish();
        return _metrics;
    }

    private void LogInfo(string message)
    {
        var now = DateTime.Now;
        Console.WriteLine($"[{now:HH:mm:ss}] 🎯 {DetectorName}: {message}");
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
            Console.WriteLine($"[{now:HH:mm:ss}] 🎯 {DetectorName}: {status} - Score: {result.Score:F3}, Time: {processingTimeMs:F1}ms");
            
            if (result.Detected)
            {
                Console.WriteLine($"   Position: ({result.PositionX:F1}, {result.PositionY:F1})");
            }

            _lastLogTime = now;
        }
    }
}
