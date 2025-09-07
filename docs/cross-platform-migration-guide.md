# 🌐 Guia de Migração Cross-Platform - Albion Online Statistics Analysis Tool

## 📋 Visão Geral

Este documento fornece um guia completo para migrar o Statistics Analysis Tool (SAT) para suporte cross-platform, permitindo execução em Windows, macOS e Linux.

## 🎯 Objetivos

- ✅ Suporte completo para Windows, macOS e Linux
- ✅ Manter funcionalidades existentes
- ✅ Interface moderna e responsiva
- ✅ Performance otimizada
- ✅ Fácil manutenção e extensão

## 📊 Status Atual

### ✅ **Já Implementado:**
- Sistema de automação para Linux (xdotool)
- Captura de tela para Linux (FFmpeg)
- Core logic cross-platform (.NET 9.0)
- Sistema de logging (Serilog)
- Visão computacional (OpenCV)

### ❌ **Precisa Implementar:**
- Interface cross-platform (atualmente apenas WPF)
- Captura de tela para macOS
- WebView cross-platform
- Dialogs cross-platform
- APIs específicas do macOS

## 🏗️ Arquitetura Proposta

```
StatisticsAnalysisTool/
├── Core/                           # Lógica de negócio (cross-platform)
│   ├── Services/                   # Serviços principais
│   ├── Models/                     # Modelos de dados
│   └── Interfaces/                 # Interfaces abstratas
├── Platform/
│   ├── Windows/                    # Implementações Windows
│   ├── Linux/                      # Implementações Linux
│   └── macOS/                      # Implementações macOS
├── UI/
│   ├── Avalonia/                   # Interface cross-platform
│   ├── WPF/                        # Interface Windows (legacy)
│   └── Console/                    # Interface console (fallback)
└── Shared/                         # Recursos compartilhados
```

## 🚀 Fases de Implementação

### **Fase 1: Preparação (1-2 semanas)**

#### 1.1 Refatoração do Target Framework
```xml
<!-- StatisticsAnalysisTool.csproj -->
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <UseWPF Condition="'$(OS)' == 'Windows_NT'">true</UseWPF>
  <UseAvalonia Condition="'$(OS)' != 'Windows_NT'">true</UseAvalonia>
  <Platforms>AnyCPU;x64</Platforms>
</PropertyGroup>
```

#### 1.2 Criação de Estrutura de Pastas
```
src/
├── StatisticsAnalysisTool.Core/
├── StatisticsAnalysisTool.Platform.Windows/
├── StatisticsAnalysisTool.Platform.Linux/
├── StatisticsAnalysisTool.Platform.macOS/
├── StatisticsAnalysisTool.UI.Avalonia/
├── StatisticsAnalysisTool.UI.WPF/
└── StatisticsAnalysisTool.UI.Console/
```

#### 1.3 Abstração de Serviços
```csharp
// Core/Interfaces/IPlatformServices.cs
public interface IPlatformServices
{
    IAutomationService Automation { get; }
    IScreenCapture ScreenCapture { get; }
    IWebViewService WebView { get; }
    IDialogService Dialogs { get; }
    IFileService FileService { get; }
    INotificationService Notifications { get; }
}
```

### **Fase 2: Implementação macOS (1-2 semanas)**

#### 2.1 MacOSScreenCapture
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
            LogError($"macOS screen capture failed: {ex.Message}");
        }
        
        return null;
    }
}
```

#### 2.2 MacOSAutomationService
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

#### 2.3 MacOSDialogService
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

### **Fase 3: Interface Cross-Platform (2-3 semanas)**

#### 3.1 Configuração Avalonia UI
```xml
<!-- StatisticsAnalysisTool.UI.Avalonia.csproj -->
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
</Project>
```

#### 3.2 MainWindow Avalonia
```xml
<!-- UI/Avalonia/Views/MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Class="StatisticsAnalysisTool.UI.Avalonia.Views.MainWindow"
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
        </TabView>
    </Grid>
</Window>
```

#### 3.3 ViewModels Compartilhados
```csharp
// UI/Shared/ViewModels/MainWindowViewModel.cs
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IPlatformServices _platformServices;
    
    public MainWindowViewModel(IPlatformServices platformServices)
    {
        _platformServices = platformServices;
    }
    
    // ViewModels compartilhados entre WPF e Avalonia
    public FishingViewModel Fishing { get; }
    public GatheringViewModel Gathering { get; }
    public DashboardViewModel Dashboard { get; }
}
```

### **Fase 4: WebView Cross-Platform (1-2 semanas)**

#### 4.1 CefSharp Integration
```xml
<!-- PackageReferences -->
<PackageReference Include="CefSharp.Avalonia" Version="120.0.0" />
<PackageReference Include="CefSharp.Common" Version="120.0.0" />
```

#### 4.2 WebView Service
```csharp
// Core/Services/WebViewService.cs
public class WebViewService : IWebViewService
{
    private WebView? _webView;
    
    public void Initialize(Control parent)
    {
        _webView = new WebView
        {
            Address = "https://albiononline.com"
        };
        
        parent.Content = _webView;
    }
    
    public void Navigate(string url)
    {
        _webView?.Navigate(url);
    }
}
```

### **Fase 5: Interface Console (1 semana)**

#### 5.1 Console Interface
```csharp
// UI.Console/ConsoleInterface.cs
public class ConsoleInterface : IUserInterface
{
    public void ShowMainWindow()
    {
        Console.Clear();
        Console.WriteLine("🎣 Albion Online Statistics Analysis Tool");
        Console.WriteLine("==========================================");
        Console.WriteLine("1. Iniciar Pesca");
        Console.WriteLine("2. Ver Estatísticas");
        Console.WriteLine("3. Configurações");
        Console.WriteLine("4. Sair");
        Console.Write("Escolha uma opção: ");
        
        var choice = Console.ReadLine();
        HandleMenuChoice(choice);
    }
    
    public void ShowFishingPanel()
    {
        Console.WriteLine("\n🎣 Painel de Pesca");
        Console.WriteLine("==================");
        Console.WriteLine("Status: Ativo");
        Console.WriteLine("Peixes: 0");
        Console.WriteLine("Tentativas: 0");
        
        // Implementar interface de pesca
    }
}
```

## 🔧 Configuração de Build

### **Multi-Platform Build Script**
```bash
#!/bin/bash
# build.sh

echo "Building Statistics Analysis Tool for all platforms..."

# Windows
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/windows-x64

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64

# macOS
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/macos-x64

echo "Build completed!"
```

### **GitHub Actions CI/CD**
```yaml
# .github/workflows/build.yml
name: Build Cross-Platform

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest, macos-latest]
        dotnet-version: [9.0.x]
    
    runs-on: ${{ matrix.os }}
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ matrix.dotnet-version }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --configuration Release --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -r ${{ matrix.os == 'windows-latest' && 'win-x64' || matrix.os == 'ubuntu-latest' && 'linux-x64' || 'osx-x64' }} --self-contained true -o ./publish
```

## 📦 Dependências por Plataforma

### **Windows**
```xml
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.3405.78" />
<PackageReference Include="Ookii.Dialogs.Wpf" Version="5.0.1" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc5.4" />
```

### **Linux**
```bash
# Ubuntu/Debian
sudo apt-get install xdotool ffmpeg imagemagick

# Arch Linux
sudo pacman -S xdotool ffmpeg imagemagick

# Fedora
sudo dnf install xdotool ffmpeg ImageMagick
```

### **macOS**
```bash
# Homebrew
brew install xdotool ffmpeg imagemagick

# MacPorts
sudo port install xdotool ffmpeg ImageMagick
```

## 🧪 Testes

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
public void PlatformServices_ShouldWorkOnAllPlatforms()
{
    var services = PlatformServiceFactory.Create();
    Assert.IsNotNull(services.Automation);
    Assert.IsNotNull(services.ScreenCapture);
    Assert.IsTrue(services.Automation.IsSupported());
}
```

## 📋 Checklist de Implementação

### **Fase 1: Preparação**
- [ ] Refatorar target framework
- [ ] Criar estrutura de pastas
- [ ] Implementar abstrações de serviços
- [ ] Configurar build multi-platform

### **Fase 2: macOS**
- [ ] Implementar MacOSScreenCapture
- [ ] Implementar MacOSAutomationService
- [ ] Implementar MacOSDialogService
- [ ] Testar funcionalidades básicas

### **Fase 3: Interface**
- [ ] Configurar Avalonia UI
- [ ] Migrar ViewModels
- [ ] Implementar controles customizados
- [ ] Testar interface em todas as plataformas

### **Fase 4: WebView**
- [ ] Integrar CefSharp
- [ ] Implementar WebViewService
- [ ] Testar navegação web
- [ ] Otimizar performance

### **Fase 5: Console**
- [ ] Implementar ConsoleInterface
- [ ] Criar menus interativos
- [ ] Implementar logging visual
- [ ] Testar em modo servidor

## 🚀 Próximos Passos

1. **Implementar MacOSScreenCapture** (1-2 dias)
2. **Criar interface console** (3-5 dias)
3. **Configurar Avalonia UI** (1-2 semanas)
4. **Migrar ViewModels** (1 semana)
5. **Testes e validação** (1-2 semanas)

## 📞 Suporte

Para dúvidas ou problemas durante a implementação:
- Criar issue no GitHub
- Consultar documentação do Avalonia UI
- Verificar logs de erro detalhados
- Testar em ambiente isolado

---

**Tempo total estimado: 6-8 semanas** para implementação completa do suporte cross-platform.
