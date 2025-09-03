# Documentação de Fluxo de Funcionamento - Statistics Analysis Tool

## Visão Geral do Projeto

O **Statistics Analysis Tool** é uma ferramenta de análise de estatísticas para o jogo Albion Online, desenvolvida em C# (.NET 9.0) com interface WPF. A ferramenta monitora o tráfego de rede do jogo para extrair dados em tempo real sobre loot, dano, dungeons, crafting e outras atividades do jogador.

## Arquitetura Geral

### Componentes Principais

1. **StatisticsAnalysisTool** - Aplicação principal WPF
2. **StatisticsAnalysisTool.Network** - Gerenciamento de rede e parsing de pacotes
3. **StatisticsAnalysisTool.PhotonPackageParser** - Parser do protocolo Photon
4. **StatisticsAnalysisTool.Protocol16** - Deserialização de dados do protocolo
5. **StatisticAnalysisTool.Extractor** - Extração e decriptação de dados do jogo
6. **StatisticsAnalysisTool.Abstractions** - Interfaces e abstrações
7. **StatisticsAnalysisTool.Diagnostics** - Sistema de debug e logging

## Fluxo de Funcionamento Detalhado

### 1. Inicialização da Aplicação

#### 1.1 App.xaml.cs - Ponto de Entrada
```csharp
protected override async void OnStartup(StartupEventArgs e)
{
    // 1. Configuração do logger
    InitLogger();
    
    // 2. Carregamento de configurações
    SettingsController.LoadSettings();
    
    // 3. Inicialização do console de debug (se habilitado)
    if (SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked)
        DebugConsole.Attach("SAT Debug Console");
    
    // 4. Atualização automática
    await AutoUpdateController.AutoUpdateAsync();
    
    // 5. Configuração de localização
    Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(...));
    LocalizationController.Init();
    
    // 6. Configuração do servidor
    Server.SetServerLocationWithDialogAsync();
    
    // 7. Inicialização dos dados do jogo
    await GameData.InitializeMainGameDataFilesAsync(...);
    
    // 8. Registro de serviços
    RegisterServicesEarly();
    RegisterServicesLate();
    
    // 9. Criação e exibição da janela principal
    Current.MainWindow = new MainWindow(_mainWindowViewModel);
    Current.MainWindow.Show();
}
```

#### 1.2 Sistema de Logging
- **Serilog** para logging estruturado
- Logs salvos em `logs/sat-.logs` com rotação diária
- Níveis: Verbose, Information, Warning, Error, Fatal
- Console e arquivo simultaneamente

### 2. Captura de Pacotes de Rede

#### 2.1 Packet Providers
A ferramenta suporta dois métodos de captura:

**SocketsPacketProvider (Padrão)**
- Usa raw sockets para capturar tráfego de rede
- Requer execução como administrador
- Captura pacotes IPv4 e IPv6
- Filtra por protocolo IP

**LibpcapPacketProvider (Alternativo)**
- Usa Npcap para captura de pacotes
- Não requer privilégios de administrador
- Suporte a filtros IP/porta para VPN
- Mais flexível para usuários com VPN

#### 2.2 NetworkManager
```csharp
public class NetworkManager
{
    private readonly PacketProvider _packetProvider;
    
    public NetworkManager(TrackingController trackingController)
    {
        IPhotonReceiver photonReceiver = Build(trackingController);
        
        if (SettingsController.CurrentSettings.PacketProvider == PacketProviderKind.Npcap)
            _packetProvider = new LibpcapPacketProvider(photonReceiver);
        else
            _packetProvider = new SocketsPacketProvider(photonReceiver);
    }
}
```

### 3. Parsing de Protocolo Photon

#### 3.1 PhotonParser
O `PhotonParser` é responsável por interpretar os pacotes do protocolo Photon:

```csharp
public abstract class PhotonParser : IPhotonReceiver
{
    public void ReceivePacket(byte[] payload)
    {
        // 1. Validação do header Photon (12 bytes)
        // 2. Leitura de flags (criptografia, CRC)
        // 3. Processamento de comandos
        // 4. Deserialização de mensagens
    }
}
```

#### 3.2 Tipos de Mensagens
- **OperationRequest** - Requisições do cliente
- **OperationResponse** - Respostas do servidor
- **Event** - Eventos do jogo

#### 3.3 AlbionParser
Especialização do PhotonParser para Albion Online:

```csharp
internal sealed class AlbionParser : PhotonParser
{
    private readonly HandlersCollection _handlers = new();
    
    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        short eventCode = ParseEventCode(parameters);
        var eventPacket = new EventPacket(eventCode, parameters);
        _ = _handlers.HandleAsync(eventPacket);
    }
}
```

### 4. Sistema de Handlers

#### 4.1 Estrutura de Handlers
Cada tipo de evento tem um handler específico:

```csharp
public class NewLootEventHandler : EventPacketHandler<NewLootEvent>
{
    protected override async Task OnActionAsync(NewLootEvent value)
    {
        if (value?.ObjectId != null)
        {
            _trackingController.LootController.SetIdentifiedBody((long) value.ObjectId, value.LootBody);
        }
    }
}
```

#### 4.2 Tipos de Handlers
- **EventPacketHandler** - Para eventos do jogo
- **RequestPacketHandler** - Para requisições
- **ResponsePacketHandler** - Para respostas

#### 4.3 Eventos Principais
- **NewLootEvent** - Novo loot encontrado
- **HealthUpdateEvent** - Atualizações de vida/dano
- **NewCharacterEvent** - Novos personagens
- **InventoryPutItemEvent** - Itens no inventário
- **TakeSilverEvent** - Coleta de prata
- **DiedEvent** - Morte de personagem

### 5. Funcionalidades Principais

#### 5.1 Damage Meter (Medidor de Dano)
```csharp
public class DamageMeterFragment
{
    private long _damage;
    private double _dps;
    private long _heal;
    private double _hps;
    private long _takenDamage;
    
    // Cálculo de DPS/HPS em tempo real
    // Rastreamento de spells utilizadas
    // Estatísticas de combate
}
```

**Fluxo:**
1. Captura de eventos `HealthUpdateEvent`
2. Processamento no `CombatController`
3. Cálculo de DPS/HPS
4. Atualização da interface

#### 5.2 Dungeon Tracker (Rastreador de Dungeons)
```csharp
public sealed class DungeonController
{
    public async Task AddDungeonAsync(MapType mapType, Guid? mapGuid)
    {
        // 1. Verificação de permissões
        // 2. Identificação do tipo de dungeon
        // 3. Criação ou atualização de dungeon
        // 4. Rastreamento de tempo
        // 5. Coleta de estatísticas
    }
}
```

**Funcionalidades:**
- Detecção automática de entrada/saída
- Rastreamento de tempo
- Estatísticas de loot
- Mapeamento de dungeons

#### 5.3 Loot Logger (Registrador de Loot)
```csharp
public class LootController
{
    public void SetIdentifiedBody(long objectId, string lootBody)
    {
        // Processamento de loot identificado
        // Cálculo de valor de mercado
        // Armazenamento de dados
    }
}
```

**Processo:**
1. Captura de eventos de loot
2. Identificação de itens
3. Cálculo de valores
4. Armazenamento em banco de dados

#### 5.4 Trade Monitoring (Monitoramento de Comércio)
```csharp
public class TradeController
{
    // Rastreamento de transações
    // Cálculo de lucros/prejuízos
    // Estatísticas de mercado
    // Exportação de dados
}
```

### 6. Extração de Dados do Jogo

#### 6.1 Extractor
```csharp
public class Extractor
{
    public async Task ExtractIndexedItemGameDataAsync(string outputDirPath, string indexedItemsFileName)
    {
        await LoadLocationDataAsync();
        await ItemData.CreateItemDataAsync(_mainGameServerFolderString, _localizationData, outputDirPath, indexedItemsFileName);
    }
}
```

#### 6.2 BinaryDecrypter
```csharp
internal static class BinaryDecrypter
{
    private static readonly byte[] Key = { 48, 239, 114, 71, 66, 242, 4, 50 };
    private static readonly byte[] Iv = { 14, 166, 220, 137, 219, 237, 220, 79 };
    
    public static async Task DecryptBinaryFileAsync(string inputPath, Stream outputStream)
    {
        // 1. Leitura do arquivo criptografado
        // 2. Descriptografia com DES-CBC
        // 3. Descompressão GZip
        // 4. Extração de dados
    }
}
```

**Processo de Extração:**
1. Localização dos arquivos `.bin` do jogo
2. Descriptografia com chave fixa
3. Descompressão GZip
4. Parsing de dados JSON/XML
5. Armazenamento local

### 7. Padrões de Dados

#### 7.1 Estrutura de Eventos
```csharp
public class NewLootEvent
{
    public readonly long? ObjectId;      // ID do objeto
    public readonly string LootBody;     // Dados do loot
    
    public NewLootEvent(Dictionary<byte, object> parameters)
    {
        if (parameters.ContainsKey(0))
            ObjectId = parameters[0].ObjectToLong();
        
        if (parameters.ContainsKey(3))
            LootBody = parameters[3].ToString();
    }
}
```

#### 7.2 Mapeamento de Códigos
- **EventCodes** - Códigos de eventos (643 códigos)
- **OperationCodes** - Códigos de operações (530 códigos)
- **MessageType** - Tipos de mensagem Photon

### 8. Personalização e Configuração

#### 8.1 SettingsController
```csharp
public class SettingsController
{
    public static UserSettings CurrentSettings { get; set; }
    
    public static void LoadSettings()
    {
        // Carregamento de configurações do usuário
        // Configurações de servidor
        // Preferências de interface
        // Configurações de rede
    }
}
```

#### 8.2 Configurações Principais
- **ServerLocation** - Localização do servidor (America/Europe/Asia)
- **PacketProvider** - Método de captura (Sockets/Npcap)
- **CurrentCulture** - Idioma da interface
- **DebugConsole** - Console de debug
- **AutoUpdate** - Atualizações automáticas

### 9. Sistema de Notificações

#### 9.1 SatNotificationManager
```csharp
public class SatNotificationManager
{
    private readonly NotificationManager _notificationManager;
    
    public async Task ShowTrackingStatusAsync(string title, string message)
    {
        // Exibição de notificações do sistema
        // Status de rastreamento
        // Alertas importantes
    }
}
```

### 10. Armazenamento de Dados

#### 10.1 BackupController
```csharp
public class BackupController
{
    public static async Task Save()
    {
        // Backup automático de dados
        // Compressão de arquivos
        // Limpeza de backups antigos
    }
}
```

#### 10.2 CriticalData
```csharp
public class CriticalData
{
    public static void Save()
    {
        // Salvamento de dados críticos
        // Estatísticas de sessão
        // Configurações importantes
    }
}
```

## Fluxo de Dados Completo

### 1. Captura → Parsing → Processamento → Armazenamento

```
[Pacotes de Rede] 
    ↓ (PacketProvider)
[PhotonParser] 
    ↓ (Deserialização)
[AlbionParser] 
    ↓ (Handlers)
[Eventos Específicos] 
    ↓ (Controllers)
[Processamento de Dados] 
    ↓ (ViewModels)
[Interface do Usuário]
```

### 2. Exemplo: Processamento de Loot

```
1. [Rede] Pacote NewLoot capturado
2. [PhotonParser] Deserialização do pacote
3. [AlbionParser] Identificação como EventCode.NewLoot
4. [NewLootEventHandler] Processamento do evento
5. [LootController] Extração de dados do loot
6. [ItemData] Identificação do item
7. [MarketValue] Cálculo de valor
8. [Database] Armazenamento
9. [UI] Atualização da interface
```

## Considerações de Segurança

### 1. Conformidade com ToS
- ✅ Apenas monitora (não modifica)
- ✅ Não modifica o cliente do jogo
- ✅ Não rastreia jogadores fora da visão
- ✅ Sem overlay no jogo

### 2. Métodos de Captura
- **Sockets**: Requer privilégios de administrador
- **Npcap**: Alternativa para usuários com VPN
- **Filtros**: Suporte a filtros IP/porta

## Conclusão

O Statistics Analysis Tool é uma ferramenta complexa que combina captura de rede, parsing de protocolos, processamento de dados em tempo real e interface de usuário para fornecer análises detalhadas das atividades do jogador em Albion Online. A arquitetura modular permite fácil manutenção e extensão de funcionalidades.

### Pontos Fortes da Arquitetura:
- **Modularidade**: Componentes bem separados
- **Extensibilidade**: Fácil adição de novos handlers
- **Robustez**: Tratamento de erros e logging
- **Performance**: Processamento assíncrono
- **Flexibilidade**: Múltiplos métodos de captura

### Padrões Utilizados:
- **MVVM**: Para interface WPF
- **Dependency Injection**: Para injeção de dependências
- **Observer Pattern**: Para eventos de rede
- **Factory Pattern**: Para criação de handlers
- **Strategy Pattern**: Para diferentes providers de pacotes