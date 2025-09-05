namespace StatisticsAnalysisTool.Fishing.Interfaces;

/// <summary>
/// Interface para serviços de automação
/// </summary>
public interface IAutomationService
{
    /// <summary>
    /// Clica em uma posição específica
    /// </summary>
    Task ClickAtAsync(int x, int y, string button = "left");
    
    /// <summary>
    /// Clica no botão especificado na posição atual
    /// </summary>
    Task ClickAsync(string button = "left");
    
    /// <summary>
    /// Move o cursor para uma posição
    /// </summary>
    Task MoveToAsync(int x, int y);
    
    /// <summary>
    /// Pressiona uma tecla
    /// </summary>
    Task PressKeyAsync(string key);
    
    /// <summary>
    /// Obtém a posição atual do mouse
    /// </summary>
    Task<(int x, int y)> GetMousePositionAsync();
}
