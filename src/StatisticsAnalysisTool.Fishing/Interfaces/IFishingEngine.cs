using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Interfaces;

/// <summary>
/// Interface principal do engine de pesca
/// </summary>
public interface IFishingEngine
{
    /// <summary>
    /// Estado atual do sistema de pesca
    /// </summary>
    FishingState CurrentState { get; }
    
    /// <summary>
    /// Evento de pesca ativo
    /// </summary>
    FishingEvent? ActiveFishingEvent { get; }
    
    /// <summary>
    /// Indica se o sistema está ativo
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Evento disparado quando o estado muda
    /// </summary>
    event EventHandler<FishingStateChangedEventArgs>? StateChanged;
    
    /// <summary>
    /// Evento disparado quando uma zona de pesca é detectada
    /// </summary>
    event EventHandler<FishingZoneDetectedEventArgs>? ZoneDetected;
    
    /// <summary>
    /// Evento disparado quando o bobber é detectado
    /// </summary>
    event EventHandler<BobberDetectedEventArgs>? BobberDetected;
    
    /// <summary>
    /// Evento disparado quando o minigame inicia
    /// </summary>
    event EventHandler<MinigameStartedEventArgs>? MinigameStarted;
    
    /// <summary>
    /// Evento disparado quando a pesca é finalizada
    /// </summary>
    event EventHandler<FishingCompletedEventArgs>? FishingCompleted;
    
    /// <summary>
    /// Inicia o sistema de pesca
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Para o sistema de pesca
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Inicia uma nova sessão de pesca
    /// </summary>
    Task<bool> StartFishingAsync(FishingZone zone);
    
    /// <summary>
    /// Cancela a pesca atual
    /// </summary>
    Task CancelFishingAsync();
    
    /// <summary>
    /// Processa um evento de pesca recebido do jogo
    /// </summary>
    Task ProcessFishingEventAsync(object eventData, string eventType);
    
    /// <summary>
    /// Obtém estatísticas da sessão atual
    /// </summary>
    FishingStats GetStats();
}

/// <summary>
/// Argumentos do evento de mudança de estado
/// </summary>
public class FishingStateChangedEventArgs : EventArgs
{
    public FishingState OldState { get; }
    public FishingState NewState { get; }
    public DateTime Timestamp { get; }
    
    public FishingStateChangedEventArgs(FishingState oldState, FishingState newState)
    {
        OldState = oldState;
        NewState = newState;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de zona detectada
/// </summary>
public class FishingZoneDetectedEventArgs : EventArgs
{
    public FishingZone Zone { get; }
    public DateTime Timestamp { get; }
    
    public FishingZoneDetectedEventArgs(FishingZone zone)
    {
        Zone = zone;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de bobber detectado
/// </summary>
public class BobberDetectedEventArgs : EventArgs
{
    public BobberInfo Bobber { get; }
    public DateTime Timestamp { get; }
    
    public BobberDetectedEventArgs(BobberInfo bobber)
    {
        Bobber = bobber;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de minigame iniciado
/// </summary>
public class MinigameStartedEventArgs : EventArgs
{
    public BobberInfo Bobber { get; }
    public DateTime Timestamp { get; }
    
    public MinigameStartedEventArgs(BobberInfo bobber)
    {
        Bobber = bobber;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de pesca completada
/// </summary>
public class FishingCompletedEventArgs : EventArgs
{
    public bool IsSuccessful { get; }
    public List<DiscoveredItem> CaughtItems { get; }
    public TimeSpan Duration { get; }
    public DateTime Timestamp { get; }
    
    public FishingCompletedEventArgs(bool isSuccessful, List<DiscoveredItem> caughtItems, TimeSpan duration)
    {
        IsSuccessful = isSuccessful;
        CaughtItems = caughtItems;
        Duration = duration;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Estatísticas de pesca
/// </summary>
public class FishingStats
{
    public int TotalAttempts { get; set; }
    public int SuccessfulCatches { get; set; }
    public int FailedAttempts { get; set; }
    public TimeSpan TotalFishingTime { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulCatches / TotalAttempts : 0;
    public TimeSpan AverageFishingTime { get; set; }
    public DateTime LastFishingTime { get; set; }
}
