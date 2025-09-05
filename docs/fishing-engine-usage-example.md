# 🎣 Exemplo de Uso do Sistema de Pesca

## 📋 Configuração Básica

### 1. **Registrar Serviços no DI Container**

```csharp
// Em Program.cs ou Startup.cs
services.AddFishingServices(config =>
{
    config.EnableAutoFishing = true;
    config.EnableMinigameResolution = true;
    config.BobberDetectionThreshold = 0.7;
    config.MaxFishingAttempts = 10;
    config.FishingTimeout = TimeSpan.FromMinutes(5);
    config.MinigameTimeout = TimeSpan.FromSeconds(30);
    config.EnableDebugMode = true;
});
```

### 2. **Injetar e Usar o Sistema**

```csharp
public class FishingController
{
    private readonly IFishingEngine _fishingEngine;
    private readonly IFishingZoneService _zoneService;
    private readonly SATFishingIntegration _integration;
    
    public FishingController(
        IFishingEngine fishingEngine,
        IFishingZoneService zoneService,
        SATFishingIntegration integration)
    {
        _fishingEngine = fishingEngine;
        _zoneService = zoneService;
        _integration = integration;
    }
    
    public async Task StartFishingSystem()
    {
        // Iniciar sistema
        await _integration.StartAsync();
        
        // Iniciar pesca automática
        var success = await _integration.StartAutoFishingAsync();
        if (success)
        {
            Console.WriteLine("Pesca automática iniciada!");
        }
    }
    
    public async Task StopFishingSystem()
    {
        await _integration.StopAsync();
    }
}
```

## 🔄 Fluxo de Funcionamento

### **1. Detecção de Zona de Pesca**
```csharp
// O sistema automaticamente detecta zonas quando o evento NewFishingZoneObject é recebido
// Não é necessário código adicional - funciona automaticamente
```

### **2. Início da Pesca**
```csharp
// Quando uma zona é detectada, você pode iniciar a pesca
var nearestZone = _zoneService.GetNearestZone(playerX, playerY);
if (nearestZone != null)
{
    await _fishingEngine.StartFishingAsync(nearestZone);
}
```

### **3. Monitoramento de Eventos**
```csharp
// Subscrever eventos para monitoramento
_fishingEngine.StateChanged += (sender, e) =>
{
    Console.WriteLine($"Estado mudou: {e.OldState} -> {e.NewState}");
};

_fishingEngine.BobberDetected += (sender, e) =>
{
    Console.WriteLine($"Bobber detectado em ({e.Bobber.PositionX}, {e.Bobber.PositionY})");
};

_fishingEngine.MinigameStarted += (sender, e) =>
{
    Console.WriteLine("Minigame iniciado!");
};

_fishingEngine.FishingCompleted += (sender, e) =>
{
    Console.WriteLine($"Pesca completada: {(e.IsSuccessful ? "Sucesso" : "Falha")}");
    Console.WriteLine($"Itens capturados: {e.CaughtItems.Count}");
};
```

## 🎮 Integração com SAT

### **1. Registrar Event Handlers**
```csharp
// Os event handlers são automaticamente registrados quando você usa AddFishingServices()
// Eles processam os eventos do Albion Online e integram com o sistema de pesca
```

### **2. Usar com GatheringController**
```csharp
// O sistema automaticamente integra com o GatheringController existente
// Itens pescados são automaticamente adicionados ao sistema de gathering
```

### **3. Obter Estatísticas**
```csharp
var stats = _integration.GetStats();
Console.WriteLine($"Estado atual: {stats.CurrentState}");
Console.WriteLine($"Zonas ativas: {stats.ZoneStats.ActiveZones}");
Console.WriteLine($"Taxa de sucesso: {stats.EngineStats.SuccessRate:P2}");
```

## 🔧 Configurações Avançadas

### **1. Configuração de Detecção de Bobber**
```csharp
var config = new FishingConfig
{
    BobberDetectionThreshold = 0.8, // Threshold mais alto para maior precisão
    BobberTemplatePath = "custom/bobber_template.png",
    BobberInWaterTemplatePath = "custom/bobber_in_water_template.png"
};
```

### **2. Configuração de Minigame**
```csharp
var config = new FishingConfig
{
    MinigameTimeout = TimeSpan.FromSeconds(45), // Timeout mais longo
    EnableMinigameResolution = true
};
```

### **3. Configuração de Debug**
```csharp
var config = new FishingConfig
{
    EnableDebugMode = true, // Ativa logs detalhados
    MaxFishingAttempts = 5 // Limite de tentativas
};
```

## 📊 Monitoramento e Logs

### **1. Logs Automáticos**
O sistema gera logs detalhados para todas as operações:
- Detecção de zonas
- Início/fim de pesca
- Detecção de bobber
- Resolução de minigame
- Erros e exceções

### **2. Métricas de Performance**
```csharp
var stats = _integration.GetStats();

// Estatísticas do engine
Console.WriteLine($"Tentativas totais: {stats.EngineStats.TotalAttempts}");
Console.WriteLine($"Taxa de sucesso: {stats.EngineStats.SuccessRate:P2}");
Console.WriteLine($"Tempo médio de pesca: {stats.EngineStats.AverageFishingTime}");

// Estatísticas de zonas
Console.WriteLine($"Zonas detectadas: {stats.ZoneStats.TotalZonesDetected}");
Console.WriteLine($"Zonas ativas: {stats.ZoneStats.ActiveZones}");

// Estatísticas de minigame
Console.WriteLine($"Minigames resolvidos: {stats.MinigameStats.SuccessfulMinigames}");
Console.WriteLine($"Taxa de sucesso do minigame: {stats.MinigameStats.SuccessRate:P2}");
```

## 🚨 Tratamento de Erros

### **1. Erros de Automação**
```csharp
try
{
    await _fishingEngine.StartFishingAsync(zone);
}
catch (AutomationException ex)
{
    Console.WriteLine($"Erro de automação: {ex.Message}");
    // Implementar fallback ou retry
}
```

### **2. Erros de Detecção**
```csharp
try
{
    await _bobberService.StartTrackingAsync(bobber);
}
catch (DetectionException ex)
{
    Console.WriteLine($"Erro de detecção: {ex.Message}");
    // Implementar fallback
}
```

### **3. Timeouts**
```csharp
// O sistema automaticamente trata timeouts
// Configurar timeouts apropriados no FishingConfig
var config = new FishingConfig
{
    FishingTimeout = TimeSpan.FromMinutes(3),
    MinigameTimeout = TimeSpan.FromSeconds(20)
};
```

## 🔄 Ciclo de Vida Completo

```csharp
public async Task CompleteFishingSession()
{
    try
    {
        // 1. Iniciar sistema
        await _integration.StartAsync();
        
        // 2. Aguardar detecção de zona
        while (_zoneService.ActiveZones.Count == 0)
        {
            await Task.Delay(1000);
        }
        
        // 3. Iniciar pesca
        var success = await _integration.StartAutoFishingAsync();
        if (!success)
        {
            Console.WriteLine("Falha ao iniciar pesca");
            return;
        }
        
        // 4. Aguardar conclusão
        var fishingCompleted = new TaskCompletionSource<bool>();
        _fishingEngine.FishingCompleted += (sender, e) =>
        {
            fishingCompleted.SetResult(e.IsSuccessful);
        };
        
        await fishingCompleted.Task;
        
        // 5. Obter estatísticas
        var stats = _integration.GetStats();
        Console.WriteLine($"Sessão concluída: {stats.EngineStats.SuccessRate:P2} de sucesso");
        
        // 6. Parar sistema
        await _integration.StopAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro na sessão de pesca: {ex.Message}");
        await _integration.StopAsync();
    }
}
```
