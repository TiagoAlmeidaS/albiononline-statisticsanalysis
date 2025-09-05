using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Saúde do DecisionEngine
    /// </summary>
    public class DecisionEngineHealth
    {
        /// <summary>
        /// Status geral de saúde
        /// </summary>
        public HealthStatus Status { get; set; }
        
        /// <summary>
        /// Pontuação de saúde (0-100)
        /// </summary>
        public double HealthScore { get; set; }
        
        /// <summary>
        /// Mensagem de status
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Problemas identificados
        /// </summary>
        public List<HealthIssue> Issues { get; set; } = new();
        
        /// <summary>
        /// Recomendações
        /// </summary>
        public List<HealthRecommendation> Recommendations { get; set; } = new();
        
        /// <summary>
        /// Timestamp da verificação
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Detalhes adicionais
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new();
    }
    
    /// <summary>
    /// Status de saúde
    /// </summary>
    public enum HealthStatus
    {
        Healthy = 0,
        Warning = 1,
        Critical = 2,
        Unknown = 3
    }
    
    /// <summary>
    /// Problema de saúde
    /// </summary>
    public class HealthIssue
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public HealthStatus Severity { get; set; }
        public DateTime FirstDetected { get; set; } = DateTime.UtcNow;
        public DateTime LastDetected { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = new();
    }
    
    /// <summary>
    /// Recomendação de saúde
    /// </summary>
    public class HealthRecommendation
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public HealthStatus Priority { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
