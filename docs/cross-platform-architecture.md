# 🏗️ Arquitetura Cross-Platform - SAT

## 📋 Visão Geral

Este documento descreve a arquitetura proposta para suporte cross-platform no Statistics Analysis Tool.

## 🎯 Princípios Arquiteturais

1. **Separação de Responsabilidades** - Core logic separado de implementações específicas
2. **Abstração de Plataforma** - Interfaces comuns para funcionalidades específicas
3. **Injeção de Dependência** - Facilita testes e manutenção
4. **Modularidade** - Componentes independentes e reutilizáveis

## 🏗️ Estrutura de Camadas

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                      │
├─────────────────────────────────────────────────────────────┤
│  Avalonia UI  │  WPF (Windows)  │  Console Interface      │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                     CORE LAYER                             │
├─────────────────────────────────────────────────────────────┤
│  ViewModels  │  Services  │  Models  │  Interfaces         │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   PLATFORM LAYER                           │
├─────────────────────────────────────────────────────────────┤
│  Windows  │  Linux  │  macOS  │  Shared                   │
└─────────────────────────────────────────────────────────────┘
```

## 🧩 Componentes Principais

### **1. Core Layer**
```csharp
// Interfaces abstratas
public interface IPlatformServices
{
    IAutomationService Automation { get; }
    IScreenCapture ScreenCapture { get; }
    IWebViewService WebView { get; }
    IDialogService Dialogs { get; }
}

// Serviços principais
public class FishingService
{
    private readonly IPlatformServices _platform;
    
    public FishingService(IPlatformServices platform)
    {
        _platform = platform;
    }
}
```

### **2. Platform Layer**
```csharp
// Windows
public class WindowsPlatformServices : IPlatformServices
{
    public IAutomationService Automation => new WindowsAutomationService();
    public IScreenCapture ScreenCapture => new WindowsScreenCapture();
    // ...
}

// Linux
public class LinuxPlatformServices : IPlatformServices
{
    public IAutomationService Automation => new UnixAutomationService();
    public IScreenCapture ScreenCapture => new LinuxScreenCapture();
    // ...
}

// macOS
public class MacOSPlatformServices : IPlatformServices
{
    public IAutomationService Automation => new UnixAutomationService();
    public IScreenCapture ScreenCapture => new MacOSScreenCapture();
    // ...
}
```

### **3. Presentation Layer**
```csharp
// Interface comum
public interface IUserInterface
{
    void ShowMainWindow();
    void ShowFishingPanel();
    void ShowGatheringPanel();
}

// Implementações específicas
public class AvaloniaInterface : IUserInterface { }
public class WPFInterface : IUserInterface { }
public class ConsoleInterface : IUserInterface { }
```

## 🔧 Factory Pattern

### **Platform Service Factory**
```csharp
public static class PlatformServiceFactory
{
    public static IPlatformServices Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsPlatformServices();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxPlatformServices();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOSPlatformServices();
        
        throw new PlatformNotSupportedException();
    }
}
```

### **UI Factory**
```csharp
public static class UIFactory
{
    public static IUserInterface Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WPFInterface();
        
        return new AvaloniaInterface();
    }
}
```

## 📦 Estrutura de Projetos

```
src/
├── StatisticsAnalysisTool.Core/
│   ├── Interfaces/
│   ├── Services/
│   ├── Models/
│   └── ViewModels/
├── StatisticsAnalysisTool.Platform.Windows/
│   ├── Automation/
│   ├── ScreenCapture/
│   ├── Dialogs/
│   └── WebView/
├── StatisticsAnalysisTool.Platform.Linux/
│   ├── Automation/
│   ├── ScreenCapture/
│   └── Dialogs/
├── StatisticsAnalysisTool.Platform.macOS/
│   ├── Automation/
│   ├── ScreenCapture/
│   └── Dialogs/
├── StatisticsAnalysisTool.UI.Avalonia/
│   ├── Views/
│   ├── Controls/
│   └── Styles/
├── StatisticsAnalysisTool.UI.WPF/
│   ├── Views/
│   ├── Controls/
│   └── Styles/
└── StatisticsAnalysisTool.UI.Console/
    ├── Menus/
    └── Interfaces/
```

## 🔄 Fluxo de Execução

### **1. Inicialização**
```csharp
// Program.cs
var platformServices = PlatformServiceFactory.Create();
var ui = UIFactory.Create();

// Configurar DI
services.AddSingleton(platformServices);
services.AddSingleton(ui);

// Iniciar aplicação
ui.ShowMainWindow();
```

### **2. Execução de Serviços**
```csharp
// FishingService
public async Task StartFishingAsync()
{
    // Usar serviços de plataforma
    var screenCapture = _platform.ScreenCapture;
    var automation = _platform.Automation;
    
    // Lógica de pesca
    var region = new Rectangle(100, 100, 600, 400);
    var bitmap = screenCapture.CaptureRegion(region);
    
    if (bitmap != null)
    {
        // Processar imagem
        automation.ClickAt(300, 200);
    }
}
```

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
public void PlatformServices_ShouldWorkOnAllPlatforms()
{
    var services = PlatformServiceFactory.Create();
    Assert.IsNotNull(services.Automation);
    Assert.IsNotNull(services.ScreenCapture);
}
```

## 📊 Benefícios da Arquitetura

1. **Manutenibilidade** - Código organizado e modular
2. **Testabilidade** - Componentes isolados e testáveis
3. **Extensibilidade** - Fácil adição de novas plataformas
4. **Reutilização** - Lógica compartilhada entre plataformas
5. **Performance** - Implementações otimizadas por plataforma

## 🚀 Próximos Passos

1. **Implementar interfaces abstratas**
2. **Criar implementações específicas**
3. **Configurar DI container**
4. **Migrar ViewModels**
5. **Testar em todas as plataformas**

---

Esta arquitetura garante suporte robusto e escalável para múltiplas plataformas.
