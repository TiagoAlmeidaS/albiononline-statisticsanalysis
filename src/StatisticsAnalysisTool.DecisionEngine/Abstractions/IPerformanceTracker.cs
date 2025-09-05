using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para rastreamento de performance
    /// </summary>
    public interface IPerformanceTracker
    {
        /// <summary>
        /// Se o rastreamento está ativo
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Inicializa o rastreador
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o rastreador
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Inicia rastreamento de uma operação
        /// </summary>
        Task<string> StartTrackingAsync(string operationName, Dictionary<string, object>? metadata = null);
        
        /// <summary>
        /// Finaliza rastreamento de uma operação
        /// </summary>
        Task EndTrackingAsync(string trackingId, bool success = true, Dictionary<string, object>? results = null);
        
        /// <summary>
        /// Obtém estatísticas de performance
        /// </summary>
        Task<PerformanceStats> GetStatsAsync();
        
        /// <summary>
        /// Obtém estatísticas de uma operação específica
        /// </summary>
        Task<OperationStats> GetOperationStatsAsync(string operationName);
        
        /// <summary>
        /// Obtém histórico de performance
        /// </summary>
        Task<List<PerformanceRecord>> GetPerformanceHistoryAsync(TimeSpan timeRange);
        
        /// <summary>
        /// Obtém operações mais lentas
        /// </summary>
        Task<List<SlowOperation>> GetSlowOperationsAsync(int count = 10);
        
        /// <summary>
        /// Obtém operações com mais falhas
        /// </summary>
        Task<List<FailedOperation>> GetFailedOperationsAsync(int count = 10);
        
        /// <summary>
        /// Reseta estatísticas
        /// </summary>
        Task ResetStatsAsync();
        
        /// <summary>
        /// Obtém configuração do rastreador
        /// </summary>
        Task<PerformanceTrackerConfiguration> GetConfigurationAsync();
        
        /// <summary>
        /// Atualiza configuração do rastreador
        /// </summary>
        Task UpdateConfigurationAsync(PerformanceTrackerConfiguration config);
    }
    
    /// <summary>
    /// Estatísticas de performance
    /// </summary>
    public class PerformanceStats
    {
        public long TotalOperations { get; set; }
        public long SuccessfulOperations { get; set; }
        public long FailedOperations { get; set; }
        public double SuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations : 0;
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan AverageExecutionTime => 
            TotalOperations > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / TotalOperations) : TimeSpan.Zero;
        public TimeSpan MinExecutionTime { get; set; } = TimeSpan.MaxValue;
        public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.Zero;
        public Dictionary<string, OperationStats> OperationStats { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estatísticas de uma operação
    /// </summary>
    public class OperationStats
    {
        public string OperationName { get; set; } = string.Empty;
        public long ExecutionCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public double SuccessRate => ExecutionCount > 0 ? (double)SuccessCount / ExecutionCount : 0;
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan AverageExecutionTime => 
            ExecutionCount > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / ExecutionCount) : TimeSpan.Zero;
        public TimeSpan MinExecutionTime { get; set; } = TimeSpan.MaxValue;
        public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.Zero;
        public DateTime LastExecution { get; set; }
        public DateTime FirstExecution { get; set; }
    }
    
    /// <summary>
    /// Registro de performance
    /// </summary>
    public class PerformanceRecord
    {
        public string Id { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public Dictionary<string, object> Results { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Operação lenta
    /// </summary>
    public class SlowOperation
    {
        public string OperationName { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
    /// <summary>
    /// Operação com falha
    /// </summary>
    public class FailedOperation
    {
        public string OperationName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
    
    /// <summary>
    /// Configuração do rastreador de performance
    /// </summary>
    public class PerformanceTrackerConfiguration
    {
        public bool EnableTracking { get; set; } = true;
        public TimeSpan MaxHistoryTime { get; set; } = TimeSpan.FromHours(24);
        public int MaxHistoryRecords { get; set; } = 10000;
        public TimeSpan SlowOperationThreshold { get; set; } = TimeSpan.FromSeconds(1);
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }
}
