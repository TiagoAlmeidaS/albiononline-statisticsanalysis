using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.DecisionEngine.Abstractions
{
    /// <summary>
    /// Interface para integração com IA
    /// </summary>
    public interface IAIBridge
    {
        /// <summary>
        /// Nome do provedor de IA
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// Se a IA está disponível
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Se a IA está configurada
        /// </summary>
        bool IsConfigured { get; }
        
        /// <summary>
        /// Inicializa a IA
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Finaliza a IA
        /// </summary>
        Task ShutdownAsync();
        
        /// <summary>
        /// Processa uma consulta de IA
        /// </summary>
        Task<AIResponse> ProcessQueryAsync(AIQuery query);
        
        /// <summary>
        /// Analisa um contexto de jogo
        /// </summary>
        Task<AIAnalysis> AnalyzeContextAsync(IGameContext context);
        
        /// <summary>
        /// Sugere uma decisão baseada no contexto
        /// </summary>
        Task<DecisionSuggestion> SuggestDecisionAsync(IGameContext context);
        
        /// <summary>
        /// Valida uma decisão
        /// </summary>
        Task<DecisionValidation> ValidateDecisionAsync(DecisionResult decision, IGameContext context);
        
        /// <summary>
        /// Obtém configuração da IA
        /// </summary>
        Task<Dictionary<string, object>> GetConfigurationAsync();
        
        /// <summary>
        /// Atualiza configuração da IA
        /// </summary>
        Task UpdateConfigurationAsync(Dictionary<string, object> config);
        
        /// <summary>
        /// Obtém estatísticas da IA
        /// </summary>
        Task<AIStats> GetStatsAsync();
        
        /// <summary>
        /// Reseta estatísticas da IA
        /// </summary>
        Task ResetStatsAsync();
    }
    
    /// <summary>
    /// Consulta para IA
    /// </summary>
    public class AIQuery
    {
        public string Query { get; set; } = string.Empty;
        public IGameContext? Context { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Model { get; set; } = string.Empty;
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
    }
    
    /// <summary>
    /// Resposta da IA
    /// </summary>
    public class AIResponse
    {
        public string Response { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Análise de contexto pela IA
    /// </summary>
    public class AIAnalysis
    {
        public string Analysis { get; set; } = string.Empty;
        public Dictionary<string, double> Scores { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public Dictionary<string, object> Insights { get; set; } = new();
        public double OverallConfidence { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Sugestão de decisão pela IA
    /// </summary>
    public class DecisionSuggestion
    {
        public string SuggestedAction { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public Dictionary<string, object> Alternatives { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validação de decisão pela IA
    /// </summary>
    public class DecisionValidation
    {
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
        public double RiskScore { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Estatísticas da IA
    /// </summary>
    public class AIStats
    {
        public long QueryCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public double SuccessRate => QueryCount > 0 ? (double)SuccessCount / QueryCount : 0;
        public TimeSpan TotalProcessingTime { get; set; }
        public TimeSpan AverageProcessingTime => 
            QueryCount > 0 ? TimeSpan.FromTicks(TotalProcessingTime.Ticks / QueryCount) : TimeSpan.Zero;
        public DateTime LastQuery { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}
