using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para análise de contexto
    /// </summary>
    public interface IContextAnalyzer
    {
        /// <summary>
        /// Analisa um contexto de jogo
        /// </summary>
        Task<ContextAnalysis> AnalyzeAsync(IGameContext context);
        
        /// <summary>
        /// Analisa mudanças entre contextos
        /// </summary>
        Task<ContextChangeAnalysis> AnalyzeChangesAsync(IGameContext oldContext, IGameContext newContext);
        
        /// <summary>
        /// Identifica padrões no contexto
        /// </summary>
        Task<List<ContextPattern>> IdentifyPatternsAsync(IGameContext context);
        
        /// <summary>
        /// Prediz próximo estado baseado no contexto
        /// </summary>
        Task<StatePrediction> PredictNextStateAsync(IGameContext context);
        
        /// <summary>
        /// Obtém recomendações baseadas no contexto
        /// </summary>
        Task<List<ContextRecommendation>> GetRecommendationsAsync(IGameContext context);
        
        /// <summary>
        /// Valida contexto
        /// </summary>
        Task<ContextValidation> ValidateContextAsync(IGameContext context);
        
        /// <summary>
        /// Obtém estatísticas do analisador
        /// </summary>
        Task<ContextAnalyzerStats> GetStatsAsync();
    }
    
    /// <summary>
    /// Análise de contexto
    /// </summary>
    public class ContextAnalysis
    {
        public string CurrentState { get; set; } = string.Empty;
        public Dictionary<string, double> StateProbabilities { get; set; } = new();
        public List<string> ActiveFlags { get; set; } = new();
        public Dictionary<string, object> KeyValues { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public double OverallHealth { get; set; }
        public double RiskLevel { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Análise de mudanças de contexto
    /// </summary>
    public class ContextChangeAnalysis
    {
        public List<string> ChangedFlags { get; set; } = new();
        public List<string> ChangedValues { get; set; } = new();
        public bool StateChanged { get; set; }
        public string? OldState { get; set; }
        public string? NewState { get; set; }
        public double ChangeSignificance { get; set; }
        public List<string> ImpactedBehaviors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Padrão identificado no contexto
    /// </summary>
    public class ContextPattern
    {
        public string PatternName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Dictionary<string, object> PatternData { get; set; } = new();
        public List<string> MatchedFlags { get; set; } = new();
        public List<string> MatchedValues { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Predição de estado
    /// </summary>
    public class StatePrediction
    {
        public string PredictedState { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<string> AlternativeStates { get; set; } = new();
        public Dictionary<string, double> StateProbabilities { get; set; } = new();
        public TimeSpan PredictedTimeToChange { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Recomendação de contexto
    /// </summary>
    public class ContextRecommendation
    {
        public string Recommendation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Priority { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validação de contexto
    /// </summary>
    public class ContextValidation
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public double OverallScore { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estatísticas do analisador de contexto
    /// </summary>
    public class ContextAnalyzerStats
    {
        public long AnalysisCount { get; set; }
        public long PatternCount { get; set; }
        public long PredictionCount { get; set; }
        public double AverageAnalysisTime { get; set; }
        public double AveragePredictionAccuracy { get; set; }
        public Dictionary<string, int> PatternCounts { get; set; } = new();
        public DateTime LastAnalysis { get; set; }
    }
}
