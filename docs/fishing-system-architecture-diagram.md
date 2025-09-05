# 🏗️ Diagrama de Arquitetura do Sistema de Pesca

## 📋 Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION LAYER                                │
├─────────────────────────────────────────────────────────────────────────────────┤
│  FishingBotController  │  FishingZoneManager  │  MinigameUI  │  StatsPanel    │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                ENGINE LAYER                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│  FishingEngine  │  StateManager  │  DecisionEngine  │  EventBus  │  ConfigMgr  │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                               SERVICE LAYER                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│  FishingZoneService  │  BobberDetectionService  │  MinigameResolutionService   │
│  AutomationService   │  VisionAnalysisService   │  EventProcessingService     │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            INFRASTRUCTURE LAYER                                │
├─────────────────────────────────────────────────────────────────────────────────┤
│  AlbionFishing.Vision  │  AlbionFishing.Automation  │  AlbionFishing.Engine   │
│  SAT Event Handlers    │  SAT Network Manager       │  SAT Gathering System    │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 🔄 Fluxo de Dados

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Albion    │───▶│   SAT       │───▶│  Fishing    │───▶│ Automation  │
│   Online    │    │  Network    │    │  Engine     │    │  System     │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │                   │
       ▼                   ▼                   ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Game       │    │  Event      │    │  Decision   │    │  Mouse/     │
│  Events     │    │  Processing │    │  Making     │    │  Keyboard   │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

## 🎯 Estados do Sistema

```
┌─────────┐    ┌─────────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│  IDLE   │───▶│ ZONE_DETECT │───▶│ CASTING │───▶│  CAST   │───▶│TRACKING │
└─────────┘    └─────────────┘    └─────────┘    └─────────┘    └─────────┘
     ▲                                                              │
     │                                                              ▼
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│CANCELLED│◀───│  FAILED │◀───│ SUCCESS │◀───│CATCHING │◀───│MINIGAME │
└─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘
```

## 🧩 Componentes e Responsabilidades

### **FishingEngine**
- Coordena todos os componentes
- Gerencia estados do sistema
- Processa eventos de pesca
- Toma decisões de alto nível

### **FishingZoneService**
- Detecta e gerencia zonas de pesca
- Mantém estado das zonas ativas
- Calcula distâncias e proximidade
- Limpa zonas inativas

### **BobberDetectionService**
- Detecta bobbers na tela
- Monitora movimento do bobber
- Identifica padrões de minigame
- Mantém histórico de posições

### **MinigameResolutionService**
- Analisa movimento do bobber
- Decide quando puxar a linha
- Executa ações de minigame
- Calcula taxa de sucesso

### **AutomationService**
- Integra com sistema de automação
- Executa cliques e movimentos
- Gerencia delays e timing
- Abstrai diferenças de plataforma

## 🔗 Integração com SAT

```
┌─────────────────────────────────────────────────────────────────┐
│                        SAT EXISTENTE                           │
├─────────────────────────────────────────────────────────────────┤
│  TrackingController  │  GatheringController  │  NetworkManager │
└─────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────┐
│                    SAT FISHING INTEGRATION                     │
├─────────────────────────────────────────────────────────────────┤
│  Event Forwarding  │  Data Synchronization  │  Stats Integration│
└─────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────┐
│                    FISHING SYSTEM                              │
├─────────────────────────────────────────────────────────────────┤
│  FishingEngine  │  ZoneService  │  BobberService  │  MinigameService │
└─────────────────────────────────────────────────────────────────┘
```

## 📊 Fluxo de Eventos

```
1. NewFishingZoneObject ──┐
                          │
2. FishingStart ──────────┼───▶ FishingEngine ──▶ State: CASTING
                          │
3. FishingCast ───────────┼───▶ State: CAST
                          │
4. NewFloatObject ────────┼───▶ BobberDetectionService ──▶ State: TRACKING
                          │
5. Bobber Movement ───────┼───▶ MinigameResolutionService ──▶ State: MINIGAME
                          │
6. FishingCatch ──────────┼───▶ State: CATCHING
                          │
7. FishingFinished ───────┘───▶ State: SUCCESS/FAILED ──▶ State: IDLE
```

## 🎮 Ciclo de Minigame

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Detect    │───▶│  Analyze    │───▶│  Decide     │───▶│  Execute    │
│  Movement   │    │  Pattern    │    │  Action     │    │  Pull       │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
       │                   │                   │                   │
       ▼                   ▼                   ▼                   ▼
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Position   │    │  Velocity   │    │  Confidence │    │  Success/   │
│  Tracking   │    │  Analysis   │    │  Score      │    │  Failure    │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

## 🔧 Configuração e Dependências

```
┌─────────────────────────────────────────────────────────────────┐
│                    DEPENDENCY INJECTION                        │
├─────────────────────────────────────────────────────────────────┤
│  FishingConfig  │  Logging  │  Event Handlers  │  Services     │
└─────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────┐
│                    EXTERNAL DEPENDENCIES                       │
├─────────────────────────────────────────────────────────────────┤
│  AlbionFishing.Vision  │  AlbionFishing.Automation  │  OpenCV   │
│  Microsoft.Extensions  │  Serilog                   │  SAT Core │
└─────────────────────────────────────────────────────────────────┘
```

## 📈 Métricas e Monitoramento

```
┌─────────────────────────────────────────────────────────────────┐
│                        METRICS COLLECTION                      │
├─────────────────────────────────────────────────────────────────┤
│  Engine Stats  │  Zone Stats  │  Bobber Stats  │  Minigame Stats │
└─────────────────────────────────────────────────────────────────┘
                                        │
┌─────────────────────────────────────────────────────────────────┐
│                        PERFORMANCE DATA                        │
├─────────────────────────────────────────────────────────────────┤
│  Success Rate  │  Avg Duration  │  Detection Rate  │  Error Rate  │
└─────────────────────────────────────────────────────────────────┘
```

## 🚀 Pontos de Extensão

```
┌─────────────────────────────────────────────────────────────────┐
│                      EXTENSION POINTS                          │
├─────────────────────────────────────────────────────────────────┤
│  Custom Detectors  │  Custom Strategies  │  Custom Handlers     │
│  Custom Automation │  Custom Analytics   │  Custom UI           │
└─────────────────────────────────────────────────────────────────┘
```

Esta arquitetura modular permite fácil extensão e manutenção, com separação clara de responsabilidades e integração suave com o sistema SAT existente.
