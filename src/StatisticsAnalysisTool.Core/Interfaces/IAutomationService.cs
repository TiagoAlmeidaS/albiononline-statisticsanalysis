using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Core.Interfaces;

/// <summary>
/// Interface para serviços de automação cross-platform
/// Permite controle de mouse e teclado em diferentes sistemas operacionais
/// </summary>
public interface IAutomationService
{
    /// <summary>
    /// Verifica se o serviço de automação está disponível no sistema atual
    /// </summary>
    /// <returns>True se disponível, False caso contrário</returns>
    bool IsSupported();
    
    /// <summary>
    /// Move o cursor do mouse para uma posição específica
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    void MoveTo(int x, int y);
    
    /// <summary>
    /// Clica no botão especificado na posição atual do mouse
    /// </summary>
    /// <param name="button">Botão do mouse ("left", "right", "middle")</param>
    void Click(string button = "left");
    
    /// <summary>
    /// Clica em uma posição específica da tela
    /// </summary>
    /// <param name="x">Posição X</param>
    /// <param name="y">Posição Y</param>
    /// <param name="button">Botão do mouse ("left", "right", "middle")</param>
    void ClickAt(int x, int y, string button = "left");
    
    /// <summary>
    /// Pressiona uma tecla específica
    /// </summary>
    /// <param name="key">Nome da tecla (ex: "Enter", "Space", "Ctrl")</param>
    void KeyPress(string key);
    
    /// <summary>
    /// Digita um texto
    /// </summary>
    /// <param name="text">Texto a ser digitado</param>
    void TypeText(string text);
}
