using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AlbionFishing.Vision.Analysis.Models;

/// <summary>
/// Representa uma sessão de análise visual com métricas e configurações
/// </summary>
public class VisionAnalysisSession
{
    public string Name { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public int TotalFrames { get; set; }
    public int DetectionFrames { get; set; }
    public double DetectionRate => TotalFrames > 0 ? (double)DetectionFrames / TotalFrames : 0.0;
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

    public VisionAnalysisSession()
    {
        StartTime = DateTime.Now;
        EndTime = DateTime.Now;
    }

    public void AddFrame(VisionAnalysisFrame frame)
    {
        Frames.Add(frame);
        TotalFrames++;
        
        if (frame.IsDetected)
        {
            DetectionFrames++;
            TotalDetections++;
            
            if (frame.DetectionPosition.HasValue)
                DetectionPositions.Add(frame.DetectionPosition.Value);
            
            if (frame.DetectionSize.HasValue)
                DetectionSizes.Add(frame.DetectionSize.Value);
        }
    }

    public void EndSession()
    {
        EndTime = DateTime.Now;
        CalculateSessionMetrics();
    }

    private void CalculateSessionMetrics()
    {
        if (Frames.Count == 0) return;

        var scores = Frames.Select(f => f.Score).ToList();
        AverageScore = scores.Average();
        MaxScore = scores.Max();
        MinScore = scores.Min();
        
        if (scores.Count > 1)
        {
            var variance = scores.Select(s => Math.Pow(s - AverageScore, 2)).Average();
            StandardDeviation = Math.Sqrt(variance);
        }

        // Calcular métricas de FPS
        var fpsValues = Frames.Select(f => f.FPS).Where(f => f > 0).ToList();
        if (fpsValues.Count > 0)
        {
            AverageFPS = fpsValues.Average();
            MaxFPS = fpsValues.Max();
            MinFPS = fpsValues.Min();
        }

        // Calcular métricas de tamanho de detecção
        var detectionSizes = DetectionSizes.Select(s => s.Width * s.Height).ToList();
        if (detectionSizes.Count > 0)
        {
            AverageDetectionSize = detectionSizes.Average();
            MaxDetectionSize = detectionSizes.Max();
        }

        // Calcular métricas de performance
        PerformanceMetrics["AverageProcessingTime"] = Frames.Select(f => f.ProcessingTimeMs).Average();
        PerformanceMetrics["MaxProcessingTime"] = Frames.Select(f => f.ProcessingTimeMs).Max();
        PerformanceMetrics["MinProcessingTime"] = Frames.Select(f => f.ProcessingTimeMs).Min();
        PerformanceMetrics["DetectionAccuracy"] = DetectionRate;
        PerformanceMetrics["ScoreStability"] = StandardDeviation;
    }
} 