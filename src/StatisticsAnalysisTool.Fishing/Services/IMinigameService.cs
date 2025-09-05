using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Fishing.Services
{
    /// <summary>
    /// Interface para o serviço de minigame de pesca
    /// </summary>
    public interface IMinigameService
    {
        /// <summary>
        /// Evento disparado quando o minigame é iniciado
        /// </summary>
        event EventHandler<MinigameStartedEventArgs> MinigameStarted;
        
        /// <summary>
        /// Evento disparado quando o minigame é finalizado
        /// </summary>
        event EventHandler<MinigameFinishedEventArgs> MinigameFinished;
        
        /// <summary>
        /// Evento disparado quando o bobber é detectado
        /// </summary>
        event EventHandler<BobberDetectedEventArgs> BobberDetected;
        
        /// <summary>
        /// Evento disparado quando há mudança no estado do mouse
        /// </summary>
        event EventHandler<MouseStateChangedEventArgs> MouseStateChanged;
        
        /// <summary>
        /// Inicia o tracking e resolução do minigame
        /// </summary>
        /// <param name="region">Região da tela para capturar</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        Task StartMinigameAsync(System.Drawing.Rectangle region);
        
        /// <summary>
        /// Para o minigame atual
        /// </summary>
        void StopMinigame();
        
        /// <summary>
        /// Verifica se o minigame está ativo
        /// </summary>
        bool IsMinigameActive { get; }
        
        /// <summary>
        /// Obtém estatísticas do minigame atual
        /// </summary>
        MinigameStats CurrentStats { get; }
    }
    
    /// <summary>
    /// Argumentos do evento de início do minigame
    /// </summary>
    public class MinigameStartedEventArgs : EventArgs
    {
        public DateTime StartTime { get; set; }
        public System.Drawing.Rectangle Region { get; set; }
    }
    
    /// <summary>
    /// Argumentos do evento de finalização do minigame
    /// </summary>
    public class MinigameFinishedEventArgs : EventArgs
    {
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public MinigameStats FinalStats { get; set; }
        public bool Success { get; set; }
    }
    
    /// <summary>
    /// Argumentos do evento de detecção do bobber
    /// </summary>
    public class BobberDetectedEventArgs : EventArgs
    {
        public float PositionX { get; set; }
        public float Width { get; set; }
        public float Middle { get; set; }
        public string Side { get; set; }
        public string SideIndicator { get; set; }
        public float DistanceFromCenter { get; set; }
        public bool ShouldHoldMouse { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Argumentos do evento de mudança de estado do mouse
    /// </summary>
    public class MouseStateChangedEventArgs : EventArgs
    {
        public bool IsMouseDown { get; set; }
        public string Action { get; set; } // "MouseDown" ou "MouseUp"
        public float PositionX { get; set; }
        public string Side { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Estatísticas do minigame
    /// </summary>
    public class MinigameStats
    {
        public int FrameCount { get; set; }
        public int DetectionCount { get; set; }
        public double DetectionRate => FrameCount > 0 ? (double)DetectionCount / FrameCount * 100 : 0;
        public TimeSpan Duration { get; set; }
        public int MouseDownCount { get; set; }
        public int MouseUpCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
