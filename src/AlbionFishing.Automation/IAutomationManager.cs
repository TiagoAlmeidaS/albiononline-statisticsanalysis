using AlbionFishing.Core;

namespace AlbionFishing.Automation;

/// <summary>
/// Interface para o gerenciador de automação
/// </summary>
public interface IAutomationManager
{
    /// <summary>
    /// Clica com delay
    /// </summary>
    /// <param name="minDelay">Delay mínimo</param>
    /// <param name="maxDelay">Delay máximo</param>
    /// <param name="button">Botão</param>
    void ClickWithDelay(double minDelay = 0.2, double maxDelay = 0.4, string button = "left");
    
    /// <summary>
    /// Clica em uma posição específica com delay
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="minDelay">Delay mínimo</param>
    /// <param name="maxDelay">Delay máximo</param>
    /// <param name="button">Botão</param>
    void ClickAtWithDelay(int x, int y, double minDelay = 0.2, double maxDelay = 0.4, string button = "left");
    
    /// <summary>
    /// Clica na área de pesca
    /// </summary>
    /// <param name="config">Configuração do bot</param>
    void ClickFishingArea(BotConfig config);
    
    /// <summary>
    /// Move o cursor para uma posição
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="durationMs">Duração da animação</param>
    void MoveTo(int x, int y, int durationMs = 0);
    
    /// <summary>
    /// Pressiona uma tecla
    /// </summary>
    /// <param name="key">Tecla</param>
    void PressKey(string key);
    
    /// <summary>
    /// Obtém a posição atual do mouse
    /// </summary>
    /// <returns>Posição do mouse</returns>
    (int x, int y) GetMousePosition();
    
    /// <summary>
    /// Pressiona o botão do mouse
    /// </summary>
    /// <param name="button">Botão</param>
    void MouseDown(string button = "left");
    
    /// <summary>
    /// Solta o botão do mouse
    /// </summary>
    /// <param name="button">Botão</param>
    void MouseUp(string button = "left");

    void ClickFishingAreaToCastHook(BotConfig config, double minDelay = 0.2, double maxDelay = 0.4);
} 