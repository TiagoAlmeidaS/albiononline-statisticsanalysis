using StatisticsAnalysisTool.DecisionEngine.Models;

namespace StatisticsAnalysisTool.DecisionEngine.AI;

/// <summary>
/// Interface para integração com sistemas de IA
/// </summary>
public interface IAIBridge
{
    /// <summary>
    /// Nome do provedor de IA
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// Versão da IA
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// Indica se a IA está disponível
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Evento disparado quando a IA toma uma decisão
    /// </summary>
    event EventHandler<AIDecisionMadeEventArgs>? DecisionMade;
    
    /// <summary>
    /// Evento disparado quando há erro na IA
    /// </summary>
    event EventHandler<AIErrorEventArgs>? ErrorOccurred;
    
    /// <summary>
    /// Inicializa a conexão com a IA
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Finaliza a conexão com a IA
    /// </summary>
    Task ShutdownAsync();
    
    /// <summary>
    /// Toma uma decisão baseada no contexto usando IA
    /// </summary>
    Task<DecisionResult> MakeDecisionAsync(GameContext context);
    
    /// <summary>
    /// Avalia a qualidade de uma decisão
    /// </summary>
    Task<DecisionQuality> EvaluateDecisionAsync(DecisionResult decision, GameContext context);
    
    /// <summary>
    /// Aprende com o resultado de uma decisão
    /// </summary>
    Task LearnFromResultAsync(DecisionResult decision, ExecutionResult result, GameContext context);
    
    /// <summary>
    /// Obtém sugestões para melhorar o comportamento
    /// </summary>
    Task<List<BehaviorSuggestion>> GetBehaviorSuggestionsAsync(GameContext context);
    
    /// <summary>
    /// Obtém estatísticas da IA
    /// </summary>
    AIStats GetStats();
    
    /// <summary>
    /// Configura parâmetros da IA
    /// </summary>
    Task ConfigureAsync(Dictionary<string, object> configuration);
    
    /// <summary>
    /// Valida se a configuração está correta
    /// </summary>
    Task<bool> ValidateConfigurationAsync();
}

/// <summary>
/// Argumentos do evento de decisão tomada pela IA
/// </summary>
public class AIDecisionMadeEventArgs : EventArgs
{
    public DecisionResult Decision { get; }
    public GameContext Context { get; }
    public TimeSpan ProcessingTime { get; }
    public double Confidence { get; }
    public DateTime Timestamp { get; }
    
    public AIDecisionMadeEventArgs(DecisionResult decision, GameContext context, TimeSpan processingTime, double confidence)
    {
        Decision = decision;
        Context = context;
        ProcessingTime = processingTime;
        Confidence = confidence;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de erro na IA
/// </summary>
public class AIErrorEventArgs : EventArgs
{
    public Exception Error { get; }
    public string Context { get; }
    public DateTime Timestamp { get; }
    
    public AIErrorEventArgs(Exception error, string context)
    {
        Error = error;
        Context = context;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Qualidade de uma decisão avaliada pela IA
/// </summary>
public class DecisionQuality
{
    public double Score { get; set; } // 0-100
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sugestão de comportamento da IA
/// </summary>
public class BehaviorSuggestion
{
    public string BehaviorType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Estatísticas da IA
/// </summary>
public class AIStats
{
    public int TotalDecisions { get; set; }
    public int SuccessfulDecisions { get; set; }
    public int FailedDecisions { get; set; }
    public double SuccessRate => TotalDecisions > 0 ? (double)SuccessfulDecisions / TotalDecisions : 0;
    public TimeSpan AverageProcessingTime { get; set; }
    public double AverageConfidence { get; set; }
    public int LearningCycles { get; set; }
    public DateTime LastDecisionTime { get; set; }
    public DateTime LastLearningTime { get; set; }
    public bool IsHealthy { get; set; } = true;
    public string LastError { get; set; } = string.Empty;
}
