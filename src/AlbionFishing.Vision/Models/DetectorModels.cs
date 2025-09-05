using System;
using System.Collections.Generic;
using System.Drawing;

namespace AlbionFishing.Vision.Models;

/// <summary>
/// Configurações para detecção de bobber
/// </summary>
public class BobberDetectionConfig
{
    public double ConfidenceThreshold { get; set; } = 0.5;
    public string TemplatePath { get; set; } = "data/images/bobber.png";
    public bool EnableDebugWindow { get; set; } = false;
    public bool EnableColorFiltering { get; set; } = true;
    public bool EnableHSVFiltering { get; set; } = true;
    public bool EnableGradientChannel { get; set; } = false;
    public bool EnableMultiScale { get; set; } = false;
    public bool EnableSignalAnalysis { get; set; } = false;
    public double[] Scales { get; set; } = { 0.60, 0.70, 0.80, 0.90, 1.00, 1.10, 1.20 };
    public int MaxProcessingTimeMs { get; set; } = 100;
    public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Métricas de performance do detector
/// </summary>
public class DetectorPerformanceMetrics
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalFramesProcessed { get; set; }
    public int SuccessfulDetections { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double MinProcessingTimeMs { get; set; } = double.MaxValue;
    public double MaxProcessingTimeMs { get; set; }
    public double AverageScore { get; set; }
    public double AverageFPS { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public double DetectionRate => TotalFramesProcessed > 0 ? (double)SuccessfulDetections / TotalFramesProcessed : 0;
    public double ErrorRate => TotalFramesProcessed > 0 ? (double)ErrorCount / TotalFramesProcessed : 0;
    public TimeSpan TotalRunTime => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.UtcNow - StartTime;

    public void RecordFrame(bool detected, double score, double processingTimeMs, string? error = null)
    {
        TotalFramesProcessed++;
        
        if (detected)
        {
            SuccessfulDetections++;
        }
        
        if (error != null)
        {
            ErrorCount++;
            Errors.Add(error);
        }
        
        // Atualizar métricas de tempo
        if (processingTimeMs < MinProcessingTimeMs)
            MinProcessingTimeMs = processingTimeMs;
        if (processingTimeMs > MaxProcessingTimeMs)
            MaxProcessingTimeMs = processingTimeMs;
        
        // Calcular média de tempo de processamento
        AverageProcessingTimeMs = ((AverageProcessingTimeMs * (TotalFramesProcessed - 1)) + processingTimeMs) / TotalFramesProcessed;
        
        // Calcular média de score
        AverageScore = ((AverageScore * (TotalFramesProcessed - 1)) + score) / TotalFramesProcessed;
        
        // Calcular FPS
        var elapsed = TotalRunTime.TotalSeconds;
        if (elapsed > 0)
            AverageFPS = TotalFramesProcessed / elapsed;
    }

    public void Finish()
    {
        EndTime = DateTime.UtcNow;
    }
}
