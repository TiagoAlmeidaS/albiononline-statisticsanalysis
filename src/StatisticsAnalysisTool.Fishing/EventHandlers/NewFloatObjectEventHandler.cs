using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Models;
using StatisticsAnalysisTool.Network;
using System.Drawing;

namespace StatisticsAnalysisTool.Fishing.EventHandlers;

/// <summary>
/// Handler para o evento NewFloatObject (bobber)
/// </summary>
public class NewFloatObjectEventHandler : EventPacketHandler<NewFloatObjectEvent>
{
    private readonly IFishingEngine _fishingEngine;
    private readonly IBobberDetectionService _bobberService;
    private readonly ILogger<NewFloatObjectEventHandler> _logger;
    
    public NewFloatObjectEventHandler(
        IFishingEngine fishingEngine,
        IBobberDetectionService bobberService,
        ILogger<NewFloatObjectEventHandler> logger) 
        : base(360) // NewFloatObject
    {
        _fishingEngine = fishingEngine;
        _bobberService = bobberService;
        _logger = logger;
    }
    
    protected override async Task OnActionAsync(NewFloatObjectEvent value)
    {
        try
        {
            _logger.LogInformation("NewFloatObject detectado: ObjectId={ObjectId}, Position=({X}, {Y})", 
                value.ObjectId, value.PositionX, value.PositionY);
            
            // Criar informações do bobber
            var bobberInfo = new BobberInfo
            {
                ObjectId = value.ObjectId,
                PositionX = value.PositionX,
                PositionY = value.PositionY,
                SpawnedAt = DateTime.UtcNow,
                IsDetected = true,
                Confidence = 1.0, // Confiança inicial alta para objetos detectados pelo jogo
                DetectionArea = new Rectangle(
                    (int)(value.PositionX - 25), 
                    (int)(value.PositionY - 25), 
                    50, 
                    50
                )
            };
            
            // Iniciar tracking do bobber
            await _bobberService.StartTrackingAsync(bobberInfo);
            
            // Processar evento no engine de pesca
            await _fishingEngine.ProcessFishingEventAsync(value, "NewFloatObject");
            
            _logger.LogInformation("NewFloatObject processado com sucesso: {ObjectId}", value.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar NewFloatObject: {ObjectId}", value.ObjectId);
        }
    }
}

/// <summary>
/// Evento de novo objeto flutuante (bobber)
/// </summary>
public class NewFloatObjectEvent
{
    public long ObjectId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string ObjectType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public NewFloatObjectEvent(Dictionary<byte, object> parameters)
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
            
            // ObjectType
            if (parameters.TryGetValue(2, out object objectType))
            {
                ObjectType = objectType.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Erro ao processar parâmetros do NewFloatObject: {ex.Message}", ex);
        }
    }
}
