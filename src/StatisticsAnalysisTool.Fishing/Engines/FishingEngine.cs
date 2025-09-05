using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Models;
using StatisticsAnalysisTool.Fishing.Services;

namespace StatisticsAnalysisTool.Fishing.Engines;

/// <summary>
/// Engine principal de pesca que coordena todos os componentes
/// </summary>
public class FishingEngine : IFishingEngine
{
    private readonly ILogger<FishingEngine> _logger;
    private readonly IFishingZoneService _zoneService;
    private readonly IBobberDetectionService _bobberService;
    private readonly IMinigameResolutionService _minigameService;
    private readonly Interfaces.IAutomationService _automationService;
    private readonly FishingConfig _config;
    
    private FishingState _currentState = FishingState.IDLE;
    private FishingEvent? _activeFishingEvent;
    private bool _isActive;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public FishingState CurrentState => _currentState;
    public FishingEvent? ActiveFishingEvent => _activeFishingEvent;
    public bool IsActive => _isActive;
    
    public event EventHandler<FishingStateChangedEventArgs>? StateChanged;
    public event EventHandler<FishingZoneDetectedEventArgs>? ZoneDetected;
    public event EventHandler<Interfaces.BobberDetectedEventArgs>? BobberDetected;
    public event EventHandler<Interfaces.MinigameStartedEventArgs>? MinigameStarted;
    public event EventHandler<FishingCompletedEventArgs>? FishingCompleted;
    
    public FishingEngine(
        ILogger<FishingEngine> logger,
        IFishingZoneService zoneService,
        IBobberDetectionService bobberService,
        IMinigameResolutionService minigameService,
        Interfaces.IAutomationService automationService,
        FishingConfig config)
    {
        _logger = logger;
        _zoneService = zoneService;
        _bobberService = bobberService;
        _minigameService = minigameService;
        _automationService = automationService;
        _config = config;
        
        // Subscrever eventos dos serviços
        _zoneService.ZoneDetected += OnZoneDetected;
        _bobberService.BobberDetected += OnBobberDetected;
        _bobberService.MinigameMovementDetected += OnMinigameMovementDetected;
        _minigameService.MinigameResolved += OnMinigameResolved;
        _minigameService.MinigameFailed += OnMinigameFailed;
    }
    
    public async Task StartAsync()
    {
        if (_isActive)
        {
            _logger.LogWarning("FishingEngine já está ativo");
            return;
        }
        
        _logger.LogInformation("Iniciando FishingEngine");
        _isActive = true;
        await ChangeStateAsync(FishingState.IDLE);
    }
    
    public async Task StopAsync()
    {
        if (!_isActive)
        {
            _logger.LogWarning("FishingEngine já está parado");
            return;
        }
        
        _logger.LogInformation("Parando FishingEngine");
        _isActive = false;
        
        // Cancelar pesca ativa se houver
        if (_activeFishingEvent != null)
        {
            await CancelFishingAsync();
        }
        
        // Parar serviços
        await _bobberService.StopTrackingAsync();
        await _minigameService.StopMinigameResolutionAsync();
        
        await ChangeStateAsync(FishingState.IDLE);
    }
    
    public async Task<bool> StartFishingAsync(FishingZone zone)
    {
        if (!_isActive)
        {
            _logger.LogWarning("FishingEngine não está ativo");
            return false;
        }
        
        if (_activeFishingEvent != null)
        {
            _logger.LogWarning("Já existe uma pesca ativa");
            return false;
        }
        
        _logger.LogInformation("Iniciando pesca na zona {ZoneId} em ({X}, {Y})", 
            zone.ObjectId, zone.PositionX, zone.PositionY);
        
        // Criar evento de pesca
        _activeFishingEvent = new FishingEvent
        {
            EventId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            FishingRodItemIndex = 0, // Será definido pelo evento FishingStart
            CurrentState = FishingState.CASTING,
            StartedAt = DateTime.UtcNow,
            ActiveZone = zone
        };
        
        await ChangeStateAsync(FishingState.CASTING);
        
        // Executar cast
        try
        {
            await ExecuteCastAsync(zone);
            await ChangeStateAsync(FishingState.CAST);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar cast na zona {ZoneId}", zone.ObjectId);
            await CancelFishingAsync();
            return false;
        }
    }
    
    public async Task CancelFishingAsync()
    {
        if (_activeFishingEvent == null)
        {
            _logger.LogWarning("Nenhuma pesca ativa para cancelar");
            return;
        }
        
        _logger.LogInformation("Cancelando pesca {EventId}", _activeFishingEvent.EventId);
        
        // Parar serviços
        await _bobberService.StopTrackingAsync();
        await _minigameService.StopMinigameResolutionAsync();
        
        // Limpar evento ativo
        _activeFishingEvent = null;
        
        await ChangeStateAsync(FishingState.CANCELLED);
        await ChangeStateAsync(FishingState.IDLE);
    }
    
    public async Task ProcessFishingEventAsync(object eventData, string eventType)
    {
        if (!_isActive)
            return;
        
        _logger.LogDebug("Processando evento de pesca: {EventType}", eventType);
        
        try
        {
            switch (eventType)
            {
                case "FishingStart":
                    await ProcessFishingStartEventAsync(eventData);
                    break;
                case "FishingCast":
                    await ProcessFishingCastEventAsync(eventData);
                    break;
                case "FishingCatch":
                    await ProcessFishingCatchEventAsync(eventData);
                    break;
                case "FishingFinished":
                    await ProcessFishingFinishedEventAsync(eventData);
                    break;
                case "FishingCancel":
                    await ProcessFishingCancelEventAsync(eventData);
                    break;
                case "NewFloatObject":
                    await ProcessNewFloatObjectEventAsync(eventData);
                    break;
                case "NewFishingZoneObject":
                    await ProcessNewFishingZoneObjectEventAsync(eventData);
                    break;
                case "FishingMiniGame":
                    await ProcessFishingMiniGameEventAsync(eventData);
                    break;
                default:
                    _logger.LogDebug("Evento de pesca não reconhecido: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento de pesca: {EventType}", eventType);
        }
    }
    
    public FishingStats GetStats()
    {
        return new FishingStats
        {
            TotalAttempts = 0, // Será implementado com persistência
            SuccessfulCatches = 0,
            FailedAttempts = 0,
            TotalFishingTime = TimeSpan.Zero,
            AverageFishingTime = TimeSpan.Zero,
            LastFishingTime = DateTime.MinValue
        };
    }
    
    private async Task ChangeStateAsync(FishingState newState)
    {
        if (_currentState == newState)
            return;
        
        var oldState = _currentState;
        _currentState = newState;
        
        if (_activeFishingEvent != null)
        {
            _activeFishingEvent.CurrentState = newState;
        }
        
        _logger.LogInformation("Estado da pesca mudou: {OldState} -> {NewState}", oldState, newState);
        
        StateChanged?.Invoke(this, new FishingStateChangedEventArgs(oldState, newState));
    }
    
    private async Task ExecuteCastAsync(FishingZone zone)
    {
        _logger.LogInformation("Executando cast na zona {ZoneId}", zone.ObjectId);
        
        // Calcular posição para clicar
        var clickX = (int)(zone.PositionX + zone.DetectionArea.Width / 2);
        var clickY = (int)(zone.PositionY + zone.DetectionArea.Height / 2);
        
        // Executar clique com delay
        await _automationService.ClickAtAsync(clickX, clickY, "left");
        await Task.Delay(500); // Aguardar animação
    }
    
    private async Task ProcessFishingStartEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingStart recebido");
        // Implementar processamento do evento FishingStart
    }
    
    private async Task ProcessFishingCastEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingCast recebido");
        // Implementar processamento do evento FishingCast
    }
    
    private async Task ProcessFishingCatchEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingCatch recebido");
        // Implementar processamento do evento FishingCatch
    }
    
    private async Task ProcessFishingFinishedEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingFinished recebido");
        // Implementar processamento do evento FishingFinished
    }
    
    private async Task ProcessFishingCancelEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingCancel recebido");
        await CancelFishingAsync();
    }
    
    private async Task ProcessNewFloatObjectEventAsync(object eventData)
    {
        _logger.LogInformation("Evento NewFloatObject recebido");
        // Implementar processamento do evento NewFloatObject
    }
    
    private async Task ProcessNewFishingZoneObjectEventAsync(object eventData)
    {
        _logger.LogInformation("Evento NewFishingZoneObject recebido");
        // Implementar processamento do evento NewFishingZoneObject
    }
    
    private async Task ProcessFishingMiniGameEventAsync(object eventData)
    {
        _logger.LogInformation("Evento FishingMiniGame recebido");
        // Implementar processamento do evento FishingMiniGame
    }
    
    private void OnZoneDetected(object? sender, FishingZoneDetectedEventArgs e)
    {
        _logger.LogInformation("Zona de pesca detectada: {ZoneId}", e.Zone.ObjectId);
        ZoneDetected?.Invoke(this, e);
    }
    
    private void OnBobberDetected(object? sender, Interfaces.BobberDetectedEventArgs e)
    {
        _logger.LogInformation("Bobber detectado: {BobberId}", e.Bobber.ObjectId);
        BobberDetected?.Invoke(this, e);
        
        if (_activeFishingEvent != null)
        {
            _activeFishingEvent.ActiveBobber = e.Bobber;
        }
    }
    
    private void OnMinigameMovementDetected(object? sender, MinigameMovementDetectedEventArgs e)
    {
        _logger.LogInformation("Movimento de minigame detectado: confiança {Confidence}", e.Confidence);
        MinigameStarted?.Invoke(this, new Interfaces.MinigameStartedEventArgs(e.Bobber));
    }
    
    private void OnMinigameResolved(object? sender, MinigameResolvedEventArgs e)
    {
        _logger.LogInformation("Minigame resolvido: sucesso {Success}", e.IsSuccessful);
        // Implementar lógica de resolução do minigame
    }
    
    private void OnMinigameFailed(object? sender, MinigameFailedEventArgs e)
    {
        _logger.LogWarning("Minigame falhou: {Reason}", e.Reason);
        // Implementar lógica de falha do minigame
    }
}
