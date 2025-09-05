using System.Drawing;

namespace StatisticsAnalysisTool.Fishing.Models;

/// <summary>
/// Estados possíveis do sistema de pesca
/// </summary>
public enum FishingState
{
    /// <summary>
    /// Sistema em estado ocioso, aguardando início
    /// </summary>
    IDLE,
    
    /// <summary>
    /// Zona de pesca detectada e disponível
    /// </summary>
    ZONE_DETECTED,
    
    /// <summary>
    /// Lançando a linha de pesca
    /// </summary>
    CASTING,
    
    /// <summary>
    /// Linha lançada, aguardando bobber
    /// </summary>
    CAST,
    
    /// <summary>
    /// Monitorando posição e movimento do bobber
    /// </summary>
    TRACKING,
    
    /// <summary>
    /// Resolvendo minigame de pesca
    /// </summary>
    MINIGAME,
    
    /// <summary>
    /// Puxando a linha após detecção
    /// </summary>
    CATCHING,
    
    /// <summary>
    /// Pesca bem-sucedida
    /// </summary>
    SUCCESS,
    
    /// <summary>
    /// Pesca falhou
    /// </summary>
    FAILED,
    
    /// <summary>
    /// Pesca cancelada pelo usuário
    /// </summary>
    CANCELLED
}

/// <summary>
/// Informações sobre uma zona de pesca
/// </summary>
public class FishingZone
{
    public long ObjectId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string ZoneType { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public bool IsActive { get; set; }
    public Rectangle DetectionArea { get; set; }
}

/// <summary>
/// Informações sobre o bobber
/// </summary>
public class BobberInfo
{
    public long ObjectId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public DateTime SpawnedAt { get; set; }
    public bool IsDetected { get; set; }
    public double Confidence { get; set; }
    public Rectangle DetectionArea { get; set; }
}

/// <summary>
/// Evento de pesca ativo
/// </summary>
public class FishingEvent
{
    public long EventId { get; set; }
    public int FishingRodItemIndex { get; set; }
    public FishingState CurrentState { get; set; }
    public DateTime StartedAt { get; set; }
    public FishingZone? ActiveZone { get; set; }
    public BobberInfo? ActiveBobber { get; set; }
    public bool IsSuccessful { get; set; }
    public List<DiscoveredItem> CaughtItems { get; set; } = new();
}

/// <summary>
/// Configurações do sistema de pesca
/// </summary>
public class FishingConfig
{
    public bool EnableAutoFishing { get; set; } = true;
    public bool EnableMinigameResolution { get; set; } = true;
    public double BobberDetectionThreshold { get; set; } = 0.7;
    public int MaxFishingAttempts { get; set; } = 10;
    public TimeSpan FishingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan MinigameTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableDebugMode { get; set; } = false;
    public string BobberTemplatePath { get; set; } = "data/images/bobber.png";
    public string BobberInWaterTemplatePath { get; set; } = "data/images/bobber_in_water.png";
}
