using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para gerenciamento de contexto
    /// </summary>
    public interface IContextManager
    {
        /// <summary>
        /// Contexto atual
        /// </summary>
        IGameContext CurrentContext { get; }
        
        /// <summary>
        /// Histórico de contextos
        /// </summary>
        IReadOnlyList<IGameContext> ContextHistory { get; }
        
        /// <summary>
        /// Evento disparado quando o contexto muda
        /// </summary>
        event EventHandler<ContextChangedEventArgs> ContextChanged;
        
        /// <summary>
        /// Inicializa o gerenciador
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o gerenciador
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Atualiza o contexto
        /// </summary>
        Task UpdateContextAsync(IGameContext newContext);
        
        /// <summary>
        /// Obtém contexto por índice
        /// </summary>
        Task<IGameContext?> GetContextAsync(int index);
        
        /// <summary>
        /// Obtém contexto mais recente
        /// </summary>
        Task<IGameContext?> GetLatestContextAsync();
        
        /// <summary>
        /// Obtém contexto por timestamp
        /// </summary>
        Task<IGameContext?> GetContextByTimestampAsync(DateTime timestamp);
        
        /// <summary>
        /// Limpa histórico de contextos
        /// </summary>
        Task ClearHistoryAsync();
        
        /// <summary>
        /// Obtém estatísticas do contexto
        /// </summary>
        Task<ContextStats> GetStatsAsync();
        
        /// <summary>
        /// Valida contexto
        /// </summary>
        Task<bool> ValidateContextAsync(IGameContext context);
        
        /// <summary>
        /// Clona contexto atual
        /// </summary>
        Task<IGameContext> CloneCurrentContextAsync();
        
        /// <summary>
        /// Mescla contextos
        /// </summary>
        Task<IGameContext> MergeContextsAsync(IGameContext context1, IGameContext context2);
        
        /// <summary>
        /// Obtém diferenças entre contextos
        /// </summary>
        Task<ContextDiff> GetContextDiffAsync(IGameContext oldContext, IGameContext newContext);
    }
    
    /// <summary>
    /// Estatísticas do contexto
    /// </summary>
    public class ContextStats
    {
        public int TotalContexts { get; set; }
        public DateTime FirstContext { get; set; }
        public DateTime LastContext { get; set; }
        public TimeSpan AverageContextLifetime { get; set; }
        public Dictionary<string, int> StateCounts { get; set; } = new();
        public Dictionary<string, int> FlagCounts { get; set; } = new();
        public Dictionary<string, int> ValueCounts { get; set; } = new();
    }
    
    /// <summary>
    /// Diferenças entre contextos
    /// </summary>
    public class ContextDiff
    {
        public List<string> ChangedFlags { get; set; } = new();
        public List<string> ChangedValues { get; set; } = new();
        public List<string> AddedFlags { get; set; } = new();
        public List<string> RemovedFlags { get; set; } = new();
        public List<string> AddedValues { get; set; } = new();
        public List<string> RemovedValues { get; set; } = new();
        public bool StateChanged { get; set; }
        public string? OldState { get; set; }
        public string? NewState { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
