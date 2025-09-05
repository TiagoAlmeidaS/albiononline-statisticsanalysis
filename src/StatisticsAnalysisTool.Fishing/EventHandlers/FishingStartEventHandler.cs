using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Network;

namespace StatisticsAnalysisTool.Fishing.EventHandlers;

/// <summary>
/// Handler para o evento FishingStart
/// </summary>
public class FishingStartEventHandler : EventPacketHandler<FishingStartEvent>
{
    private readonly IFishingEngine _fishingEngine;
    private readonly ILogger<FishingStartEventHandler> _logger;
    
    public FishingStartEventHandler(
        IFishingEngine fishingEngine,
        ILogger<FishingStartEventHandler> logger) 
        : base(355) // FishingStart
    {
        _fishingEngine = fishingEngine;
        _logger = logger;
    }
    
    protected override async Task OnActionAsync(FishingStartEvent value)
    {
        try
        {
            _logger.LogInformation("FishingStart detectado: EventId={EventId}, FishingRod={FishingRod}", 
                value.EventId, value.FishingRodItemIndex);
            
            // Processar evento no engine de pesca
            await _fishingEngine.ProcessFishingEventAsync(value, "FishingStart");
            
            _logger.LogInformation("FishingStart processado com sucesso: {EventId}", value.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar FishingStart: {EventId}", value.EventId);
        }
    }
}

/// <summary>
/// Evento de início de pesca
/// </summary>
public class FishingStartEvent
{
    public long EventId { get; set; }
    public int FishingRodItemIndex { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public FishingStartEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            // EventId
            if (parameters.TryGetValue(0, out object eventId))
            {
                EventId = Convert.ToInt64(eventId);
            }
            
            // FishingRodItemIndex
            if (parameters.TryGetValue(1, out object fishingRod))
            {
                FishingRodItemIndex = Convert.ToInt32(fishingRod);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Erro ao processar parâmetros do FishingStart: {ex.Message}", ex);
        }
    }
}
