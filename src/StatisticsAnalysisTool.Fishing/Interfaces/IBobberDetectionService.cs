using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Interfaces;

/// <summary>
/// Interface para detecção e tracking do bobber
/// </summary>
public interface IBobberDetectionService
{
    /// <summary>
    /// Bobber ativo sendo monitorado
    /// </summary>
    BobberInfo? ActiveBobber { get; }
    
    /// <summary>
    /// Indica se está fazendo tracking do bobber
    /// </summary>
    bool IsTracking { get; }
    
    /// <summary>
    /// Evento disparado quando o bobber é detectado
    /// </summary>
    event EventHandler<BobberDetectedEventArgs>? BobberDetected;
    
    /// <summary>
    /// Evento disparado quando o bobber se move
    /// </summary>
    event EventHandler<BobberMovedEventArgs>? BobberMoved;
    
    /// <summary>
    /// Evento disparado quando o bobber desaparece
    /// </summary>
    event EventHandler<BobberLostEventArgs>? BobberLost;
    
    /// <summary>
    /// Evento disparado quando movimento de minigame é detectado
    /// </summary>
    event EventHandler<MinigameMovementDetectedEventArgs>? MinigameMovementDetected;
    
    /// <summary>
    /// Inicia o tracking do bobber
    /// </summary>
    Task StartTrackingAsync(BobberInfo bobber);
    
    /// <summary>
    /// Para o tracking do bobber
    /// </summary>
    Task StopTrackingAsync();
    
    /// <summary>
    /// Atualiza a posição do bobber
    /// </summary>
    Task UpdateBobberPositionAsync(long objectId, float positionX, float positionY);
    
    /// <summary>
    /// Detecta movimento do bobber para minigame
    /// </summary>
    Task<bool> DetectMinigameMovementAsync();
    
    /// <summary>
    /// Obtém a posição atual do bobber
    /// </summary>
    (float x, float y)? GetCurrentBobberPosition();
    
    /// <summary>
    /// Obtém estatísticas de tracking
    /// </summary>
    BobberTrackingStats GetStats();
}

/// <summary>
/// Argumentos do evento de movimento do bobber
/// </summary>
public class BobberMovedEventArgs : EventArgs
{
    public BobberInfo Bobber { get; }
    public float OldPositionX { get; }
    public float OldPositionY { get; }
    public float NewPositionX { get; }
    public float NewPositionY { get; }
    public DateTime Timestamp { get; }
    
    public BobberMovedEventArgs(BobberInfo bobber, float oldX, float oldY, float newX, float newY)
    {
        Bobber = bobber;
        OldPositionX = oldX;
        OldPositionY = oldY;
        NewPositionX = newX;
        NewPositionY = newY;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de bobber perdido
/// </summary>
public class BobberLostEventArgs : EventArgs
{
    public long ObjectId { get; }
    public DateTime Timestamp { get; }
    
    public BobberLostEventArgs(long objectId)
    {
        ObjectId = objectId;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de movimento de minigame detectado
/// </summary>
public class MinigameMovementDetectedEventArgs : EventArgs
{
    public BobberInfo Bobber { get; }
    public float MovementX { get; }
    public float MovementY { get; }
    public double Confidence { get; }
    public DateTime Timestamp { get; }
    
    public MinigameMovementDetectedEventArgs(BobberInfo bobber, float movementX, float movementY, double confidence)
    {
        Bobber = bobber;
        MovementX = movementX;
        MovementY = movementY;
        Confidence = confidence;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Estatísticas de tracking do bobber
/// </summary>
public class BobberTrackingStats
{
    public int TotalBobbersTracked { get; set; }
    public int SuccessfulMinigames { get; set; }
    public int FailedMinigames { get; set; }
    public TimeSpan AverageTrackingTime { get; set; }
    public double AverageDetectionConfidence { get; set; }
    public DateTime LastTrackingTime { get; set; }
}
