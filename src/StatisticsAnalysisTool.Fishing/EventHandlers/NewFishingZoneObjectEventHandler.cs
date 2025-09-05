using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Network;

namespace StatisticsAnalysisTool.Fishing.EventHandlers;

/// <summary>
/// Handler para o evento NewFishingZoneObject
/// </summary>
public class NewFishingZoneObjectEventHandler : EventPacketHandler<NewFishingZoneObjectEvent>
{
    private readonly IFishingZoneService _fishingZoneService;
    private readonly ILogger<NewFishingZoneObjectEventHandler> _logger;
    
    public NewFishingZoneObjectEventHandler(
        IFishingZoneService fishingZoneService,
        ILogger<NewFishingZoneObjectEventHandler> logger) 
        : base(361) // NewFishingZoneObject
    {
        _fishingZoneService = fishingZoneService;
        _logger = logger;
    }
    
    protected override async Task OnActionAsync(NewFishingZoneObjectEvent value)
    {
        try
        {
            _logger.LogInformation("NewFishingZoneObject detectado: ObjectId={ObjectId}, Position=({X}, {Y}), ZoneType={ZoneType}", 
                value.ObjectId, value.PositionX, value.PositionY, value.ZoneType);
            
            // Adicionar zona de pesca
            await _fishingZoneService.AddZoneAsync(
                value.ObjectId, 
                value.PositionX, 
                value.PositionY, 
                value.ZoneType
            );
            
            _logger.LogInformation("Zona de pesca adicionada com sucesso: {ObjectId}", value.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar NewFishingZoneObject: {ObjectId}", value.ObjectId);
        }
    }
}

/// <summary>
/// Evento de nova zona de pesca
/// </summary>
public class NewFishingZoneObjectEvent
{
    public long ObjectId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string ZoneType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public NewFishingZoneObjectEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            // ObjectId
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = Convert.ToInt64(objectId);
            }
            
            // Position
            if (parameters.TryGetValue(1, out object position))
            {
                if (position is float[] positionArray && positionArray.Length >= 2)
                {
                    PositionX = positionArray[0];
                    PositionY = positionArray[1];
                }
            }
            
            // ZoneType
            if (parameters.TryGetValue(2, out object zoneType))
            {
                ZoneType = zoneType.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Erro ao processar parâmetros do NewFishingZoneObject: {ex.Message}", ex);
        }
    }
}
