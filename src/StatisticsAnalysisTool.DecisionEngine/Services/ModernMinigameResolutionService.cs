using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StatisticsAnalysisTool.DecisionEngine.Models;
using AlbionFishing.Vision; // Reutilizando o sistema de visão existente
using AlbionFishing.Vision.Models;

namespace StatisticsAnalysisTool.DecisionEngine.Services;

/// <summary>
/// Serviço moderno de resolução de minigame que integra com o sistema de visão existente
/// </summary>
public class ModernMinigameResolutionService : IMinigameResolutionService
{
    private readonly ILogger<ModernMinigameResolutionService> _logger;
    private readonly IBobberDetector _bobberDetector;
    private readonly IScreenCapture _screenCapture;
    
    private MinigameConfiguration _configuration;
    private bool _isMinigameActive;
    private Rectangle _fishingArea;
    private DateTime _minigameStartTime;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _minigameTask;
    
    private readonly List<BobberPosition> _positionHistory = new();
    private readonly List<MinigameDecision> _decisionHistory = new();
    private readonly MinigameStatistics _statistics = new();
    
    public event EventHandler<MinigameStartedEventArgs>? MinigameStarted;
    public event EventHandler<MinigameCompletedEventArgs>? MinigameCompleted;
    public event EventHandler<MouseDecisionEventArgs>? MouseDecisionMade;
    
    public bool IsMinigameActive => _isMinigameActive;
    public MinigameConfiguration Configuration => _configuration;
    public MinigameStatistics Statistics => _statistics;
    
    public ModernMinigameResolutionService(
        ILogger<ModernMinigameResolutionService> logger,
        IBobberDetector bobberDetector,
        IScreenCapture screenCapture)
    {
        _logger = logger;
        _bobberDetector = bobberDetector;
        _screenCapture = screenCapture;
        _configuration = new MinigameConfiguration();
    }
    
    public async Task StartMinigameAsync(Rectangle fishingArea, MinigameConfiguration? configuration = null)
    {
        if (_isMinigameActive)
        {
            _logger.LogWarning("Minigame já está ativo, parando o anterior");
            await StopMinigameAsync();
        }
        
        _configuration = configuration ?? new MinigameConfiguration();
        _fishingArea = fishingArea;
        _isMinigameActive = true;
        _minigameStartTime = DateTime.UtcNow;
        _cancellationTokenSource = new CancellationTokenSource();
        
        _logger.LogInformation("Iniciando minigame na área {FishingArea} com modo {Mode}", 
            fishingArea, _configuration.Mode);
        
        // Configurar detector de visão
        ConfigureVisionDetector();
        
        // Disparar evento de início
        MinigameStarted?.Invoke(this, new MinigameStartedEventArgs(fishingArea, _configuration));
        
        // Iniciar processamento do minigame
        _minigameTask = ProcessMinigameAsync(_cancellationTokenSource.Token);
        
        await Task.CompletedTask;
    }
    
    public async Task StopMinigameAsync()
    {
        if (!_isMinigameActive)
            return;
        
        _logger.LogInformation("Parando minigame");
        
        _isMinigameActive = false;
        _cancellationTokenSource?.Cancel();
        
        if (_minigameTask != null)
        {
            try
            {
                await _minigameTask;
            }
            catch (OperationCanceledException)
            {
                // Esperado quando cancelamos
            }
        }
        
        var duration = DateTime.UtcNow - _minigameStartTime;
        var wasSuccessful = _statistics.SuccessfulMinigames > 0;
        
        _logger.LogInformation("Minigame finalizado. Sucesso: {Success}, Duração: {Duration}", 
            wasSuccessful, duration);
        
        // Disparar evento de conclusão
        MinigameCompleted?.Invoke(this, new MinigameCompletedEventArgs(wasSuccessful, duration, _statistics));
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _minigameTask = null;
    }
    
    public async Task<MinigameDecision> ProcessFrameAsync(Bitmap frame)
    {
        if (!_isMinigameActive)
        {
            return new MinigameDecision
            {
                ShouldHoldMouse = false,
                Reason = "Minigame não está ativo",
                Confidence = 0,
                Timestamp = DateTime.UtcNow
            };
        }
        
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Detectar bobber usando o sistema de visão existente
            var detectionResult = _bobberDetector.DetectInArea(_fishingArea, _configuration.ConfidenceThreshold);
            
            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _statistics.TotalFramesProcessed++;
            _statistics.AverageFrameProcessingTime = 
                (_statistics.AverageFrameProcessingTime * (_statistics.TotalFramesProcessed - 1) + processingTime) / 
                _statistics.TotalFramesProcessed;
            
            if (!detectionResult.Detected)
            {
                return new MinigameDecision
                {
                    ShouldHoldMouse = false,
                    Reason = "Bobber não detectado",
                    Confidence = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
            
            // Criar posição do bobber
            var bobberPosition = new BobberPosition
            {
                X = detectionResult.PositionX,
                Y = detectionResult.PositionY,
                Confidence = detectionResult.Score,
                Timestamp = DateTime.UtcNow,
                IsValid = true,
                DetectionMethod = "TemplateMatching"
            };
            
            // Adicionar ao histórico
            _positionHistory.Add(bobberPosition);
            if (_positionHistory.Count > _configuration.PositionHistorySize)
            {
                _positionHistory.RemoveAt(0);
            }
            
            // Analisar o minigame
            var analysis = await AnalyzeMinigameAsync(bobberPosition);
            
            // Tomar decisão
            var decision = await MakeDecisionAsync(bobberPosition, analysis);
            
            // Adicionar ao histórico de decisões
            _decisionHistory.Add(decision);
            if (_decisionHistory.Count > 100) // Manter apenas as últimas 100
            {
                _decisionHistory.RemoveAt(0);
            }
            
            // Atualizar estatísticas
            _statistics.TotalMouseDecisions++;
            if (decision.ShouldHoldMouse)
            {
                _statistics.CorrectMouseDecisions++;
            }
            
            _statistics.MouseDecisionAccuracy = 
                _statistics.TotalMouseDecisions > 0 ? 
                (double)_statistics.CorrectMouseDecisions / _statistics.TotalMouseDecisions : 0;
            
            _statistics.AverageConfidence = 
                (_statistics.AverageConfidence * (_statistics.TotalMouseDecisions - 1) + decision.Confidence) / 
                _statistics.TotalMouseDecisions;
            
            // Disparar evento de decisão
            MouseDecisionMade?.Invoke(this, new MouseDecisionEventArgs(
                new MouseDecision
                {
                    ShouldHold = decision.ShouldHoldMouse,
                    Reason = decision.Reason,
                    Confidence = decision.Confidence,
                    Duration = decision.Duration,
                    Timestamp = decision.Timestamp
                },
                bobberPosition,
                analysis
            ));
            
            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar frame do minigame");
            _statistics.ErrorCount++;
            _statistics.LastError = ex.Message;
            
            return new MinigameDecision
            {
                ShouldHoldMouse = false,
                Reason = $"Erro: {ex.Message}",
                Confidence = 0,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    public MouseDecision ShouldHoldMouse(BobberPosition bobberPosition, int fishingAreaWidth)
    {
        if (!bobberPosition.IsValid)
        {
            return new MouseDecision
            {
                ShouldHold = false,
                Reason = "Posição do bobber inválida",
                Confidence = 0,
                Timestamp = DateTime.UtcNow
            };
        }
        
        // Calcular threshold baseado na sensibilidade
        var threshold = fishingAreaWidth / 2.0 * _configuration.ReactionSensitivity;
        
        var shouldHold = bobberPosition.X < threshold;
        var reason = shouldHold ? "Bobber na esquerda" : "Bobber na direita";
        var confidence = Math.Abs(bobberPosition.X - threshold) / threshold;
        
        return new MouseDecision
        {
            ShouldHold = shouldHold,
            Reason = reason,
            Confidence = Math.Min(1.0, confidence),
            Duration = shouldHold ? _configuration.MinMouseHoldDuration : 0,
            Timestamp = DateTime.UtcNow
        };
    }
    
    public async Task ConfigureAsync(MinigameConfiguration configuration)
    {
        _configuration = configuration;
        ConfigureVisionDetector();
        
        _logger.LogInformation("Configuração do minigame atualizada: {Mode}", configuration.Mode);
        await Task.CompletedTask;
    }
    
    public MinigameStatistics GetDetailedStatistics()
    {
        return new MinigameStatistics
        {
            TotalMinigames = _statistics.TotalMinigames,
            SuccessfulMinigames = _statistics.SuccessfulMinigames,
            FailedMinigames = _statistics.FailedMinigames,
            SuccessRate = _statistics.SuccessRate,
            AverageMinigameDuration = _statistics.AverageMinigameDuration,
            TotalMinigameTime = _statistics.TotalMinigameTime,
            TotalFramesProcessed = _statistics.TotalFramesProcessed,
            AverageFrameProcessingTime = _statistics.AverageFrameProcessingTime,
            TotalMouseDecisions = _statistics.TotalMouseDecisions,
            CorrectMouseDecisions = _statistics.CorrectMouseDecisions,
            MouseDecisionAccuracy = _statistics.MouseDecisionAccuracy,
            AverageConfidence = _statistics.AverageConfidence,
            LastMinigameTime = _statistics.LastMinigameTime,
            LastError = _statistics.LastError,
            ErrorCount = _statistics.ErrorCount
        };
    }
    
    public void ResetStatistics()
    {
        _statistics.TotalMinigames = 0;
        _statistics.SuccessfulMinigames = 0;
        _statistics.FailedMinigames = 0;
        _statistics.SuccessRate = 0;
        _statistics.AverageMinigameDuration = TimeSpan.Zero;
        _statistics.TotalMinigameTime = TimeSpan.Zero;
        _statistics.TotalFramesProcessed = 0;
        _statistics.AverageFrameProcessingTime = 0;
        _statistics.TotalMouseDecisions = 0;
        _statistics.CorrectMouseDecisions = 0;
        _statistics.MouseDecisionAccuracy = 0;
        _statistics.AverageConfidence = 0;
        _statistics.LastMinigameTime = DateTime.MinValue;
        _statistics.LastError = null;
        _statistics.ErrorCount = 0;
        
        _positionHistory.Clear();
        _decisionHistory.Clear();
        
        _logger.LogInformation("Estatísticas do minigame resetadas");
    }
    
    #region Métodos Privados
    
    private void ConfigureVisionDetector()
    {
        // Configurar o detector de visão baseado na configuração
        _bobberDetector.SetTemplatePath(_configuration.VisionConfiguration.TemplatePath);
        
        _logger.LogDebug("Detector de visão configurado: {TemplatePath}", 
            _configuration.VisionConfiguration.TemplatePath);
    }
    
    private async Task ProcessMinigameAsync(CancellationToken cancellationToken)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            _statistics.TotalMinigames++;
            _statistics.LastMinigameTime = DateTime.UtcNow;
            
            while (_isMinigameActive && !cancellationToken.IsCancellationRequested)
            {
                // Verificar timeout
                if ((DateTime.UtcNow - startTime).TotalMilliseconds > _configuration.MaxMinigameDuration)
                {
                    _logger.LogWarning("Minigame atingiu duração máxima: {MaxDuration}ms", 
                        _configuration.MaxMinigameDuration);
                    break;
                }
                
                // Capturar frame
                var frame = _screenCapture.CaptureRegion(_fishingArea);
                if (frame == null)
                {
                    _logger.LogWarning("Falha ao capturar frame");
                    await Task.Delay(_configuration.FrameInterval, cancellationToken);
                    continue;
                }
                
                // Processar frame
                var decision = await ProcessFrameAsync(frame);
                
                // Executar decisão se necessário
                if (decision.ShouldHoldMouse)
                {
                    await ExecuteMouseActionAsync(decision);
                }
                
                // Aguardar próximo frame
                await Task.Delay(_configuration.FrameInterval, cancellationToken);
            }
            
            // Finalizar minigame
            var duration = DateTime.UtcNow - startTime;
            _statistics.AverageMinigameDuration = 
                (_statistics.AverageMinigameDuration * (_statistics.TotalMinigames - 1) + duration) / 
                _statistics.TotalMinigames;
            _statistics.TotalMinigameTime += duration;
            
            // Determinar sucesso baseado em critérios
            var wasSuccessful = DetermineMinigameSuccess();
            if (wasSuccessful)
            {
                _statistics.SuccessfulMinigames++;
            }
            else
            {
                _statistics.FailedMinigames++;
            }
            
            _statistics.SuccessRate = _statistics.TotalMinigames > 0 ? 
                (double)_statistics.SuccessfulMinigames / _statistics.TotalMinigames : 0;
            
            _logger.LogInformation("Minigame processado. Sucesso: {Success}, Duração: {Duration}", 
                wasSuccessful, duration);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Minigame cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante processamento do minigame");
            _statistics.ErrorCount++;
            _statistics.LastError = ex.Message;
        }
    }
    
    private async Task<MinigameAnalysis> AnalyzeMinigameAsync(BobberPosition bobberPosition)
    {
        var analysis = new MinigameAnalysis
        {
            Method = "VisualTracking"
        };
        
        // Análise de micro-movimento
        if (_configuration.EnableMicroMotionAnalysis && _positionHistory.Count > 1)
        {
            analysis.HasMicroMotion = AnalyzeMicroMotion(bobberPosition);
            analysis.MicroMotionScore = CalculateMicroMotionScore(bobberPosition);
        }
        
        // Análise de ondulação
        if (_configuration.EnableRippleAnalysis)
        {
            analysis.HasRipple = AnalyzeRipple(bobberPosition);
            analysis.RippleScore = CalculateRippleScore(bobberPosition);
        }
        
        // Análise de movimento brusco
        analysis.HasJerk = AnalyzeJerk(bobberPosition);
        analysis.JerkScore = CalculateJerkScore(bobberPosition);
        
        // Análise de fluxo
        analysis.HasFlow = AnalyzeFlow(bobberPosition);
        analysis.FlowScore = CalculateFlowScore(bobberPosition);
        
        // Análise de diferença
        analysis.HasDiff = AnalyzeDiff(bobberPosition);
        analysis.DiffScore = CalculateDiffScore(bobberPosition);
        
        // Calcular votos
        analysis.Votes = CalculateVotes(analysis);
        
        // Calcular score geral
        analysis.OverallScore = CalculateOverallScore(analysis);
        
        return await Task.FromResult(analysis);
    }
    
    private async Task<MinigameDecision> MakeDecisionAsync(BobberPosition bobberPosition, MinigameAnalysis analysis)
    {
        var decision = new MinigameDecision
        {
            BobberPosition = bobberPosition,
            Analysis = analysis,
            Timestamp = DateTime.UtcNow
        };
        
        switch (_configuration.Mode)
        {
            case MinigameResolutionMode.StandardReaction:
                decision = MakeStandardReactionDecision(bobberPosition);
                break;
                
            case MinigameResolutionMode.VisualTracking:
                decision = MakeVisualTrackingDecision(bobberPosition, analysis);
                break;
                
            case MinigameResolutionMode.AIControlled:
                decision = MakeAIControlledDecision(bobberPosition, analysis);
                break;
                
            case MinigameResolutionMode.Hybrid:
                decision = MakeHybridDecision(bobberPosition, analysis);
                break;
        }
        
        return await Task.FromResult(decision);
    }
    
    private MinigameDecision MakeStandardReactionDecision(BobberPosition bobberPosition)
    {
        var threshold = _fishingArea.Width / 2.0 * _configuration.ReactionSensitivity;
        var shouldHold = bobberPosition.X < threshold;
        
        return new MinigameDecision
        {
            ShouldHoldMouse = shouldHold,
            Reason = shouldHold ? "Bobber na esquerda (reação padrão)" : "Bobber na direita (reação padrão)",
            Confidence = bobberPosition.Confidence,
            Duration = shouldHold ? _configuration.MinMouseHoldDuration : 0,
            BobberPosition = bobberPosition
        };
    }
    
    private MinigameDecision MakeVisualTrackingDecision(BobberPosition bobberPosition, MinigameAnalysis analysis)
    {
        var threshold = _fishingArea.Width / 2.0 * _configuration.ReactionSensitivity;
        var shouldHold = bobberPosition.X < threshold;
        
        // Ajustar baseado na análise
        if (analysis.HasMicroMotion && analysis.MicroMotionScore > _configuration.MicroMotionThreshold)
        {
            shouldHold = true; // Micro-movimento indica peixe
        }
        
        if (analysis.HasRipple && analysis.RippleScore > _configuration.RippleThreshold)
        {
            shouldHold = true; // Ondulação indica peixe
        }
        
        var confidence = Math.Max(bobberPosition.Confidence, analysis.OverallScore);
        
        return new MinigameDecision
        {
            ShouldHoldMouse = shouldHold,
            Reason = $"Tracking visual: {analysis.Method} (Votos: {analysis.Votes})",
            Confidence = confidence,
            Duration = shouldHold ? _configuration.MinMouseHoldDuration : 0,
            BobberPosition = bobberPosition,
            Analysis = analysis
        };
    }
    
    private MinigameDecision MakeAIControlledDecision(BobberPosition bobberPosition, MinigameAnalysis analysis)
    {
        // Implementação futura para IA
        return new MinigameDecision
        {
            ShouldHoldMouse = false,
            Reason = "IA não implementada ainda",
            Confidence = 0,
            BobberPosition = bobberPosition,
            Analysis = analysis
        };
    }
    
    private MinigameDecision MakeHybridDecision(BobberPosition bobberPosition, MinigameAnalysis analysis)
    {
        // Combinação de visual tracking + IA (quando disponível)
        var visualDecision = MakeVisualTrackingDecision(bobberPosition, analysis);
        
        // Por enquanto, usar apenas visual tracking
        return visualDecision;
    }
    
    private async Task ExecuteMouseActionAsync(MinigameDecision decision)
    {
        // Implementar ação do mouse
        // Por enquanto, apenas log
        _logger.LogDebug("Executando ação do mouse: {Action} por {Duration}ms", 
            decision.ShouldHoldMouse ? "Segurar" : "Soltar", decision.Duration);
        
        await Task.Delay(decision.Duration);
    }
    
    private bool DetermineMinigameSuccess()
    {
        // Critérios para determinar sucesso do minigame
        var recentDecisions = _decisionHistory.TakeLast(10);
        var successfulDecisions = recentDecisions.Count(d => d.ShouldHoldMouse && d.Confidence > 0.7);
        
        return successfulDecisions >= 3; // Pelo menos 3 decisões bem-sucedidas
    }
    
    // Métodos de análise (implementações simplificadas)
    private bool AnalyzeMicroMotion(BobberPosition bobberPosition) => false;
    private double CalculateMicroMotionScore(BobberPosition bobberPosition) => 0.0;
    private bool AnalyzeRipple(BobberPosition bobberPosition) => false;
    private double CalculateRippleScore(BobberPosition bobberPosition) => 0.0;
    private bool AnalyzeJerk(BobberPosition bobberPosition) => false;
    private double CalculateJerkScore(BobberPosition bobberPosition) => 0.0;
    private bool AnalyzeFlow(BobberPosition bobberPosition) => false;
    private double CalculateFlowScore(BobberPosition bobberPosition) => 0.0;
    private bool AnalyzeDiff(BobberPosition bobberPosition) => false;
    private double CalculateDiffScore(BobberPosition bobberPosition) => 0.0;
    private int CalculateVotes(MinigameAnalysis analysis) => 0;
    private double CalculateOverallScore(MinigameAnalysis analysis) => 0.0;
    
    #endregion
}
