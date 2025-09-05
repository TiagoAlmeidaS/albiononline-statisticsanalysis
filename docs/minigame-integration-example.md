# 🎮 Exemplo de Integração do Sistema de Minigame

## 📋 **Integração Completa com Motor de Decisão Universal**

Este documento mostra como integrar o sistema de minigame de pesca com o Motor de Decisão Universal, mantendo compatibilidade com o sistema legado `AlbionFishing.Vision`.

## 🏗️ **Arquitetura da Solução**

### **1. Componentes Criados:**

```
StatisticsAnalysisTool.DecisionEngine/
├── Services/
│   ├── IMinigameResolutionService.cs          # Interface moderna
│   └── ModernMinigameResolutionService.cs     # Implementação moderna
├── Adapters/
│   └── LegacyMinigameServiceAdapter.cs        # Adaptador para sistema legado
└── Behaviors/Implementations/
    └── MinigameBehavior.cs                    # Comportamento específico
```

### **2. Integração com Sistema Existente:**

- **✅ Reutiliza** `AlbionFishing.Vision` existente
- **✅ Mantém** compatibilidade com `IMinigameService` legado
- **✅ Adiciona** funcionalidades modernas
- **✅ Integra** com Motor de Decisão Universal

## 🚀 **Como Usar**

### **1. Configuração Básica:**

```csharp
// Em Program.cs ou Startup.cs
services.AddDecisionEngineServices(config =>
{
    config.EnableAI = true;
    config.AIProvider = "OpenAI";
});

// Registrar serviços de minigame
services.AddSingleton<IMinigameResolutionService, ModernMinigameResolutionService>();
services.AddSingleton<IBobberDetector, HybridBobberDetector>(); // Do AlbionFishing.Vision
services.AddSingleton<IScreenCapture, WindowsScreenCapture>(); // Do AlbionFishing.Vision

// Registrar comportamentos
services.AddBehaviors<MinigameBehavior>();
services.AddBehaviors<FishingBehavior>();
```

### **2. Uso com Motor de Decisão:**

```csharp
// Obter serviços
var decisionEngine = serviceProvider.GetService<IUniversalDecisionEngine>();
var minigameService = serviceProvider.GetService<IMinigameResolutionService>();

// Configurar minigame
var config = new MinigameConfiguration
{
    Mode = MinigameResolutionMode.VisualTracking,
    ConfidenceThreshold = 0.7,
    ReactionSensitivity = 0.8,
    EnableMicroMotionAnalysis = true,
    EnableRippleAnalysis = true,
    EnableDebugMode = false,
    VisionConfiguration = new VisionDetectorConfiguration
    {
        TemplatePath = "data/images/bobber_in_water.png",
        EnableColorFilters = true,
        EnableHsvAnalysis = true,
        EnableMultiScale = true
    }
};

await minigameService.ConfigureAsync(config);

// Iniciar minigame
var fishingArea = new Rectangle(100, 100, 600, 400);
await minigameService.StartMinigameAsync(fishingArea, config);

// O motor de decisão automaticamente processará o minigame
```

### **3. Uso com Sistema Legado:**

```csharp
// Criar adaptador para sistema legado
var modernService = serviceProvider.GetService<IMinigameResolutionService>();
var legacyService = modernService.AsLegacyService(logger);

// Configurar com configuração moderna
await legacyService.ConfigureWithModernSettingsAsync(config);

// Usar como serviço legado
legacyService.ExecuteMinigame();
legacyService.ResolveMinigame(MinigameResolutionMode.VisualTracking);

var (shouldHold, reason) = legacyService.ShouldHoldMouse(150.0f, 600);
```

### **4. Uso com Comportamento Específico:**

```csharp
// Registrar comportamento de minigame
var minigameBehavior = new MinigameBehavior(logger, minigameService);
await decisionEngine.RegisterBehaviorAsync(minigameBehavior);

// Iniciar minigame via comportamento
await minigameBehavior.StartMinigameAsync(fishingArea, config);

// O comportamento automaticamente processará o minigame
// e tomará decisões baseadas no contexto
```

## 🎯 **Funcionalidades Implementadas**

### **1. Sistema Moderno de Minigame:**

```csharp
// Configuração avançada
var config = new MinigameConfiguration
{
    Mode = MinigameResolutionMode.Hybrid, // Visual + IA
    ConfidenceThreshold = 0.8,
    ReactionSensitivity = 0.9,
    EnableMicroMotionAnalysis = true,
    EnableRippleAnalysis = true,
    EnableMovementPrediction = true,
    EnableDebugMode = true,
    DebugImagePath = "debug/minigame/",
    VisionConfiguration = new VisionDetectorConfiguration
    {
        TemplatePath = "data/images/bobber_in_water.png",
        EnableColorFilters = true,
        EnableHsvAnalysis = true,
        EnableGradientAnalysis = true,
        EnableMultiScale = true,
        EnableSignalAnalysis = true,
        EnableKinematics = true,
        EnableMicroMotion = true,
        EnableHookHeuristic = true
    }
};
```

### **2. Análise Avançada:**

```csharp
// O sistema automaticamente analisa:
// - Micro-movimentos do bobber
// - Ondulações na água
// - Movimentos bruscos
// - Fluxo de água
// - Diferenças visuais
// - Predição de movimento
// - Análise de sinal
// - Cinemática
// - Heurística de gancho
```

### **3. Decisões Inteligentes:**

```csharp
// O sistema toma decisões baseadas em:
// - Posição do bobber
// - Confiança da detecção
// - Análise de micro-movimentos
// - Análise de ondulação
// - Histórico de posições
// - Predição de movimento
// - Múltiplos votos de análise
```

### **4. Monitoramento em Tempo Real:**

```csharp
// Estatísticas disponíveis:
var stats = minigameService.GetDetailedStatistics();
Console.WriteLine($"Total de Minigames: {stats.TotalMinigames}");
Console.WriteLine($"Taxa de Sucesso: {stats.SuccessRate:P2}");
Console.WriteLine($"Duração Média: {stats.AverageMinigameDuration}");
Console.WriteLine($"Frames Processados: {stats.TotalFramesProcessed}");
Console.WriteLine($"Decisões de Mouse: {stats.TotalMouseDecisions}");
Console.WriteLine($"Precisão: {stats.MouseDecisionAccuracy:P2}");
Console.WriteLine($"Confiança Média: {stats.AverageConfidence:P2}");
```

## 🔧 **Configurações Avançadas**

### **1. Modos de Resolução:**

```csharp
// Reação padrão (baseada em tempo)
Mode = MinigameResolutionMode.StandardReaction

// Tracking visual reativo
Mode = MinigameResolutionMode.VisualTracking

// Controle por IA (futuro)
Mode = MinigameResolutionMode.AIControlled

// Modo híbrido (visual + IA)
Mode = MinigameResolutionMode.Hybrid
```

### **2. Configurações de Visão:**

```csharp
var visionConfig = new VisionDetectorConfiguration
{
    TemplatePath = "data/images/bobber_in_water.png",
    MatchMethod = "CCoeffNormed",
    EnableColorFilters = true,
    EnableHsvAnalysis = true,
    EnableGradientAnalysis = true,
    EnableMultiScale = true,
    Scales = new[] { 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2 },
    EnableSignalAnalysis = true,
    EnableKinematics = true,
    EnableMicroMotion = true,
    EnableHookHeuristic = true
};
```

### **3. Configurações de Performance:**

```csharp
var config = new MinigameConfiguration
{
    FrameInterval = 30, // 30ms entre frames
    MaxMinigameDuration = 20000, // 20 segundos máximo
    EnablePositionSmoothing = true,
    SmoothingFactor = 0.3,
    EnableMovementPrediction = true,
    PositionHistorySize = 5,
    EnableMicroMotionAnalysis = true,
    MicroMotionThreshold = 2.0,
    EnableRippleAnalysis = true,
    RippleThreshold = 2.2
};
```

## 📊 **Eventos e Monitoramento**

### **1. Eventos Disponíveis:**

```csharp
// Eventos do serviço de minigame
minigameService.MinigameStarted += (sender, e) => {
    Console.WriteLine($"Minigame iniciado: {e.FishingArea}");
};

minigameService.MinigameCompleted += (sender, e) => {
    Console.WriteLine($"Minigame concluído: {e.WasSuccessful}");
};

minigameService.MouseDecisionMade += (sender, e) => {
    Console.WriteLine($"Decisão: {e.Decision.ShouldHold} - {e.Decision.Reason}");
};
```

### **2. Logs Detalhados:**

```csharp
// Configurar logging
services.AddLogging(builder => {
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Logs automáticos incluem:
// - Início/parada de minigames
// - Decisões de mouse
// - Análises de frame
// - Estatísticas de performance
// - Erros e avisos
```

## 🎯 **Exemplo Completo**

```csharp
public class FishingMinigameExample
{
    private readonly IUniversalDecisionEngine _decisionEngine;
    private readonly IMinigameResolutionService _minigameService;
    private readonly ILogger<FishingMinigameExample> _logger;
    
    public FishingMinigameExample(
        IUniversalDecisionEngine decisionEngine,
        IMinigameResolutionService minigameService,
        ILogger<FishingMinigameExample> logger)
    {
        _decisionEngine = decisionEngine;
        _minigameService = minigameService;
        _logger = logger;
    }
    
    public async Task RunFishingMinigameAsync()
    {
        try
        {
            // 1. Configurar minigame
            var config = new MinigameConfiguration
            {
                Mode = MinigameResolutionMode.VisualTracking,
                ConfidenceThreshold = 0.7,
                ReactionSensitivity = 0.8,
                EnableMicroMotionAnalysis = true,
                EnableRippleAnalysis = true,
                EnableDebugMode = true,
                VisionConfiguration = new VisionDetectorConfiguration
                {
                    TemplatePath = "data/images/bobber_in_water.png",
                    EnableColorFilters = true,
                    EnableHsvAnalysis = true,
                    EnableMultiScale = true
                }
            };
            
            await _minigameService.ConfigureAsync(config);
            
            // 2. Registrar comportamento
            var minigameBehavior = new MinigameBehavior(_logger, _minigameService);
            await _decisionEngine.RegisterBehaviorAsync(minigameBehavior);
            
            // 3. Iniciar minigame
            var fishingArea = new Rectangle(100, 100, 600, 400);
            await minigameBehavior.StartMinigameAsync(fishingArea, config);
            
            // 4. O motor de decisão automaticamente processará o minigame
            _logger.LogInformation("Minigame iniciado, processamento automático ativo");
            
            // 5. Monitorar estatísticas
            var timer = new Timer(async _ => {
                var stats = _minigameService.GetDetailedStatistics();
                _logger.LogInformation("Estatísticas: {Stats}", stats);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            // 6. Aguardar conclusão
            await Task.Delay(TimeSpan.FromMinutes(5)); // Exemplo
            
            // 7. Parar minigame
            await minigameBehavior.StopMinigameAsync();
            
            // 8. Exibir estatísticas finais
            var finalStats = _minigameService.GetDetailedStatistics();
            _logger.LogInformation("Estatísticas finais: {FinalStats}", finalStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante execução do minigame");
        }
    }
}
```

## 🎉 **Benefícios da Solução**

### **1. Compatibilidade:**
- ✅ Funciona com sistema legado
- ✅ Reutiliza `AlbionFishing.Vision`
- ✅ Mantém interfaces existentes

### **2. Modernização:**
- ✅ Sistema assíncrono
- ✅ Configuração flexível
- ✅ Análise avançada
- ✅ Monitoramento em tempo real

### **3. Integração:**
- ✅ Motor de Decisão Universal
- ✅ Sistema de comportamentos
- ✅ Eventos e logging
- ✅ Estatísticas detalhadas

### **4. Extensibilidade:**
- ✅ Novos modos de resolução
- ✅ Análises customizadas
- ✅ Integração com IA
- ✅ Configurações dinâmicas

A solução está pronta para uso e fornece uma integração perfeita entre o sistema legado e o novo Motor de Decisão Universal! 🎮🎣
