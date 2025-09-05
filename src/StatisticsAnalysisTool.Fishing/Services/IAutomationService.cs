using System;
using System.Drawing;

namespace StatisticsAnalysisTool.Fishing.Services
{
    /// <summary>
    /// Interface para o serviço de automação (mouse e teclado)
    /// </summary>
    public interface IAutomationService
    {
        /// <summary>
        /// Evento disparado quando uma ação de mouse é executada
        /// </summary>
        event EventHandler<MouseActionEventArgs> MouseActionExecuted;
        
        /// <summary>
        /// Evento disparado quando uma ação de teclado é executada
        /// </summary>
        event EventHandler<KeyboardActionEventArgs> KeyboardActionExecuted;
        
        /// <summary>
        /// Pressiona o botão do mouse
        /// </summary>
        /// <param name="button">Botão do mouse (left, right, middle)</param>
        void MouseDown(string button);
        
        /// <summary>
        /// Solta o botão do mouse
        /// </summary>
        /// <param name="button">Botão do mouse (left, right, middle)</param>
        void MouseUp(string button);
        
        /// <summary>
        /// Clica no mouse
        /// </summary>
        /// <param name="button">Botão do mouse (left, right, middle)</param>
        void MouseClick(string button);
        
        /// <summary>
        /// Move o mouse para uma posição específica
        /// </summary>
        /// <param name="x">Coordenada X</param>
        /// <param name="y">Coordenada Y</param>
        void MouseMove(int x, int y);
        
        /// <summary>
        /// Pressiona uma tecla
        /// </summary>
        /// <param name="key">Código da tecla</param>
        void KeyDown(int key);
        
        /// <summary>
        /// Solta uma tecla
        /// </summary>
        /// <param name="key">Código da tecla</param>
        void KeyUp(int key);
        
        /// <summary>
        /// Pressiona e solta uma tecla
        /// </summary>
        /// <param name="key">Código da tecla</param>
        void KeyPress(int key);
        
        /// <summary>
        /// Verifica se o serviço está disponível
        /// </summary>
        bool IsAvailable { get; }
    }
    
    /// <summary>
    /// Argumentos do evento de ação do mouse
    /// </summary>
    public class MouseActionEventArgs : EventArgs
    {
        public string Action { get; set; } // "MouseDown", "MouseUp", "MouseClick", "MouseMove"
        public string Button { get; set; }
        public Point? Position { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Argumentos do evento de ação do teclado
    /// </summary>
    public class KeyboardActionEventArgs : EventArgs
    {
        public string Action { get; set; } // "KeyDown", "KeyUp", "KeyPress"
        public int KeyCode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
