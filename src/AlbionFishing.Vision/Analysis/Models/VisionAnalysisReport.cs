using System;
using System.Collections.Generic;
using System.Drawing;

namespace AlbionFishing.Vision.Analysis.Models;

/// <summary>
/// Classe para relatórios de análise visual detalhados
/// </summary>
public class VisionAnalysisReport
{
    public string SessionName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int TotalFrames { get; set; }
    public int DetectionFrames { get; set; }
    public double DetectionRate { get; set; }
    public double AverageScore { get; set; }
    public double MaxScore { get; set; }
    public double MinScore { get; set; }
    public double StandardDeviation { get; set; }
    public string DetectorType { get; set; } = "";
    public double ConfidenceThreshold { get; set; }
    public Rectangle DetectionRegion { get; set; }
    public string TemplatePath { get; set; } = "";
    public string MatchMethod { get; set; } = "";
    public Dictionary<string, double> DetectorParameters { get; set; } = new Dictionary<string, double>();
    public List<VisionAnalysisFrame> Frames { get; set; } = new List<VisionAnalysisFrame>();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new Dictionary<string, double>();
    
    // Métricas específicas de vision
    public double AverageFPS { get; set; }
    public double MaxFPS { get; set; }
    public double MinFPS { get; set; }
    public int TotalDetections { get; set; }
    public double AverageDetectionSize { get; set; }
    public double MaxDetectionSize { get; set; }
    public List<Point> DetectionPositions { get; set; } = new List<Point>();
    public List<Size> DetectionSizes { get; set; } = new List<Size>();
    
    // Análise de qualidade
    public double DetectionAccuracy { get; set; }
    public double PositionStability { get; set; }
    public double SizeConsistency { get; set; }
    public double ScoreStability { get; set; }
    public double ProcessingEfficiency { get; set; }
    
    // Recomendações
    public List<string> Recommendations { get; set; } = new List<string>();
    public List<string> Issues { get; set; } = new List<string>();
    public List<string> Optimizations { get; set; } = new List<string>();
    
    public VisionAnalysisReport()
    {
        StartTime = DateTime.Now;
    }
} 