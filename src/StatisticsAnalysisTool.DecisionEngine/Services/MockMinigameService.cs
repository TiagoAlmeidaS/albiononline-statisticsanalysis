using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Abstractions;

namespace StatisticsAnalysisTool.DecisionEngine.Services
{
    /// <summary>
    /// Implementação mock do serviço de minigame para o DecisionEngine
    /// </summary>
    public class MockMinigameService : IMinigameService
    {
        private readonly ILogger<MockMinigameService> _logger;
        private readonly MinigameStats _stats = new();
        
        public bool IsMinigameActive { get; private set; }
        public MinigameStats CurrentStats => _stats;
        
        public event EventHandler<Abstractions.MinigameStartedEventArgs>? MinigameStarted;
        public event EventHandler<Abstractions.MinigameFinishedEventArgs>? MinigameFinished;
        public event EventHandler<Abstractions.BobberDetectedEventArgs>? BobberDetected;
        public event EventHandler<Abstractions.MouseStateChangedEventArgs>? MouseStateChanged;
        
        public MockMinigameService(ILogger<MockMinigameService> logger)
        {
            _logger = logger;
        }
        
        public async Task StartMinigameAsync(Rectangle region)
        {
            _logger.LogInformation("🎮 Iniciando minigame mock na região {Region}", region);
            
            IsMinigameActive = true;
            _stats.FrameCount = 0;
            _stats.DetectionCount = 0;
            _stats.Duration = TimeSpan.Zero;
            _stats.Success = false;
            
            // Disparar evento de início
            MinigameStarted?.Invoke(this, new Abstractions.MinigameStartedEventArgs(region, new Abstractions.MinigameConfiguration()));
            
            // Simular minigame por 2 segundos
            var startTime = DateTime.UtcNow;
            var random = new Random();
            
            while (IsMinigameActive && DateTime.UtcNow - startTime < TimeSpan.FromSeconds(2))
            {
                _stats.FrameCount++;
                
                // Simular detecção ocasional do bobber
                if (random.NextDouble() < 0.3) // 30% de chance por frame
                {
                    _stats.DetectionCount++;
                    
                    BobberDetected?.Invoke(this, new Abstractions.BobberDetectedEventArgs
                    {
                        Side = random.NextDouble() < 0.5 ? "left" : "right",
                        SideIndicator = random.NextDouble() < 0.5 ? "←" : "→",
                        PositionX = region.X + random.Next(region.Width),
                        PositionY = region.Y + random.Next(region.Height)
                    });
                    
                    // Simular movimento do mouse
                    MouseStateChanged?.Invoke(this, new Abstractions.MouseStateChangedEventArgs
                    {
                        Action = "move",
                        Side = random.NextDouble() < 0.5 ? "left" : "right"
                    });
                }
                
                await Task.Delay(50); // 20 FPS
            }
            
            // Finalizar minigame
            IsMinigameActive = false;
            _stats.Duration = DateTime.UtcNow - startTime;
            _stats.Success = _stats.DetectionCount > 0;
            
            _logger.LogInformation("✅ Minigame mock finalizado - Sucesso: {Success}, Detecções: {DetectionCount}, Taxa: {DetectionRate:F1}%", 
                _stats.Success, _stats.DetectionCount, _stats.DetectionRate);
            
            // Disparar evento de finalização
            MinigameFinished?.Invoke(this, new Abstractions.MinigameFinishedEventArgs
            {
                Success = _stats.Success,
                Duration = _stats.Duration,
                Stats = _stats
            });
        }
        
        public void StopMinigame()
        {
            if (IsMinigameActive)
            {
                _logger.LogInformation("🛑 Parando minigame mock...");
                IsMinigameActive = false;
                
                MinigameFinished?.Invoke(this, new Abstractions.MinigameFinishedEventArgs
                {
                    Success = _stats.Success,
                    Duration = _stats.Duration,
                    Stats = _stats
                });
            }
        }
    }
}
