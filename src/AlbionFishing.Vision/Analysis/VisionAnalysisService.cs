using System;
using System.Drawing;
using System.Threading.Tasks;
using AlbionFishing.Vision;
using AlbionFishing.Vision.Analysis.Models;
using OpenCvSharp;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace AlbionFishing.Vision.Analysis;

/// <summary>
/// Implementação concreta do serviço de análise visual
/// </summary>
public class VisionAnalysisService : IVisionAnalysisService
{
    private VisionAnalysisSession? _currentSession;
    private VisionDetector? _visionDetector;
    private int _frameCounter = 0;
    private DateTime _lastFrameTime = DateTime.Now;

    public event EventHandler<VisionAnalysisFrame>? FrameProcessed;
    public event EventHandler<VisionAnalysisFrame>? DetectionFound;
    public event EventHandler<VisionAnalysisSession>? SessionEnded;

    public VisionAnalysisSession StartAnalysisSession(string sessionName, string detectorType, double confidenceThreshold)
    {
        _currentSession = new VisionAnalysisSession
        {
            Name = sessionName,
            DetectorType = detectorType,
            ConfidenceThreshold = confidenceThreshold,
            StartTime = DateTime.Now
        };

        _frameCounter = 0;
        _lastFrameTime = DateTime.Now;
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: 🚀 Sessão iniciada - {sessionName}");
        
        return _currentSession;
    }

    public async Task<VisionAnalysisFrame> ProcessVisionFrameAsync(Bitmap capturedImage, Rectangle detectionRegion)
    {
        if (_currentSession == null)
            throw new InvalidOperationException("Nenhuma sessão de análise ativa");

        var frameStartTime = DateTime.Now;
        var frame = new VisionAnalysisFrame
        {
            FrameNumber = ++_frameCounter,
            Timestamp = DateTime.Now,
            DetectionRegion = detectionRegion,
            ConfidenceThreshold = _currentSession.ConfidenceThreshold,
            ImageWidth = capturedImage.Width,
            ImageHeight = capturedImage.Height,
            TemplatePath = _currentSession.TemplatePath,
            DetectorType = _currentSession.DetectorType
        };

        try
        {
            // Calcular FPS
            var timeSinceLastFrame = frame.Timestamp - _lastFrameTime;
            frame.FPS = timeSinceLastFrame.TotalSeconds > 0 ? 1.0 / timeSinceLastFrame.TotalSeconds : 0;
            _lastFrameTime = frame.Timestamp;

            // Analisar imagem
            var analysisResult = await AnalyzeImageAsync(capturedImage, detectionRegion);
            
            // Preencher dados do frame
            frame.Score = analysisResult.Score;
            frame.IsDetected = analysisResult.IsDetected;
            frame.DetectionPosition = analysisResult.Position;
            frame.DetectionSize = analysisResult.Size;
            frame.TemplateMatchScore = analysisResult.TemplateMatchScore;
            frame.MatchMethod = analysisResult.MatchMethod;
            frame.TemplateWidth = analysisResult.TemplateWidth;
            frame.TemplateHeight = analysisResult.TemplateHeight;
            frame.TemplateArea = analysisResult.TemplateArea;
            
            // Calcular métricas de imagem
            CalculateImageMetrics(capturedImage, frame);
            
            // Calcular tempo de processamento
            var processingTime = DateTime.Now - frameStartTime;
            frame.ProcessingTimeMs = processingTime.TotalMilliseconds;
            frame.TotalTimeMs = processingTime.TotalMilliseconds;
            
            // Adicionar frame à sessão
            _currentSession.AddFrame(frame);
            
            // Disparar eventos
            FrameProcessed?.Invoke(this, frame);
            if (frame.IsDetected)
            {
                DetectionFound?.Invoke(this, frame);
            }
            
            return frame;
        }
        catch (Exception ex)
        {
            frame.HasError = true;
            frame.ErrorMessage = ex.Message;
            frame.ProcessingTimeMs = (DateTime.Now - frameStartTime).TotalMilliseconds;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: ❌ Erro no processamento do frame {frame.FrameNumber}: {ex.Message}");
            
            return frame;
        }
    }

    public void EndAnalysisSession()
    {
        if (_currentSession != null)
        {
            _currentSession.EndSession();
            SessionEnded?.Invoke(this, _currentSession);
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: ✅ Sessão finalizada - {_currentSession.Name}");
            Console.WriteLine($"   - Total de Frames: {_currentSession.TotalFrames}");
            Console.WriteLine($"   - Detecções: {_currentSession.DetectionFrames}");
            Console.WriteLine($"   - Taxa de Detecção: {_currentSession.DetectionRate:P2}");
            Console.WriteLine($"   - Score Médio: {_currentSession.AverageScore:F4}");
            Console.WriteLine($"   - FPS Médio: {_currentSession.AverageFPS:F1}");
        }
    }

    public VisionAnalysisSession? GetCurrentSession()
    {
        return _currentSession;
    }

    private async Task<VisionAnalysisResult> AnalyzeImageAsync(Bitmap capturedImage, Rectangle detectionRegion)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Criar detector se necessário
                if (_visionDetector == null)
                {
                    _visionDetector = new VisionDetector(
                        detectionRegion,
                        _currentSession?.ConfidenceThreshold ?? 0.5,
                        _currentSession?.TemplatePath ?? "data/images/bobber.png"
                    );
                }

                // ✅ CORREÇÃO: Usar DetectModular para aproveitar a imagem já capturada
                // Antes: var (detected, score, posX) = _visionDetector.Detect(); // ❌ Faz recaptura
                // Agora: usar a imagem já fornecida como parâmetro
                var (detected, score, posX) = _visionDetector.DetectModular(capturedImage);
                
                // Calcular métricas de template
                var templateInfo = GetTemplateInfo(_currentSession?.TemplatePath ?? "");
                
                return new VisionAnalysisResult
                {
                    IsDetected = detected,
                    Score = score,
                    Position = detected ? new Point((int)posX, detectionRegion.Y) : null,
                    Size = detected ? new Size(detectionRegion.Width, detectionRegion.Height) : null,
                    TemplateMatchScore = score,
                    MatchMethod = "TemplateMatching (Modular)",
                    TemplateWidth = templateInfo.Width,
                    TemplateHeight = templateInfo.Height,
                    TemplateArea = templateInfo.Width * templateInfo.Height
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: ❌ Erro na análise: {ex.Message}");
                
                return new VisionAnalysisResult
                {
                    IsDetected = false,
                    Score = 0.0,
                    TemplateMatchScore = 0.0,
                    MatchMethod = "Error",
                    TemplateWidth = 0,
                    TemplateHeight = 0,
                    TemplateArea = 0
                };
            }
        });
    }

    private void CalculateImageMetrics(Bitmap image, VisionAnalysisFrame frame)
    {
        try
        {
            // Calcular brilho médio
            double totalBrightness = 0;
            int pixelCount = 0;
            
            for (int x = 0; x < image.Width; x += 10) // Amostragem para performance
            {
                for (int y = 0; y < image.Height; y += 10)
                {
                    var pixel = image.GetPixel(x, y);
                    totalBrightness += (pixel.R + pixel.G + pixel.B) / 3.0;
                    pixelCount++;
                }
            }
            
            frame.ImageBrightness = pixelCount > 0 ? totalBrightness / pixelCount : 0;
            
            // Calcular contraste (simplificado)
            frame.ImageContrast = 1.0; // Placeholder
            
            // Calcular nitidez (simplificado)
            frame.ImageSharpness = 1.0; // Placeholder
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: ⚠️ Erro ao calcular métricas de imagem: {ex.Message}");
        }
    }

    private (double Width, double Height) GetTemplateInfo(string templatePath)
    {
        try
        {
            if (File.Exists(templatePath))
            {
                using var template = new Bitmap(templatePath);
                return (template.Width, template.Height);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 👁️ VisionAnalysisService: ⚠️ Erro ao obter informações do template: {ex.Message}");
        }
        
        return (0, 0);
    }

    private class VisionAnalysisResult
    {
        public bool IsDetected { get; set; }
        public double Score { get; set; }
        public Point? Position { get; set; }
        public Size? Size { get; set; }
        public double TemplateMatchScore { get; set; }
        public string MatchMethod { get; set; } = "";
        public double TemplateWidth { get; set; }
        public double TemplateHeight { get; set; }
        public double TemplateArea { get; set; }
    }
} 