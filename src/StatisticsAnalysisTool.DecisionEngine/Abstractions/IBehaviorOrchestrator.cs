using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para orquestração de comportamentos
    /// </summary>
    public interface IBehaviorOrchestrator
    {
        /// <summary>
        /// Comportamentos registrados
        /// </summary>
        IReadOnlyList<IBehavior> Behaviors { get; }
        
        /// <summary>
        /// Comportamentos ativos
        /// </summary>
        IReadOnlyList<IBehavior> ActiveBehaviors { get; }
        
        /// <summary>
        /// Evento disparado quando um comportamento é executado
        /// </summary>
        event EventHandler<BehaviorExecutedEventArgs> BehaviorExecuted;
        
        /// <summary>
        /// Inicializa o orquestrador
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o orquestrador
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Registra um comportamento
        /// </summary>
        Task RegisterBehaviorAsync(IBehavior behavior);
        
        /// <summary>
        /// Remove um comportamento
        /// </summary>
        Task UnregisterBehaviorAsync(string behaviorName);
        
        /// <summary>
        /// Obtém um comportamento por nome
        /// </summary>
        Task<IBehavior?> GetBehaviorAsync(string behaviorName);
        
        /// <summary>
        /// Executa todos os comportamentos aplicáveis
        /// </summary>
        Task<List<BehaviorResult>> ExecuteBehaviorsAsync(IGameContext context);
        
        /// <summary>
        /// Executa um comportamento específico
        /// </summary>
        Task<BehaviorResult> ExecuteBehaviorAsync(string behaviorName, IGameContext context);
        
        /// <summary>
        /// Habilita/desabilita um comportamento
        /// </summary>
        Task SetBehaviorEnabledAsync(string behaviorName, bool enabled);
        
        /// <summary>
        /// Atualiza prioridade de um comportamento
        /// </summary>
        Task SetBehaviorPriorityAsync(string behaviorName, int priority);
        
        /// <summary>
        /// Obtém estatísticas de todos os comportamentos
        /// </summary>
        Task<Dictionary<string, BehaviorStats>> GetBehaviorStatsAsync();
        
        /// <summary>
        /// Obtém estatísticas de um comportamento
        /// </summary>
        Task<BehaviorStats?> GetBehaviorStatsAsync(string behaviorName);
        
        /// <summary>
        /// Reseta estatísticas de um comportamento
        /// </summary>
        Task ResetBehaviorStatsAsync(string behaviorName);
        
        /// <summary>
        /// Reseta estatísticas de todos os comportamentos
        /// </summary>
        Task ResetAllBehaviorStatsAsync();
        
        /// <summary>
        /// Valida configuração de um comportamento
        /// </summary>
        Task<bool> ValidateBehaviorConfigurationAsync(string behaviorName);
        
        /// <summary>
        /// Obtém comportamentos por tipo
        /// </summary>
        Task<IEnumerable<IBehavior>> GetBehaviorsByTypeAsync<T>() where T : IBehavior;
        
        /// <summary>
        /// Obtém comportamentos por prioridade
        /// </summary>
        Task<IEnumerable<IBehavior>> GetBehaviorsByPriorityAsync(int minPriority, int maxPriority);
        
        /// <summary>
        /// Obtém comportamentos habilitados
        /// </summary>
        Task<IEnumerable<IBehavior>> GetEnabledBehaviorsAsync();
        
        /// <summary>
        /// Obtém comportamentos ativos
        /// </summary>
        Task<IEnumerable<IBehavior>> GetActiveBehaviorsAsync();
    }
}
