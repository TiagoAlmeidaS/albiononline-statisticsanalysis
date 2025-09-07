using System.Drawing;

namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface para captura de tela cross-platform
/// Permite implementações diferentes para Windows, macOS, Linux, etc.
/// </summary>
public interface IScreenCapture
{
    /// <summary>
    /// Verifica se a captura de tela está disponível no sistema atual
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Nome da implementação de captura
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Versão da implementação
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Captura uma região específica da tela
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="width">Largura</param>
    /// <param name="height">Altura</param>
    /// <returns>Bitmap capturado ou null se falhou</returns>
    Bitmap? CaptureRegion(int x, int y, int width, int height);
    
    /// <summary>
    /// Captura uma região específica da tela
    /// </summary>
    /// <param name="region">Região para capturar</param>
    /// <returns>Bitmap capturado ou null se falhou</returns>
    Bitmap? CaptureRegion(Rectangle region);
}
