using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para execução de ações
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// Ações registradas
        /// </summary>
        IReadOnlyDictionary<string, IAction> Actions { get; }
        
        /// <summary>
        /// Evento disparado quando uma ação é executada
        /// </summary>
        event EventHandler<ActionExecutedEventArgs> ActionExecuted;
        
        /// <summary>
        /// Inicializa o executor
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o executor
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Registra uma ação
        /// </summary>
        Task RegisterActionAsync(IAction action);
        
        /// <summary>
        /// Remove uma ação
        /// </summary>
        Task UnregisterActionAsync(string actionName);
        
        /// <summary>
        /// Executa uma ação
        /// </summary>
        Task<ActionResult> ExecuteActionAsync(string actionName, IGameContext context, Dictionary<string, object>? parameters = null);
        
        /// <summary>
        /// Verifica se uma ação pode ser executada
        /// </summary>
        Task<bool> CanExecuteActionAsync(string actionName, IGameContext context);
        
        /// <summary>
        /// Obtém ações disponíveis para um contexto
        /// </summary>
        Task<IEnumerable<string>> GetAvailableActionsAsync(IGameContext context);
        
        /// <summary>
        /// Obtém estatísticas de execução
        /// </summary>
        Task<ActionExecutionStats> GetStatsAsync();
        
        /// <summary>
        /// Reseta estatísticas
        /// </summary>
        Task ResetStatsAsync();
    }
    
    /// <summary>
    /// Interface para ações
    /// </summary>
    public interface IAction
    {
        string Name { get; }
        string Description { get; }
        string Category { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }
        
        Task<bool> CanExecuteAsync(IGameContext context, Dictionary<string, object>? parameters = null);
        Task<ActionResult> ExecuteAsync(IGameContext context, Dictionary<string, object>? parameters = null);
        Task<ActionValidation> ValidateAsync(IGameContext context, Dictionary<string, object>? parameters = null);
    }
    
    /// <summary>
    /// Resultado de execução de ação
    /// </summary>
    public class ActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validação de ação
    /// </summary>
    public class ActionValidation
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Suggestions { get; set; } = new();
    }
    
    /// <summary>
    /// Evento de ação executada
    /// </summary>
    public class ActionExecutedEventArgs : EventArgs
    {
        public string ActionName { get; set; } = string.Empty;
        public ActionResult Result { get; set; } = new();
        public IGameContext Context { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estatísticas de execução de ações
    /// </summary>
    public class ActionExecutionStats
    {
        public long TotalExecutions { get; set; }
        public long SuccessfulExecutions { get; set; }
        public long FailedExecutions { get; set; }
        public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions : 0;
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan AverageExecutionTime => 
            TotalExecutions > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / TotalExecutions) : TimeSpan.Zero;
        public Dictionary<string, long> ActionCounts { get; set; } = new();
        public DateTime LastExecution { get; set; }
    }
}
