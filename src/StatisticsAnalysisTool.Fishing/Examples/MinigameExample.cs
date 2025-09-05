using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.Fishing.Services;
using StatisticsAnalysisTool.Fishing.Extensions;

namespace StatisticsAnalysisTool.Fishing.Examples
{
    /// <summary>
    /// Exemplo de uso do serviço de minigame
    /// </summary>
    public class MinigameExample
    {
        private readonly IMinigameService _minigameService;
        private readonly ILogger<MinigameExample> _logger;
        
        public MinigameExample(IMinigameService minigameService, ILogger<MinigameExample> logger)
        {
            _minigameService = minigameService;
            _logger = logger;
            
            // Configurar eventos
            _minigameService.MinigameStarted += OnMinigameStarted;
            _minigameService.MinigameFinished += OnMinigameFinished;
            _minigameService.BobberDetected += OnBobberDetected;
            _minigameService.MouseStateChanged += OnMouseStateChanged;
        }
        
        /// <summary>
        /// Executa um exemplo de minigame
        /// </summary>
        public async Task RunExampleAsync()
        {
            _logger.LogInformation("🎣 Iniciando exemplo de minigame de pesca...");
            
            // Definir região da tela para capturar (exemplo: centro da tela)
            var region = new Rectangle(400, 300, 400, 200);
            
            try
            {
                // Iniciar minigame
                await _minigameService.StartMinigameAsync(region);
                
                // Aguardar um pouco para demonstração
                await Task.Delay(5000);
                
                // Parar minigame
                _minigameService.StopMinigame();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o exemplo de minigame");
            }
        }
        
        private void OnMinigameStarted(object sender, MinigameStartedEventArgs e)
        {
            _logger.LogInformation("🎮 Minigame iniciado em {StartTime} para região {Region}", 
                e.StartTime, e.Region);
        }
        
        private void OnMinigameFinished(object sender, MinigameFinishedEventArgs e)
        {
            _logger.LogInformation("✅ Minigame finalizado - Sucesso: {Success}, Duração: {Duration:F1}ms", 
                e.Success, e.Duration.TotalMilliseconds);
            _logger.LogInformation("📊 Estatísticas finais - Frames: {FrameCount}, Detecções: {DetectionCount}, Taxa: {DetectionRate:F1}%", 
                e.FinalStats.FrameCount, e.FinalStats.DetectionCount, e.FinalStats.DetectionRate);
        }
        
        private void OnBobberDetected(object sender, BobberDetectedEventArgs e)
        {
            _logger.LogDebug("🧭 Bobber detectado - Lado: {Side} {Indicator}, PosX: {PosX:F1}, Distância: {Distance:F1}px", 
                e.Side.ToUpper(), e.SideIndicator, e.PositionX, e.DistanceFromCenter);
        }
        
        private void OnMouseStateChanged(object sender, MouseStateChangedEventArgs e)
        {
            _logger.LogDebug("🖱️ Mouse {Action} - Lado: {Side}, PosX: {PosX:F1}", 
                e.Action, e.Side, e.PositionX);
        }
        
        /// <summary>
        /// Configura o container de DI para o exemplo
        /// </summary>
        public static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Adicionar logging
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            // Adicionar serviços de pesca
            services.AddFishingServices();
            
            // Adicionar serviços de visão (se necessário)
            // services.AddVisionServices();
            
            return services;
        }
    }
}
