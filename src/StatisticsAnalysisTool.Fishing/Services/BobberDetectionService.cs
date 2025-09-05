using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Services;

/// <summary>
/// Serviço para detecção e tracking do bobber
/// </summary>
public class BobberDetectionService : IBobberDetectionService
{
    private readonly ILogger<BobberDetectionService> _logger;
    private readonly FishingConfig _config;
    
    private BobberInfo? _activeBobber;
    private bool _isTracking;
    private DateTime _trackingStartTime;
    private readonly List<(DateTime timestamp, float x, float y)> _positionHistory = new();
    
    public BobberInfo? ActiveBobber => _activeBobber;
    public bool IsTracking => _isTracking;
    
    public event EventHandler<Interfaces.BobberDetectedEventArgs>? BobberDetected;
    public event EventHandler<BobberMovedEventArgs>? BobberMoved;
    public event EventHandler<BobberLostEventArgs>? BobberLost;
    public event EventHandler<MinigameMovementDetectedEventArgs>? MinigameMovementDetected;
    
    public BobberDetectionService(ILogger<BobberDetectionService> logger, FishingConfig config)
    {
        _logger = logger;
        _config = config;
    }
    
    public async Task StartTrackingAsync(BobberInfo bobber)
    {
        if (_isTracking)
        {
            _logger.LogWarning("Já está fazendo tracking de um bobber");
            return;
        }
        
        _logger.LogInformation("Iniciando tracking do bobber: {ObjectId} em ({X}, {Y})", 
            bobber.ObjectId, bobber.PositionX, bobber.PositionY);
        
        _activeBobber = bobber;
        _isTracking = true;
        _trackingStartTime = DateTime.UtcNow;
        _positionHistory.Clear();
        
        // Adicionar posição inicial ao histórico
        _positionHistory.Add((DateTime.UtcNow, bobber.PositionX, bobber.PositionY));
        
        BobberDetected?.Invoke(this, new Interfaces.BobberDetectedEventArgs(bobber));
    }
    
    public async Task StopTrackingAsync()
    {
        if (!_isTracking)
        {
            _logger.LogWarning("Não está fazendo tracking de nenhum bobber");
            return;
        }
        
        var objectId = _activeBobber?.ObjectId ?? 0;
        _logger.LogInformation("Parando tracking do bobber: {ObjectId}", objectId);
        
        _isTracking = false;
        _activeBobber = null;
        _positionHistory.Clear();
        
        if (objectId > 0)
        {
            BobberLost?.Invoke(this, new BobberLostEventArgs(objectId));
        }
    }
    
    public async Task UpdateBobberPositionAsync(long objectId, float positionX, float positionY)
    {
        if (!_isTracking || _activeBobber?.ObjectId != objectId)
        {
            return;
        }
        
        var oldX = _activeBobber.PositionX;
        var oldY = _activeBobber.PositionY;
        
        // Verificar se houve movimento significativo
        var distance = CalculateDistance(oldX, oldY, positionX, positionY);
        if (distance < 2.0f) // Threshold de 2 pixels
        {
            return;
        }
        
        _logger.LogDebug("Bobber movido: ({OldX}, {OldY}) -> ({NewX}, {NewY})", 
            oldX, oldY, positionX, positionY);
        
        _activeBobber.PositionX = positionX;
        _activeBobber.PositionY = positionY;
        
        // Adicionar ao histórico
        _positionHistory.Add((DateTime.UtcNow, positionX, positionY));
        
        // Manter apenas as últimas 50 posições
        if (_positionHistory.Count > 50)
        {
            _positionHistory.RemoveAt(0);
        }
        
        BobberMoved?.Invoke(this, new BobberMovedEventArgs(_activeBobber, oldX, oldY, positionX, positionY));
        
        // Verificar se é movimento de minigame
        await CheckForMinigameMovementAsync();
    }
    
    public async Task<bool> DetectMinigameMovementAsync()
    {
        if (!_isTracking || _activeBobber == null || _positionHistory.Count < 3)
        {
            return false;
        }
        
        // Analisar padrão de movimento recente
        var recentPositions = _positionHistory.TakeLast(5).ToList();
        if (recentPositions.Count < 3)
        {
            return false;
        }
        
        // Calcular velocidade e aceleração
        var velocities = new List<float>();
        for (int i = 1; i < recentPositions.Count; i++)
        {
            var prev = recentPositions[i - 1];
            var curr = recentPositions[i];
            var timeDiff = (float)(curr.timestamp - prev.timestamp).TotalSeconds;
            
            if (timeDiff > 0)
            {
                var velocity = CalculateDistance(prev.x, prev.y, curr.x, curr.y) / timeDiff;
                velocities.Add(velocity);
            }
        }
        
        if (velocities.Count < 2)
        {
            return false;
        }
        
        // Verificar se há movimento rápido e irregular (característico de minigame)
        var avgVelocity = velocities.Average();
        var maxVelocity = velocities.Max();
        var velocityVariance = CalculateVariance(velocities);
        
        // Critérios para detectar minigame:
        // 1. Velocidade média alta
        // 2. Picos de velocidade
        // 3. Alta variância (movimento irregular)
        var isMinigameMovement = avgVelocity > 10.0f && 
                                maxVelocity > 20.0f && 
                                velocityVariance > 50.0f;
        
        if (isMinigameMovement)
        {
            _logger.LogInformation("Movimento de minigame detectado: velocidade média {AvgVel}, máxima {MaxVel}, variância {Variance}", 
                avgVelocity, maxVelocity, velocityVariance);
            
            var confidence = Math.Min(1.0, (avgVelocity / 20.0f + maxVelocity / 40.0f + velocityVariance / 100.0f) / 3.0);
            
            MinigameMovementDetected?.Invoke(this, new MinigameMovementDetectedEventArgs(
                _activeBobber, 
                _activeBobber.PositionX, 
                _activeBobber.PositionY, 
                confidence
            ));
            
            return true;
        }
        
        return false;
    }
    
    public (float x, float y)? GetCurrentBobberPosition()
    {
        if (!_isTracking || _activeBobber == null)
        {
            return null;
        }
        
        return (_activeBobber.PositionX, _activeBobber.PositionY);
    }
    
    public BobberTrackingStats GetStats()
    {
        return new BobberTrackingStats
        {
            TotalBobbersTracked = 0, // Será implementado com persistência
            SuccessfulMinigames = 0,
            FailedMinigames = 0,
            AverageTrackingTime = _isTracking ? DateTime.UtcNow - _trackingStartTime : TimeSpan.Zero,
            AverageDetectionConfidence = _activeBobber?.Confidence ?? 0,
            LastTrackingTime = _isTracking ? DateTime.UtcNow : DateTime.MinValue
        };
    }
    
    private async Task CheckForMinigameMovementAsync()
    {
        await DetectMinigameMovementAsync();
    }
    
    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    private float CalculateVariance(List<float> values)
    {
        if (values.Count < 2)
            return 0;
        
        var mean = values.Average();
        var sumSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));
        return sumSquaredDiffs / (values.Count - 1);
    }
}
