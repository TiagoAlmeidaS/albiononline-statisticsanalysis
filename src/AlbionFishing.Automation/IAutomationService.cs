using System;

namespace AlbionFishing.Automation;

/// <summary>
/// Interface abstrata para serviços de automação multiplataforma
/// </summary>
public interface IAutomationService
{
    /// <summary>
    /// Move o cursor para a posição especificada
    /// </summary>
    void MoveTo(int x, int y);
    
    /// <summary>
    /// Move o cursor para a posição especificada com animação
    /// </summary>
    void MoveTo(int x, int y, int durationMs);
    
    /// <summary>
    /// Clica no botão especificado na posição atual
    /// </summary>
    void Click(string button = "left");
    
    /// <summary>
    /// Clica no botão especificado na posição especificada
    /// </summary>
    void ClickAt(int x, int y, string button = "left");
    
    /// <summary>
    /// Obtém a posição atual do cursor
    /// </summary>
    (int x, int y) GetCurrentPosition();
    
    /// <summary>
    /// Pressiona o botão do mouse
    /// </summary>
    void MouseDown(string button = "left");
    
    /// <summary>
    /// Solta o botão do mouse
    /// </summary>
    void MouseUp(string button = "left");
    
    /// <summary>
    /// Envia uma tecla
    /// </summary>
    void SendKey(string key);
    
    /// <summary>
    /// Envia uma combinação de teclas
    /// </summary>
    void SendKeyCombination(params string[] keys);
    
    /// <summary>
    /// Verifica se a plataforma é suportada
    /// </summary>
    bool IsSupported();
}