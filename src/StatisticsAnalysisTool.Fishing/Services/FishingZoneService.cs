using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Models;
using System.Drawing;

namespace StatisticsAnalysisTool.Fishing.Services;

/// <summary>
/// Serviço para gerenciamento de zonas de pesca
/// </summary>
public class FishingZoneService : IFishingZoneService
{
    private readonly ILogger<FishingZoneService> _logger;
    private readonly List<FishingZone> _zones = new();
    private readonly object _lock = new();
    
    public IReadOnlyList<FishingZone> ActiveZones
    {
        get
        {
            lock (_lock)
            {
                return _zones.Where(z => z.IsActive).ToList().AsReadOnly();
            }
        }
    }
    
    public event EventHandler<FishingZoneDetectedEventArgs>? ZoneDetected;
    public event EventHandler<FishingZoneRemovedEventArgs>? ZoneRemoved;
    
    public FishingZoneService(ILogger<FishingZoneService> logger)
    {
        _logger = logger;
    }
    
    public async Task<FishingZone> AddZoneAsync(long objectId, float positionX, float positionY, string zoneType)
    {
        _logger.LogInformation("Adicionando zona de pesca: {ObjectId} em ({X}, {Y}) tipo {ZoneType}", 
            objectId, positionX, positionY, zoneType);
        
        var zone = new FishingZone
        {
            ObjectId = objectId,
            PositionX = positionX,
            PositionY = positionY,
            ZoneType = zoneType,
            DetectedAt = DateTime.UtcNow,
            IsActive = true,
            DetectionArea = new Rectangle(
                (int)(positionX - 50), 
                (int)(positionY - 50), 
                100, 
                100
            )
        };
        
        lock (_lock)
        {
            // Remover zona existente com mesmo ID se houver
            _zones.RemoveAll(z => z.ObjectId == objectId);
            _zones.Add(zone);
        }
        
        ZoneDetected?.Invoke(this, new FishingZoneDetectedEventArgs(zone));
        
        return zone;
    }
    
    public async Task RemoveZoneAsync(long objectId)
    {
        _logger.LogInformation("Removendo zona de pesca: {ObjectId}", objectId);
        
        FishingZone? removedZone = null;
        lock (_lock)
        {
            removedZone = _zones.FirstOrDefault(z => z.ObjectId == objectId);
            if (removedZone != null)
            {
                _zones.Remove(removedZone);
            }
        }
        
        if (removedZone != null)
        {
            ZoneRemoved?.Invoke(this, new FishingZoneRemovedEventArgs(objectId));
        }
    }
    
    public FishingZone? GetNearestZone(float positionX, float positionY)
    {
        lock (_lock)
        {
            return _zones
                .Where(z => z.IsActive)
                .OrderBy(z => CalculateDistance(positionX, positionY, z.PositionX, z.PositionY))
                .FirstOrDefault();
        }
    }
    
    public bool IsPositionInFishingZone(float positionX, float positionY)
    {
        lock (_lock)
        {
            return _zones.Any(z => z.IsActive && z.DetectionArea.Contains((int)positionX, (int)positionY));
        }
    }
    
    public async Task CleanupInactiveZonesAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-10); // Zonas inativas há mais de 10 minutos
        
        List<FishingZone> zonesToRemove;
        lock (_lock)
        {
            zonesToRemove = _zones.Where(z => !z.IsActive && z.DetectedAt < cutoffTime).ToList();
            foreach (var zone in zonesToRemove)
            {
                _zones.Remove(zone);
            }
        }
        
        foreach (var zone in zonesToRemove)
        {
            _logger.LogDebug("Removendo zona inativa: {ObjectId}", zone.ObjectId);
            ZoneRemoved?.Invoke(this, new FishingZoneRemovedEventArgs(zone.ObjectId));
        }
        
        if (zonesToRemove.Count > 0)
        {
            _logger.LogInformation("Removidas {Count} zonas inativas", zonesToRemove.Count);
        }
    }
    
    public FishingZoneStats GetStats()
    {
        lock (_lock)
        {
            var activeZones = _zones.Count(z => z.IsActive);
            var inactiveZones = _zones.Count(z => !z.IsActive);
            var lastDetected = _zones.Any() ? _zones.Max(z => z.DetectedAt) : DateTime.MinValue;
            
            return new FishingZoneStats
            {
                TotalZonesDetected = _zones.Count,
                ActiveZones = activeZones,
                InactiveZones = inactiveZones,
                AverageZoneLifetime = CalculateAverageLifetime(),
                LastZoneDetected = lastDetected
            };
        }
    }
    
    private double CalculateDistance(float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    private TimeSpan CalculateAverageLifetime()
    {
        if (!_zones.Any())
            return TimeSpan.Zero;
        
        var now = DateTime.UtcNow;
        var totalLifetime = _zones.Sum(z => (now - z.DetectedAt).TotalMilliseconds);
        return TimeSpan.FromMilliseconds(totalLifetime / _zones.Count);
    }
}
