using System;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Log do DecisionEngine
    /// </summary>
    public class DecisionEngineLog
    {
        /// <summary>
        /// Nível do log
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// Mensagem do log
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Categoria do log
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Nome do comportamento (se aplicável)
        /// </summary>
        public string? BehaviorName { get; set; }
        
        /// <summary>
        /// Nome da regra (se aplicável)
        /// </summary>
        public string? RuleName { get; set; }
        
        /// <summary>
        /// Nome da ação (se aplicável)
        /// </summary>
        public string? ActionName { get; set; }
        
        /// <summary>
        /// Dados adicionais
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();
        
        /// <summary>
        /// Timestamp do log
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// ID único do log
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Níveis de log
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }
}
