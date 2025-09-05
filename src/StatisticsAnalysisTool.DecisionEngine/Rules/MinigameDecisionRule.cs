using System;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;

namespace StatisticsAnalysisTool.DecisionEngine.Rules
{
    /// <summary>
    /// Regra de decisão para ativar o minigame de pesca
    /// </summary>
    public class MinigameDecisionRule : IDecisionRule
    {
        private readonly ILogger<MinigameDecisionRule> _logger;
        
        public string Name => "MinigameActivation";
        public string Description => "Decide quando ativar o minigame de pesca";
        public int Priority { get; } = 90; // Alta prioridade
        public bool IsEnabled { get; set; } = true;
        
        public MinigameDecisionRule(ILogger<MinigameDecisionRule> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Avalia se o minigame deve ser ativado
        /// </summary>
        public DecisionResult Evaluate(IGameContext context)
        {
            try
            {
                // Verificar se estamos em estado de pesca
                if (!IsFishingState(context))
                {
                    return DecisionResult.CreateNoAction("Não está em estado de pesca");
                }
                
                // Verificar se há um bobber ativo
                if (!HasActiveBobber(context))
                {
                    return DecisionResult.CreateNoAction("Nenhum bobber ativo detectado");
                }
                
                // Verificar se o minigame já está ativo
                if (IsMinigameActive(context))
                {
                    return DecisionResult.CreateNoAction("Minigame já está ativo");
                }
                
                // Verificar se há condições para iniciar o minigame
                if (!CanStartMinigame(context))
                {
                    return DecisionResult.CreateNoAction("Condições não atendidas para iniciar minigame");
                }
                
                _logger.LogInformation("🎮 Decisão: Ativar minigame de pesca");
                
                return DecisionResult.CreateSuccess("MinigameResolution", "Iniciar resolução do minigame de pesca");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao avaliar regra de minigame");
                return DecisionResult.CreateFailure("Erro na avaliação: " + ex.Message);
            }
        }
        
        private bool IsFishingState(IGameContext context)
        {
            var currentState = context.CurrentState?.ToUpper();
            return currentState == "FISHING" || 
                   currentState == "CASTING" || 
                   currentState == "WAITING_FOR_BITE" ||
                   context.HasFlag("FISHING_ACTIVE");
        }
        
        private bool HasActiveBobber(IGameContext context)
        {
            // Verificar se há um bobber detectado recentemente
            if (context.TryGetValue<bool>("bobber_detected", out var bobberDetected) && bobberDetected)
            {
                return true;
            }
            
            // Verificar se há posição do bobber
            if (context.TryGetValue<object>("bobber_position", out var position) && position != null)
            {
                return true;
            }
            
            // Verificar flag de bobber ativo
            return context.HasFlag("BOBBER_ACTIVE");
        }
        
        private bool IsMinigameActive(IGameContext context)
        {
            return context.HasFlag("MINIGAME_ACTIVE") || 
                   context.CurrentState == "MINIGAME";
        }
        
        private bool CanStartMinigame(IGameContext context)
        {
            // Verificar se não há outros processos críticos em andamento
            if (context.HasFlag("COMBAT_ACTIVE") || 
                context.HasFlag("MOVEMENT_ACTIVE") ||
                context.HasFlag("INVENTORY_OPEN"))
            {
                return false;
            }
            
            // Verificar se o jogador está em posição segura
            if (context.HasFlag("PLAYER_IN_DANGER"))
            {
                return false;
            }
            
            // Verificar se há recursos suficientes (opcional)
            if (context.TryGetValue<int>("fishing_energy", out var energyValue) && energyValue < 10)
            {
                _logger.LogDebug("Energia de pesca baixa: {Energy}", energyValue);
                return false;
            }
            
            return true;
        }
    }
}