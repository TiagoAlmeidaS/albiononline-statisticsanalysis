using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface base para todos os comportamentos do DecisionEngine
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Nome único do comportamento
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Descrição do comportamento
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Versão do comportamento
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Prioridade do comportamento (maior = mais prioritário)
        /// </summary>
        int Priority { get; set; }
        
        /// <summary>
        /// Se o comportamento está habilitado
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Se o comportamento está ativo
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Tipo de contexto suportado
        /// </summary>
        Type SupportedContextType { get; }
        
        /// <summary>
        /// Evento disparado quando o comportamento é executado
        /// </summary>
        event EventHandler<BehaviorExecutedEventArgs> Executed;
        
        /// <summary>
        /// Evento disparado quando o comportamento é ativado
        /// </summary>
        event EventHandler<BehaviorActivatedEventArgs> Activated;
        
        /// <summary>
        /// Evento disparado quando o comportamento é desativado
        /// </summary>
        event EventHandler<BehaviorDeactivatedEventArgs> Deactivated;
        
        /// <summary>
        /// Inicializa o comportamento
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o comportamento
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Verifica se o comportamento pode processar o contexto
        /// </summary>
        Task<bool> CanProcessAsync(IGameContext context);
        
        /// <summary>
        /// Toma uma decisão baseada no contexto
        /// </summary>
        Task<DecisionResult> MakeDecisionAsync(IGameContext context);
        
        /// <summary>
        /// Executa uma ação baseada na decisão
        /// </summary>
        Task<BehaviorResult> ExecuteActionAsync(DecisionResult decision);
        
        /// <summary>
        /// Atualiza o comportamento com o contexto atual
        /// </summary>
        Task UpdateAsync(IGameContext context);
        
        /// <summary>
        /// Obtém estatísticas do comportamento
        /// </summary>
        BehaviorStats GetStats();
        
        /// <summary>
        /// Configura o comportamento
        /// </summary>
        Task ConfigureAsync(Dictionary<string, object> config);
        
        /// <summary>
        /// Valida a configuração do comportamento
        /// </summary>
        Task<bool> ValidateConfigurationAsync();
        
        /// <summary>
        /// Obtém ações suportadas pelo comportamento
        /// </summary>
        IEnumerable<string> GetSupportedActions();
        
        /// <summary>
        /// Obtém condições suportadas pelo comportamento
        /// </summary>
        IEnumerable<string> GetSupportedConditions();
    }
}
