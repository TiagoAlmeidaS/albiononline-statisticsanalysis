# 🎣 Exemplo de Integração do Painel de Pesca

## 📋 Como Adicionar o Painel de Pesca ao SAT

### 1. **Adicionar ao MainWindow.xaml**

Adicione a seguinte aba após a aba de Gathering (linha 239):

```xml
<!-- Aba de Pesca com Motor de Decisão -->
<TabItem Visibility="{Binding FishingTabVisibility, Mode=OneWay}" Header="{Binding Translation.Fishing, FallbackValue=FISHING}">
    <Grid Style="{StaticResource TabItemGridStyle}">
        <userControls:FishingControl DataContext="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext}" />
    </Grid>
</TabItem>
```

### 2. **Adicionar ao MainWindowViewModel.cs**

#### **2.1. Adicionar propriedades privadas:**

```csharp
// Adicionar após linha 111 (GatheringBindings)
private FishingBindings _fishingBindings = new();
private Visibility _fishingTabVisibility = Visibility.Visible;
```

#### **2.2. Adicionar propriedade pública:**

```csharp
// Adicionar após a propriedade GatheringBindings
public FishingBindings FishingBindings
{
    get => _fishingBindings;
    set
    {
        _fishingBindings = value;
        OnPropertyChanged();
    }
}

public Visibility FishingTabVisibility
{
    get => _fishingTabVisibility;
    set
    {
        _fishingTabVisibility = value;
        OnPropertyChanged();
    }
}
```

#### **2.3. Inicializar no construtor:**

```csharp
// Adicionar no método SetUiElements() após linha 180
// Fishing
FishingBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.FishingGridSplitterPosition);
```

### 3. **Adicionar ao SettingsController**

#### **3.1. Adicionar propriedade de configuração:**

```csharp
// Em UserSettings.cs
public double FishingGridSplitterPosition { get; set; } = 1.0;
```

#### **3.2. Adicionar ao método de reset:**

```csharp
// No método ResetSettings()
FishingGridSplitterPosition = 1.0;
```

### 4. **Adicionar Traduções**

#### **4.1. Em MainWindowTranslation.cs:**

```csharp
public string Fishing { get; set; } = "FISHING";
```

#### **4.2. Nos arquivos de tradução:**

```json
// Em translation files
"FISHING": "Pesca"
```

### 5. **Configurar Dependency Injection**

#### **5.1. Em Program.cs ou Startup.cs:**

```csharp
// Adicionar serviços do motor de decisão
services.AddDecisionEngineServices(config =>
{
    config.EnableAI = true;
    config.AIProvider = "OpenAI";
    config.AIModel = "gpt-4";
    config.MaxConcurrentDecisions = 5;
    config.DecisionTimeout = TimeSpan.FromSeconds(30);
});

// Registrar comportamentos específicos
services.AddBehaviors<FishingBehavior>();
services.AddBehaviors<GatheringBehavior>();
services.AddBehaviors<CombatBehavior>();

// Configurar IA se necessário
services.AddAIIntegration<OpenAIBridge>();
```

### 6. **Integração com Eventos do SAT**

#### **6.1. Criar integração automática:**

```csharp
// Em MainWindowViewModel.cs, adicionar método de inicialização
private async Task InitializeFishingSystemAsync()
{
    try
    {
        var serviceProvider = Application.Current.Resources["ServiceProvider"] as IServiceProvider;
        if (serviceProvider != null)
        {
            var decisionEngine = serviceProvider.GetService<IUniversalDecisionEngine>();
            var integration = serviceProvider.GetService<SATDecisionEngineIntegration>();
            
            if (decisionEngine != null && integration != null)
            {
                await integration.StartAsync();
            }
        }
    }
    catch (Exception ex)
    {
        // Log error
        System.Diagnostics.Debug.WriteLine($"Erro ao inicializar sistema de pesca: {ex.Message}");
    }
}
```

### 7. **Exemplo de Uso Completo**

#### **7.1. Inicialização do Sistema:**

```csharp
public async Task StartFishingSystem()
{
    // 1. Inicializar motor de decisão
    await _decisionEngine.StartAsync();
    
    // 2. Registrar comportamentos
    await _decisionEngine.RegisterBehaviorAsync(new FishingBehavior(_logger));
    
    // 3. Configurar IA (opcional)
    var aiBridge = new OpenAIBridge(_logger);
    await aiBridge.ConfigureAsync(new Dictionary<string, object>
    {
        ["ApiKey"] = "your-openai-api-key",
        ["Model"] = "gpt-4"
    });
    await _decisionEngine.ConfigureAIAsync(aiBridge);
    
    // 4. Iniciar integração com SAT
    var integration = new SATDecisionEngineIntegration(_decisionEngine, _logger, _trackingController);
    await integration.StartAsync();
}
```

#### **7.2. Uso do Painel:**

```csharp
// O painel automaticamente:
// - Monitora eventos do jogo
// - Toma decisões baseadas no contexto
// - Executa ações de pesca
// - Exibe estatísticas em tempo real
// - Permite configuração dinâmica
```

### 8. **Funcionalidades do Painel**

#### **8.1. Monitoramento em Tempo Real:**
- Status do sistema de pesca
- Informações do jogador (saúde, energia, posição)
- Zonas de pesca disponíveis
- Estatísticas de captura

#### **8.2. Controle de Automação:**
- Iniciar/parar pesca
- Configurar parâmetros de segurança
- Habilitar/desabilitar IA
- Ajustar prioridades

#### **8.3. Logs e Estatísticas:**
- Log de decisões tomadas
- Histórico de capturas
- Performance do motor de decisão
- Status dos comportamentos

#### **8.4. Configurações Avançadas:**
- Saúde e energia mínimas
- Cooldowns e tentativas
- Configurações de IA
- Comportamentos customizados

### 9. **Exemplo de Configuração**

```csharp
// Configurar sistema de pesca
var config = new FishingConfiguration
{
    IsAutoFishingEnabled = true,
    IsMinigameResolutionEnabled = true,
    EnableAIDecisions = false, // Começar sem IA
    MinHealthPercentage = 60,
    MinEnergyPercentage = 40,
    MaxAttempts = 15,
    CooldownSeconds = 3,
    MaxConsecutiveFailures = 5,
    AIConfidenceThreshold = 75
};

await fishingControl.ApplyConfigurationAsync(config);
```

### 10. **Monitoramento de Estatísticas**

```csharp
// Obter estatísticas do sistema
var stats = fishingControl.GetSystemStats();
if (stats != null)
{
    Console.WriteLine($"Sistema Ativo: {stats.IsActive}");
    Console.WriteLine($"Taxa de Sucesso: {stats.SuccessRate:F1}%");
    Console.WriteLine($"Total de Decisões: {stats.TotalDecisions}");
    Console.WriteLine($"Comportamentos Ativos: {stats.ActiveBehaviors}/{stats.TotalBehaviors}");
}
```

## 🎯 **Resultado Final**

Após a integração, você terá:

1. **Nova aba "Pesca"** no SAT
2. **Painel completo** com controles e monitoramento
3. **Sistema de automação** baseado em motor de decisão
4. **Integração com IA** (opcional)
5. **Estatísticas em tempo real**
6. **Configuração dinâmica**
7. **Logs detalhados** de todas as ações

O sistema está pronto para uso e pode ser expandido com novos comportamentos conforme necessário! 🎣
