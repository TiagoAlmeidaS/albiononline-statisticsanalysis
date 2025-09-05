using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;

namespace StatisticsAnalysisTool.DecisionEngine.Core
{
    /// <summary>
    /// Serviço de inicialização do DecisionEngine
    /// </summary>
    public class DecisionEngineInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DecisionEngineInitializer> _logger;
        
        public DecisionEngineInitializer(IServiceProvider serviceProvider, ILogger<DecisionEngineInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 Inicializando DecisionEngine...");
            
            try
            {
                // Obter o motor de decisão
                var decisionEngine = _serviceProvider.GetRequiredService<IUniversalDecisionEngine>();
                
                // Inicializar o motor
                await decisionEngine.InitializeAsync();
                
                // Registrar comportamentos e regras se o motor for UniversalDecisionEngine
                if (decisionEngine is UniversalDecisionEngine universalEngine)
                {
                    await RegisterBehaviorsAndRules(universalEngine);
                }
                
                _logger.LogInformation("✅ DecisionEngine inicializado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao inicializar DecisionEngine");
                throw;
            }
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Finalizando DecisionEngine...");
            
            try
            {
                var decisionEngine = _serviceProvider.GetRequiredService<IUniversalDecisionEngine>();
                await decisionEngine.ShutdownAsync();
                
                _logger.LogInformation("✅ DecisionEngine finalizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao finalizar DecisionEngine");
            }
        }
        
        private async Task RegisterBehaviorsAndRules(UniversalDecisionEngine engine)
        {
            // Registrar comportamentos
            var behaviors = _serviceProvider.GetServices<Abstractions.IBehavior>();
            foreach (var behavior in behaviors)
            {
                try
                {
                    await behavior.InitializeAsync();
                    engine.RegisterBehavior((Behaviors.BasicMinigameBehavior)behavior);
                    _logger.LogDebug("Comportamento registrado: {BehaviorName}", behavior.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao inicializar comportamento {BehaviorName}", behavior.Name);
                }
            }
            
            // Registrar regras
            var rules = _serviceProvider.GetServices<IDecisionRule>();
            foreach (var rule in rules)
            {
                try
                {
                    engine.RegisterRule(rule);
                    _logger.LogDebug("Regra registrada: {RuleName}", rule.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao registrar regra {RuleName}", rule.Name);
                }
            }
        }
    }
}
