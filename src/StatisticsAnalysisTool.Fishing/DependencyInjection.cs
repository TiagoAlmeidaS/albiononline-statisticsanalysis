using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Engines;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Services;
using StatisticsAnalysisTool.Fishing;

namespace StatisticsAnalysisTool.Fishing;

/// <summary>
/// Configuração de injeção de dependência para o sistema de pesca
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços de pesca ao container de DI
    /// </summary>
    public static IServiceCollection AddFishingServices(this IServiceCollection services)
    {
        // Configuração
        services.AddSingleton<FishingConfig>();
        
        // Serviços principais
        services.AddSingleton<IFishingZoneService, FishingZoneService>();
        services.AddSingleton<IBobberDetectionService, BobberDetectionService>();
        services.AddSingleton<IMinigameResolutionService, MinigameResolutionService>();
        
        // Adicionar novos serviços de pesca
        services.AddFishingServices();
        
        // Engine principal
        services.AddSingleton<IFishingEngine, FishingEngine>();
        
        // Event Handlers
        services.AddTransient<EventHandlers.NewFishingZoneObjectEventHandler>();
        services.AddTransient<EventHandlers.FishingStartEventHandler>();
        services.AddTransient<EventHandlers.NewFloatObjectEventHandler>();
        
        return services;
    }
    
    /// <summary>
    /// Configura o sistema de pesca com configurações personalizadas
    /// </summary>
    public static IServiceCollection AddFishingServices(this IServiceCollection services, Action<FishingConfig> configure)
    {
        services.Configure<FishingConfig>(configure);
        return services.AddFishingServices();
    }
}
