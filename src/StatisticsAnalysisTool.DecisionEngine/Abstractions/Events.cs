using System;
using System.Drawing;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Evento de decisão tomada
    /// </summary>
    public class DecisionMadeEventArgs : EventArgs
    {
        public DecisionResult Decision { get; set; } = new();
        public IGameContext Context { get; set; } = null!;
        public string BehaviorName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de mudança de contexto
    /// </summary>
    public class ContextChangedEventArgs : EventArgs
    {
        public IGameContext OldContext { get; set; } = null!;
        public IGameContext NewContext { get; set; } = null!;
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de comportamento executado
    /// </summary>
    public class BehaviorExecutedEventArgs : EventArgs
    {
        public string BehaviorName { get; set; } = string.Empty;
        public BehaviorResult Result { get; set; } = new();
        public IGameContext Context { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de comportamento ativado
    /// </summary>
    public class BehaviorActivatedEventArgs : EventArgs
    {
        public string BehaviorName { get; set; } = string.Empty;
        public IGameContext Context { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de comportamento desativado
    /// </summary>
    public class BehaviorDeactivatedEventArgs : EventArgs
    {
        public string BehaviorName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de minigame iniciado
    /// </summary>
    public class MinigameStartedEventArgs : EventArgs
    {
        public Rectangle FishingArea { get; }
        public MinigameConfiguration Configuration { get; }
        public DateTime StartTime { get; } = DateTime.UtcNow;
        
        public MinigameStartedEventArgs(Rectangle fishingArea, MinigameConfiguration configuration)
        {
            FishingArea = fishingArea;
            Configuration = configuration;
        }
    }
    
    /// <summary>
    /// Evento de minigame finalizado
    /// </summary>
    public class MinigameFinishedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public MinigameStats Stats { get; set; } = new();
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de bobber detectado
    /// </summary>
    public class BobberDetectedEventArgs : EventArgs
    {
        public string Side { get; set; } = string.Empty;
        public string SideIndicator { get; set; } = string.Empty;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Evento de mudança de estado do mouse
    /// </summary>
    public class MouseStateChangedEventArgs : EventArgs
    {
        public string Action { get; set; } = string.Empty;
        public string Side { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estatísticas do minigame
    /// </summary>
    public class MinigameStats
    {
        public int FrameCount { get; set; }
        public int DetectionCount { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        
        public double DetectionRate => FrameCount > 0 ? (double)DetectionCount / FrameCount * 100 : 0;
    }
}
