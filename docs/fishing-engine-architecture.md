# 🎣 Arquitetura do Engine de Pesca Sofisticado

## 📋 Visão Geral

Este documento descreve a arquitetura proposta para integrar um sistema de pesca sofisticado ao projeto StatisticsAnalysisTool, aproveitando componentes dos projetos AlbionFishing e o sistema de eventos do Albion Online.

## 🏗️ Arquitetura Proposta

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                          │
├─────────────────────────────────────────────────────────────────┤
│  FishingBotController  │  FishingZoneManager  │  MinigameUI    │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                     ENGINE LAYER                               │
├─────────────────────────────────────────────────────────────────┤
│  FishingEngine  │  StateManager  │  DecisionEngine  │  EventBus │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                               │
├─────────────────────────────────────────────────────────────────┤
│  FishingZoneService  │  BobberDetectionService  │  AutomationService │
└─────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE LAYER                        │
├─────────────────────────────────────────────────────────────────┤
│  Vision System  │  Automation System  │  Event Handlers  │  SAT │
└─────────────────────────────────────────────────────────────────┘
```

## 🔄 Fluxo de Funcionamento

### 1. **Detecção de Zona de Pesca**
- Evento `NewFishingZoneObject` é capturado
- `FishingZoneService` processa e armazena informações da zona
- `FishingZoneManager` mantém estado das zonas ativas

### 2. **Início da Pesca**
- Evento `FishingStart` é detectado
- `FishingEngine` inicia o ciclo de pesca
- `StateManager` transiciona para estado `CASTING`

### 3. **Lançamento da Linha**
- `AutomationService` executa ação de cast
- Evento `FishingCast` confirma o lançamento
- Estado transiciona para `CAST`

### 4. **Detecção do Bobber**
- `VisionSystem` detecta `NewFloatObject` (bobber)
- `BobberDetectionService` monitora posição e movimento
- Estado transiciona para `TRACKING`

### 5. **Resolução do Minigame**
- Evento `FishingMiniGame` inicia o minigame
- `VisionSystem` detecta movimento do bobber
- `DecisionEngine` decide quando puxar a linha
- `AutomationService` executa a ação

### 6. **Finalização**
- Evento `FishingCatch` confirma sucesso/falha
- Evento `FishingFinished` finaliza o ciclo
- Estado retorna para `IDLE`

## 🧩 Componentes Reutilizáveis

### **Do AlbionFishing.Automation:**
- ✅ `IAutomationService` - Interface para automação cross-platform
- ✅ `WindowsAutomationService` - Implementação Windows
- ✅ `UnixAutomationService` - Implementação Linux/macOS
- ✅ `AutomationManager` - Gerenciador de automação

### **Do AlbionFishing.Engine:**
- ✅ `IDecisionEngine` - Interface para engine de decisões
- ✅ `DecisionEngineV2` - Engine de decisões avançado
- ✅ `IEventBus` - Sistema de eventos
- ✅ `IStateMachine` - Máquina de estados

### **Do AlbionFishing.Vision:**
- ✅ `IBobberDetector` - Interface para detecção de bobber
- ✅ `BobberInWaterDetector` - Detector avançado
- ✅ `IScreenCapture` - Interface para captura de tela
- ✅ `VisionAnalysisService` - Serviço de análise visual

## 🎯 Estados do Sistema

```csharp
public enum FishingState
{
    IDLE,           // Aguardando início
    ZONE_DETECTED,  // Zona de pesca detectada
    CASTING,        // Lançando linha
    CAST,           // Linha lançada
    TRACKING,       // Monitorando bobber
    MINIGAME,       // Resolvendo minigame
    CATCHING,       // Puxando linha
    SUCCESS,        // Pesca bem-sucedida
    FAILED,         // Pesca falhou
    CANCELLED       // Pesca cancelada
}
```

## 🔧 Integração com SAT

### **Event Handlers Necessários:**
- `NewFishingZoneObjectEventHandler`
- `FishingStartEventHandler`
- `FishingCastEventHandler`
- `FishingCatchEventHandler`
- `FishingFinishedEventHandler`
- `FishingCancelEventHandler`
- `NewFloatObjectEventHandler`
- `FishingMiniGameEventHandler`

### **Modificações no GatheringController:**
- Integrar com `FishingEngine`
- Manter compatibilidade com sistema atual
- Adicionar suporte a minigame

## 📊 Benefícios da Arquitetura

1. **Modularidade** - Componentes independentes e reutilizáveis
2. **Extensibilidade** - Fácil adição de novos detectores e estratégias
3. **Manutenibilidade** - Separação clara de responsabilidades
4. **Performance** - Sistema assíncrono e otimizado
5. **Compatibilidade** - Integração suave com SAT existente

## 🚀 Próximos Passos

1. Criar estrutura de projetos
2. Implementar interfaces base
3. Integrar componentes do AlbionFishing
4. Desenvolver event handlers
5. Implementar sistema de estados
6. Testes e validação
