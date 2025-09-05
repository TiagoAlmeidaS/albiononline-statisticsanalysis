using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Core;
using StatisticsAnalysisTool.DecisionEngine.Behaviors;
using StatisticsAnalysisTool.DecisionEngine.Rules;
using StatisticsAnalysisTool.DecisionEngine.Services;
// using StatisticsAnalysisTool.DecisionEngine.AI.Implementations;

namespace StatisticsAnalysisTool.DecisionEngine;

/// <summary>
/// Configuração de injeção de dependência para o Motor de Decisão Universal
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona todos os serviços do Motor de Decisão Universal
    /// </summary>
    public static IServiceCollection AddDecisionEngineServices(
        this IServiceCollection services, 
        Action<DecisionEngineConfig>? configure = null)
    {
        var config = new DecisionEngineConfig();
        configure?.Invoke(config);
        
        // Registrar configuração
        services.AddSingleton(config);
        
        // Registrar motor principal
        services.AddSingleton<Abstractions.IUniversalDecisionEngine, Core.UniversalDecisionEngine>();
        
        // Registrar comportamentos padrão (se existirem)
        // services.AddSingleton<FishingBehavior>();
        // services.AddSingleton<GatheringBehavior>();
        // services.AddSingleton<CombatBehavior>();
        // services.AddSingleton<MovementBehavior>();
        // services.AddSingleton<TradingBehavior>();
        // services.AddSingleton<InventoryBehavior>();
        
        // Registrar serviços mock
        services.AddSingleton<Abstractions.IMinigameService, Services.MockMinigameService>();
        
        // Registrar comportamentos específicos de pesca
        services.AddSingleton<BasicMinigameBehavior>();
        
        // Registrar regras de decisão
        services.AddSingleton<MinigameDecisionRule>();
        
        // Registrar serviços de contexto
        services.AddSingleton<GameContext>();
        
        // Registrar inicializador
        services.AddHostedService<DecisionEngineInitializer>();
        
        // Registrar serviços de IA (desabilitado por enquanto)
        // if (config.EnableAI)
        // {
        //     services.AddSingleton<IAIBridge, OpenAIBridge>();
        // }
        
        // Registrar serviços de contexto (desabilitados por enquanto)
        // services.AddSingleton<IContextAnalyzer, ContextAnalyzer>();
        // services.AddSingleton<IContextManager, ContextManager>();
        
        // Registrar serviços de execução (desabilitados por enquanto)
        // services.AddSingleton<IActionExecutor, ActionExecutor>();
        // services.AddSingleton<IBehaviorOrchestrator, BehaviorOrchestrator>();
        
        // Registrar serviços de plugins (desabilitados por enquanto)
        // services.AddSingleton<IPluginManager, PluginManager>();
        // services.AddSingleton<IPluginLoader, PluginLoader>();
        
        // Registrar serviços de monitoramento (desabilitados por enquanto)
        // services.AddSingleton<IDecisionEngineMonitor, DecisionEngineMonitor>();
        // services.AddSingleton<IPerformanceTracker, PerformanceTracker>();
        
        return services;
    }
    
    /// <summary>
    /// Adiciona apenas o motor de decisão básico
    /// </summary>
    public static IServiceCollection AddBasicDecisionEngine(this IServiceCollection services)
    {
        services.AddSingleton<Abstractions.IUniversalDecisionEngine, Core.UniversalDecisionEngine>();
        return services;
    }
    
    /// <summary>
    /// Adiciona comportamentos específicos
    /// </summary>
    public static IServiceCollection AddBehaviors<T>(this IServiceCollection services) where T : class, Abstractions.IBehavior
    {
        services.AddSingleton<T>();
        return services;
    }
    
            /// <summary>
        /// Adiciona integração com IA (desabilitado por enquanto)
        /// </summary>
        // public static IServiceCollection AddAIIntegration<T>(this IServiceCollection services) where T : class, IAIBridge
        // {
        //     services.AddSingleton<IAIBridge, T>();
        //     return services;
        // }
    
    /// <summary>
    /// Adiciona sistema de plugins
    /// </summary>
    // public static IServiceCollection AddPluginSystem(this IServiceCollection services)
    // {
    //     services.AddSingleton<IPluginManager, PluginManager>();
    //     services.AddSingleton<IPluginLoader, PluginLoader>();
    //     return services;
    // }
}

/// <summary>
/// Configuração do Motor de Decisão Universal
/// </summary>
public class DecisionEngineConfig
{
    /// <summary>
    /// Habilita integração com IA
    /// </summary>
    public bool EnableAI { get; set; } = false;
    
    /// <summary>
    /// Provedor de IA a ser usado
    /// </summary>
    public string AIProvider { get; set; } = "OpenAI";
    
    /// <summary>
    /// Modelo de IA a ser usado
    /// </summary>
    public string AIModel { get; set; } = "gpt-4";
    
    /// <summary>
    /// Número máximo de decisões concorrentes
    /// </summary>
    public int MaxConcurrentDecisions { get; set; } = 5;
    
    /// <summary>
    /// Timeout para decisões
    /// </summary>
    public TimeSpan DecisionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Habilita sistema de plugins
    /// </summary>
    public bool EnablePlugins { get; set; } = true;
    
    /// <summary>
    /// Habilita hot reload de plugins
    /// </summary>
    public bool EnableHotReload { get; set; } = false;
    
    /// <summary>
    /// Habilita monitoramento de performance
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;
    
    /// <summary>
    /// Habilita logging detalhado
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
    
    /// <summary>
    /// Intervalo de limpeza de estatísticas
    /// </summary>
    public TimeSpan StatsCleanupInterval { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Configurações específicas de comportamentos
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> BehaviorConfigs { get; set; } = new();
    
    /// <summary>
    /// Configurações de IA
    /// </summary>
    public Dictionary<string, object> AIConfig { get; set; } = new();
    
    /// <summary>
    /// Configurações de plugins
    /// </summary>
    public Dictionary<string, object> PluginConfig { get; set; } = new();
}
