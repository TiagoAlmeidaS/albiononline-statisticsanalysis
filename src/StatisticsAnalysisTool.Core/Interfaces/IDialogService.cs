namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface para serviços de diálogo cross-platform
/// Permite abrir/salvar arquivos e mostrar mensagens em diferentes sistemas operacionais
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Mostra um diálogo para abrir arquivo
    /// </summary>
    /// <param name="title">Título do diálogo</param>
    /// <param name="filter">Filtro de arquivos (ex: "Arquivos de texto|*.txt")</param>
    /// <returns>Caminho do arquivo selecionado ou null se cancelado</returns>
    string? ShowOpenFileDialog(string title, string filter);
    
    /// <summary>
    /// Mostra um diálogo para salvar arquivo
    /// </summary>
    /// <param name="title">Título do diálogo</param>
    /// <param name="filter">Filtro de arquivos (ex: "Arquivos de texto|*.txt")</param>
    /// <returns>Caminho do arquivo para salvar ou null se cancelado</returns>
    string? ShowSaveFileDialog(string title, string filter);
    
    /// <summary>
    /// Mostra uma caixa de mensagem
    /// </summary>
    /// <param name="title">Título da mensagem</param>
    /// <param name="message">Conteúdo da mensagem</param>
    void ShowMessageBox(string title, string message);
    
    /// <summary>
    /// Mostra um diálogo para selecionar pasta
    /// </summary>
    /// <param name="title">Título do diálogo</param>
    /// <returns>Caminho da pasta selecionada ou null se cancelado</returns>
    string? ShowFolderDialog(string title);
}
