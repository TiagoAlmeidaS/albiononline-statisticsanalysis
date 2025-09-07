using System.Drawing;

namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface principal para serviços de plataforma
/// Fornece acesso a todos os serviços específicos da plataforma
/// </summary>
public interface IPlatformServices
{
    /// <summary>
    /// Serviço de automação (mouse, teclado)
    /// </summary>
    IAutomationService Automation { get; }
    
    /// <summary>
    /// Serviço de captura de tela
    /// </summary>
    IScreenCapture ScreenCapture { get; }
    
    /// <summary>
    /// Serviço de WebView
    /// </summary>
    IWebViewService WebView { get; }
    
    /// <summary>
    /// Serviço de diálogos (abrir/salvar arquivos, mensagens)
    /// </summary>
    IDialogService Dialogs { get; }
    
    /// <summary>
    /// Serviço de notificações
    /// </summary>
    INotificationService Notifications { get; }
}
