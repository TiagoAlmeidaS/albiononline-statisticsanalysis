using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Models;

namespace StatisticsAnalysisTool.Fishing.Services;

/// <summary>
/// Serviço para resolução do minigame de pesca
/// </summary>
public class MinigameResolutionService : IMinigameResolutionService
{
    private readonly ILogger<MinigameResolutionService> _logger;
    private readonly IAutomationService _automationService;
    private readonly FishingConfig _config;
    
    private bool _isMinigameActive;
    private BobberInfo? _activeBobber;
    private DateTime _minigameStartTime;
    private readonly List<(DateTime timestamp, float x, float y, double confidence)> _movementHistory = new();
    
    public bool IsMinigameActive => _isMinigameActive;
    
    public event EventHandler<Interfaces.MinigameStartedEventArgs>? MinigameStarted;
    public event EventHandler<MinigameResolvedEventArgs>? MinigameResolved;
    public event EventHandler<MinigameFailedEventArgs>? MinigameFailed;
    
    public MinigameResolutionService(
        ILogger<MinigameResolutionService> logger,
        IAutomationService automationService,
        FishingConfig config)
    {
        _logger = logger;
        _automationService = automationService;
        _config = config;
    }
    
    public async Task StartMinigameResolutionAsync(BobberInfo bobber)
    {
        if (_isMinigameActive)
        {
            _logger.LogWarning("Minigame já está ativo");
            return;
        }
        
        _logger.LogInformation("Iniciando resolução do minigame para bobber: {ObjectId}", bobber.ObjectId);
        
        _isMinigameActive = true;
        _activeBobber = bobber;
        _minigameStartTime = DateTime.UtcNow;
        _movementHistory.Clear();
        
        MinigameStarted?.Invoke(this, new Interfaces.MinigameStartedEventArgs(bobber));
        
        // Iniciar timeout do minigame
        _ = Task.Run(async () =>
        {
            await Task.Delay(_config.MinigameTimeout);
            if (_isMinigameActive)
            {
                await StopMinigameResolutionAsync();
                MinigameFailed?.Invoke(this, new MinigameFailedEventArgs("Timeout", DateTime.UtcNow - _minigameStartTime));
            }
        });
    }
    
    public async Task StopMinigameResolutionAsync()
    {
        if (!_isMinigameActive)
        {
            _logger.LogWarning("Minigame não está ativo");
            return;
        }
        
        _logger.LogInformation("Parando resolução do minigame");
        
        _isMinigameActive = false;
        _activeBobber = null;
        _movementHistory.Clear();
    }
    
    public async Task ProcessBobberMovementAsync(float positionX, float positionY, double confidence)
    {
        if (!_isMinigameActive || _activeBobber == null)
        {
            return;
        }
        
        // Adicionar movimento ao histórico
        _movementHistory.Add((DateTime.UtcNow, positionX, positionY, confidence));
        
        // Manter apenas os últimos 20 movimentos
        if (_movementHistory.Count > 20)
        {
            _movementHistory.RemoveAt(0);
        }
        
        // Analisar padrão de movimento para decidir quando puxar
        var shouldPull = await AnalyzeMovementPatternAsync();
        
        if (shouldPull)
        {
            _logger.LogInformation("Executando ação de puxar baseada no padrão de movimento");
            await ExecutePullActionAsync();
        }
    }
    
    public async Task ExecutePullActionAsync()
    {
        if (!_isMinigameActive)
        {
            _logger.LogWarning("Tentativa de executar pull sem minigame ativo");
            return;
        }
        
        _logger.LogInformation("Executando ação de puxar a linha");
        
        try
        {
            // Simular clique para puxar a linha
            // Em uma implementação real, isso seria coordenado com o sistema de automação
            _automationService.MouseClick("left");
            
            // Aguardar um pouco para a animação
            await Task.Delay(200);
            
            // Parar o minigame
            await StopMinigameResolutionAsync();
            
            // Determinar sucesso baseado no histórico de movimento
            var success = DetermineSuccess();
            var resolutionTime = DateTime.UtcNow - _minigameStartTime;
            var finalConfidence = _movementHistory.Any() ? _movementHistory.Average(m => m.confidence) : 0;
            
            MinigameResolved?.Invoke(this, new MinigameResolvedEventArgs(success, resolutionTime, finalConfidence));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar ação de puxar");
            await StopMinigameResolutionAsync();
            MinigameFailed?.Invoke(this, new MinigameFailedEventArgs($"Erro na execução: {ex.Message}", DateTime.UtcNow - _minigameStartTime));
        }
    }
    
    public Interfaces.MinigameStats GetStats()
    {
        return new Interfaces.MinigameStats
        {
            TotalMinigames = 0, // Será implementado com persistência
            SuccessfulMinigames = 0,
            FailedMinigames = 0,
            AverageResolutionTime = TimeSpan.Zero,
            FastestResolution = TimeSpan.Zero,
            SlowestResolution = TimeSpan.Zero,
            LastMinigameTime = DateTime.MinValue
        };
    }
    
    private async Task<bool> AnalyzeMovementPatternAsync()
    {
        if (_movementHistory.Count < 5)
        {
            return false;
        }
        
        // Analisar os últimos movimentos
        var recentMovements = _movementHistory.TakeLast(5).ToList();
        
        // Calcular métricas de movimento
        var avgConfidence = recentMovements.Average(m => m.confidence);
        var movementIntensity = CalculateMovementIntensity(recentMovements);
        var movementConsistency = CalculateMovementConsistency(recentMovements);
        
        // Critérios para decidir puxar:
        // 1. Confiança alta nos últimos movimentos
        // 2. Intensidade de movimento adequada
        // 3. Consistência no padrão de movimento
        var shouldPull = avgConfidence > 0.7 && 
                        movementIntensity > 0.5 && 
                        movementConsistency > 0.6;
        
        _logger.LogDebug("Análise de movimento: confiança={Confidence:F2}, intensidade={Intensity:F2}, consistência={Consistency:F2}, puxar={ShouldPull}", 
            avgConfidence, movementIntensity, movementConsistency, shouldPull);
        
        return shouldPull;
    }
    
    private double CalculateMovementIntensity(List<(DateTime timestamp, float x, float y, double confidence)> movements)
    {
        if (movements.Count < 2)
            return 0;
        
        var totalDistance = 0.0;
        for (int i = 1; i < movements.Count; i++)
        {
            var prev = movements[i - 1];
            var curr = movements[i];
            var distance = Math.Sqrt(Math.Pow(curr.x - prev.x, 2) + Math.Pow(curr.y - prev.y, 2));
            totalDistance += distance;
        }
        
        return totalDistance / (movements.Count - 1);
    }
    
    private double CalculateMovementConsistency(List<(DateTime timestamp, float x, float y, double confidence)> movements)
    {
        if (movements.Count < 3)
            return 0;
        
        // Calcular consistência baseada na variação da direção do movimento
        var directions = new List<double>();
        for (int i = 1; i < movements.Count; i++)
        {
            var prev = movements[i - 1];
            var curr = movements[i];
            var direction = Math.Atan2(curr.y - prev.y, curr.x - prev.x);
            directions.Add(direction);
        }
        
        // Calcular variância das direções
        var meanDirection = directions.Average();
        var variance = directions.Sum(d => Math.Pow(d - meanDirection, 2)) / directions.Count;
        
        // Retornar consistência (inverso da variância normalizada)
        return Math.Max(0, 1 - variance / Math.PI);
    }
    
    private bool DetermineSuccess()
    {
        if (!_movementHistory.Any())
            return false;
        
        // Determinar sucesso baseado no histórico de movimento
        var avgConfidence = _movementHistory.Average(m => m.confidence);
        var recentConfidence = _movementHistory.TakeLast(3).Average(m => m.confidence);
        
        // Critérios para sucesso:
        // 1. Confiança média alta
        // 2. Confiança recente alta
        // 3. Tempo de resolução adequado
        var resolutionTime = DateTime.UtcNow - _minigameStartTime;
        var timeScore = resolutionTime.TotalSeconds > 1.0 && resolutionTime.TotalSeconds < 10.0 ? 1.0 : 0.5;
        
        var successScore = (avgConfidence * 0.4 + recentConfidence * 0.6) * timeScore;
        
        _logger.LogInformation("Determinando sucesso: confiança média={AvgConf:F2}, recente={RecentConf:F2}, tempo={Time:F1}s, score={Score:F2}", 
            avgConfidence, recentConfidence, resolutionTime.TotalSeconds, successScore);
        
        return successScore > 0.6;
    }
}
