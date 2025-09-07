# ⚡ Quick Start - Cross-Platform SAT

## 🎯 Implementação Rápida (1-2 dias)

### **Passo 1: Implementar MacOSScreenCapture**

Crie o arquivo `src/AlbionFishing.Vision/ScreenCapture/MacOSScreenCapture.cs`:

```csharp
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AlbionFishing.Vision.ScreenCapture;

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
            Console.WriteLine($"macOS screen capture failed: {ex.Message}");
        }
        
        return null;
    }

    public Bitmap? CaptureRegion(Rectangle region)
    {
        return CaptureRegion(region.X, region.Y, region.Width, region.Height);
    }
}
```

### **Passo 2: Atualizar ScreenCaptureFactory**

Modifique `src/AlbionFishing.Vision/ScreenCapture/ScreenCaptureFactory.cs`:

```csharp
public static IScreenCapture[] GetAllProviders()
{
    return new IScreenCapture[]
    {
        new WindowsScreenCapture(),
        new LinuxScreenCapture(),
        new MacOSScreenCapture(), // ← ADICIONAR
    };
}

private static IScreenCapture CreateProvider()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        var macProvider = new MacOSScreenCapture();
        if (macProvider.IsAvailable)
            return macProvider;
    }
    // ... resto da implementação existente
}
```

### **Passo 3: Criar Interface Console**

Crie `src/StatisticsAnalysisTool/UI/Console/ConsoleInterface.cs`:

```csharp
using System;

namespace StatisticsAnalysisTool.UI.Console;

public class ConsoleInterface
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
    }
    
    private void HandleMenuChoice(string? choice)
    {
        switch (choice)
        {
            case "1":
                ShowFishingPanel();
                break;
            case "2":
                Console.WriteLine("Estatísticas em desenvolvimento...");
                break;
            case "3":
                Console.WriteLine("Configurações em desenvolvimento...");
                break;
            case "4":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Opção inválida!");
                break;
        }
    }
}
```

### **Passo 4: Configurar Build Multi-Platform**

Modifique `src/StatisticsAnalysisTool/StatisticsAnalysisTool.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <UseWPF Condition="'$(OS)' == 'Windows_NT'">true</UseWPF>
  <Platforms>AnyCPU;x64</Platforms>
</PropertyGroup>
```

### **Passo 5: Testar em macOS**

```bash
# Instalar dependências
brew install xdotool ffmpeg imagemagick

# Build e execução
dotnet build
dotnet run
```

## 🧪 Testes Rápidos

### **Teste de Captura de Tela**
```csharp
var capture = new MacOSScreenCapture();
if (capture.IsAvailable)
{
    var bitmap = capture.CaptureRegion(0, 0, 100, 100);
    Console.WriteLine($"Captura: {bitmap != null}");
}
```

### **Teste de Automação**
```csharp
var automation = new UnixAutomationService();
Console.WriteLine($"Automação disponível: {automation.IsSupported()}");
```

## 📋 Checklist

- [ ] MacOSScreenCapture implementado
- [ ] ScreenCaptureFactory atualizado
- [ ] Interface console criada
- [ ] Build multi-platform configurado
- [ ] Testado em macOS
- [ ] Dependências instaladas

## 🚀 Próximos Passos

1. **Implementar Avalonia UI** (1-2 semanas)
2. **Migrar ViewModels** (1 semana)
3. **WebView cross-platform** (1-2 semanas)
4. **Testes completos** (1 semana)

---

**Tempo estimado para implementação básica: 1-2 dias**
