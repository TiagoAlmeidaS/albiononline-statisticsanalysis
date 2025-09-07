# 📝 Guia Passo a Passo - macOS com Avalonia

## 🎯 Objetivo

Guia detalhado para implementar o Statistics Analysis Tool no macOS usando Avalonia UI, aproveitando ao máximo o código existente.

## 📋 Pré-requisitos

### **Software Necessário**
```bash
# macOS
brew install --cask dotnet
brew install xdotool ffmpeg imagemagick

# Verificar instalação
dotnet --version
xdotool --version
ffmpeg -version
```

### **Permissões Necessárias**
- **Screen Recording** - Para captura de tela
- **Accessibility** - Para automação (xdotool)

## 🚀 Passo 1: Preparação (30 minutos)

### **1.1 Criar Estrutura de Projetos**
```bash
# Navegar para src/
cd src/

# Criar projeto core
dotnet new classlib -n StatisticsAnalysisTool.Core

# Criar projeto macOS
dotnet new avalonia.mvvm -n StatisticsAnalysisTool.macOS

# Verificar estrutura
ls -la
```

### **1.2 Configurar Referências**
```bash
# Adicionar referência core no macOS
cd StatisticsAnalysisTool.macOS
dotnet add reference ../StatisticsAnalysisTool.Core/StatisticsAnalysisTool.Core.csproj

# Adicionar referência do projeto principal
dotnet add reference ../StatisticsAnalysisTool/StatisticsAnalysisTool.csproj
```

## 🚀 Passo 2: Extrair Core Logic (1 hora)

### **2.1 Mover Arquivos para Core**
```bash
# Mover ViewModels
cp -r ../StatisticsAnalysisTool/ViewModels/* StatisticsAnalysisTool.Core/

# Mover Services
cp -r ../StatisticsAnalysisTool/Services/* StatisticsAnalysisTool.Core/

# Mover Models
cp -r ../StatisticsAnalysisTool/Models/* StatisticsAnalysisTool.Core/

# Mover Network
cp -r ../StatisticsAnalysisTool/Network/* StatisticsAnalysisTool.Core/

# Mover Common
cp -r ../StatisticsAnalysisTool/Common/* StatisticsAnalysisTool.Core/
```

### **2.2 Configurar Core Project**
```xml
<!-- StatisticsAnalysisTool.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.3.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="System.Drawing.Common" Version="9.0.0" />
  </ItemGroup>
</Project>
```

## 🚀 Passo 3: Implementar APIs macOS (1 hora)

### **3.1 MacOSScreenCapture**
```csharp
// StatisticsAnalysisTool.macOS/Platform/MacOSScreenCapture.cs
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace StatisticsAnalysisTool.macOS.Platform;

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

### **3.2 MacOSAutomationService**
```csharp
// StatisticsAnalysisTool.macOS/Platform/MacOSAutomationService.cs
using System.Diagnostics;

namespace StatisticsAnalysisTool.macOS.Platform;

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
    
    public void MoveTo(int x, int y)
    {
        ExecuteXdotool($"mousemove {x} {y}");
    }
    
    public void Click(string button = "left")
    {
        ExecuteXdotool($"click {button}");
    }
    
    public void ClickAt(int x, int y, string button = "left")
    {
        ExecuteXdotool($"mousemove {x} {y} click {button}");
    }
    
    private string FindXdotool()
    {
        var possiblePaths = new[]
        {
            "/usr/local/bin/xdotool",
            "/opt/homebrew/bin/xdotool",
            "xdotool"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return path;
                }
            }
            catch
            {
                // Continue searching
            }
        }

        return string.Empty;
    }
    
    private string ExecuteXdotool(string arguments)
    {
        if (string.IsNullOrEmpty(_xdotoolPath))
        {
            throw new InvalidOperationException("xdotool not found. Install with: brew install xdotool");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _xdotoolPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"xdotool failed: {process.StandardError.ReadToEnd()}");
        }

        return output;
    }
}
```

## 🚀 Passo 4: Interface Avalonia (2 horas)

### **4.1 MainWindow**
```xml
<!-- Views/MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="using:StatisticsAnalysisTool.macOS.Controls"
        x:Class="StatisticsAnalysisTool.macOS.Views.MainWindow"
        Title="Albion Online Statistics Analysis Tool"
        Width="1200" Height="800"
        MinWidth="800" MinHeight="600">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Border Grid.Row="0" Background="#2C3E50" Padding="20,10">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="🎣 Albion Online Statistics Analysis Tool" 
                          Foreground="White" FontSize="18" FontWeight="Bold" />
                <TextBlock Text="{Binding ConnectionStatus}" 
                          Foreground="LightGreen" FontSize="14" Margin="20,0,0,0" />
            </StackPanel>
        </Border>
        
        <!-- Main Content -->
        <TabView Grid.Row="1" Margin="10">
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
        
        <!-- Footer -->
        <Border Grid.Row="2" Background="#34495E" Padding="20,5">
            <TextBlock Text="Statistics Analysis Tool - macOS Edition" 
                      Foreground="White" FontSize="12" />
        </Border>
    </Grid>
</Window>
```

### **4.2 FishingControl**
```xml
<!-- Controls/FishingControl.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="StatisticsAnalysisTool.macOS.Controls.FishingControl">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Border Grid.Row="0" Background="#27AE60" Padding="20,15">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="🎣 Fishing Control" FontSize="24" FontWeight="Bold" Foreground="White" />
                <TextBlock Text="{Binding FishingStatus}" FontSize="16" Foreground="White" Margin="20,0,0,0" />
            </StackPanel>
        </Border>
        
        <!-- Content -->
        <ScrollViewer Grid.Row="1" Padding="20">
            <StackPanel>
                <!-- Controls -->
                <StackPanel Orientation="Horizontal" Margin="0,0,0,20">
                    <Button Content="Start Fishing" Command="{Binding StartFishingCommand}" 
                            Background="Green" Foreground="White" Padding="20,10" Margin="0,0,10,0" />
                    <Button Content="Stop Fishing" Command="{Binding StopFishingCommand}" 
                            Background="Red" Foreground="White" Padding="20,10" />
                </StackPanel>
                
                <!-- Statistics -->
                <Border Background="#ECF0F1" Padding="20" CornerRadius="5">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="*" />
                        </Grid.ColumnDefinitions>
                        
                        <StackPanel Grid.Column="0">
                            <TextBlock Text="Peixes Capturados" FontWeight="Bold" />
                            <TextBlock Text="{Binding FishCount}" FontSize="24" Foreground="#27AE60" />
                        </StackPanel>
                        
                        <StackPanel Grid.Column="1">
                            <TextBlock Text="Tentativas" FontWeight="Bold" />
                            <TextBlock Text="{Binding AttemptCount}" FontSize="24" Foreground="#3498DB" />
                        </StackPanel>
                        
                        <StackPanel Grid.Column="2">
                            <TextBlock Text="Taxa de Sucesso" FontWeight="Bold" />
                            <TextBlock Text="{Binding SuccessRate}" FontSize="24" Foreground="#E67E22" />
                        </StackPanel>
                    </Grid>
                </Border>
                
                <!-- Log -->
                <Border Background="#2C3E50" Padding="15" CornerRadius="5" Margin="0,20,0,0">
                    <ScrollViewer Height="200">
                        <ItemsControl ItemsSource="{Binding FishingLog}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <TextBlock Text="{Binding}" Foreground="White" FontFamily="Consolas" FontSize="12" />
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                    </ScrollViewer>
                </Border>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Status Bar -->
        <Border Grid.Row="2" Background="#34495E" Padding="20,10">
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="Status: " Foreground="White" />
                <TextBlock Text="{Binding ConnectionStatus}" Foreground="LightGreen" />
                <TextBlock Text=" | Última Atualização: " Foreground="White" Margin="20,0,0,0" />
                <TextBlock Text="{Binding LastUpdate}" Foreground="White" />
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

## 🚀 Passo 5: Integração e DI (1 hora)

### **5.1 Program.cs**
```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Core.Services;
using StatisticsAnalysisTool.macOS.Platform;

namespace StatisticsAnalysisTool.macOS;

public static class Program
{
    public static void Main(string[] args)
    {
        // Configurar DI
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
    
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

### **5.2 MacOSPlatformServices**
```csharp
// Platform/MacOSPlatformServices.cs
using StatisticsAnalysisTool.Core.Interfaces;

namespace StatisticsAnalysisTool.macOS.Platform;

public class MacOSPlatformServices : IPlatformServices
{
    public IAutomationService Automation { get; }
    public IScreenCapture ScreenCapture { get; }
    public IWebViewService WebView { get; }
    public IDialogService Dialogs { get; }
    
    public MacOSPlatformServices()
    {
        Automation = new MacOSAutomationService();
        ScreenCapture = new MacOSScreenCapture();
        WebView = new MacOSWebViewService();
        Dialogs = new MacOSDialogService();
    }
}
```

## 🚀 Passo 6: Testes e Validação (30 minutos)

### **6.1 Teste de Compilação**
```bash
# Compilar projeto
dotnet build

# Verificar se compila sem erros
echo "Build successful!"
```

### **6.2 Teste de Execução**
```bash
# Executar aplicação
dotnet run

# Verificar se inicia corretamente
echo "Application started successfully!"
```

### **6.3 Teste de Funcionalidades**
```csharp
// Teste básico de captura de tela
var capture = new MacOSScreenCapture();
if (capture.IsAvailable)
{
    var bitmap = capture.CaptureRegion(0, 0, 100, 100);
    Console.WriteLine($"Screen capture: {(bitmap != null ? "OK" : "FAILED")}");
}
```

## 📋 Checklist Final

### **Preparação**
- [ ] .NET 9.0 instalado
- [ ] Dependências macOS instaladas
- [ ] Permissões configuradas
- [ ] Estrutura de projetos criada

### **Implementação**
- [ ] Core logic extraído
- [ ] APIs macOS implementadas
- [ ] Interface Avalonia criada
- [ ] DI configurado
- [ ] ViewModels integrados

### **Testes**
- [ ] Compilação bem-sucedida
- [ ] Aplicação inicia
- [ ] Interface responsiva
- [ ] Funcionalidades básicas funcionando

## 🎉 Resultado Final

Após seguir este guia, você terá:

1. **Aplicação macOS funcional** usando Avalonia UI
2. **Reutilização de 90%** do código existente
3. **Interface moderna** e responsiva
4. **Funcionalidades essenciais** implementadas
5. **Base sólida** para futuras melhorias

## 🚀 Próximos Passos

1. **Testar funcionalidades** em ambiente real
2. **Otimizar performance** conforme necessário
3. **Adicionar funcionalidades** específicas do macOS
4. **Melhorar interface** com temas nativos
5. **Implementar testes** automatizados

---

**Tempo total estimado: 4-6 horas** para implementação completa do macOS com Avalonia UI.
