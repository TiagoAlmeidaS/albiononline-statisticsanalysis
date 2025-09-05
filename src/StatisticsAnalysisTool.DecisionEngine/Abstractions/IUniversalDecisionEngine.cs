using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface principal do Motor de Decisão Universal
    /// </summary>
    public interface IUniversalDecisionEngine
    {
        /// <summary>
        /// Se o motor está ativo
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Se o motor está habilitado
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Estatísticas do motor
        /// </summary>
        DecisionEngineStats Stats { get; }
        
        /// <summary>
        /// Evento disparado quando uma decisão é tomada
        /// </summary>
        event EventHandler<DecisionMadeEventArgs> DecisionMade;
        
        /// <summary>
        /// Evento disparado quando o contexto muda
        /// </summary>
        event EventHandler<ContextChangedEventArgs> ContextChanged;
        
        /// <summary>
        /// Inicializa o motor
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Para o motor
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Processa uma decisão baseada no contexto
        /// </summary>
        Task<DecisionResult> ProcessDecisionAsync(IGameContext context);
        
        /// <summary>
        /// Atualiza o contexto do motor
        /// </summary>
        Task UpdateContextAsync(IGameContext context);
        
        /// <summary>
        /// Executa uma ação específica
        /// </summary>
        Task<BehaviorResult> ExecuteActionAsync(string action, IGameContext context);
        
        /// <summary>
        /// Obtém comportamentos ativos
        /// </summary>
        IEnumerable<IBehavior> GetActiveBehaviors();
        
        /// <summary>
        /// Obtém regras ativas
        /// </summary>
        IEnumerable<IDecisionRule> GetActiveRules();
        
        /// <summary>
        /// Habilita/desabilita um comportamento
        /// </summary>
        Task SetBehaviorEnabledAsync(string behaviorName, bool enabled);
        
        /// <summary>
        /// Habilita/desabilita uma regra
        /// </summary>
        Task SetRuleEnabledAsync(string ruleName, bool enabled);
        
        /// <summary>
        /// Obtém estatísticas detalhadas
        /// </summary>
        Task<DecisionEngineStats> GetDetailedStatsAsync();
        
        /// <summary>
        /// Reseta as estatísticas
        /// </summary>
        Task ResetStatsAsync();
        
        /// <summary>
        /// Obtém configuração do motor
        /// </summary>
        Task<Dictionary<string, object>> GetConfigurationAsync();
        
        /// <summary>
        /// Atualiza configuração do motor
        /// </summary>
        Task UpdateConfigurationAsync(Dictionary<string, object> config);
        
        /// <summary>
        /// Valida configuração do motor
        /// </summary>
        Task<bool> ValidateConfigurationAsync();
        
        /// <summary>
        /// Obtém logs do motor
        /// </summary>
        Task<IEnumerable<DecisionEngineLog>> GetLogsAsync(int maxCount = 100);
        
        /// <summary>
        /// Limpa logs do motor
        /// </summary>
        Task ClearLogsAsync();
        
        /// <summary>
        /// Obtém performance do motor
        /// </summary>
        Task<DecisionEnginePerformance> GetPerformanceAsync();
        
        /// <summary>
        /// Obtém saúde do motor
        /// </summary>
        Task<DecisionEngineHealth> GetHealthAsync();
        
        /// <summary>
        /// Obtém suporte a IA
        /// </summary>
        IAIBridge? GetAIBridge();
        
        /// <summary>
        /// Obtém gerenciador de contexto
        /// </summary>
        IContextManager? GetContextManager();
        
        /// <summary>
        /// Obtém orquestrador de comportamentos
        /// </summary>
        IBehaviorOrchestrator? GetBehaviorOrchestrator();
    }
}
