namespace StatisticsAnalysisTool.DecisionEngine.Models;

/// <summary>
/// Contexto geral do jogo que contém todas as informações relevantes para tomada de decisão
/// </summary>
public class GameContext
{
    /// <summary>
    /// ID único do contexto
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Timestamp de criação do contexto
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Tipo de contexto (Fishing, Gathering, Combat, etc.)
    /// </summary>
    public ContextType Type { get; set; }
    
    /// <summary>
    /// Prioridade do contexto (0-100)
    /// </summary>
    public int Priority { get; set; } = 50;
    
    /// <summary>
    /// Informações do jogador
    /// </summary>
    public PlayerInfo Player { get; set; } = new();
    
    /// <summary>
    /// Informações do ambiente
    /// </summary>
    public EnvironmentInfo Environment { get; set; } = new();
    
    /// <summary>
    /// Informações de recursos
    /// </summary>
    public ResourceInfo Resources { get; set; } = new();
    
    /// <summary>
    /// Informações de ameaças
    /// </summary>
    public ThreatInfo Threats { get; set; } = new();
    
    /// <summary>
    /// Informações de oportunidades
    /// </summary>
    public OpportunityInfo Opportunities { get; set; } = new();
    
    /// <summary>
    /// Estado do inventário
    /// </summary>
    public InventoryInfo Inventory { get; set; } = new();
    
    /// <summary>
    /// Estado do personagem
    /// </summary>
    public CharacterInfo Character { get; set; } = new();
    
    /// <summary>
    /// Dados específicos do contexto
    /// </summary>
    public Dictionary<string, object> ContextData { get; set; } = new();
    
    /// <summary>
    /// Histórico de decisões anteriores
    /// </summary>
    public List<DecisionHistory> DecisionHistory { get; set; } = new();
    
    /// <summary>
    /// Configurações específicas do contexto
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Tipos de contexto disponíveis
/// </summary>
public enum ContextType
{
    /// <summary>
    /// Contexto de pesca
    /// </summary>
    Fishing,
    
    /// <summary>
    /// Contexto de coleta de recursos
    /// </summary>
    Gathering,
    
    /// <summary>
    /// Contexto de combate
    /// </summary>
    Combat,
    
    /// <summary>
    /// Contexto de comércio
    /// </summary>
    Trading,
    
    /// <summary>
    /// Contexto de exploração
    /// </summary>
    Exploration,
    
    /// <summary>
    /// Contexto social
    /// </summary>
    Social,
    
    /// <summary>
    /// Contexto de movimento
    /// </summary>
    Movement,
    
    /// <summary>
    /// Contexto de inventário
    /// </summary>
    Inventory,
    
    /// <summary>
    /// Contexto customizado
    /// </summary>
    Custom
}

/// <summary>
/// Informações do jogador
/// </summary>
public class PlayerInfo
{
    public long ObjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float Energy { get; set; }
    public float MaxEnergy { get; set; }
    public int Level { get; set; }
    public string Guild { get; set; } = string.Empty;
    public bool IsInCombat { get; set; }
    public bool IsInSafeZone { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Informações do ambiente
/// </summary>
public class EnvironmentInfo
{
    public string MapName { get; set; } = string.Empty;
    public string ZoneType { get; set; } = string.Empty;
    public bool IsPvPZone { get; set; }
    public bool IsSafeZone { get; set; }
    public int PlayerCount { get; set; }
    public List<PlayerInfo> NearbyPlayers { get; set; } = new();
    public WeatherInfo Weather { get; set; } = new();
    public TimeInfo Time { get; set; } = new();
}

/// <summary>
/// Informações de recursos
/// </summary>
public class ResourceInfo
{
    public List<ResourceNode> AvailableNodes { get; set; } = new();
    public List<FishingZone> FishingZones { get; set; } = new();
    public List<MobInfo> AvailableMobs { get; set; } = new();
    public List<ItemInfo> AvailableItems { get; set; } = new();
}

/// <summary>
/// Informações de ameaças
/// </summary>
public class ThreatInfo
{
    public List<PlayerInfo> HostilePlayers { get; set; } = new();
    public List<MobInfo> AggressiveMobs { get; set; } = new();
    public List<EnvironmentalHazard> Hazards { get; set; } = new();
    public int ThreatLevel { get; set; } // 0-100
    public bool IsInDanger { get; set; }
}

/// <summary>
/// Informações de oportunidades
/// </summary>
public class OpportunityInfo
{
    public List<ResourceNode> HighValueNodes { get; set; } = new();
    public List<FishingZone> PrimeFishingSpots { get; set; } = new();
    public List<TradeOpportunity> TradeOpportunities { get; set; } = new();
    public List<QuestInfo> AvailableQuests { get; set; } = new();
    public int OpportunityScore { get; set; } // 0-100
}

/// <summary>
/// Informações do inventário
/// </summary>
public class InventoryInfo
{
    public int UsedSlots { get; set; }
    public int TotalSlots { get; set; }
    public int AvailableSlots => TotalSlots - UsedSlots;
    public List<ItemInfo> Items { get; set; } = new();
    public long TotalValue { get; set; }
    public bool IsFull => AvailableSlots <= 0;
    public bool IsNearFull => AvailableSlots <= 5;
}

/// <summary>
/// Informações do personagem
/// </summary>
public class CharacterInfo
{
    public Dictionary<string, int> Skills { get; set; } = new();
    public Dictionary<string, int> Equipment { get; set; } = new();
    public List<string> ActiveBuffs { get; set; } = new();
    public List<string> ActiveDebuffs { get; set; } = new();
    public int Fame { get; set; }
    public int Silver { get; set; }
    public int Gold { get; set; }
}

/// <summary>
/// Histórico de decisões
/// </summary>
public class DecisionHistory
{
    public string DecisionId { get; set; } = string.Empty;
    public string BehaviorType { get; set; } = string.Empty;
    public DecisionResult Result { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public bool WasSuccessful { get; set; }
}

/// <summary>
/// Informações de clima
/// </summary>
public class WeatherInfo
{
    public string Type { get; set; } = string.Empty;
    public int Intensity { get; set; } // 0-100
    public bool AffectsVisibility { get; set; }
    public bool AffectsMovement { get; set; }
}

/// <summary>
/// Informações de tempo
/// </summary>
public class TimeInfo
{
    public TimeSpan GameTime { get; set; }
    public TimeSpan RealTime { get; set; }
    public bool IsDay { get; set; }
    public bool IsNight { get; set; }
}

/// <summary>
/// Nó de recurso
/// </summary>
public class ResourceNode
{
    public long ObjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int Charges { get; set; }
    public int MaxCharges { get; set; }
    public long EstimatedValue { get; set; }
    public float Distance { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Zona de pesca
/// </summary>
public class FishingZone
{
    public long ObjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int FishCount { get; set; }
    public long EstimatedValue { get; set; }
    public float Distance { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Informações de mob
/// </summary>
public class MobInfo
{
    public long ObjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public bool IsAggressive { get; set; }
    public bool IsElite { get; set; }
    public long EstimatedValue { get; set; }
    public float Distance { get; set; }
}

/// <summary>
/// Informações de item
/// </summary>
public class ItemInfo
{
    public long ObjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public long EstimatedValue { get; set; }
    public int Quality { get; set; }
    public string Tier { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Distance { get; set; }
}

/// <summary>
/// Ameaça ambiental
/// </summary>
public class EnvironmentalHazard
{
    public string Type { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Radius { get; set; }
    public int Damage { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Oportunidade de comércio
/// </summary>
public class TradeOpportunity
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public long BuyPrice { get; set; }
    public long SellPrice { get; set; }
    public long Profit => SellPrice - BuyPrice;
    public float ProfitMargin => BuyPrice > 0 ? (float)Profit / BuyPrice : 0;
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// Informações de quest
/// </summary>
public class QuestInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public long Reward { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
