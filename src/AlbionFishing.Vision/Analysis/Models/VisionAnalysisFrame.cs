using System;
using System.Drawing;

namespace AlbionFishing.Vision.Analysis.Models;

/// <summary>
/// Classe para armazenar dados detalhados de cada frame de análise visual
/// </summary>
public class VisionAnalysisFrame
{
    public int FrameNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public double Score { get; set; }
    public bool IsDetected { get; set; }
    public double FPS { get; set; }
    public double ProcessingTimeMs { get; set; }
    
    // Informações de detecção
    public Point? DetectionPosition { get; set; }
    public Size? DetectionSize { get; set; }
    public Rectangle DetectionRegion { get; set; }
    public double ConfidenceThreshold { get; set; }
    
    // Métricas de template matching
    public double TemplateMatchScore { get; set; }
    public string MatchMethod { get; set; } = "";
    public double TemplateWidth { get; set; }
    public double TemplateHeight { get; set; }
    public double TemplateArea { get; set; }
    
    // Métricas de imagem
    public double ImageBrightness { get; set; }
    public double ImageContrast { get; set; }
    public double ImageSharpness { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    
    // Métricas de performance
    public double CaptureTimeMs { get; set; }
    public double AnalysisTimeMs { get; set; }
    public double TotalTimeMs { get; set; }
    
    // Informações de debug
    public string TemplatePath { get; set; } = "";
    public string DetectorType { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public bool HasError { get; set; }
    
    // Métricas de qualidade de detecção
    public double DetectionQuality { get; set; }
    public double PositionStability { get; set; }
    public double SizeConsistency { get; set; }
    public double ScoreConsistency { get; set; }
    
    // Histórico para análise temporal
    public int ConsecutiveDetections { get; set; }
    public int ConsecutiveNonDetections { get; set; }
    public double AverageScoreLast10Frames { get; set; }
    public double ScoreVariance { get; set; }

    public VisionAnalysisFrame()
    {
        Timestamp = DateTime.Now;
    }
} 