# ⚡ Implementação Mínima macOS - SAT

## 🎯 Objetivo

Implementação rápida e minimalista do macOS focando apenas nas funcionalidades essenciais, aproveitando ao máximo o código existente.

## 🚀 Estratégia Minimalista

### **Abordagem: "Wrapping" do Core Logic**
- ✅ **Manter** toda a lógica de negócio existente
- ✅ **Reutilizar** ViewModels e Services
- ✅ **Apenas** trocar a camada de apresentação
- ✅ **Mínima** refatoração necessária

## 📊 Componentes por Prioridade

### **🥇 Prioridade 1 - Essenciais**
- **NetworkManager** - ✅ Já cross-platform
- **TrackingController** - ✅ Já cross-platform  
- **ViewModels** - ✅ Reutilizar 100%
- **Services** - ✅ Reutilizar 100%
- **Settings** - ✅ Reutilizar 100%

### **🥈 Prioridade 2 - Importantes**
- **FishingControl** - 🎯 Foco principal
- **GatheringControl** - 🎯 Foco principal
- **DashboardControl** - 🎯 Foco principal
- **NetworkDebugPanel** - 🎯 Foco principal

### **🥉 Prioridade 3 - Opcionais**
- **ItemSearchControl** - Pode ser simplificado
- **DungeonControl** - Pode ser simplificado
- **DamageMeterControl** - Pode ser simplificado

## 🏗️ Estrutura Minimalista

```
src/
├── StatisticsAnalysisTool.Core/           # Extrair apenas o essencial
│   ├── ViewModels/                        # ✅ REUTILIZAR
│   ├── Services/                          # ✅ REUTILIZAR
│   ├── Models/                            # ✅ REUTILIZAR
│   └── Network/                           # ✅ REUTILIZAR
├── StatisticsAnalysisTool.macOS/          # 🆕 Apenas interface
│   ├── Views/                             # Avalonia UI
│   ├── Controls/                          # UserControls Avalonia
│   └── Platform/                          # APIs macOS
└── StatisticsAnalysisTool.Windows/        # WPF (existente)
```

## ⚡ Implementação Rápida (3-5 dias)

### **Dia 1: Preparação**
```bash
# 1. Criar projeto macOS
dotnet new avalonia.mvvm -n StatisticsAnalysisTool.macOS

# 2. Referenciar projeto core
dotnet add reference ../StatisticsAnalysisTool.Core/StatisticsAnalysisTool.Core.csproj

# 3. Instalar dependências
brew install xdotool ffmpeg imagemagick
```

### **Dia 2: APIs macOS**
```csharp
// Platform/MacOSScreenCapture.cs
public class MacOSScreenCapture : IScreenCapture
{
    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        var tempFile = Path.GetTempFileName() + ".png";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "screencapture",
                Arguments = $"-R {x},{y},{width},{height} \"{tempFile}\"",
                UseShellExecute = false
            }
        };
        
        process.Start();
        process.WaitForExit(3000);
        
        if (process.ExitCode == 0 && File.Exists(tempFile))
        {
            var bitmap = new Bitmap(tempFile);
            File.Delete(tempFile);
            return bitmap;
        }
        
        return null;
    }
}
```

### **Dia 3: Interface Principal**
```xml
<!-- Views/MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        x:Class="StatisticsAnalysisTool.macOS.Views.MainWindow"
        Title="Albion Online Statistics Analysis Tool"
        Width="1200" Height="800">
    
    <Grid>
        <TabView>
            <TabViewItem Header="🎣 Fishing">
                <local:FishingControl />
            </TabViewItem>
            <TabViewItem Header="⛏️ Gathering">
                <local:GatheringControl />
            </TabViewItem>
            <TabViewItem Header="📊 Dashboard">
                <local:DashboardControl />
            </TabViewItem>
            <TabViewItem Header="🌐 Network">
                <local:NetworkDebugPanel />
            </TabViewItem>
        </TabView>
    </Grid>
</Window>
```

### **Dia 4: Controles Essenciais**
```xml
<!-- Controls/FishingControl.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             x:Class="StatisticsAnalysisTool.macOS.Controls.FishingControl">
    
    <Grid>
        <StackPanel Margin="20">
            <TextBlock Text="🎣 Fishing Control" FontSize="24" FontWeight="Bold" />
            
            <StackPanel Orientation="Horizontal" Margin="0,20,0,0">
                <Button Content="Start Fishing" Command="{Binding StartFishingCommand}" 
                        Background="Green" Foreground="White" Margin="0,0,10,0" />
                <Button Content="Stop Fishing" Command="{Binding StopFishingCommand}" 
                        Background="Red" Foreground="White" />
            </StackPanel>
            
            <TextBlock Text="{Binding FishingStatus}" FontSize="16" Margin="0,20,0,0" />
            
            <StackPanel Orientation="Horizontal" Margin="0,20,0,0">
                <TextBlock Text="Peixes: " FontWeight="Bold" />
                <TextBlock Text="{Binding FishCount}" />
                <TextBlock Text=" | Tentativas: " FontWeight="Bold" Margin="20,0,0,0" />
                <TextBlock Text="{Binding AttemptCount}" />
            </StackPanel>
        </StackPanel>
    </Grid>
</UserControl>
```

### **Dia 5: Integração e Testes**
```csharp
// Program.cs
public static void Main(string[] args)
{
    // Configurar DI
    var services = new ServiceCollection();
    services.AddSingleton<NetworkManager>();
    services.AddSingleton<TrackingController>();
    services.AddSingleton<IPlatformServices, MacOSPlatformServices>();
    
    var serviceProvider = services.BuildServiceProvider();
    ServiceLocator.Configure(serviceProvider);
    
    // Start Avalonia
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}
```

## 🔧 Configuração Mínima

### **1. Projeto Core (Extrair)**
```xml
<!-- StatisticsAnalysisTool.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.3.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
  </ItemGroup>
</Project>
```

### **2. Projeto macOS**
```xml
<!-- StatisticsAnalysisTool.macOS.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <UseAvalonia>true</UseAvalonia>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\StatisticsAnalysisTool.Core\StatisticsAnalysisTool.Core.csproj" />
  </ItemGroup>
</Project>
```

## 🎯 Funcionalidades Focadas

### **1. Fishing Control (Prioridade Máxima)**
- ✅ Start/Stop Fishing
- ✅ Status em tempo real
- ✅ Contadores de peixes
- ✅ Integração com sistema de pesca existente

### **2. Gathering Control (Prioridade Alta)**
- ✅ Status de coleta
- ✅ Recursos coletados
- ✅ Integração com sistema de gathering existente

### **3. Dashboard (Prioridade Alta)**
- ✅ Estatísticas gerais
- ✅ Status da conexão
- ✅ Informações do jogador

### **4. Network Debug (Prioridade Média)**
- ✅ Status da rede
- ✅ Pacotes recebidos
- ✅ Debug de conexão

## 🧪 Testes Mínimos

### **Teste de Captura de Tela**
```csharp
[Test]
public void MacOSScreenCapture_ShouldWork()
{
    var capture = new MacOSScreenCapture();
    Assert.IsTrue(capture.IsAvailable);
    
    var bitmap = capture.CaptureRegion(0, 0, 100, 100);
    Assert.IsNotNull(bitmap);
}
```

### **Teste de Interface**
```csharp
[Test]
public void MainWindow_ShouldLoad()
{
    var window = new MainWindow();
    Assert.IsNotNull(window);
}
```

## 📋 Checklist de Implementação

### **Dia 1: Preparação**
- [ ] Criar projeto macOS
- [ ] Extrair core logic
- [ ] Configurar referências
- [ ] Testar compilação

### **Dia 2: APIs macOS**
- [ ] Implementar MacOSScreenCapture
- [ ] Implementar MacOSAutomationService
- [ ] Testar APIs básicas
- [ ] Verificar dependências

### **Dia 3: Interface Principal**
- [ ] Implementar MainWindow
- [ ] Configurar TabView
- [ ] Testar navegação
- [ ] Verificar layout

### **Dia 4: Controles Essenciais**
- [ ] Implementar FishingControl
- [ ] Implementar GatheringControl
- [ ] Implementar DashboardControl
- [ ] Testar funcionalidades

### **Dia 5: Integração**
- [ ] Configurar DI
- [ ] Integrar ViewModels
- [ ] Testar aplicação completa
- [ ] Verificar performance

## 🚀 Próximos Passos

1. **Implementar funcionalidades básicas** (3-5 dias)
2. **Testar em macOS** (1 dia)
3. **Otimizar performance** (1-2 dias)
4. **Adicionar funcionalidades avançadas** (conforme necessário)

## 📊 Benefícios da Abordagem Minimalista

1. **Implementação Rápida** - 3-5 dias para versão funcional
2. **Reutilização Máxima** - 90% do código existente
3. **Manutenção Simples** - Poucas mudanças necessárias
4. **Teste Fácil** - Foco nas funcionalidades essenciais
5. **Extensibilidade** - Base sólida para futuras melhorias

---

**Tempo total estimado: 3-5 dias** para implementação mínima funcional do macOS.
