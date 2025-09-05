# 🧠 Exemplo de Uso do Motor de Decisão Universal

## 📋 Configuração Básica

### 1. **Registrar Serviços no DI Container**

```csharp
// Em Program.cs ou Startup.cs
services.AddDecisionEngineServices(config =>
{
    config.EnableAI = true;
    config.AIProvider = "OpenAI";
    config.AIModel = "gpt-4";
    config.MaxConcurrentDecisions = 5;
    config.DecisionTimeout = TimeSpan.FromSeconds(30);
});
```

### 2. **Configurar Comportamentos**

```csharp
public class GameBotController
{
    private readonly IUniversalDecisionEngine _decisionEngine;
    private readonly ILogger<GameBotController> _logger;
    
    public GameBotController(
        IUniversalDecisionEngine decisionEngine,
        ILogger<GameBotController> logger)
    {
        _decisionEngine = decisionEngine;
        _logger = logger;
    }
    
    public async Task InitializeBot()
    {
        // Registrar comportamentos
        await _decisionEngine.RegisterBehaviorAsync(new FishingBehavior(_logger));
        await _decisionEngine.RegisterBehaviorAsync(new GatheringBehavior(_logger));
        await _decisionEngine.RegisterBehaviorAsync(new CombatBehavior(_logger));
        await _decisionEngine.RegisterBehaviorAsync(new MovementBehavior(_logger));
        
        // Configurar IA
        var aiBridge = new OpenAIBridge(_logger);
        await aiBridge.ConfigureAsync(new Dictionary<string, object>
        {
            ["ApiKey"] = "your-openai-api-key",
            ["Model"] = "gpt-4",
            ["MaxTokens"] = 1000,
            ["Temperature"] = 0.7
        });
        
        await _decisionEngine.ConfigureAIAsync(aiBridge);
        
        // Iniciar motor
        await _decisionEngine.StartAsync();
    }
}
```

## 🎮 Exemplos de Uso

### **1. Decisão Simples de Pesca**

```csharp
public async Task HandleFishingContext()
{
    // Criar contexto de pesca
    var fishingContext = new GameContext
    {
        Type = ContextType.Fishing,
        Priority = 70,
        Player = new PlayerInfo
        {
            Name = "PlayerName",
            Level = 50,
            PositionX = 100,
            PositionY = 200,
            Health = 800,
            MaxHealth = 1000,
            Energy = 600,
            MaxEnergy = 1000,
            IsInCombat = false,
            IsInSafeZone = true
        },
        Resources = new ResourceInfo
        {
            FishingZones = new List<FishingZone>
            {
                new()
                {
                    ObjectId = 12345,
                    Type = "Freshwater",
                    PositionX = 150,
                    PositionY = 250,
                    FishCount = 5,
                    EstimatedValue = 5000,
                    Distance = 50,
                    IsActive = true
                }
            }
        },
        Threats = new ThreatInfo
        {
            ThreatLevel = 10,
            IsInDanger = false
        },
        Inventory = new InventoryInfo
        {
            UsedSlots = 20,
            TotalSlots = 50,
            TotalValue = 100000
        }
    };
    
    // Tomar decisão
    var decision = await _decisionEngine.MakeDecisionAsync(fishingContext);
    
    if (decision.IsSuccessful)
    {
        Console.WriteLine($"Decisão tomada: {decision.Action.Name}");
        Console.WriteLine($"Confiança: {decision.Confidence}%");
        Console.WriteLine($"Razão: {decision.Reason}");
        
        // Executar ação
        var behavior = _decisionEngine.GetRegisteredBehaviors()
            .FirstOrDefault(b => b.Name == decision.BehaviorType);
        
        if (behavior != null)
        {
            var result = await behavior.ExecuteActionAsync(decision);
            Console.WriteLine($"Ação executada: {result.IsSuccessful}");
        }
    }
}
```

### **2. Múltiplas Decisões Simultâneas**

```csharp
public async Task HandleMultipleContexts()
{
    var contexts = new List<GameContext>
    {
        CreateFishingContext(),
        CreateGatheringContext(),
        CreateCombatContext()
    };
    
    // Tomar decisões para todos os contextos
    var decisions = await _decisionEngine.MakeDecisionsAsync(contexts);
    
    foreach (var decision in decisions)
    {
        Console.WriteLine($"Contexto: {decision.Context.Type}");
        Console.WriteLine($"Decisão: {decision.Action.Name}");
        Console.WriteLine($"Confiança: {decision.Confidence}%");
        Console.WriteLine($"Prioridade: {decision.Priority}");
        Console.WriteLine("---");
    }
}
```

### **3. Decisão Forçada**

```csharp
public async Task ForceSpecificDecision()
{
    var context = CreateFishingContext();
    
    var request = new DecisionRequest
    {
        Context = context,
        RequestedBehaviorType = "FishingBehavior",
        RequestedAction = "StartFishing",
        Priority = 90,
        UseAI = true
    };
    
    var decision = await _decisionEngine.ForceDecisionAsync(request);
    
    Console.WriteLine($"Decisão forçada: {decision.Action.Name}");
    Console.WriteLine($"Forçada: {decision.Data.ContainsKey("Forced")}");
}
```

### **4. Integração com IA**

```csharp
public async Task UseAIForComplexDecision()
{
    var complexContext = new GameContext
    {
        Type = ContextType.Custom,
        Priority = 80,
        Player = new PlayerInfo
        {
            Name = "PlayerName",
            Level = 75,
            PositionX = 500,
            PositionY = 300,
            Health = 900,
            MaxHealth = 1000,
            Energy = 800,
            MaxEnergy = 1000,
            IsInCombat = false,
            IsInSafeZone = false
        },
        Resources = new ResourceInfo
        {
            FishingZones = GetNearbyFishingZones(),
            AvailableNodes = GetNearbyResourceNodes(),
            AvailableMobs = GetNearbyMobs()
        },
        Threats = new ThreatInfo
        {
            HostilePlayers = GetNearbyHostilePlayers(),
            ThreatLevel = 60,
            IsInDanger = true
        },
        Opportunities = new OpportunityInfo
        {
            HighValueNodes = GetHighValueNodes(),
            OpportunityScore = 85
        }
    };
    
    // IA tomará uma decisão complexa considerando todos os fatores
    var decision = await _decisionEngine.MakeDecisionAsync(complexContext);
    
    Console.WriteLine($"IA decidiu: {decision.Action.Name}");
    Console.WriteLine($"Confiança: {decision.Confidence}%");
    Console.WriteLine($"Razão: {decision.Reason}");
    Console.WriteLine($"É decisão de IA: {decision.IsAIDecision}");
}
```

### **5. Monitoramento de Eventos**

```csharp
public async Task SetupEventMonitoring()
{
    // Subscrever eventos do motor
    _decisionEngine.DecisionMade += OnDecisionMade;
    _decisionEngine.ContextChanged += OnContextChanged;
    _decisionEngine.BehaviorExecuted += OnBehaviorExecuted;
    
    // Subscrever eventos de comportamentos específicos
    var fishingBehavior = _decisionEngine.GetRegisteredBehaviors()
        .OfType<FishingBehavior>()
        .FirstOrDefault();
    
    if (fishingBehavior != null)
    {
        fishingBehavior.Executed += OnFishingExecuted;
    }
}

private void OnDecisionMade(object? sender, DecisionMadeEventArgs e)
{
    Console.WriteLine($"Decisão tomada: {e.Decision.Action.Name}");
    Console.WriteLine($"Tempo de processamento: {e.ProcessingTime.TotalMilliseconds}ms");
    Console.WriteLine($"Contexto: {e.Context.Type}");
}

private void OnContextChanged(object? sender, ContextChangedEventArgs e)
{
    Console.WriteLine($"Contexto mudou: {e.OldContext.Type} -> {e.NewContext.Type}");
}

private void OnBehaviorExecuted(object? sender, BehaviorExecutedEventArgs e)
{
    Console.WriteLine($"Comportamento executado: {e.Behavior.Name}");
    Console.WriteLine($"Sucesso: {e.Result.IsSuccessful}");
    Console.WriteLine($"Tempo de execução: {e.Result.ExecutionTime.TotalMilliseconds}ms");
}
```

### **6. Configuração Dinâmica**

```csharp
public async Task ConfigureBehaviorsDynamically()
{
    // Habilitar/desabilitar comportamentos
    await _decisionEngine.SetBehaviorEnabledAsync<FishingBehavior>(true);
    await _decisionEngine.SetBehaviorEnabledAsync<CombatBehavior>(false);
    
    // Alterar prioridades
    await _decisionEngine.SetBehaviorPriorityAsync<FishingBehavior>(80);
    await _decisionEngine.SetBehaviorPriorityAsync<GatheringBehavior>(60);
    
    // Configurar comportamentos específicos
    var fishingBehavior = _decisionEngine.GetRegisteredBehaviors()
        .OfType<FishingBehavior>()
        .FirstOrDefault();
    
    if (fishingBehavior != null)
    {
        await fishingBehavior.ConfigureAsync(new Dictionary<string, object>
        {
            ["AutoStart"] = true,
            ["MinigameResolution"] = true,
            ["MaxAttempts"] = 15,
            ["CooldownSeconds"] = 3,
            ["MinHealthPercentage"] = 60,
            ["MinEnergyPercentage"] = 40
        });
    }
}
```

### **7. Obter Estatísticas**

```csharp
public void DisplayStats()
{
    var stats = _decisionEngine.GetStats();
    
    Console.WriteLine("=== Estatísticas do Motor de Decisão ===");
    Console.WriteLine($"Total de decisões: {stats.TotalDecisions}");
    Console.WriteLine($"Decisões bem-sucedidas: {stats.SuccessfulDecisions}");
    Console.WriteLine($"Taxa de sucesso: {stats.SuccessRate:P2}");
    Console.WriteLine($"Tempo médio de processamento: {stats.AverageProcessingTime.TotalMilliseconds}ms");
    Console.WriteLine($"Comportamentos ativos: {stats.ActiveBehaviors}/{stats.TotalBehaviors}");
    Console.WriteLine($"Última decisão: {stats.LastDecisionTime}");
    
    Console.WriteLine("\n=== Uso por Comportamento ===");
    foreach (var kvp in stats.BehaviorUsageCount)
    {
        var successRate = stats.BehaviorSuccessRate.GetValueOrDefault(kvp.Key, 0);
        Console.WriteLine($"{kvp.Key}: {kvp.Value} usos, {successRate:P2} sucesso");
    }
    
    // Estatísticas de comportamentos específicos
    foreach (var behavior in _decisionEngine.GetRegisteredBehaviors())
    {
        var behaviorStats = behavior.GetStats();
        Console.WriteLine($"\n{behavior.Name}:");
        Console.WriteLine($"  Total de execuções: {behaviorStats.TotalExecutions}");
        Console.WriteLine($"  Taxa de sucesso: {behaviorStats.SuccessRate:P2}");
        Console.WriteLine($"  Tempo médio: {behaviorStats.AverageExecutionTime.TotalMilliseconds}ms");
        Console.WriteLine($"  Saudável: {behaviorStats.IsHealthy}");
    }
}
```

### **8. Sistema de Plugins**

```csharp
public async Task LoadCustomBehavior()
{
    // Carregar comportamento customizado
    var customBehavior = new CustomFishingBehavior(_logger);
    
    // Configurar comportamento
    await customBehavior.ConfigureAsync(new Dictionary<string, object>
    {
        ["CustomParameter1"] = "value1",
        ["CustomParameter2"] = 42,
        ["EnableAdvancedFeatures"] = true
    });
    
    // Registrar comportamento
    await _decisionEngine.RegisterBehaviorAsync(customBehavior);
    
    Console.WriteLine("Comportamento customizado carregado e registrado");
}

// Exemplo de comportamento customizado
public class CustomFishingBehavior : BaseBehavior
{
    public override string Name => "CustomFishingBehavior";
    public override string Version => "1.0.0";
    public override string Description => "Comportamento de pesca customizado";
    public override ContextType SupportedContextType => ContextType.Fishing;
    
    public CustomFishingBehavior(ILogger logger) : base(logger) { }
    
    protected override async Task<DecisionResult> OnMakeDecisionAsync(GameContext context)
    {
        // Lógica customizada de decisão
        return new DecisionResult
        {
            Confidence = 95,
            Priority = 90,
            Reason = "Decisão customizada baseada em lógica específica",
            IsSuccessful = true,
            Action = new DecisionAction
            {
                Type = ActionType.Fishing,
                Name = "CustomFishingAction",
                Description = "Ação de pesca customizada",
                EstimatedDuration = TimeSpan.FromMinutes(3)
            }
        };
    }
    
    protected override async Task<ExecutionResult> OnExecuteActionAsync(DecisionResult decision)
    {
        // Lógica customizada de execução
        return new ExecutionResult
        {
            IsSuccessful = true,
            Message = "Ação customizada executada com sucesso"
        };
    }
    
    public override List<ActionInfo> GetSupportedActions()
    {
        return new List<ActionInfo>
        {
            new()
            {
                Name = "CustomFishingAction",
                Description = "Ação de pesca customizada",
                Type = ActionType.Fishing,
                EstimatedDuration = TimeSpan.FromMinutes(3)
            }
        };
    }
    
    public override List<ConditionInfo> GetSupportedConditions()
    {
        return new List<ConditionInfo>();
    }
}
```

## 🔧 Configurações Avançadas

### **1. Configuração de IA**

```csharp
var aiConfig = new Dictionary<string, object>
{
    ["ApiKey"] = "your-openai-api-key",
    ["Model"] = "gpt-4",
    ["MaxTokens"] = 1500,
    ["Temperature"] = 0.5,
    ["Timeout"] = 45,
    ["EnableLearning"] = true,
    ["LearningRate"] = 0.1
};

await aiBridge.ConfigureAsync(aiConfig);
```

### **2. Configuração de Comportamentos**

```csharp
var behaviorConfigs = new Dictionary<string, Dictionary<string, object>>
{
    ["FishingBehavior"] = new()
    {
        ["AutoStart"] = true,
        ["MinigameResolution"] = true,
        ["MaxAttempts"] = 20,
        ["CooldownSeconds"] = 2
    },
    ["GatheringBehavior"] = new()
    {
        ["AutoGather"] = true,
        ["PrioritizeHighValue"] = true,
        ["MaxDistance"] = 200
    },
    ["CombatBehavior"] = new()
    {
        ["AutoFight"] = false,
        ["FleeOnLowHealth"] = true,
        ["MinHealthToFight"] = 70
    }
};

foreach (var kvp in behaviorConfigs)
{
    var behavior = _decisionEngine.GetRegisteredBehaviors()
        .FirstOrDefault(b => b.Name == kvp.Key);
    
    if (behavior != null)
    {
        await behavior.ConfigureAsync(kvp.Value);
    }
}
```

### **3. Sistema de Fallback**

```csharp
public async Task<DecisionResult> MakeDecisionWithFallback(GameContext context)
{
    try
    {
        // Tentar com IA primeiro
        var decision = await _decisionEngine.MakeDecisionAsync(context);
        
        if (decision.IsAIDecision && decision.Confidence > 80)
        {
            return decision;
        }
        
        // Fallback para regras tradicionais
        var fallbackDecision = await _decisionEngine.MakeDecisionAsync(context);
        fallbackDecision.Data["FallbackUsed"] = true;
        
        return fallbackDecision;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao tomar decisão, usando fallback");
        
        // Fallback de emergência
        return new DecisionResult
        {
            Confidence = 50,
            Priority = 10,
            Reason = "Fallback de emergência",
            IsSuccessful = true,
            Action = new DecisionAction
            {
                Type = ActionType.Wait,
                Name = "EmergencyWait",
                Description = "Aguardar até que o sistema se recupere",
                EstimatedDuration = TimeSpan.FromSeconds(10)
            }
        };
    }
}
```

Este sistema permite crescimento incremental e integração futura com IA, mantendo flexibilidade e extensibilidade! 🧠
