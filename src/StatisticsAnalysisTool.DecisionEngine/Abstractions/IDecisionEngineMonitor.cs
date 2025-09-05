using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para monitoramento do DecisionEngine
    /// </summary>
    public interface IDecisionEngineMonitor
    {
        /// <summary>
        /// Se o monitoramento está ativo
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Evento disparado quando há mudança de status
        /// </summary>
        event EventHandler<MonitorStatusChangedEventArgs> StatusChanged;
        
        /// <summary>
        /// Inicializa o monitor
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza o monitor
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Inicia monitoramento
        /// </summary>
        Task StartMonitoringAsync();
        
        /// <summary>
        /// Para monitoramento
        /// </summary>
        Task StopMonitoringAsync();
        
        /// <summary>
        /// Obtém status atual
        /// </summary>
        Task<MonitorStatus> GetStatusAsync();
        
        /// <summary>
        /// Obtém métricas atuais
        /// </summary>
        Task<MonitorMetrics> GetMetricsAsync();
        
        /// <summary>
        /// Obtém alertas ativos
        /// </summary>
        Task<List<MonitorAlert>> GetActiveAlertsAsync();
        
        /// <summary>
        /// Obtém histórico de métricas
        /// </summary>
        Task<List<MonitorMetrics>> GetMetricsHistoryAsync(TimeSpan timeRange);
        
        /// <summary>
        /// Obtém logs do monitor
        /// </summary>
        Task<List<MonitorLog>> GetLogsAsync(int maxCount = 100);
        
        /// <summary>
        /// Limpa logs
        /// </summary>
        Task ClearLogsAsync();
        
        /// <summary>
        /// Obtém configuração do monitor
        /// </summary>
        Task<MonitorConfiguration> GetConfigurationAsync();
        
        /// <summary>
        /// Atualiza configuração do monitor
        /// </summary>
        Task UpdateConfigurationAsync(MonitorConfiguration config);
    }
    
    /// <summary>
    /// Status do monitor
    /// </summary>
    public class MonitorStatus
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
        public Dictionary<string, object> Details { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Métricas do monitor
    /// </summary>
    public class MonitorMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public long DecisionCount { get; set; }
        public long BehaviorExecutionCount { get; set; }
        public double AverageDecisionTime { get; set; }
        public double AverageBehaviorExecutionTime { get; set; }
        public int ActiveBehaviors { get; set; }
        public int ActiveRules { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Alerta do monitor
    /// </summary>
    public class MonitorAlert
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
    
    /// <summary>
    /// Log do monitor
    /// </summary>
    public class MonitorLog
    {
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Configuração do monitor
    /// </summary>
    public class MonitorConfiguration
    {
        public bool EnableCpuMonitoring { get; set; } = true;
        public bool EnableMemoryMonitoring { get; set; } = true;
        public bool EnablePerformanceMonitoring { get; set; } = true;
        public TimeSpan MetricsInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan AlertThreshold { get; set; } = TimeSpan.FromMinutes(1);
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }
    
    /// <summary>
    /// Evento de mudança de status do monitor
    /// </summary>
    public class MonitorStatusChangedEventArgs : EventArgs
    {
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
