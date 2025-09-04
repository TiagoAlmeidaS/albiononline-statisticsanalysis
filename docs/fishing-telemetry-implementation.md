# Implementação de Telemetria para Eventos de Pesca

## Visão Geral

Este documento descreve a implementação de telemetria detalhada para o evento `FishingCatch` no Statistics Analysis Tool (SAT). A telemetria foi implementada em três camadas principais para fornecer visibilidade completa do fluxo de dados.

## Arquivos Modificados

### 1. `FishingCatchEvent.cs`
**Localização:** `src/StatisticsAnalysisTool/Network/Events/FishingCatchEvent.cs`

**Melhorias implementadas:**
- Logging detalhado dos parâmetros brutos recebidos
- Logging individual de cada parâmetro extraído
- Logging consolidado do evento processado
- Tratamento de erros com logging completo

**Logs gerados:**
- `DEBUG`: Parâmetros brutos e extração individual de cada campo
- `INFORMATION`: Evento processado com todos os dados extraídos
- `ERROR`: Erros durante o processamento com contexto completo

### 2. `FishingCatchEventHandler.cs`
**Localização:** `src/StatisticsAnalysisTool/Network/Handler/FishingCatchEventHandler.cs`

**Melhorias implementadas:**
- Logging da recepção do evento
- Verificação e logging do status de tracking
- Logging do processamento no GatheringController
- Tratamento de erros com contexto

**Logs gerados:**
- `INFORMATION`: Evento recebido e processamento
- `DEBUG`: Status de tracking
- `WARNING`: Tracking não permitido
- `ERROR`: Erros durante o processamento

### 3. `GatheringController.cs`
**Localização:** `src/StatisticsAnalysisTool/Gathering/GatheringController.cs`

**Melhorias implementadas:**
- Logging detalhado no método `OnFishingCatch`
- Verificação de evento de pesca ativo
- Logging de fisgadas detectadas
- Tratamento de cenários de erro

**Logs gerados:**
- `INFORMATION`: Processamento da fisgada e eventos ativos
- `WARNING`: Eventos de pesca não encontrados ou fechados
- `ERROR`: Erros durante o processamento

## Tipos de Logs

### 1. Serilog (Arquivo)
- **Localização:** `logs/sat-YYYY-MM-DD.logs`
- **Níveis:** DEBUG, INFORMATION, WARNING, ERROR
- **Formato:** Estruturado com propriedades nomeadas
- **Retenção:** 7 dias

### 2. DebugConsole (Console)
- **Ativação:** Configurável nas configurações
- **Cores:** Azul (#5196A6) para eventos de pesca
- **Formato:** Timestamp + nível + fonte + mensagem

## Como Usar a Telemetria

### 1. Ativar o Debug Console
```csharp
// Nas configurações do SAT
SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked = true;
```

### 2. Filtrar Logs de Pesca
```bash
# No Debug Console, use filtros para eventos de pesca
-events 357  # FishingCatch event code
```

### 3. Analisar Logs de Arquivo
```bash
# Buscar por eventos de pesca nos logs
grep "FishingCatch" logs/sat-*.logs
grep "OnFishingCatch" logs/sat-*.logs
```

## Estrutura dos Dados Logados

### Parâmetros do Evento
- **EventId (0):** ID único do evento de pesca
- **ItemIndex (1):** Índice do item fisgado
- **IsSuccessful (2):** Se a fisgada foi bem-sucedida
- **Timestamp (3):** Momento da fisgada

### Informações Adicionais
- **ActiveEventId:** ID do evento de pesca ativo
- **UsedFishingRod:** Índice da vara de pesca usada
- **Tracking Status:** Se o tracking está permitido

## Exemplo de Logs

### Debug Console
```
[2024-01-15T10:30:45] INFO FishingCatchEvent: Parâmetros brutos: 0:12345, 1:6789, 2:true, 3:1705312245000
[2024-01-15T10:30:45] INFO FishingCatchEvent: Evento processado: EventId=12345, ItemIndex=6789, IsSuccessful=true, Timestamp=1705312245000
[2024-01-15T10:30:45] INFO FishingCatchEventHandler: Evento recebido - EventId=12345, ItemIndex=6789, IsSuccessful=true, Timestamp=1705312245000
[2024-01-15T10:30:45] INFO GatheringController.OnFishingCatch: Fisgada detectada - EventId=12345, ItemIndex=6789, IsSuccessful=true, Timestamp=1705312245000
```

### Arquivo de Log
```json
{
  "@t": "2024-01-15T10:30:45.123Z",
  "@l": "Information",
  "@mt": "FishingCatchEvent processado: EventId={EventId}, ItemIndex={ItemIndex}, IsSuccessful={IsSuccessful}, Timestamp={Timestamp}",
  "EventId": 12345,
  "ItemIndex": 6789,
  "IsSuccessful": true,
  "Timestamp": 1705312245000
}
```

## Troubleshooting

### Evento Não Aparece nos Logs
1. Verificar se o Debug Console está ativo
2. Verificar se o tracking está permitido
3. Verificar se há um evento de pesca ativo
4. Verificar se o evento está sendo recebido do jogo

### Logs Muito Verbosos
1. Ajustar nível de log para WARNING ou ERROR
2. Usar filtros no Debug Console
3. Filtrar por tipo de evento específico

### Performance
- Os logs são assíncronos e não afetam a performance do jogo
- O Debug Console usa uma fila para evitar bloqueios
- Os logs de arquivo são escritos em background

## Próximos Passos

1. **Métricas de Performance:** Adicionar timing dos eventos
2. **Estatísticas:** Contar tentativas de fisgada e taxa de sucesso
3. **Notificações:** Alertas para eventos específicos
4. **Dashboard:** Interface visual para monitorar eventos de pesca
5. **Exportação:** Exportar dados de telemetria para análise externa

## Configurações Recomendadas

### Para Desenvolvimento
```json
{
  "IsOpenDebugConsoleWhenStartingTheToolChecked": true,
  "DebugConsoleFilter": "-events 357"
}
```

### Para Produção
```json
{
  "IsOpenDebugConsoleWhenStartingTheToolChecked": false,
  "LogLevel": "Warning"
}
```

## Conclusão

A implementação de telemetria fornece visibilidade completa do fluxo de eventos de pesca, permitindo:
- Debug de problemas em tempo real
- Análise de padrões de comportamento
- Monitoramento de performance
- Troubleshooting de issues

Os logs são estruturados e podem ser facilmente analisados tanto no console quanto em arquivos, proporcionando flexibilidade para diferentes cenários de uso.
