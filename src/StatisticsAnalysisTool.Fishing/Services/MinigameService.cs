using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AlbionFishing.Vision.ScreenCapture;
using StatisticsAnalysisTool.Fishing.Interfaces;
using StatisticsAnalysisTool.Fishing.Utils;

namespace StatisticsAnalysisTool.Fishing.Services
{
    /// <summary>
    /// Implementação do serviço de minigame de pesca
    /// </summary>
    public class MinigameService : IMinigameService
    {
        private readonly ILogger<MinigameService> _logger;
        private readonly IScreenCaptureService _screenCaptureService;
        private readonly IBobberDetector _bobberDetector;
        private readonly IAutomationService _automationService;
        
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isMinigameActive;
        private bool _isMouseDown;
        private MinigameStats _currentStats;
        
        // Constantes de configuração
        private const int TRACKING_LOOP_INTERVAL_MS = 50; // 50ms = ~20 FPS
        private const int MAX_TRACKING_DURATION_MS = 10000; // 10 segundos máximo
        private const int LOG_STATS_INTERVAL_FRAMES = 20; // Log a cada 20 frames (1 segundo)
        
        public event EventHandler<MinigameStartedEventArgs> MinigameStarted;
        public event EventHandler<MinigameFinishedEventArgs> MinigameFinished;
        public event EventHandler<BobberDetectedEventArgs> BobberDetected;
        public event EventHandler<MouseStateChangedEventArgs> MouseStateChanged;
        
        public bool IsMinigameActive => _isMinigameActive;
        public MinigameStats CurrentStats => _currentStats;
        
        public MinigameService(
            ILogger<MinigameService> logger,
            IScreenCaptureService screenCaptureService,
            IBobberDetector bobberDetector,
            IAutomationService automationService)
        {
            _logger = logger;
            _screenCaptureService = screenCaptureService;
            _bobberDetector = bobberDetector;
            _automationService = automationService;
            
            _currentStats = new MinigameStats();
        }
        
        public async Task StartMinigameAsync(Rectangle region)
        {
            if (_isMinigameActive)
            {
                _logger.LogWarning("Minigame já está ativo, parando o anterior...");
                StopMinigame();
            }
            
            _logger.LogInformation("🎮 Iniciando minigame de pesca...");
            _logger.LogInformation("⏱️ Loop interval: {Interval}ms, Max duration: {MaxDuration}ms", 
                TRACKING_LOOP_INTERVAL_MS, MAX_TRACKING_DURATION_MS);
            
            _isMinigameActive = true;
            _isMouseDown = false;
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Inicializar estatísticas
            _currentStats = new MinigameStats
            {
                StartTime = DateTime.Now,
                FrameCount = 0,
                DetectionCount = 0,
                MouseDownCount = 0,
                MouseUpCount = 0
            };
            
            // Disparar evento de início
            MinigameStarted?.Invoke(this, new MinigameStartedEventArgs
            {
                StartTime = _currentStats.StartTime,
                Region = region
            });
            
            try
            {
                await TrackAndSolveMinigameAsync(region, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Minigame cancelado pelo usuário");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o minigame");
            }
            finally
            {
                FinishMinigame(false);
            }
        }
        
        public void StopMinigame()
        {
            if (!_isMinigameActive) return;
            
            _logger.LogInformation("🛑 Parando minigame...");
            _cancellationTokenSource?.Cancel();
        }
        
        private async Task TrackAndSolveMinigameAsync(Rectangle region, CancellationToken cancellationToken)
        {
            
            while (_isMinigameActive && !cancellationToken.IsCancellationRequested)
            {
                _currentStats.FrameCount++;
                var elapsedMs = (DateTime.Now - _currentStats.StartTime).TotalMilliseconds;
                
                // Verificar timeout
                if (elapsedMs > MAX_TRACKING_DURATION_MS)
                {
                    _logger.LogWarning("⏰ Timeout do minigame atingido - {ElapsedMs:F0}ms > {MaxDuration}ms", 
                        elapsedMs, MAX_TRACKING_DURATION_MS);
                    break;
                }
                
                try
                {
                    // Capturar tela
                    using var screenshot = _screenCaptureService.CaptureScreen();
                    if (screenshot == null)
                    {
                        _logger.LogWarning("Falha ao capturar tela, tentando novamente...");
                        await Task.Delay(TRACKING_LOOP_INTERVAL_MS, cancellationToken);
                        continue;
                    }
                    
                    // Detectar bobber
                    var detectionResult = _bobberDetector.DetectBobber(screenshot);
                    
                    if (detectionResult.IsDetected)
                    {
                        _currentStats.DetectionCount++;
                        ProcessBobberDetection(detectionResult.Position.X, detectionResult.Size.Width);
                    }
                    else
                    {
                        // Bobber não detectado - soltar mouse se estiver pressionado
                        if (_isMouseDown)
                        {
                            ReleaseMouse("Bobber não detectado");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante detecção do bobber");
                }
                
                // Log periódico de estatísticas
                if (_currentStats.FrameCount % LOG_STATS_INTERVAL_FRAMES == 0)
                {
                    LogStats();
                }
                
                // Aguardar próximo frame
                await Task.Delay(TRACKING_LOOP_INTERVAL_MS, cancellationToken);
            }
        }
        
        private void ProcessBobberDetection(float posX, float width)
        {
            var middle = width / 2.0f;
            var shouldHoldMouse = VisionUtils.ShouldHoldMouse(posX, width);
            var lado = VisionUtils.GetBobberLado(posX, width);
            var ladoIndicator = VisionUtils.GetBobberSideIndicator(posX, width);
            var distanceFromCenter = VisionUtils.GetDistanceFromCenter(posX, width);
            
            // Disparar evento de detecção do bobber
            BobberDetected?.Invoke(this, new BobberDetectedEventArgs
            {
                PositionX = posX,
                Width = width,
                Middle = middle,
                Side = lado,
                SideIndicator = ladoIndicator,
                DistanceFromCenter = distanceFromCenter,
                ShouldHoldMouse = shouldHoldMouse,
                Timestamp = DateTime.Now
            });
            
            _logger.LogDebug("🧭 Bobber detectado no lado {Lado} {Indicator} - PosX: {PosX:F1}, Distância do centro: {Distance:F1}px", 
                lado.ToUpper(), ladoIndicator, posX, distanceFromCenter);
            
            // Controlar mouse baseado na posição
            if (shouldHoldMouse && !_isMouseDown)
            {
                PressMouse(posX, lado);
            }
            else if (!shouldHoldMouse && _isMouseDown)
            {
                ReleaseMouse($"PosX: {posX:F1} >= {middle:F1}");
            }
        }
        
        private void PressMouse(float posX, string lado)
        {
            _automationService.MouseDown("left");
            _isMouseDown = true;
            _currentStats.MouseDownCount++;
            
            _logger.LogDebug("🎯 MouseDown (PosX: {PosX:F1}) - Lado: {Lado}", posX, lado.ToUpper());
            
            MouseStateChanged?.Invoke(this, new MouseStateChangedEventArgs
            {
                IsMouseDown = true,
                Action = "MouseDown",
                PositionX = posX,
                Side = lado,
                Timestamp = DateTime.Now
            });
        }
        
        private void ReleaseMouse(string reason)
        {
            _automationService.MouseUp("left");
            _isMouseDown = false;
            _currentStats.MouseUpCount++;
            
            _logger.LogDebug("🎯 MouseUp - {Reason}", reason);
            
            MouseStateChanged?.Invoke(this, new MouseStateChangedEventArgs
            {
                IsMouseDown = false,
                Action = "MouseUp",
                Timestamp = DateTime.Now
            });
        }
        
        private void LogStats()
        {
            var detectionRate = _currentStats.DetectionRate;
            var elapsedMs = (DateTime.Now - _currentStats.StartTime).TotalMilliseconds;
            
            _logger.LogInformation("📊 Tracking stats - Frames: {FrameCount}, Detections: {DetectionCount}, Rate: {DetectionRate:F1}%, Elapsed: {ElapsedMs:F0}ms", 
                _currentStats.FrameCount, _currentStats.DetectionCount, detectionRate, elapsedMs);
        }
        
        private void FinishMinigame(bool success)
        {
            if (!_isMinigameActive) return;
            
            _isMinigameActive = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            // Garantir que o mouse seja solto
            if (_isMouseDown)
            {
                _automationService.MouseUp("left");
                _isMouseDown = false;
            }
            
            // Finalizar estatísticas
            _currentStats.EndTime = DateTime.Now;
            _currentStats.Duration = _currentStats.EndTime.Value - _currentStats.StartTime;
            
            var finalDetectionRate = _currentStats.DetectionRate;
            _logger.LogInformation("📊 Tracking finalizado - Total frames: {FrameCount}, Detections: {DetectionCount}, Rate: {DetectionRate:F1}%, Duration: {Duration:F0}ms", 
                _currentStats.FrameCount, _currentStats.DetectionCount, finalDetectionRate, _currentStats.Duration.TotalMilliseconds);
            
            _logger.LogInformation("✅ Minigame finalizado - Sucesso: {Success}", success);
            
            // Disparar evento de finalização
            MinigameFinished?.Invoke(this, new MinigameFinishedEventArgs
            {
                EndTime = _currentStats.EndTime.Value,
                Duration = _currentStats.Duration,
                FinalStats = _currentStats,
                Success = success
            });
        }
    }
}
