using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Estatísticas de um comportamento
    /// </summary>
    public class BehaviorStats
    {
        /// <summary>
        /// Nome do comportamento
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de execuções
        /// </summary>
        public long ExecutionCount { get; set; }
        
        /// <summary>
        /// Número de execuções bem-sucedidas
        /// </summary>
        public long SuccessCount { get; set; }
        
        /// <summary>
        /// Número de execuções falhadas
        /// </summary>
        public long FailureCount { get; set; }
        
        /// <summary>
        /// Taxa de sucesso (0-1)
        /// </summary>
        public double SuccessRate => ExecutionCount > 0 ? (double)SuccessCount / ExecutionCount : 0;
        
        /// <summary>
        /// Tempo total de execução
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }
        
        /// <summary>
        /// Tempo médio de execução
        /// </summary>
        public TimeSpan AverageExecutionTime => 
            ExecutionCount > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / ExecutionCount) : TimeSpan.Zero;
        
        /// <summary>
        /// Tempo mínimo de execução
        /// </summary>
        public TimeSpan MinExecutionTime { get; set; } = TimeSpan.MaxValue;
        
        /// <summary>
        /// Tempo máximo de execução
        /// </summary>
        public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Última execução
        /// </summary>
        public DateTime? LastExecution { get; set; }
        
        /// <summary>
        /// Primeira execução
        /// </summary>
        public DateTime? FirstExecution { get; set; }
        
        /// <summary>
        /// Se o comportamento está ativo
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Se o comportamento está habilitado
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Prioridade atual
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Dados adicionais
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}
