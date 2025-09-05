namespace StatisticsAnalysisTool.DecisionEngine.Models;

/// <summary>
/// Resultado de uma decisão tomada pelo motor
/// </summary>
public class DecisionResult
{
    /// <summary>
    /// ID único da decisão
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp da decisão
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Tipo de comportamento que tomou a decisão
    /// </summary>
    public string BehaviorType { get; set; } = string.Empty;
    
    /// <summary>
    /// Ação a ser executada
    /// </summary>
    public DecisionAction Action { get; set; } = new();
    
    /// <summary>
    /// Confiança na decisão (0-100)
    /// </summary>
    public int Confidence { get; set; }
    
    /// <summary>
    /// Prioridade da decisão (0-100)
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Razão da decisão
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Contexto usado para a decisão
    /// </summary>
    public GameContext Context { get; set; } = new();
    
    /// <summary>
    /// Dados adicionais da decisão
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Indica se a decisão foi tomada por IA
    /// </summary>
    public bool IsAIDecision { get; set; }
    
    /// <summary>
    /// Indica se a decisão foi bem-sucedida
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Tempo de processamento da decisão
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Ação a ser executada
/// </summary>
public class DecisionAction
{
    /// <summary>
    /// Tipo da ação
    /// </summary>
    public ActionType Type { get; set; }
    
    /// <summary>
    /// Nome da ação
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da ação
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Parâmetros da ação
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    /// <summary>
    /// Tempo estimado para execução
    /// </summary>
    public TimeSpan EstimatedDuration { get; set; }
    
    /// <summary>
    /// Recursos necessários para execução
    /// </summary>
    public List<string> RequiredResources { get; set; } = new();
    
    /// <summary>
    /// Indica se a ação pode ser cancelada
    /// </summary>
    public bool CanBeCancelled { get; set; } = true;
    
    /// <summary>
    /// Indica se a ação é crítica
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// Condições para execução
    /// </summary>
    public List<ActionCondition> Conditions { get; set; } = new();
}

/// <summary>
/// Tipos de ação disponíveis
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Ação de pesca
    /// </summary>
    Fishing,
    
    /// <summary>
    /// Ação de coleta
    /// </summary>
    Gathering,
    
    /// <summary>
    /// Ação de combate
    /// </summary>
    Combat,
    
    /// <summary>
    /// Ação de movimento
    /// </summary>
    Movement,
    
    /// <summary>
    /// Ação de comércio
    /// </summary>
    Trading,
    
    /// <summary>
    /// Ação de inventário
    /// </summary>
    Inventory,
    
    /// <summary>
    /// Ação de comunicação
    /// </summary>
    Communication,
    
    /// <summary>
    /// Ação de espera
    /// </summary>
    Wait,
    
    /// <summary>
    /// Ação de cancelamento
    /// </summary>
    Cancel,
    
    /// <summary>
    /// Ação customizada
    /// </summary>
    Custom
}

/// <summary>
/// Condição para execução de ação
/// </summary>
public class ActionCondition
{
    /// <summary>
    /// Tipo da condição
    /// </summary>
    public ConditionType Type { get; set; }
    
    /// <summary>
    /// Parâmetros da condição
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    /// <summary>
    /// Indica se a condição é obrigatória
    /// </summary>
    public bool IsRequired { get; set; } = true;
    
    /// <summary>
    /// Mensagem de erro se a condição falhar
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de condição
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// Condição de saúde
    /// </summary>
    Health,
    
    /// <summary>
    /// Condição de energia
    /// </summary>
    Energy,
    
    /// <summary>
    /// Condição de inventário
    /// </summary>
    Inventory,
    
    /// <summary>
    /// Condição de posição
    /// </summary>
    Position,
    
    /// <summary>
    /// Condição de tempo
    /// </summary>
    Time,
    
    /// <summary>
    /// Condição de recursos
    /// </summary>
    Resources,
    
    /// <summary>
    /// Condição de ameaças
    /// </summary>
    Threats,
    
    /// <summary>
    /// Condição customizada
    /// </summary>
    Custom
}

/// <summary>
/// Solicitação de decisão
/// </summary>
public class DecisionRequest
{
    /// <summary>
    /// ID único da solicitação
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp da solicitação
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Contexto para a decisão
    /// </summary>
    public GameContext Context { get; set; } = new();
    
    /// <summary>
    /// Tipo de comportamento solicitado
    /// </summary>
    public string RequestedBehaviorType { get; set; } = string.Empty;
    
    /// <summary>
    /// Ação específica solicitada
    /// </summary>
    public string RequestedAction { get; set; } = string.Empty;
    
    /// <summary>
    /// Prioridade da solicitação
    /// </summary>
    public int Priority { get; set; } = 50;
    
    /// <summary>
    /// Timeout para a decisão
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Indica se deve usar IA
    /// </summary>
    public bool UseAI { get; set; }
    
    /// <summary>
    /// Parâmetros adicionais
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Resultado da execução de uma ação
/// </summary>
public class ExecutionResult
{
    /// <summary>
    /// ID único do resultado
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp da execução
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Ação executada
    /// </summary>
    public DecisionAction Action { get; set; } = new();
    
    /// <summary>
    /// Indica se a execução foi bem-sucedida
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Mensagem de resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Tempo de execução
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }
    
    /// <summary>
    /// Dados do resultado
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Erro ocorrido durante a execução
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Impacto da execução no contexto
    /// </summary>
    public ContextImpact Impact { get; set; } = new();
}

/// <summary>
/// Impacto da execução no contexto
/// </summary>
public class ContextImpact
{
    /// <summary>
    /// Mudanças na posição do jogador
    /// </summary>
    public PositionChange? PositionChange { get; set; }
    
    /// <summary>
    /// Mudanças no inventário
    /// </summary>
    public List<InventoryChange> InventoryChanges { get; set; } = new();
    
    /// <summary>
    /// Mudanças na saúde
    /// </summary>
    public HealthChange? HealthChange { get; set; }
    
    /// <summary>
    /// Mudanças na energia
    /// </summary>
    public EnergyChange? EnergyChange { get; set; }
    
    /// <summary>
    /// Mudanças nos recursos
    /// </summary>
    public List<ResourceChange> ResourceChanges { get; set; } = new();
    
    /// <summary>
    /// Mudanças nas ameaças
    /// </summary>
    public List<ThreatChange> ThreatChanges { get; set; } = new();
    
    /// <summary>
    /// Mudanças nas oportunidades
    /// </summary>
    public List<OpportunityChange> OpportunityChanges { get; set; } = new();
}

/// <summary>
/// Mudança de posição
/// </summary>
public class PositionChange
{
    public float OldX { get; set; }
    public float OldY { get; set; }
    public float NewX { get; set; }
    public float NewY { get; set; }
    public float Distance => (float)Math.Sqrt(Math.Pow(NewX - OldX, 2) + Math.Pow(NewY - OldY, 2));
}

/// <summary>
/// Mudança no inventário
/// </summary>
public class InventoryChange
{
    public string ItemName { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public long ValueChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Modified
}

/// <summary>
/// Mudança na saúde
/// </summary>
public class HealthChange
{
    public float OldHealth { get; set; }
    public float NewHealth { get; set; }
    public float Change => NewHealth - OldHealth;
    public float PercentageChange => OldHealth > 0 ? (Change / OldHealth) * 100 : 0;
}

/// <summary>
/// Mudança na energia
/// </summary>
public class EnergyChange
{
    public float OldEnergy { get; set; }
    public float NewEnergy { get; set; }
    public float Change => NewEnergy - OldEnergy;
    public float PercentageChange => OldEnergy > 0 ? (Change / OldEnergy) * 100 : 0;
}

/// <summary>
/// Mudança em recursos
/// </summary>
public class ResourceChange
{
    public string ResourceType { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public long ValueChange { get; set; }
    public string ChangeType { get; set; } = string.Empty;
}

/// <summary>
/// Mudança em ameaças
/// </summary>
public class ThreatChange
{
    public string ThreatType { get; set; } = string.Empty;
    public int LevelChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Modified
}

/// <summary>
/// Mudança em oportunidades
/// </summary>
public class OpportunityChange
{
    public string OpportunityType { get; set; } = string.Empty;
    public int ScoreChange { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Modified
}
