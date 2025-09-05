using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;

namespace StatisticsAnalysisTool.DecisionEngine.Behaviors
{
    /// <summary>
    /// Comportamento básico para resolução do minigame de pesca
    /// </summary>
    public class BasicMinigameBehavior
    {
        private readonly ILogger<BasicMinigameBehavior> _logger;
        private readonly IMinigameService _minigameService;
        
        public string Name => "BasicMinigameResolution";
        public string Description => "Resolve o minigame de pesca de forma básica";
        public int Priority { get; set; } = 100;
        public bool IsEnabled { get; set; } = true;
        public bool IsActive => _minigameService.IsMinigameActive;
        
        public BasicMinigameBehavior(ILogger<BasicMinigameBehavior> logger, IMinigameService minigameService)
        {
            _logger = logger;
            _minigameService = minigameService;
        }
        
        public async Task InitializeAsync()
        {
            _logger.LogInformation("🎮 Inicializando BasicMinigameBehavior...");
            await Task.CompletedTask;
        }
        
        public async Task ShutdownAsync()
        {
            _logger.LogInformation("🛑 Finalizando BasicMinigameBehavior...");
            if (_minigameService.IsMinigameActive)
            {
                _minigameService.StopMinigame();
            }
            await Task.CompletedTask;
        }
        
        public async Task<bool> CanProcessAsync(IGameContext context)
        {
            return await Task.FromResult(
                context.CurrentState == "MINIGAME" || 
                context.CurrentState == "FISHING_MINIGAME" ||
                context.HasFlag("MINIGAME_ACTIVE")
            );
        }
        
        public async Task<DecisionResult> MakeDecisionAsync(IGameContext context)
        {
            if (await CanProcessAsync(context))
            {
                return DecisionResult.CreateSuccess("MinigameResolution", "Iniciar resolução do minigame");
            }
            
            return DecisionResult.CreateNoAction("Não está em estado de minigame");
        }
        
        public async Task<BehaviorResult> ExecuteActionAsync(DecisionResult decision)
        {
            if (decision.Action != "MinigameResolution")
            {
                return BehaviorResult.CreateFailure("Ação não suportada: " + decision.Action);
            }
            
            try
            {
                _logger.LogInformation("🎮 Executando minigame de pesca...");
                
                if (_minigameService.IsMinigameActive)
                {
                    _logger.LogWarning("Minigame já está ativo, ignorando nova execução");
                    return BehaviorResult.CreateSuccess("Minigame já ativo");
                }
                
                var region = new System.Drawing.Rectangle(400, 300, 400, 200);
                await _minigameService.StartMinigameAsync(region);
                
                while (_minigameService.IsMinigameActive)
                {
                    await Task.Delay(100);
                }
                
                var stats = _minigameService.CurrentStats;
                _logger.LogInformation("✅ Minigame concluído - Taxa: {DetectionRate:F1}%", stats.DetectionRate);
                
                return BehaviorResult.CreateSuccess($"Minigame concluído - Taxa: {stats.DetectionRate:F1}%");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante execução do minigame");
                return BehaviorResult.CreateFailure($"Erro no minigame: {ex.Message}");
            }
        }
        
        public async Task UpdateAsync(IGameContext context)
        {
            await Task.CompletedTask;
        }
        
        public BehaviorStats GetStats()
        {
            return new BehaviorStats
            {
                Name = Name,
                IsActive = IsActive,
                IsEnabled = IsEnabled,
                Priority = Priority
            };
        }
        
        public IEnumerable<string> GetSupportedActions()
        {
            return new[] { "MinigameResolution" };
        }
        
        public IEnumerable<string> GetSupportedConditions()
        {
            return new[] { "MINIGAME_ACTIVE", "FISHING_MINIGAME", "BOBBER_DETECTED" };
        }
    }
}
