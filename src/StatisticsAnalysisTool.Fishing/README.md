# 🎣 StatisticsAnalysisTool.Fishing

Sistema de pesca sofisticado integrado ao StatisticsAnalysisTool (SAT) que aproveita componentes dos projetos AlbionFishing para fornecer automação inteligente de pesca no Albion Online.

## 🏗️ Arquitetura

O sistema é construído em camadas bem definidas:

- **Presentation Layer**: Controllers e UI
- **Engine Layer**: Lógica de negócio e coordenação
- **Service Layer**: Serviços especializados
- **Infrastructure Layer**: Integração com sistemas externos

## 🧩 Componentes Principais

### **Engines**
- `FishingEngine`: Engine principal que coordena todo o sistema
- `DecisionEngine`: Toma decisões baseadas em eventos e estado

### **Services**
- `FishingZoneService`: Gerencia zonas de pesca detectadas
- `BobberDetectionService`: Detecta e monitora bobbers
- `MinigameResolutionService`: Resolve minigames de pesca
- `AutomationService`: Integra com sistema de automação

### **Event Handlers**
- `NewFishingZoneObjectEventHandler`: Processa detecção de zonas
- `FishingStartEventHandler`: Processa início de pesca
- `NewFloatObjectEventHandler`: Processa detecção de bobbers

### **Models**
- `FishingState`: Estados do sistema de pesca
- `FishingZone`: Informações sobre zonas de pesca
- `BobberInfo`: Informações sobre bobbers
- `FishingEvent`: Evento de pesca ativo

## 🔄 Fluxo de Funcionamento

1. **Detecção de Zona**: Sistema detecta `NewFishingZoneObject`
2. **Início da Pesca**: Usuário ou sistema inicia pesca na zona
3. **Lançamento**: Sistema executa cast na zona
4. **Detecção de Bobber**: Sistema detecta `NewFloatObject`
5. **Tracking**: Sistema monitora movimento do bobber
6. **Minigame**: Sistema detecta movimento e resolve minigame
7. **Finalização**: Sistema processa resultado da pesca

## 🚀 Como Usar

### **1. Configuração**
```csharp
services.AddFishingServices(config =>
{
    config.EnableAutoFishing = true;
    config.EnableMinigameResolution = true;
    config.BobberDetectionThreshold = 0.7;
});
```

### **2. Uso Básico**
```csharp
// Injetar serviços
private readonly IFishingEngine _fishingEngine;
private readonly SATFishingIntegration _integration;

// Iniciar sistema
await _integration.StartAsync();

// Iniciar pesca automática
await _integration.StartAutoFishingAsync();
```

### **3. Monitoramento**
```csharp
// Subscrever eventos
_fishingEngine.StateChanged += OnStateChanged;
_fishingEngine.FishingCompleted += OnFishingCompleted;

// Obter estatísticas
var stats = _integration.GetStats();
```

## 🔧 Configurações

### **FishingConfig**
- `EnableAutoFishing`: Habilita pesca automática
- `EnableMinigameResolution`: Habilita resolução de minigame
- `BobberDetectionThreshold`: Threshold para detecção de bobber
- `MaxFishingAttempts`: Máximo de tentativas de pesca
- `FishingTimeout`: Timeout para operações de pesca
- `MinigameTimeout`: Timeout para minigame
- `EnableDebugMode`: Habilita logs detalhados

## 📊 Eventos do Albion Online

O sistema processa os seguintes eventos:

- `FishingStart` (355): Início da pesca
- `FishingCast` (356): Lançamento da linha
- `FishingCatch` (357): Fisgada detectada
- `FishingFinished` (358): Pesca finalizada
- `FishingCancel` (359): Pesca cancelada
- `NewFloatObject` (360): Novo bobber
- `NewFishingZoneObject` (361): Nova zona de pesca
- `FishingMiniGame` (362): Minigame de pesca

## 🔗 Integração com SAT

O sistema se integra perfeitamente com o SAT existente:

- **Event Handlers**: Processa eventos do Albion Online
- **GatheringController**: Integra com sistema de gathering
- **TrackingController**: Usa sistema de tracking existente
- **Logging**: Usa sistema de logging do SAT

## 🧪 Componentes Reutilizados

### **Do AlbionFishing.Automation**
- `IAutomationService`: Interface de automação
- `WindowsAutomationService`: Implementação Windows
- `UnixAutomationService`: Implementação Linux/macOS
- `AutomationManager`: Gerenciador de automação

### **Do AlbionFishing.Engine**
- `IDecisionEngine`: Interface de decisões
- `DecisionEngineV2`: Engine de decisões
- `IEventBus`: Sistema de eventos
- `IStateMachine`: Máquina de estados

### **Do AlbionFishing.Vision**
- `IBobberDetector`: Interface de detecção
- `BobberInWaterDetector`: Detector avançado
- `IScreenCapture`: Interface de captura
- `VisionAnalysisService`: Análise visual

## 📈 Benefícios

1. **Modularidade**: Componentes independentes e reutilizáveis
2. **Extensibilidade**: Fácil adição de novos recursos
3. **Manutenibilidade**: Código bem estruturado e documentado
4. **Performance**: Sistema assíncrono e otimizado
5. **Compatibilidade**: Integração suave com SAT

## 🔍 Debugging

Para ativar logs detalhados:

```csharp
var config = new FishingConfig
{
    EnableDebugMode = true
};
```

Os logs incluem:
- Detecção de zonas e bobbers
- Transições de estado
- Decisões do engine
- Performance de detecção
- Erros e exceções

## 📝 Licença

Este projeto segue a mesma licença do StatisticsAnalysisTool.
