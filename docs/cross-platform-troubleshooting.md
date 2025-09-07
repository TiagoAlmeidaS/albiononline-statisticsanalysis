# 🔧 Troubleshooting Cross-Platform - SAT

## 🚨 Problemas Comuns e Soluções

### **1. Captura de Tela**

#### **Problema: MacOSScreenCapture não funciona**
```bash
# Erro: screencapture command not found
# Solução: Verificar se está no macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "macOS detectado"
else
    echo "Não é macOS"
fi
```

#### **Problema: Permissões de captura de tela**
```bash
# macOS: Habilitar permissões
# System Preferences > Security & Privacy > Privacy > Screen Recording
# Adicionar o aplicativo à lista
```

#### **Problema: FFmpeg não encontrado no Linux**
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install ffmpeg

# Arch Linux
sudo pacman -S ffmpeg

# Fedora
sudo dnf install ffmpeg
```

### **2. Automação**

#### **Problema: xdotool não encontrado**
```bash
# Ubuntu/Debian
sudo apt-get install xdotool

# Arch Linux
sudo pacman -S xdotool

# Fedora
sudo dnf install xdotool

# macOS
brew install xdotool
```

#### **Problema: xdotool não funciona no Wayland**
```bash
# Verificar se está usando Wayland
echo $XDG_SESSION_TYPE

# Se for Wayland, usar X11
# No login, escolher "GNOME on Xorg" em vez de "GNOME"
```

### **3. Interface**

#### **Problema: Avalonia UI não inicia**
```xml
<!-- Verificar se UseAvalonia está habilitado -->
<PropertyGroup>
  <UseAvalonia>true</UseAvalonia>
</PropertyGroup>
```

#### **Problema: WPF não funciona no Linux/macOS**
```csharp
// Usar interface console como fallback
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    return new WPFInterface();
}
else
{
    return new ConsoleInterface();
}
```

### **4. Build e Compilação**

#### **Problema: Target framework incorreto**
```xml
<!-- Antes -->
<TargetFramework>net9.0-windows</TargetFramework>

<!-- Depois -->
<TargetFramework>net9.0</TargetFramework>
<UseWPF Condition="'$(OS)' == 'Windows_NT'">true</UseWPF>
```

#### **Problema: Dependências não encontradas**
```bash
# Limpar e restaurar
dotnet clean
dotnet restore
dotnet build
```

### **5. Runtime**

#### **Problema: Exceção de plataforma não suportada**
```csharp
// Verificar plataforma antes de usar APIs específicas
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Usar APIs Windows
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Usar APIs Linux
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    // Usar APIs macOS
}
```

## 🧪 Testes de Diagnóstico

### **Teste de Plataforma**
```csharp
public void TestPlatformDetection()
{
    Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
    Console.WriteLine($"Platform: {RuntimeInformation.OSPlatform}");
    Console.WriteLine($"Architecture: {RuntimeInformation.OSArchitecture}");
    Console.WriteLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
}
```

### **Teste de Dependências**
```csharp
public void TestDependencies()
{
    // Testar xdotool
    try
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xdotool",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        process.WaitForExit();
        Console.WriteLine($"xdotool: {(process.ExitCode == 0 ? "OK" : "FALHOU")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"xdotool: ERRO - {ex.Message}");
    }
    
    // Testar FFmpeg
    try
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        process.WaitForExit();
        Console.WriteLine($"FFmpeg: {(process.ExitCode == 0 ? "OK" : "FALHOU")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FFmpeg: ERRO - {ex.Message}");
    }
}
```

### **Teste de Captura de Tela**
```csharp
public void TestScreenCapture()
{
    var capture = ScreenCaptureFactory.GetProvider();
    Console.WriteLine($"Provider: {capture.ProviderName}");
    Console.WriteLine($"Available: {capture.IsAvailable}");
    
    if (capture.IsAvailable)
    {
        var bitmap = capture.CaptureRegion(0, 0, 100, 100);
        Console.WriteLine($"Capture: {(bitmap != null ? "OK" : "FALHOU")}");
    }
}
```

## 📋 Checklist de Diagnóstico

### **Antes de Executar**
- [ ] .NET 9.0 instalado
- [ ] Dependências externas instaladas
- [ ] Permissões configuradas
- [ ] Build bem-sucedido

### **Durante a Execução**
- [ ] Logs de erro verificados
- [ ] Dependências detectadas
- [ ] APIs funcionando
- [ ] Interface responsiva

### **Após a Execução**
- [ ] Funcionalidades testadas
- [ ] Performance aceitável
- [ ] Sem vazamentos de memória
- [ ] Logs limpos

## 🚨 Logs de Erro Comuns

### **Erro: PlatformNotSupportedException**
```
System.PlatformNotSupportedException: This operation is not supported on this platform
```
**Solução**: Verificar se está usando APIs corretas para a plataforma

### **Erro: FileNotFoundException**
```
System.IO.FileNotFoundException: Could not find file 'xdotool'
```
**Solução**: Instalar xdotool ou verificar PATH

### **Erro: UnauthorizedAccessException**
```
System.UnauthorizedAccessException: Access to the path is denied
```
**Solução**: Verificar permissões de arquivo/diretório

### **Erro: ProcessStartException**
```
System.ComponentModel.Win32Exception: No such file or directory
```
**Solução**: Verificar se comando existe no sistema

## 📞 Suporte

### **Recursos Úteis**
- **Avalonia UI**: https://docs.avaloniaui.net/
- **xdotool**: https://github.com/jordansissel/xdotool
- **FFmpeg**: https://ffmpeg.org/
- **.NET Cross-Platform**: https://docs.microsoft.com/dotnet/core/

### **Comandos de Diagnóstico**
```bash
# Verificar .NET
dotnet --version
dotnet --info

# Verificar dependências
which xdotool
which ffmpeg
which screencapture

# Verificar permissões (macOS)
ls -la /System/Library/CoreServices/SystemUIServer.app
```

---

**Lembre-se**: Sempre testar em ambiente isolado antes de implementar em produção!
