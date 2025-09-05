using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Performance do DecisionEngine
    /// </summary>
    public class DecisionEnginePerformance
    {
        /// <summary>
        /// Uso de CPU (0-100)
        /// </summary>
        public double CpuUsage { get; set; }
        
        /// <summary>
        /// Uso de memória em MB
        /// </summary>
        public double MemoryUsage { get; set; }
        
        /// <summary>
        /// Número de threads ativas
        /// </summary>
        public int ActiveThreads { get; set; }
        
        /// <summary>
        /// Número de operações por segundo
        /// </summary>
        public double OperationsPerSecond { get; set; }
        
        /// <summary>
        /// Tempo médio de resposta
        /// </summary>
        public TimeSpan AverageResponseTime { get; set; }
        
        /// <summary>
        /// Tempo de resposta P95
        /// </summary>
        public TimeSpan P95ResponseTime { get; set; }
        
        /// <summary>
        /// Tempo de resposta P99
        /// </summary>
        public TimeSpan P99ResponseTime { get; set; }
        
        /// <summary>
        /// Número de erros por segundo
        /// </summary>
        public double ErrorsPerSecond { get; set; }
        
        /// <summary>
        /// Taxa de erro (0-1)
        /// </summary>
        public double ErrorRate { get; set; }
        
        /// <summary>
        /// Timestamp da medição
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Métricas adicionais
        /// </summary>
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }
}
