# MoveOperation Handler - Análise de Movimento

## Visão Geral

O `MoveRequestHandler` foi implementado para capturar e analisar operações de movimento do jogador no jogo Albion Online. Este handler permite monitorar padrões de movimento, detectar comportamentos suspeitos e coletar dados estatísticos sobre a locomoção do personagem.

## Arquivos Implementados

### 1. MoveRequestHandler.cs
**Localização:** `src/StatisticsAnalysisTool/Network/Handler/MoveRequestHandler.cs`

Handler que captura operações de movimento seguindo o padrão estabelecido pelos outros handlers do sistema.

**Características:**
- Herda de `RequestPacketHandler<MoveOperation>`
- Usa o `OperationCodes.Move` (código 21)
- Chama `TrackingController.AnalyzeMovement()` para processar os dados

### 2. Funcionalidades no TrackingController
**Localização:** `src/StatisticsAnalysisTool/Network/Manager/TrackingController.cs`

Adicionada seção "Movement Analysis" com as seguintes funcionalidades:

#### Métodos Principais:
- `AnalyzeMovement(MoveOperation moveOperation)` - Processa cada operação de movimento
- `GetMovementHistory()` - Retorna histórico de movimentos
- `ClearMovementHistory()` - Limpa o histórico

#### Métodos de Análise:
- `AnalyzeMovementPatterns()` - Analisa padrões de movimento
- `CalculateDistance()` - Calcula distância entre duas posições
- `CalculateRealSpeed()` - Calcula velocidade real baseada na distância e tempo
- `DetectSuspiciousMovement()` - Detecta movimentos suspeitos

## Dados Capturados

Cada operação de movimento contém:
- **Time**: Timestamp da operação
- **Position**: Posição atual [X, Y]
- **NewPosition**: Nova posição [X, Y]
- **Direction**: Direção do movimento
- **Speed**: Velocidade do movimento

## Funcionalidades de Análise

### 1. Histórico de Movimento
- Mantém os últimos 1000 movimentos em memória
- Permite análise de padrões temporais

### 2. Cálculo de Distância
- Calcula distância euclidiana entre posições
- Usado para validar consistência dos dados

### 3. Cálculo de Velocidade
- Calcula velocidade real baseada na distância percorrida e tempo
- Detecta movimentos impossíveis ou suspeitos

### 4. Detecção de Comportamentos Suspeitos
- Identifica movimentos muito rápidos (> 50 unidades/segundo)
- Detecta distâncias muito grandes (> 100 unidades)
- Registra warnings no log para análise

## Configuração

### Limites Configuráveis:
```csharp
private const int MaxMovementHistory = 1000; // Máximo de movimentos em memória
const double maxReasonableSpeed = 50.0; // Velocidade máxima considerada normal
const double maxReasonableDistance = 100.0; // Distância máxima considerada normal
```

### Logs:
- **Debug**: Registra cada movimento detectado
- **Warning**: Registra movimentos suspeitos

## Uso

O handler é automaticamente registrado no `NetworkManager` e começa a capturar movimentos assim que o tracking é iniciado.

### Acessar Dados:
```csharp
// Obter histórico de movimentos
var movementHistory = trackingController.GetMovementHistory();

// Limpar histórico
trackingController.ClearMovementHistory();
```

## Casos de Uso

1. **Análise de Performance**: Monitorar eficiência de movimento
2. **Detecção de Cheats**: Identificar movimentos impossíveis
3. **Estatísticas de Jogabilidade**: Coletar dados sobre padrões de movimento
4. **Debugging**: Rastrear problemas de sincronização de posição

## Considerações Técnicas

- O handler respeita as configurações de tracking por personagem principal
- Dados são mantidos em memória para análise em tempo real
- Logs podem ser desabilitados em produção para melhor performance
- Validação de arrays de posição para evitar erros de índice

## Extensibilidade

O sistema foi projetado para ser facilmente extensível:
- Adicionar novos tipos de análise em `AnalyzeMovementPatterns()`
- Implementar persistência de dados em arquivo
- Criar notificações visuais para movimentos suspeitos
- Integrar com sistema de estatísticas existente
