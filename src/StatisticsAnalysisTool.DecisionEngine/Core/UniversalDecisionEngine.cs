using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;
using StatisticsAnalysisTool.DecisionEngine.Behaviors;

namespace StatisticsAnalysisTool.DecisionEngine.Core
{
    /// <summary>
    /// Implementação principal do Motor de Decisão Universal
    /// </summary>
    public class UniversalDecisionEngine : IUniversalDecisionEngine
    {
        private readonly ILogger<UniversalDecisionEngine> _logger;
        private readonly List<BasicMinigameBehavior> _behaviors = new();
        private readonly List<IDecisionRule> _rules = new();
        private readonly DecisionEngineStats _stats = new();
        
        public bool IsActive { get; private set; }
        public bool IsEnabled { get; set; } = true;
        public DecisionEngineStats Stats => _stats;
        
        public event EventHandler<DecisionMadeEventArgs>? DecisionMade;
        public event EventHandler<ContextChangedEventArgs>? ContextChanged;
        
        private IGameContext? _currentContext;
        private DateTime _startTime = DateTime.UtcNow;
        
        public UniversalDecisionEngine(ILogger<UniversalDecisionEngine> logger)
        {
            _logger = logger;
        }
        
        public async Task InitializeAsync()
        {
            _logger.LogInformation("🚀 Inicializando UniversalDecisionEngine...");
            IsActive = true;
            _startTime = DateTime.UtcNow;
            await Task.CompletedTask;
        }
        
        public async Task ShutdownAsync()
        {
            _logger.LogInformation("🛑 Finalizando UniversalDecisionEngine...");
            IsActive = false;
            
            // Finalizar todos os comportamentos
            foreach (var behavior in _behaviors)
            {
                try
                {
                    await behavior.ShutdownAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao finalizar comportamento {BehaviorName}", behavior.Name);
                }
            }
            
            await Task.CompletedTask;
        }
        
        public async Task<DecisionResult> ProcessDecisionAsync(IGameContext context)
        {
            if (!IsActive || !IsEnabled)
            {
                return DecisionResult.CreateNoAction("Motor não está ativo ou habilitado");
            }
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogDebug("🔄 Processando decisão para estado: {State}", context.CurrentState);
                
                // Atualizar contexto
                await UpdateContextAsync(context);
                
                // Avaliar regras
                var ruleResults = new List<DecisionResult>();
                foreach (var rule in _rules.Where(r => r.IsEnabled).OrderByDescending(r => r.Priority))
                {
                    try
                    {
                        var result = rule.Evaluate(context);
                        if (result.Success && result.Action != "NO_ACTION")
                        {
                            ruleResults.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao avaliar regra {RuleName}", rule.Name);
                    }
                }
                
                // Escolher melhor decisão
                var bestDecision = ruleResults
                    .OrderByDescending(r => r.Confidence)
                    .ThenByDescending(r => r.Priority)
                    .FirstOrDefault();
                
                if (bestDecision == null)
                {
                    bestDecision = DecisionResult.CreateNoAction("Nenhuma regra aplicável");
                }
                
                // Atualizar estatísticas
                _stats.TotalDecisions++;
                if (bestDecision.Success)
                {
                    _stats.SuccessfulDecisions++;
                }
                else
                {
                    _stats.FailedDecisions++;
                }
                
                var executionTime = DateTime.UtcNow - startTime;
                _stats.TotalProcessingTime += executionTime;
                _stats.LastDecision = DateTime.UtcNow;
                
                if (_stats.FirstDecision == null)
                {
                    _stats.FirstDecision = DateTime.UtcNow;
                }
                
                // Disparar evento
                DecisionMade?.Invoke(this, new DecisionMadeEventArgs
                {
                    Decision = bestDecision,
                    Context = context,
                    BehaviorName = bestDecision.Action
                });
                
                _logger.LogDebug("✅ Decisão processada: {Action} - {Reason}", 
                    bestDecision.Action, bestDecision.Reason);
                
                return bestDecision;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processamento de decisão");
                _stats.FailedDecisions++;
                return DecisionResult.CreateFailure("Erro interno: " + ex.Message);
            }
        }
        
        public async Task UpdateContextAsync(IGameContext context)
        {
            var oldContext = _currentContext;
            _currentContext = context;
            
            // Disparar evento de mudança de contexto
            if (oldContext != null)
            {
                ContextChanged?.Invoke(this, new ContextChangedEventArgs
                {
                    OldContext = oldContext,
                    NewContext = context,
                    ChangeReason = "Context update"
                });
            }
            
            // Atualizar comportamentos
            foreach (var behavior in _behaviors.Where(b => b.IsEnabled))
            {
                try
                {
                    await behavior.UpdateAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar comportamento {BehaviorName}", behavior.Name);
                }
            }
            
            _stats.ContextsProcessed++;
            await Task.CompletedTask;
        }
        
        public async Task<BehaviorResult> ExecuteActionAsync(string action, IGameContext context)
        {
            var behavior = _behaviors.FirstOrDefault(b => b.GetSupportedActions().Contains(action));
            if (behavior == null)
            {
                return BehaviorResult.CreateFailure($"Ação não suportada: {action}");
            }
            
            try
            {
                var decision = DecisionResult.CreateSuccess(action, "Execução direta");
                var result = await behavior.ExecuteActionAsync(decision);
                
                _stats.ActionsExecuted++;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar ação {Action}", action);
                return BehaviorResult.CreateFailure($"Erro na execução: {ex.Message}");
            }
        }
        
        public IEnumerable<Abstractions.IBehavior> GetActiveBehaviors() => 
            _behaviors.Where(b => b.IsActive && b.IsEnabled).Cast<Abstractions.IBehavior>();
        
        public IEnumerable<IDecisionRule> GetActiveRules() => 
            _rules.Where(r => r.IsEnabled);
        
        public async Task SetBehaviorEnabledAsync(string behaviorName, bool enabled)
        {
            var behavior = _behaviors.FirstOrDefault(b => b.Name == behaviorName);
            if (behavior != null)
            {
                behavior.IsEnabled = enabled;
                _logger.LogInformation("Comportamento {BehaviorName} {Status}", 
                    behaviorName, enabled ? "habilitado" : "desabilitado");
            }
            await Task.CompletedTask;
        }
        
        public async Task SetRuleEnabledAsync(string ruleName, bool enabled)
        {
            var rule = _rules.FirstOrDefault(r => r.Name == ruleName);
            if (rule != null)
            {
                rule.IsEnabled = enabled;
                _logger.LogInformation("Regra {RuleName} {Status}", 
                    ruleName, enabled ? "habilitada" : "desabilitada");
            }
            await Task.CompletedTask;
        }
        
        public async Task<DecisionEngineStats> GetDetailedStatsAsync()
        {
            _stats.Uptime = DateTime.UtcNow - _startTime;
            _stats.ActiveBehaviors = GetActiveBehaviors().Count();
            _stats.ActiveRules = GetActiveRules().Count();
            
            // Atualizar estatísticas de comportamentos
            foreach (var behavior in _behaviors)
            {
                _stats.BehaviorStats[behavior.Name] = behavior.GetStats();
            }
            
            return await Task.FromResult(_stats);
        }
        
        public async Task ResetStatsAsync()
        {
            _stats.TotalDecisions = 0;
            _stats.SuccessfulDecisions = 0;
            _stats.FailedDecisions = 0;
            _stats.TotalProcessingTime = TimeSpan.Zero;
            _stats.ActionsExecuted = 0;
            _stats.ContextsProcessed = 0;
            _stats.FirstDecision = null;
            _stats.LastDecision = null;
            _stats.BehaviorStats.Clear();
            _stats.RuleStats.Clear();
            _stats.ActionStats.Clear();
            
            _logger.LogInformation("Estatísticas resetadas");
            await Task.CompletedTask;
        }
        
        public async Task<Dictionary<string, object>> GetConfigurationAsync()
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["IsEnabled"] = IsEnabled,
                ["IsActive"] = IsActive,
                ["BehaviorCount"] = _behaviors.Count,
                ["RuleCount"] = _rules.Count
            });
        }
        
        public async Task UpdateConfigurationAsync(Dictionary<string, object> config)
        {
            if (config.TryGetValue("IsEnabled", out var enabled) && enabled is bool e)
            {
                IsEnabled = e;
            }
            await Task.CompletedTask;
        }
        
        public async Task<bool> ValidateConfigurationAsync()
        {
            // Validar se há pelo menos um comportamento e uma regra
            return await Task.FromResult(_behaviors.Count > 0 && _rules.Count > 0);
        }
        
        public async Task<IEnumerable<DecisionEngineLog>> GetLogsAsync(int maxCount = 100)
        {
            // Implementação básica - em produção seria integrada com um sistema de logging
            return await Task.FromResult(Enumerable.Empty<DecisionEngineLog>());
        }
        
        public async Task ClearLogsAsync()
        {
            await Task.CompletedTask;
        }
        
        public async Task<DecisionEnginePerformance> GetPerformanceAsync()
        {
            return await Task.FromResult(new DecisionEnginePerformance
            {
                OperationsPerSecond = _stats.TotalDecisions / Math.Max(1, _stats.Uptime.TotalSeconds),
                AverageResponseTime = _stats.AverageProcessingTime,
                ErrorRate = _stats.SuccessRate
            });
        }
        
        public async Task<DecisionEngineHealth> GetHealthAsync()
        {
            var health = new DecisionEngineHealth
            {
                Status = IsActive ? HealthStatus.Healthy : HealthStatus.Critical,
                HealthScore = IsActive ? 100 : 0,
                Message = IsActive ? "Motor funcionando normalmente" : "Motor inativo"
            };
            
            if (!IsActive)
            {
                health.Issues.Add(new HealthIssue
                {
                    Id = "ENGINE_INACTIVE",
                    Category = "System",
                    Description = "Motor de decisão está inativo",
                    Severity = HealthStatus.Critical
                });
            }
            
            return await Task.FromResult(health);
        }
        
        public IAIBridge? GetAIBridge() => null; // Implementar se necessário
        
        public IContextManager? GetContextManager() => null; // Implementar se necessário
        
        public IBehaviorOrchestrator? GetBehaviorOrchestrator() => null; // Implementar se necessário
        
        // Métodos internos para registrar comportamentos e regras
        internal void RegisterBehavior(BasicMinigameBehavior behavior)
        {
            _behaviors.Add(behavior);
            _logger.LogInformation("Comportamento registrado: {BehaviorName}", behavior.Name);
        }
        
        internal void RegisterRule(IDecisionRule rule)
        {
            _rules.Add(rule);
            _logger.LogInformation("Regra registrada: {RuleName}", rule.Name);
        }
    }
}