using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Estatísticas do DecisionEngine
    /// </summary>
    public class DecisionEngineStats
    {
        /// <summary>
        /// Número total de decisões
        /// </summary>
        public long TotalDecisions { get; set; }
        
        /// <summary>
        /// Número de decisões bem-sucedidas
        /// </summary>
        public long SuccessfulDecisions { get; set; }
        
        /// <summary>
        /// Número de decisões falhadas
        /// </summary>
        public long FailedDecisions { get; set; }
        
        /// <summary>
        /// Taxa de sucesso das decisões
        /// </summary>
        public double SuccessRate => TotalDecisions > 0 ? (double)SuccessfulDecisions / TotalDecisions : 0;
        
        /// <summary>
        /// Tempo total de processamento
        /// </summary>
        public TimeSpan TotalProcessingTime { get; set; }
        
        /// <summary>
        /// Tempo médio de processamento
        /// </summary>
        public TimeSpan AverageProcessingTime => 
            TotalDecisions > 0 ? TimeSpan.FromTicks(TotalProcessingTime.Ticks / TotalDecisions) : TimeSpan.Zero;
        
        /// <summary>
        /// Tempo mínimo de processamento
        /// </summary>
        public TimeSpan MinProcessingTime { get; set; } = TimeSpan.MaxValue;
        
        /// <summary>
        /// Tempo máximo de processamento
        /// </summary>
        public TimeSpan MaxProcessingTime { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Número de comportamentos ativos
        /// </summary>
        public int ActiveBehaviors { get; set; }
        
        /// <summary>
        /// Número de regras ativas
        /// </summary>
        public int ActiveRules { get; set; }
        
        /// <summary>
        /// Número de ações executadas
        /// </summary>
        public long ActionsExecuted { get; set; }
        
        /// <summary>
        /// Número de contextos processados
        /// </summary>
        public long ContextsProcessed { get; set; }
        
        /// <summary>
        /// Última decisão
        /// </summary>
        public DateTime? LastDecision { get; set; }
        
        /// <summary>
        /// Primeira decisão
        /// </summary>
        public DateTime? FirstDecision { get; set; }
        
        /// <summary>
        /// Tempo de atividade
        /// </summary>
        public TimeSpan Uptime { get; set; }
        
        /// <summary>
        /// Estatísticas por comportamento
        /// </summary>
        public Dictionary<string, BehaviorStats> BehaviorStats { get; set; } = new();
        
        /// <summary>
        /// Estatísticas por regra
        /// </summary>
        public Dictionary<string, RuleStats> RuleStats { get; set; } = new();
        
        /// <summary>
        /// Estatísticas por ação
        /// </summary>
        public Dictionary<string, ActionStats> ActionStats { get; set; } = new();
        
        /// <summary>
        /// Dados adicionais
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
    
    /// <summary>
    /// Estatísticas de uma regra
    /// </summary>
    public class RuleStats
    {
        public string RuleName { get; set; } = string.Empty;
        public long EvaluationCount { get; set; }
        public long ActionCount { get; set; }
        public long NoActionCount { get; set; }
        public double ActionRate => EvaluationCount > 0 ? (double)ActionCount / EvaluationCount : 0;
        public TimeSpan TotalEvaluationTime { get; set; }
        public TimeSpan AverageEvaluationTime => 
            EvaluationCount > 0 ? TimeSpan.FromTicks(TotalEvaluationTime.Ticks / EvaluationCount) : TimeSpan.Zero;
        public DateTime LastEvaluation { get; set; }
    }
    
    /// <summary>
    /// Estatísticas de uma ação
    /// </summary>
    public class ActionStats
    {
        public string ActionName { get; set; } = string.Empty;
        public long ExecutionCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public double SuccessRate => ExecutionCount > 0 ? (double)SuccessCount / ExecutionCount : 0;
        public TimeSpan TotalExecutionTime { get; set; }
        public TimeSpan AverageExecutionTime => 
            ExecutionCount > 0 ? TimeSpan.FromTicks(TotalExecutionTime.Ticks / ExecutionCount) : TimeSpan.Zero;
        public DateTime LastExecution { get; set; }
    }
}
