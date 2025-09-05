using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Fishing.Services;
using StatisticsAnalysisTool.Fishing.Interfaces;

namespace StatisticsAnalysisTool.Fishing.Extensions
{
    /// <summary>
    /// Extensões para configurar serviços de pesca no container de DI
    /// </summary>
    public static class FishingServiceCollectionExtensions
    {
        /// <summary>
        /// Adiciona serviços de pesca ao container de DI
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <returns>Coleção de serviços para encadeamento</returns>
        public static IServiceCollection AddFishingServices(this IServiceCollection services)
        {
            // Registrar serviços de automação
            services.AddSingleton<Services.IAutomationService, AutomationService>();
            
            // Registrar detector de bobber
            services.AddSingleton<IBobberDetectionService, BobberDetectionService>();
            
            // Registrar serviço de minigame
            services.AddTransient<IMinigameService, MinigameService>();
            
            return services;
        }
    }
}
