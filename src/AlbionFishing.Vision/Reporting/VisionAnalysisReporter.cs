using System.Text;
using System.Text.Json;
using AlbionFishing.Vision.Analysis.Models;

namespace AlbionFishing.Vision.Reporting;

/// <summary>
/// Implementação concreta do gerador de relatórios de análise visual
/// </summary>
public class VisionAnalysisReporter : IVisionAnalysisReporter
{
    public string GenerateBasicReport(VisionAnalysisSession session)
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== RELATÓRIO BÁSICO DE ANÁLISE VISUAL ===");
        report.AppendLine($"Sessão: {session.Name}");
        report.AppendLine($"Início: {session.StartTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Fim: {session.EndTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Duração: {session.Duration:hh\\:mm\\:ss}");
        report.AppendLine($"Total de Frames: {session.TotalFrames}");
        report.AppendLine($"Frames com Detecção: {session.DetectionFrames}");
        report.AppendLine($"Taxa de Detecção: {session.DetectionRate:P2}");
        report.AppendLine($"Score Médio: {session.AverageScore:F6}");
        report.AppendLine($"Score Máximo: {session.MaxScore:F6}");
        report.AppendLine($"Score Mínimo: {session.MinScore:F6}");
        report.AppendLine($"Desvio Padrão: {session.StandardDeviation:F6}");
        report.AppendLine($"Tipo de Detector: {session.DetectorType}");
        report.AppendLine($"Threshold de Confiança: {session.ConfidenceThreshold:F4}");
        report.AppendLine($"FPS Médio: {session.AverageFPS:F1}");
        report.AppendLine($"FPS Máximo: {session.MaxFPS:F1}");
        report.AppendLine($"FPS Mínimo: {session.MinFPS:F1}");
        report.AppendLine($"Total de Detecções: {session.TotalDetections}");
        report.AppendLine($"Tamanho Médio de Detecção: {session.AverageDetectionSize:F1} pixels²");
        report.AppendLine($"Tamanho Máximo de Detecção: {session.MaxDetectionSize:F1} pixels²");
        
        return report.ToString();
    }

    public string GenerateDetailedReport(VisionAnalysisSession session)
    {
        var report = new StringBuilder();
        
        report.AppendLine(GenerateBasicReport(session));
        report.AppendLine();
        report.AppendLine("=== MÉTRICAS DETALHADAS ===");
        
        foreach (var metric in session.PerformanceMetrics)
        {
            report.AppendLine($"{metric.Key}: {metric.Value:F6}");
        }
        
        if (session.Frames.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("=== ÚLTIMOS 10 FRAMES ===");
            var lastFrames = session.Frames.TakeLast(10);
            foreach (var frame in lastFrames)
            {
                report.AppendLine($"Frame {frame.FrameNumber}: Score={frame.Score:F6}, Detected={frame.IsDetected}, FPS={frame.FPS:F1}, ProcessingTime={frame.ProcessingTimeMs:F1}ms");
            }
        }
        
        if (session.DetectionPositions.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("=== POSIÇÕES DE DETECÇÃO ===");
            var uniquePositions = session.DetectionPositions.GroupBy(p => p).Select(g => new { Position = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count);
            foreach (var pos in uniquePositions.Take(10))
            {
                report.AppendLine($"Posição ({pos.Position.X}, {pos.Position.Y}): {pos.Count} detecções");
            }
        }
        
        return report.ToString();
    }

    public string GenerateCSVReport(VisionAnalysisSession session)
    {
        var csv = new StringBuilder();
        
        // Cabeçalho
        csv.AppendLine("FrameNumber,Timestamp,Score,IsDetected,FPS,ProcessingTimeMs,DetectionPositionX,DetectionPositionY,DetectionSizeWidth,DetectionSizeHeight,TemplateMatchScore,MatchMethod,TemplateWidth,TemplateHeight,TemplateArea,ImageWidth,ImageHeight,ImageBrightness,ImageContrast,ImageSharpness,ConfidenceThreshold,DetectorType,TemplatePath,ErrorMessage,HasError");
        
        // Dados
        foreach (var frame in session.Frames)
        {
            csv.AppendLine($"{frame.FrameNumber},{frame.Timestamp:yyyy-MM-dd HH:mm:ss.fff},{frame.Score:F6},{frame.IsDetected},{frame.FPS:F1},{frame.ProcessingTimeMs:F1},{frame.DetectionPosition?.X ?? 0},{frame.DetectionPosition?.Y ?? 0},{frame.DetectionSize?.Width ?? 0},{frame.DetectionSize?.Height ?? 0},{frame.TemplateMatchScore:F6},{frame.MatchMethod},{frame.TemplateWidth:F1},{frame.TemplateHeight:F1},{frame.TemplateArea:F1},{frame.ImageWidth},{frame.ImageHeight},{frame.ImageBrightness:F2},{frame.ImageContrast:F2},{frame.ImageSharpness:F2},{frame.ConfidenceThreshold:F4},{frame.DetectorType},{frame.TemplatePath},{frame.ErrorMessage},{frame.HasError}");
        }
        
        return csv.ToString();
    }

    public string GenerateJSONReport(VisionAnalysisSession session)
    {
        var report = new
        {
            SessionName = session.Name,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Duration = session.Duration,
            TotalFrames = session.TotalFrames,
            DetectionFrames = session.DetectionFrames,
            DetectionRate = session.DetectionRate,
            AverageScore = session.AverageScore,
            MaxScore = session.MaxScore,
            MinScore = session.MinScore,
            StandardDeviation = session.StandardDeviation,
            DetectorType = session.DetectorType,
            ConfidenceThreshold = session.ConfidenceThreshold,
            DetectionRegion = new { session.DetectionRegion.X, session.DetectionRegion.Y, session.DetectionRegion.Width, session.DetectionRegion.Height },
            TemplatePath = session.TemplatePath,
            MatchMethod = session.MatchMethod,
            DetectorParameters = session.DetectorParameters,
            PerformanceMetrics = session.PerformanceMetrics,
            AverageFPS = session.AverageFPS,
            MaxFPS = session.MaxFPS,
            MinFPS = session.MinFPS,
            TotalDetections = session.TotalDetections,
            AverageDetectionSize = session.AverageDetectionSize,
            MaxDetectionSize = session.MaxDetectionSize,
            DetectionPositions = session.DetectionPositions.Select(p => new { p.X, p.Y }).ToList(),
            DetectionSizes = session.DetectionSizes.Select(s => new { s.Width, s.Height }).ToList(),
            Frames = session.Frames.Select(f => new
            {
                f.FrameNumber,
                f.Timestamp,
                f.Score,
                f.IsDetected,
                f.FPS,
                f.ProcessingTimeMs,
                DetectionPosition = f.DetectionPosition.HasValue ? new { f.DetectionPosition.Value.X, f.DetectionPosition.Value.Y } : null,
                DetectionSize = f.DetectionSize.HasValue ? new { f.DetectionSize.Value.Width, f.DetectionSize.Value.Height } : null,
                f.TemplateMatchScore,
                f.MatchMethod,
                f.TemplateWidth,
                f.TemplateHeight,
                f.TemplateArea,
                f.ImageWidth,
                f.ImageHeight,
                f.ImageBrightness,
                f.ImageContrast,
                f.ImageSharpness,
                f.ConfidenceThreshold,
                f.DetectorType,
                f.TemplatePath,
                f.ErrorMessage,
                f.HasError
            }).ToList()
        };
        
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public string GenerateConfigurationReport(VisionAnalysisSession session)
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== RELATÓRIO DE CONFIGURAÇÕES DE VISION ===");
        report.AppendLine($"Data/Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Sessão: {session.Name}");
        report.AppendLine($"Detector: {session.DetectorType}");
        report.AppendLine();
        
        report.AppendLine("=== CONFIGURAÇÕES DE DETECÇÃO ===");
        report.AppendLine($"Região de Detecção: X={session.DetectionRegion.X}, Y={session.DetectionRegion.Y}, W={session.DetectionRegion.Width}, H={session.DetectionRegion.Height}");
        report.AppendLine($"Threshold de Confiança: {session.ConfidenceThreshold:F4}");
        report.AppendLine($"Caminho do Template: {session.TemplatePath}");
        report.AppendLine($"Método de Matching: {session.MatchMethod}");
        report.AppendLine();
        
        report.AppendLine("=== PARÂMETROS DO DETECTOR ===");
        foreach (var param in session.DetectorParameters)
        {
            report.AppendLine($"{param.Key}: {param.Value:F6}");
        }
        
        return report.ToString();
    }

    public string GeneratePerformanceReport(VisionAnalysisSession session)
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== RELATÓRIO DE PERFORMANCE DE VISION ===");
        report.AppendLine($"Sessão: {session.Name}");
        report.AppendLine($"Duração: {session.Duration:hh\\:mm\\:ss}");
        report.AppendLine();
        
        report.AppendLine("=== MÉTRICAS DE FRAME ===");
        report.AppendLine($"Total de Frames: {session.TotalFrames}");
        report.AppendLine($"Frames com Detecção: {session.DetectionFrames}");
        report.AppendLine($"Taxa de Detecção: {session.DetectionRate:P2}");
        report.AppendLine();
        
        report.AppendLine("=== MÉTRICAS DE FPS ===");
        report.AppendLine($"FPS Médio: {session.AverageFPS:F1}");
        report.AppendLine($"FPS Máximo: {session.MaxFPS:F1}");
        report.AppendLine($"FPS Mínimo: {session.MinFPS:F1}");
        report.AppendLine();
        
        report.AppendLine("=== MÉTRICAS DE SCORE ===");
        report.AppendLine($"Score Médio: {session.AverageScore:F6}");
        report.AppendLine($"Score Máximo: {session.MaxScore:F6}");
        report.AppendLine($"Score Mínimo: {session.MinScore:F6}");
        report.AppendLine($"Desvio Padrão: {session.StandardDeviation:F6}");
        report.AppendLine();
        
        report.AppendLine("=== MÉTRICAS DE DETECÇÃO ===");
        report.AppendLine($"Total de Detecções: {session.TotalDetections}");
        report.AppendLine($"Tamanho Médio de Detecção: {session.AverageDetectionSize:F1} pixels²");
        report.AppendLine($"Tamanho Máximo de Detecção: {session.MaxDetectionSize:F1} pixels²");
        report.AppendLine();
        
        report.AppendLine("=== MÉTRICAS DE PERFORMANCE ===");
        foreach (var metric in session.PerformanceMetrics)
        {
            report.AppendLine($"{metric.Key}: {metric.Value:F6}");
        }
        
        return report.ToString();
    }

    public string GenerateQualityReport(VisionAnalysisSession session)
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== RELATÓRIO DE QUALIDADE DE DETECÇÃO ===");
        report.AppendLine($"Sessão: {session.Name}");
        report.AppendLine();
        
        // Análise de qualidade baseada nas métricas
        var qualityScore = CalculateQualityScore(session);
        report.AppendLine($"Score de Qualidade Geral: {qualityScore:F2}/100");
        report.AppendLine();
        
        // Recomendações
        var recommendations = GenerateRecommendations(session);
        report.AppendLine("=== RECOMENDAÇÕES ===");
        foreach (var rec in recommendations)
        {
            report.AppendLine($"• {rec}");
        }
        report.AppendLine();
        
        // Problemas identificados
        var issues = IdentifyIssues(session);
        if (issues.Count > 0)
        {
            report.AppendLine("=== PROBLEMAS IDENTIFICADOS ===");
            foreach (var issue in issues)
            {
                report.AppendLine($"⚠️ {issue}");
            }
            report.AppendLine();
        }
        
        // Otimizações sugeridas
        var optimizations = SuggestOptimizations(session);
        if (optimizations.Count > 0)
        {
            report.AppendLine("=== OTIMIZAÇÕES SUGERIDAS ===");
            foreach (var opt in optimizations)
            {
                report.AppendLine($"🔧 {opt}");
            }
        }
        
        return report.ToString();
    }

    public void SaveReport(string report, string filePath)
    {
        File.WriteAllText(filePath, report);
    }

    private double CalculateQualityScore(VisionAnalysisSession session)
    {
        double score = 0;
        
        // Taxa de detecção (40% do score)
        score += session.DetectionRate * 40;
        
        // Estabilidade do score (30% do score)
        double scoreStability = 1.0 - Math.Min(session.StandardDeviation, 1.0);
        score += scoreStability * 30;
        
        // Performance de FPS (20% do score)
        double fpsScore = Math.Min(session.AverageFPS / 30.0, 1.0); // 30 FPS como referência
        score += fpsScore * 20;
        
        // Consistência de tamanho (10% do score)
        if (session.DetectionSizes.Count > 1)
        {
            var sizes = session.DetectionSizes.Select(s => s.Width * s.Height).ToList();
            var avgSize = sizes.Average();
            var sizeVariance = sizes.Select(s => Math.Pow(s - avgSize, 2)).Average();
            var sizeConsistency = 1.0 - Math.Min(sizeVariance / (avgSize * avgSize), 1.0);
            score += sizeConsistency * 10;
        }
        else
        {
            score += 10; // Se só uma detecção, considerar consistente
        }
        
        return Math.Min(score, 100);
    }

    private List<string> GenerateRecommendations(VisionAnalysisSession session)
    {
        var recommendations = new List<string>();
        
        if (session.DetectionRate < 0.1)
        {
            recommendations.Add("Taxa de detecção muito baixa. Considere reduzir o threshold de confiança.");
        }
        
        if (session.AverageFPS < 10)
        {
            recommendations.Add("FPS muito baixo. Considere otimizar o processamento ou reduzir a resolução.");
        }
        
        if (session.StandardDeviation > 0.3)
        {
            recommendations.Add("Scores muito instáveis. Verifique a qualidade do template e condições de iluminação.");
        }
        
        if (session.DetectionSizes.Count > 1)
        {
            var sizes = session.DetectionSizes.Select(s => s.Width * s.Height).ToList();
            var avgSize = sizes.Average();
            var sizeVariance = sizes.Select(s => Math.Pow(s - avgSize, 2)).Average();
            if (sizeVariance > avgSize * 0.5)
            {
                recommendations.Add("Tamanhos de detecção muito variáveis. Verifique a estabilidade do template.");
            }
        }
        
        if (recommendations.Count == 0)
        {
            recommendations.Add("Configuração parece adequada. Mantenha os parâmetros atuais.");
        }
        
        return recommendations;
    }

    private List<string> IdentifyIssues(VisionAnalysisSession session)
    {
        var issues = new List<string>();
        
        if (session.TotalFrames == 0)
        {
            issues.Add("Nenhum frame processado. Verifique se a captura está funcionando.");
        }
        
        if (session.DetectionFrames == 0 && session.TotalFrames > 0)
        {
            issues.Add("Nenhuma detecção encontrada. Threshold pode estar muito alto.");
        }
        
        if (session.AverageFPS < 5)
        {
            issues.Add("Performance muito baixa. Sistema pode estar sobrecarregado.");
        }
        
        if (session.StandardDeviation > 0.5)
        {
            issues.Add("Scores muito instáveis. Possível problema com template ou condições.");
        }
        
        return issues;
    }

    private List<string> SuggestOptimizations(VisionAnalysisSession session)
    {
        var optimizations = new List<string>();
        
        if (session.AverageFPS < 15)
        {
            optimizations.Add("Reduzir a resolução da região de captura para melhorar FPS.");
            optimizations.Add("Considerar processamento em thread separada.");
        }
        
        if (session.DetectionRate < 0.05)
        {
            optimizations.Add("Ajustar threshold de confiança para 0.3-0.4.");
            optimizations.Add("Verificar qualidade e tamanho do template.");
        }
        
        if (session.StandardDeviation > 0.4)
        {
            optimizations.Add("Melhorar iluminação da área de captura.");
            optimizations.Add("Usar template com melhor contraste.");
        }
        
        return optimizations;
    }
} 