# 🚀 Plano de Implementação Cross-Platform - SAT

## 📋 Resumo Executivo

Este documento detalha o plano de implementação para tornar o Statistics Analysis Tool compatível com Windows, macOS e Linux.

## 🎯 Objetivos

- **Suporte completo** para Windows, macOS e Linux
- **Interface moderna** usando Avalonia UI
- **Performance otimizada** em todas as plataformas
- **Fácil manutenção** e extensão futura

## 📊 Cronograma

| Fase | Duração | Descrição |
|------|---------|-----------|
| **Fase 1** | 1-2 semanas | Preparação e estruturação |
| **Fase 2** | 1-2 semanas | Implementação macOS |
| **Fase 3** | 2-3 semanas | Interface cross-platform |
| **Fase 4** | 1-2 semanas | WebView e dialogs |
| **Fase 5** | 1 semana | Interface console |
| **Total** | **6-8 semanas** | Implementação completa |

## 🏗️ Estrutura de Projetos

```
src/
├── StatisticsAnalysisTool.Core/           # Lógica de negócio
├── StatisticsAnalysisTool.Platform.Windows/  # APIs Windows
├── StatisticsAnalysisTool.Platform.Linux/    # APIs Linux
├── StatisticsAnalysisTool.Platform.macOS/    # APIs macOS
├── StatisticsAnalysisTool.UI.Avalonia/       # Interface cross-platform
├── StatisticsAnalysisTool.UI.WPF/            # Interface Windows (legacy)
└── StatisticsAnalysisTool.UI.Console/        # Interface console
```

## 🔧 Implementações Necessárias

### **1. MacOSScreenCapture**
```csharp
public class MacOSScreenCapture : IScreenCapture
{
    public Bitmap? CaptureRegion(int x, int y, int width, int height)
    {
        // Usar screencapture nativo do macOS
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

### **2. Interface Console**
```csharp
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
    }
}
```

### **3. Configuração Avalonia UI**
```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <UseAvalonia>true</UseAvalonia>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.0.0" />
  <PackageReference Include="Avalonia.Desktop" Version="11.0.0" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.0" />
</ItemGroup>
```

## 📦 Dependências por Plataforma

### **Windows**
- WebView2 (nativo)
- WPF (nativo)
- user32.dll (nativo)

### **Linux**
```bash
sudo apt-get install xdotool ffmpeg imagemagick
```

### **macOS**
```bash
brew install xdotool ffmpeg imagemagick
```

## 🧪 Estratégia de Testes

### **Testes Unitários**
- Testar cada implementação de plataforma
- Verificar disponibilidade de dependências
- Validar funcionalidades básicas

### **Testes de Integração**
- Testar interface em todas as plataformas
- Verificar compatibilidade de APIs
- Validar performance

### **Testes de Usuário**
- Interface intuitiva
- Funcionalidades completas
- Performance aceitável

## 🚀 Próximos Passos Imediatos

1. **Implementar MacOSScreenCapture** (1-2 dias)
2. **Criar interface console** (3-5 dias)
3. **Configurar build multi-platform** (1 dia)
4. **Testar em macOS** (1 dia)

## 📞 Suporte e Recursos

- **Documentação Avalonia**: https://docs.avaloniaui.net/
- **CefSharp**: https://github.com/cefsharp/CefSharp
- **xdotool**: https://github.com/jordansissel/xdotool
- **FFmpeg**: https://ffmpeg.org/

---

**Tempo total estimado: 6-8 semanas** para suporte completo cross-platform.
