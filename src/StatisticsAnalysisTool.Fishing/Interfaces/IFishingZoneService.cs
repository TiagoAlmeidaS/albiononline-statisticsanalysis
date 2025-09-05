using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Interfaces;

/// <summary>
/// Interface para gerenciamento de zonas de pesca
/// </summary>
public interface IFishingZoneService
{
    /// <summary>
    /// Zonas de pesca ativas
    /// </summary>
    IReadOnlyList<FishingZone> ActiveZones { get; }
    
    /// <summary>
    /// Evento disparado quando uma nova zona é detectada
    /// </summary>
    event EventHandler<FishingZoneDetectedEventArgs>? ZoneDetected;
    
    /// <summary>
    /// Evento disparado quando uma zona é removida
    /// </summary>
    event EventHandler<FishingZoneRemovedEventArgs>? ZoneRemoved;
    
    /// <summary>
    /// Adiciona uma nova zona de pesca
    /// </summary>
    Task<FishingZone> AddZoneAsync(long objectId, float positionX, float positionY, string zoneType);
    
    /// <summary>
    /// Remove uma zona de pesca
    /// </summary>
    Task RemoveZoneAsync(long objectId);
    
    /// <summary>
    /// Obtém a zona mais próxima de uma posição
    /// </summary>
    FishingZone? GetNearestZone(float positionX, float positionY);
    
    /// <summary>
    /// Verifica se uma posição está dentro de uma zona de pesca
    /// </summary>
    bool IsPositionInFishingZone(float positionX, float positionY);
    
    /// <summary>
    /// Limpa todas as zonas inativas
    /// </summary>
    Task CleanupInactiveZonesAsync();
    
    /// <summary>
    /// Obtém estatísticas das zonas
    /// </summary>
    FishingZoneStats GetStats();
}

/// <summary>
/// Argumentos do evento de zona removida
/// </summary>
public class FishingZoneRemovedEventArgs : EventArgs
{
    public long ObjectId { get; }
    public DateTime Timestamp { get; }
    
    public FishingZoneRemovedEventArgs(long objectId)
    {
        ObjectId = objectId;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Estatísticas das zonas de pesca
/// </summary>
public class FishingZoneStats
{
    public int TotalZonesDetected { get; set; }
    public int ActiveZones { get; set; }
    public int InactiveZones { get; set; }
    public TimeSpan AverageZoneLifetime { get; set; }
    public DateTime LastZoneDetected { get; set; }
}
