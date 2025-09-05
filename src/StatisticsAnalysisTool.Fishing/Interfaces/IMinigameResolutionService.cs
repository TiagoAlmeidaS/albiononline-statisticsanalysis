using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Interfaces;

/// <summary>
/// Interface para resolução do minigame de pesca
/// </summary>
public interface IMinigameResolutionService
{
    /// <summary>
    /// Indica se o minigame está ativo
    /// </summary>
    bool IsMinigameActive { get; }
    
    /// <summary>
    /// Evento disparado quando o minigame inicia
    /// </summary>
    event EventHandler<MinigameStartedEventArgs>? MinigameStarted;
    
    /// <summary>
    /// Evento disparado quando o minigame é resolvido
    /// </summary>
    event EventHandler<MinigameResolvedEventArgs>? MinigameResolved;
    
    /// <summary>
    /// Evento disparado quando o minigame falha
    /// </summary>
    event EventHandler<MinigameFailedEventArgs>? MinigameFailed;
    
    /// <summary>
    /// Inicia a resolução do minigame
    /// </summary>
    Task StartMinigameResolutionAsync(BobberInfo bobber);
    
    /// <summary>
    /// Para a resolução do minigame
    /// </summary>
    Task StopMinigameResolutionAsync();
    
    /// <summary>
    /// Processa movimento do bobber durante o minigame
    /// </summary>
    Task ProcessBobberMovementAsync(float positionX, float positionY, double confidence);
    
    /// <summary>
    /// Executa ação de puxar a linha
    /// </summary>
    Task ExecutePullActionAsync();
    
    /// <summary>
    /// Obtém estatísticas do minigame
    /// </summary>
    MinigameStats GetStats();
}

/// <summary>
/// Argumentos do evento de minigame resolvido
/// </summary>
public class MinigameResolvedEventArgs : EventArgs
{
    public bool IsSuccessful { get; }
    public TimeSpan ResolutionTime { get; }
    public double FinalConfidence { get; }
    public DateTime Timestamp { get; }
    
    public MinigameResolvedEventArgs(bool isSuccessful, TimeSpan resolutionTime, double finalConfidence)
    {
        IsSuccessful = isSuccessful;
        ResolutionTime = resolutionTime;
        FinalConfidence = finalConfidence;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de minigame falhado
/// </summary>
public class MinigameFailedEventArgs : EventArgs
{
    public string Reason { get; }
    public TimeSpan Duration { get; }
    public DateTime Timestamp { get; }
    
    public MinigameFailedEventArgs(string reason, TimeSpan duration)
    {
        Reason = reason;
        Duration = duration;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Estatísticas do minigame
/// </summary>
public class MinigameStats
{
    public int TotalMinigames { get; set; }
    public int SuccessfulMinigames { get; set; }
    public int FailedMinigames { get; set; }
    public double SuccessRate => TotalMinigames > 0 ? (double)SuccessfulMinigames / TotalMinigames : 0;
    public TimeSpan AverageResolutionTime { get; set; }
    public TimeSpan FastestResolution { get; set; }
    public TimeSpan SlowestResolution { get; set; }
    public DateTime LastMinigameTime { get; set; }
}
