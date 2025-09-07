namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface para serviços de notificação cross-platform
/// Permite mostrar notificações do sistema em diferentes sistemas operacionais
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Mostra uma notificação
    /// </summary>
    /// <param name="title">Título da notificação</param>
    /// <param name="message">Mensagem da notificação</param>
    void ShowNotification(string title, string message);
    
    /// <summary>
    /// Mostra uma notificação de erro
    /// </summary>
    /// <param name="title">Título da notificação</param>
    /// <param name="message">Mensagem de erro</param>
    void ShowError(string title, string message);
    
    /// <summary>
    /// Mostra uma notificação de sucesso
    /// </summary>
    /// <param name="title">Título da notificação</param>
    /// <param name="message">Mensagem de sucesso</param>
    void ShowSuccess(string title, string message);
}
