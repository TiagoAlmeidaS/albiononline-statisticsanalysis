# 🎯 Exemplo de Configuração do Sistema de Visão

## 📋 **Sistema de Configuração Centralizado**

Este documento mostra como usar o novo sistema de configuração centralizado para o `AlbionFishing.Vision` com a pasta `Data/images/`.

## 🏗️ **Estrutura da Solução**

### **1. Componentes Criados:**

```
AlbionFishing.Vision/
├── Configuration/
│   ├── VisionConfiguration.cs              # Configuração centralizada
│   ├── IVisionConfigurationService.cs      # Interface do serviço
│   └── VisionConfigurationService.cs       # Implementação do serviço
└── DependencyInjection/
    └── VisionServiceCollectionExtensions.cs # Extensões de DI
```

### **2. Estrutura de Pastas:**

```
Data/
└── images/
    ├── bobber.png                    # Template básico do bobber
    ├── bobber_in_water.png          # Template do bobber na água
    └── bobber_in_water2.png         # Template alternativo
```

## 🚀 **Como Usar**

### **1. Configuração Básica:**

```csharp
// Em Program.cs ou Startup.cs
services.AddVisionServices();

// Ou com configuração específica
services.AddVisionServices(
    imagesBaseDirectory: "Data/images",
    enableAutoDiscovery: true,
    enableDebug: false
);
```

### **2. Configuração com Arquivo:**

```json
// appsettings.json
{
  "Vision": {
    "ImagesBaseDirectory": "Data/images",
    "BobberTemplatePath": "bobber.png",
    "BobberInWaterTemplatePath": "bobber_in_water.png",
    "BobberInWaterTemplatePath2": "bobber_in_water2.png",
    "EnableAutoTemplateDiscovery": true,
    "EnableTemplateCache": true,
    "EnableTemplateValidation": true,
    "MinTemplateSize": 16,
    "MaxTemplateSize": 128,
    "EnableDetailedLogging": false,
    "EnableVisualDebug": false,
    "DebugImageDirectory": "debug/vision"
  }
}
```

```csharp
// Em Program.cs
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

services.AddVisionServices(configuration);
```

### **3. Configuração de Detectores Específicos:**

```csharp
// Configurar detector específico
services.AddDetectorConfiguration(
    detectorName: "TemplateBobberDetector",
    templatePath: "bobber.png",
    confidenceThreshold: 0.7,
    enableDebug: true
);

services.AddDetectorConfiguration(
    detectorName: "SignalEnhancedDetector",
    templatePath: "bobber_in_water.png",
    confidenceThreshold: 0.8,
    enableDebug: false
);

services.AddDetectorConfiguration(
    detectorName: "HybridBobberDetector",
    templatePath: "bobber_in_water.png",
    confidenceThreshold: 0.75,
    enableDebug: true
);
```

### **4. Uso com Injeção de Dependência:**

```csharp
public class FishingService
{
    private readonly IVisionConfigurationService _configService;
    private readonly IBobberDetectorFactory _detectorFactory;
    private readonly ILogger<FishingService> _logger;
    
    public FishingService(
        IVisionConfigurationService configService,
        IBobberDetectorFactory detectorFactory,
        ILogger<FishingService> logger)
    {
        _configService = configService;
        _detectorFactory = detectorFactory;
        _logger = logger;
    }
    
    public async Task<bool> DetectBobberAsync(Rectangle fishingArea)
    {
        try
        {
            // Obter configuração do detector
            var config = _configService.GetDetectorConfig("HybridBobberDetector");
            
            // Criar detector com configuração
            var detector = _detectorFactory.CreateDetector(DetectorType.Hybrid, config);
            
            // Detectar bobber
            var result = detector.DetectInArea(fishingArea, config.ConfidenceThreshold);
            
            _logger.LogDebug("Detecção: {Detected}, Score: {Score}", 
                result.Detected, result.Score);
            
            return result.Detected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante detecção do bobber");
            return false;
        }
    }
}
```

### **5. Configuração Dinâmica:**

```csharp
public class DynamicVisionConfig
{
    private readonly IVisionConfigurationService _configService;
    
    public DynamicVisionConfig(IVisionConfigurationService configService)
    {
        _configService = configService;
    }
    
    public void UpdateTemplatePath(string detectorName, string newTemplatePath)
    {
        var config = _configService.GetDetectorConfig(detectorName);
        config.TemplatePath = newTemplatePath;
        _configService.SetDetectorConfig(detectorName, config);
    }
    
    public void UpdateConfidenceThreshold(string detectorName, double newThreshold)
    {
        var config = _configService.GetDetectorConfig(detectorName);
        config.ConfidenceThreshold = newThreshold;
        _configService.SetDetectorConfig(detectorName, config);
    }
}
```

## 🎯 **Funcionalidades Implementadas**

### **1. Descoberta Automática de Templates:**

```csharp
// O sistema automaticamente procura por templates em:
// 1. Data/images/bobber.png
// 2. Data/images/bobber_in_water.png
// 3. Data/images/bobber_in_water2.png
// 4. resources/bobber.png (fallback)
// 5. Busca recursiva em subdiretórios

var configService = serviceProvider.GetService<IVisionConfigurationService>();
var templatePath = configService.GetTemplatePath("bobber_in_water.png");
// Retorna: "C:\projeto\Data\images\bobber_in_water.png"
```

### **2. Validação de Templates:**

```csharp
// Validar se template é válido
var isValid = configService.ValidateTemplate(templatePath);
if (!isValid)
{
    _logger.LogWarning("Template inválido: {TemplatePath}", templatePath);
}
```

### **3. Configuração por Detector:**

```csharp
// Configuração específica para cada detector
var templateConfig = new DetectorSpecificConfig
{
    TemplatePath = "bobber.png",
    ConfidenceThreshold = 0.7,
    EnableDebugWindow = true,
    EnableColorFiltering = true,
    EnableHSVFiltering = true,
    EnableMultiScale = true,
    Scales = new[] { 0.6, 0.8, 1.0, 1.2 },
    MaxProcessingTimeMs = 100
};

configService.SetDetectorConfig("TemplateBobberDetector", templateConfig);
```

### **4. Logging Detalhado:**

```csharp
// Habilitar logs detalhados
var config = new VisionConfiguration
{
    EnableDetailedLogging = true,
    EnableVisualDebug = true,
    DebugImageDirectory = "debug/vision"
};

// Logs automáticos incluem:
// - Caminhos de templates encontrados
// - Validação de templates
// - Configurações de detectores
// - Erros e avisos
```

## 🔧 **Configurações Avançadas**

### **1. Configuração de Performance:**

```csharp
var config = new VisionConfiguration
{
    EnableTemplateCache = true,           // Cache de templates
    TemplateLoadTimeoutMs = 5000,        // Timeout de carregamento
    EnableTemplateValidation = true,      // Validação de templates
    MinTemplateSize = 16,                // Tamanho mínimo
    MaxTemplateSize = 128,               // Tamanho máximo
    EnableAutoTemplateDiscovery = true   // Descoberta automática
};
```

### **2. Configuração de Debug:**

```csharp
var config = new VisionConfiguration
{
    EnableDetailedLogging = true,        // Logs detalhados
    EnableVisualDebug = true,            // Debug visual
    DebugImageDirectory = "debug/vision" // Diretório de debug
};
```

### **3. Configuração de Detectores:**

```csharp
// Template Bobber Detector
var templateConfig = new DetectorSpecificConfig
{
    TemplatePath = "bobber.png",
    ConfidenceThreshold = 0.6,
    EnableDebugWindow = false,
    EnableColorFiltering = true,
    EnableHSVFiltering = true,
    EnableGradientChannel = false,
    EnableMultiScale = false,
    MaxProcessingTimeMs = 50
};

// Signal Enhanced Detector
var signalConfig = new DetectorSpecificConfig
{
    TemplatePath = "bobber_in_water.png",
    ConfidenceThreshold = 0.8,
    EnableDebugWindow = true,
    EnableColorFiltering = true,
    EnableHSVFiltering = true,
    EnableGradientChannel = true,
    EnableMultiScale = true,
    EnableSignalAnalysis = true,
    Scales = new[] { 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2 },
    MaxProcessingTimeMs = 100
};

// Hybrid Detector
var hybridConfig = new DetectorSpecificConfig
{
    TemplatePath = "bobber_in_water.png",
    ConfidenceThreshold = 0.75,
    EnableDebugWindow = true,
    EnableColorFiltering = true,
    EnableHSVFiltering = true,
    EnableGradientChannel = true,
    EnableMultiScale = true,
    EnableSignalAnalysis = true,
    Scales = new[] { 0.6, 0.8, 1.0, 1.2 },
    MaxProcessingTimeMs = 80
};
```

## 📊 **Monitoramento e Logs**

### **1. Logs Automáticos:**

```csharp
// O sistema gera logs automáticos para:
// - Inicialização da configuração
// - Descoberta de templates
// - Validação de templates
// - Criação de detectores
// - Erros e avisos
```

### **2. Eventos de Configuração:**

```csharp
// Escutar mudanças de configuração
configService.ConfigurationUpdated += (sender, e) => {
    Console.WriteLine($"Configuração atualizada: {e.UpdateTime}");
    Console.WriteLine($"Diretório base: {e.NewConfiguration.ImagesBaseDirectory}");
};
```

### **3. Estatísticas de Performance:**

```csharp
// Obter estatísticas de performance
var detector = detectorFactory.CreateDetector(DetectorType.Hybrid);
var metrics = detector.GetMetrics();

Console.WriteLine($"Frames processados: {metrics.TotalFramesProcessed}");
Console.WriteLine($"Taxa de detecção: {metrics.DetectionRate:P2}");
Console.WriteLine($"Tempo médio: {metrics.AverageProcessingTimeMs:F2}ms");
```

## 🎉 **Benefícios da Solução**

### **1. Configuração Centralizada:**
- ✅ Todas as configurações em um local
- ✅ Fácil de manter e atualizar
- ✅ Suporte a arquivos de configuração
- ✅ Configuração dinâmica

### **2. Descoberta Automática:**
- ✅ Busca automática de templates
- ✅ Múltiplos caminhos de fallback
- ✅ Validação automática
- ✅ Cache de templates

### **3. Injeção de Dependência:**
- ✅ Integração com DI container
- ✅ Factories para detectores
- ✅ Configuração por detector
- ✅ Logging integrado

### **4. Flexibilidade:**
- ✅ Configuração por arquivo
- ✅ Configuração por código
- ✅ Configuração dinâmica
- ✅ Múltiplos detectores

## 🚀 **Exemplo Completo**

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configurar serviços de visão
        builder.Services.AddVisionServices(
            imagesBaseDirectory: "Data/images",
            enableAutoDiscovery: true,
            enableDebug: true
        );
        
        // Configurar detectores específicos
        builder.Services.AddDetectorConfiguration(
            "TemplateBobberDetector",
            "bobber.png",
            0.7,
            true
        );
        
        builder.Services.AddDetectorConfiguration(
            "SignalEnhancedDetector",
            "bobber_in_water.png",
            0.8,
            false
        );
        
        builder.Services.AddDetectorConfiguration(
            "HybridBobberDetector",
            "bobber_in_water.png",
            0.75,
            true
        );
        
        var app = builder.Build();
        
        // Usar serviços
        using var scope = app.Services.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IVisionConfigurationService>();
        var detectorFactory = scope.ServiceProvider.GetRequiredService<IBobberDetectorFactory>();
        
        // Criar detector
        var detector = detectorFactory.CreateDetector(DetectorType.Hybrid);
        
        // Detectar bobber
        var fishingArea = new Rectangle(100, 100, 600, 400);
        var result = detector.DetectInArea(fishingArea);
        
        Console.WriteLine($"Bobber detectado: {result.Detected}");
        Console.WriteLine($"Score: {result.Score:F3}");
        Console.WriteLine($"Posição: ({result.PositionX:F1}, {result.PositionY:F1})");
    }
}
```

A solução está pronta e fornece um sistema de configuração centralizado e flexível para o `AlbionFishing.Vision`! 🎯👁️
