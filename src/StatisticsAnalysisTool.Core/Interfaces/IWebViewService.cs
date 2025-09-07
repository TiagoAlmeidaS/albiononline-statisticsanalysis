namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface para serviços de WebView cross-platform
/// Permite integrar conteúdo web em diferentes sistemas operacionais
/// </summary>
public interface IWebViewService
{
    /// <summary>
    /// Inicializa o WebView com o controle pai
    /// </summary>
    /// <param name="parent">Controle pai onde o WebView será inserido</param>
    void Initialize(object parent);
    
    /// <summary>
    /// Navega para uma URL específica
    /// </summary>
    /// <param name="url">URL para navegar</param>
    void Navigate(string url);
    
    /// <summary>
    /// Libera recursos do WebView
    /// </summary>
    void Dispose();
}
