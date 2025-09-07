# 🍎 Plano de Integração macOS com Avalonia - SAT

## 🎯 Objetivo

Criar uma versão macOS do Statistics Analysis Tool usando Avalonia UI, aproveitando ao máximo as funcionalidades existentes de networking, parsing de dados e lógica de negócio.

## 📊 Análise da Estrutura Atual

### ✅ **Componentes Reutilizáveis (Cross-Platform)**
- **NetworkManager** - Gerenciamento de rede e parsing
- **PacketProviders** - SocketsPacketProvider (cross-platform)
- **TrackingController** - Lógica de rastreamento
- **ViewModels** - Lógica de apresentação
- **Services** - Serviços de negócio
- **Models** - Modelos de dados
- **Settings** - Configurações

### ❌ **Componentes Específicos do Windows**
- **WPF Views** - MainWindow.xaml, UserControls
- **WPF Styles** - Estilos e temas
- **Windows APIs** - user32.dll, GDI+
- **WebView2** - Controle de navegador

## 🏗️ Arquitetura Proposta

```
StatisticsAnalysisTool/
├── Core/                           # ✅ REUTILIZAR
│   ├── ViewModels/                 # ✅ REUTILIZAR
│   ├── Services/                   # ✅ REUTILIZAR
│   ├── Models/                     # ✅ REUTILIZAR
│   └── Network/                    # ✅ REUTILIZAR
├── Platform/
│   ├── Windows/                    # WPF (existente)
│   └── macOS/                      # 🆕 Avalonia + macOS APIs
└── Shared/                         # Recursos compartilhados
```

## 🚀 Estratégia de Implementação

### **Fase 1: Preparação (3-5 dias)**

#### 1.1 Criar Estrutura de Projetos
```
src/
├── StatisticsAnalysisTool.Core/           # Extrair lógica comum
├── StatisticsAnalysisTool.Platform.Windows/  # WPF (existente)
├── StatisticsAnalysisTool.Platform.macOS/    # Avalonia + macOS
└── StatisticsAnalysisTool.Shared/            # Recursos compartilhados
```

#### 1.2 Extrair Core Logic
```csharp
// Mover para StatisticsAnalysisTool.Core
- ViewModels/
- Services/
- Models/
- Network/
- Common/
- Localization/
```

### **Fase 2: Implementação macOS (1-2 semanas)**

#### 2.1 Configurar Avalonia UI
```xml
<!-- StatisticsAnalysisTool.Platform.macOS.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <UseAvalonia>true</UseAvalonia>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\StatisticsAnalysisTool.Core\StatisticsAnalysisTool.Core.csproj" />
  </ItemGroup>
</Project>
```

#### 2.2 Implementar MacOSScreenCapture
```csharp
// Platform.macOS/ScreenCapture/MacOSScreenCapture.cs
public class MacOSScreenCapture : IScreenCapture
{
    public bool IsAvailable => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public string ProviderName => "macOS Screencapture";
    public string Version => "1.0.0";

    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        if (!IsAvailable) return null;
        
        var tempFile = Path.Combine(Path.GetTempPath(), 
            $"capture_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
        
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "screencapture",
                    Arguments = $"-R {x},{y},{width},{height} \"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
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
        }
        catch (Exception ex)
        {
            Log.Error($"macOS screen capture failed: {ex.Message}");
        }
        
        return null;
    }
}
```

#### 2.3 Implementar MacOSAutomationService
```csharp
// Platform.macOS/Automation/MacOSAutomationService.cs
public class MacOSAutomationService : IAutomationService
{
    private readonly string _xdotoolPath;
    
    public MacOSAutomationService()
    {
        _xdotoolPath = FindXdotool();
    }
    
    public bool IsSupported()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && 
               !string.IsNullOrEmpty(_xdotoolPath);
    }
    
    // Implementação similar ao UnixAutomationService
    // mas com verificações específicas do macOS
}
```

### **Fase 3: Interface Avalonia (2-3 semanas)**

#### 3.1 MainWindow Avalonia
```xml
<!-- Views/MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="StatisticsAnalysisTool.Platform.macOS.Views.MainWindow"
        Title="Albion Online Statistics Analysis Tool"
        Width="1200" Height="800">
    
    <Grid>
        <TabView>
            <TabViewItem Header="Dashboard">
                <local:DashboardControl />
            </TabViewItem>
            <TabViewItem Header="Fishing">
                <local:FishingControl />
            </TabViewItem>
            <TabViewItem Header="Gathering">
                <local:GatheringControl />
            </TabViewItem>
            <TabViewItem Header="Network">
                <local:NetworkDebugPanel />
            </TabViewItem>
        </TabView>
    </Grid>
</Window>
```

#### 3.2 UserControls Avalonia
```xml
<!-- Controls/FishingControl.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="StatisticsAnalysisTool.Platform.macOS.Controls.FishingControl">
    
    <Grid>
        <StackPanel>
            <TextBlock Text="🎣 Fishing Control" FontSize="18" FontWeight="Bold" />
            <TextBlock Text="{Binding FishingStatus}" />
            <Button Content="Start Fishing" Command="{Binding StartFishingCommand}" />
            <Button Content="Stop Fishing" Command="{Binding StopFishingCommand}" />
        </StackPanel>
    </Grid>
</UserControl>
```

#### 3.3 ViewModels Compartilhados
```csharp
// Core/ViewModels/FishingViewModel.cs
public class FishingViewModel : INotifyPropertyChanged
{
    private readonly IFishingService _fishingService;
    private readonly IPlatformServices _platformServices;
    
    public FishingViewModel(IFishingService fishingService, IPlatformServices platformServices)
    {
        _fishingService = fishingService;
        _platformServices = platformServices;
    }
    
    // ViewModels compartilhados entre WPF e Avalonia
    public string FishingStatus { get; set; } = "Inactive";
    public ICommand StartFishingCommand { get; }
    public ICommand StopFishingCommand { get; }
}
```

### **Fase 4: Integração com Core (1 semana)**

#### 4.1 Service Locator Cross-Platform
```csharp
// Core/ServiceLocator.cs
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;
    
    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public static T Resolve<T>() where T : class
    {
        return _serviceProvider?.GetService<T>() ?? 
               throw new InvalidOperationException("Service not registered");
    }
}
```

#### 4.2 Dependency Injection
```csharp
// Program.cs (macOS)
public static void Main(string[] args)
{
    var services = new ServiceCollection();
    
    // Core services
    services.AddSingleton<NetworkManager>();
    services.AddSingleton<TrackingController>();
    services.AddSingleton<IFishingService, FishingService>();
    
    // Platform services
    services.AddSingleton<IPlatformServices, MacOSPlatformServices>();
    
    // ViewModels
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<FishingViewModel>();
    
    var serviceProvider = services.BuildServiceProvider();
    ServiceLocator.Configure(serviceProvider);
    
    // Start Avalonia
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}
```

## 🔧 Implementações Específicas

### **1. WebView para macOS**
```csharp
// Platform.macOS/WebView/MacOSWebViewService.cs
public class MacOSWebViewService : IWebViewService
{
    private WebView? _webView;
    
    public void Initialize(Control parent)
    {
        // Usar CefSharp ou WebView nativo do macOS
        _webView = new WebView
        {
            Address = "https://albiononline.com"
        };
        
        parent.Content = _webView;
    }
}
```

### **2. Dialogs para macOS**
```csharp
// Platform.macOS/Dialogs/MacOSDialogService.cs
public class MacOSDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string title, string filter)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'tell application \"System Events\" to return POSIX path of (choose file with prompt \"{title}\")'",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        return process.ExitCode == 0 ? result.Trim() : null;
    }
}
```

### **3. Notifications para macOS**
```csharp
// Platform.macOS/Notifications/MacOSNotificationService.cs
public class MacOSNotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e 'display notification \"{message}\" with title \"{title}\"'",
                UseShellExecute = false
            }
        };
        
        process.Start();
    }
}
```

## 📦 Dependências macOS

### **Instalação via Homebrew**
```bash
# Dependências necessárias
brew install xdotool ffmpeg imagemagick

# Para desenvolvimento
brew install --cask dotnet
```

### **Permissões Necessárias**
- **Screen Recording** - Para captura de tela
- **Accessibility** - Para automação (xdotool)
- **Notifications** - Para notificações

## 🧪 Estratégia de Testes

### **Testes Unitários**
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

### **Testes de Integração**
```csharp
[Test]
public void FishingService_ShouldWorkOnMacOS()
{
    var services = new ServiceCollection();
    services.AddSingleton<IPlatformServices, MacOSPlatformServices>();
    services.AddSingleton<IFishingService, FishingService>();
    
    var provider = services.BuildServiceProvider();
    var fishingService = provider.GetService<IFishingService>();
    
    Assert.IsNotNull(fishingService);
}
```

## 📋 Checklist de Implementação

### **Fase 1: Preparação**
- [ ] Criar estrutura de projetos
- [ ] Extrair core logic
- [ ] Configurar build multi-platform
- [ ] Testar compilação

### **Fase 2: macOS APIs**
- [ ] Implementar MacOSScreenCapture
- [ ] Implementar MacOSAutomationService
- [ ] Implementar MacOSDialogService
- [ ] Testar APIs básicas

### **Fase 3: Interface Avalonia**
- [ ] Configurar Avalonia UI
- [ ] Implementar MainWindow
- [ ] Migrar UserControls
- [ ] Testar interface

### **Fase 4: Integração**
- [ ] Configurar DI
- [ ] Integrar ViewModels
- [ ] Testar funcionalidades
- [ ] Otimizar performance

## 🚀 Próximos Passos Imediatos

1. **Criar estrutura de projetos** (1 dia)
2. **Implementar MacOSScreenCapture** (1 dia)
3. **Configurar Avalonia UI** (2-3 dias)
4. **Migrar ViewModels** (2-3 dias)
5. **Testar integração** (1-2 dias)

## 📊 Benefícios da Abordagem

1. **Reutilização Máxima** - 80% do código existente
2. **Manutenção Simplificada** - Core logic compartilhado
3. **Performance Otimizada** - APIs nativas do macOS
4. **Interface Nativa** - Avalonia UI com tema macOS
5. **Extensibilidade** - Fácil adição de novas funcionalidades

---

**Tempo total estimado: 4-6 semanas** para implementação completa do macOS com Avalonia UI.
