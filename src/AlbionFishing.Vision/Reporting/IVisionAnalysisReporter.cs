using AlbionFishing.Vision.Analysis.Models;

namespace AlbionFishing.Vision.Reporting;

/// <summary>
/// Interface para geração de relatórios de análise visual
/// </summary>
public interface IVisionAnalysisReporter
{
    /// <summary>
    /// Gera um relatório básico da sessão de análise visual
    /// </summary>
    string GenerateBasicReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório detalhado da sessão de análise visual
    /// </summary>
    string GenerateDetailedReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório em formato CSV
    /// </summary>
    string GenerateCSVReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório em formato JSON
    /// </summary>
    string GenerateJSONReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório de configurações de vision
    /// </summary>
    string GenerateConfigurationReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório de performance de vision
    /// </summary>
    string GeneratePerformanceReport(VisionAnalysisSession session);

    /// <summary>
    /// Gera um relatório de qualidade de detecção
    /// </summary>
    string GenerateQualityReport(VisionAnalysisSession session);

    /// <summary>
    /// Salva o relatório em um arquivo
    /// </summary>
    void SaveReport(string report, string filePath);
} 