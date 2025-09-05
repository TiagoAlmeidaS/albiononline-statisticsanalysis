using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AlbionFishing.Vision.Configuration;
using AlbionFishing.Vision.Detectors;
using AlbionFishing.Vision.ScreenCapture;

namespace AlbionFishing.Vision.DependencyInjection;

/// <summary>
/// Extensões para injeção de dependência do sistema de visão
/// </summary>
public static class VisionServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona serviços de visão ao container de DI
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração (opcional)</param>
    /// <param name="configureVision">Ação para configurar o sistema de visão (opcional)</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddVisionServices(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<VisionConfiguration>? configureVision = null)
    {
        // Registrar configuração
        services.AddSingleton<VisionConfiguration>(provider =>
        {
            var logger = provider.GetService<ILogger<VisionConfiguration>>();
            var config = new VisionConfiguration(logger, configuration);
            
            // Aplicar configuração customizada se fornecida
            configureVision?.Invoke(config);
            
            return config;
        });
        
        // Registrar serviço de configuração
        services.AddSingleton<IVisionConfigurationService, VisionConfigurationService>();
        
        // Registrar detectores
        services.AddTransient<IBobberDetector, TemplateBobberDetector>();
        services.AddTransient<IBobberDetector, SignalEnhancedDetector>();
        services.AddTransient<IBobberDetector, HybridBobberDetector>();
        
        // Registrar factory para detectores
        services.AddTransient<IBobberDetectorFactory, BobberDetectorFactory>();
        
        // Registrar captura de tela
        services.AddSingleton<IScreenCapture, WindowsScreenCapture>();
        services.AddSingleton<IScreenCapture, LinuxScreenCapture>();
        services.AddSingleton<IScreenCaptureFactory, ScreenCaptureFactory>();
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        
        // Registrar serviços de análise (comentado temporariamente)
        // services.AddTransient<IVisionAnalysisService, VisionAnalysisService>();
        // services.AddTransient<IVisionAnalysisReporter, VisionAnalysisReporter>();
        
        // Registrar serviços de cor (comentado temporariamente)
        // services.AddTransient<ColorFilterService>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona serviços de visão com configuração específica
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="imagesBaseDirectory">Diretório base para imagens</param>
    /// <param name="enableAutoDiscovery">Habilitar descoberta automática</param>
    /// <param name="enableDebug">Habilitar debug</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddVisionServices(
        this IServiceCollection services,
        string imagesBaseDirectory = "Data/images",
        bool enableAutoDiscovery = true,
        bool enableDebug = false)
    {
        return services.AddVisionServices(
            configureVision: config =>
            {
                config.ImagesBaseDirectory = imagesBaseDirectory;
                config.EnableAutoTemplateDiscovery = enableAutoDiscovery;
                config.EnableDetailedLogging = enableDebug;
                config.EnableVisualDebug = enableDebug;
            });
    }
    
    /// <summary>
    /// Adiciona configuração de visão específica para um detector
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="detectorName">Nome do detector</param>
    /// <param name="templatePath">Caminho do template</param>
    /// <param name="confidenceThreshold">Threshold de confiança</param>
    /// <param name="enableDebug">Habilitar debug</param>
    /// <returns>Coleção de serviços</returns>
    public static IServiceCollection AddDetectorConfiguration(
        this IServiceCollection services,
        string detectorName,
        string templatePath,
        double confidenceThreshold = 0.5,
        bool enableDebug = false)
    {
        services.Configure<VisionConfiguration>(config =>
        {
            var detectorConfig = new DetectorSpecificConfig
            {
                TemplatePath = templatePath,
                ConfidenceThreshold = confidenceThreshold,
                EnableDebugWindow = enableDebug
            };
            
            config.SetDetectorConfig(detectorName, detectorConfig);
        });
        
        return services;
    }
}

/// <summary>
/// Factory para criar detectores de bobber
/// </summary>
public interface IBobberDetectorFactory
{
    /// <summary>
    /// Cria um detector específico
    /// </summary>
    /// <param name="detectorType">Tipo do detector</param>
    /// <param name="detectorName">Nome do detector (opcional)</param>
    /// <returns>Detector criado</returns>
    IBobberDetector CreateDetector(DetectorType detectorType, string? detectorName = null);
    
    /// <summary>
    /// Cria um detector com configuração específica
    /// </summary>
    /// <param name="detectorType">Tipo do detector</param>
    /// <param name="config">Configuração do detector</param>
    /// <returns>Detector criado</returns>
    IBobberDetector CreateDetector(DetectorType detectorType, DetectorSpecificConfig config);
}

/// <summary>
/// Implementação da factory de detectores
/// </summary>
public class BobberDetectorFactory : IBobberDetectorFactory
{
    private readonly IVisionConfigurationService _configService;
    private readonly ILogger<BobberDetectorFactory> _logger;
    
    public BobberDetectorFactory(
        IVisionConfigurationService configService,
        ILogger<BobberDetectorFactory> logger)
    {
        _configService = configService;
        _logger = logger;
    }
    
    public IBobberDetector CreateDetector(DetectorType detectorType, string? detectorName = null)
    {
        var config = detectorName != null ? 
            _configService.GetDetectorConfig(detectorName) : 
            new DetectorSpecificConfig();
        
        return CreateDetector(detectorType, config);
    }
    
    public IBobberDetector CreateDetector(DetectorType detectorType, DetectorSpecificConfig config)
    {
        _logger.LogDebug("Criando detector {DetectorType} com configuração", detectorType);
        
        return detectorType switch
        {
            DetectorType.Template => new TemplateBobberDetector(
                config.TemplatePath,
                OpenCvSharp.TemplateMatchModes.CCoeffNormed,
                config.EnableDebugWindow),
            
            DetectorType.SignalEnhanced => new SignalEnhancedDetector(
                config.TemplatePath),
            
            DetectorType.Hybrid => new HybridBobberDetector(
                config.TemplatePath,
                _configService.GetTemplatePath("bobber_in_water.png"),
                templateWeight: 0.6,
                signalWeight: 0.4),
            
            _ => throw new ArgumentException($"Tipo de detector não suportado: {detectorType}")
        };
    }
}

/// <summary>
/// Tipos de detectores disponíveis
/// </summary>
public enum DetectorType
{
    Template,
    SignalEnhanced,
    Hybrid
}

/// <summary>
/// Factory para captura de tela
/// </summary>
public interface IScreenCaptureFactory
{
    /// <summary>
    /// Obtém o melhor provedor de captura disponível
    /// </summary>
    /// <returns>Provedor de captura</returns>
    IScreenCapture GetBestProvider();
    
    /// <summary>
    /// Obtém um provedor específico
    /// </summary>
    /// <param name="providerName">Nome do provedor</param>
    /// <returns>Provedor de captura</returns>
    IScreenCapture GetProvider(string providerName);
    
    /// <summary>
    /// Obtém todos os provedores disponíveis
    /// </summary>
    /// <returns>Lista de provedores</returns>
    IEnumerable<IScreenCapture> GetAllProviders();
}

/// <summary>
/// Implementação da factory de captura de tela
/// </summary>
public class ScreenCaptureFactory : IScreenCaptureFactory
{
    private readonly IEnumerable<IScreenCapture> _providers;
    private readonly ILogger<ScreenCaptureFactory> _logger;
    
    public ScreenCaptureFactory(
        IEnumerable<IScreenCapture> providers,
        ILogger<ScreenCaptureFactory> logger)
    {
        _providers = providers;
        _logger = logger;
    }
    
    public IScreenCapture GetBestProvider()
    {
        var availableProvider = _providers.FirstOrDefault(p => p.IsAvailable);
        
        if (availableProvider == null)
        {
            _logger.LogWarning("Nenhum provedor de captura de tela disponível");
            throw new InvalidOperationException("Nenhum provedor de captura de tela disponível");
        }
        
        _logger.LogDebug("Usando provedor de captura: {ProviderName}", availableProvider.ProviderName);
        return availableProvider;
    }
    
    public IScreenCapture GetProvider(string providerName)
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderName == providerName);
        
        if (provider == null)
        {
            _logger.LogWarning("Provedor de captura não encontrado: {ProviderName}", providerName);
            throw new ArgumentException($"Provedor de captura não encontrado: {providerName}");
        }
        
        return provider;
    }
    
    public IEnumerable<IScreenCapture> GetAllProviders()
    {
        return _providers;
    }
}
