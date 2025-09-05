using System;
using System.Drawing;
using System.Threading.Tasks;
using StatisticsAnalysisTool.DecisionEngine.Models;

namespace StatisticsAnalysisTool.DecisionEngine.Services;

/// <summary>
/// Interface para resolução de minigames de pesca
/// Integra com o sistema de visão existente do AlbionFishing
/// </summary>
public interface IMinigameResolutionService
{
    /// <summary>
    /// Evento disparado quando o minigame inicia
    /// </summary>
    event EventHandler<MinigameStartedEventArgs>? MinigameStarted;
    
    /// <summary>
    /// Evento disparado quando o minigame é concluído
    /// </summary>
    event EventHandler<MinigameCompletedEventArgs>? MinigameCompleted;
    
    /// <summary>
    /// Evento disparado quando uma decisão de mouse é tomada
    /// </summary>
    event EventHandler<MouseDecisionEventArgs>? MouseDecisionMade;
    
    /// <summary>
    /// Indica se o minigame está ativo
    /// </summary>
    bool IsMinigameActive { get; }
    
    /// <summary>
    /// Configuração atual do minigame
    /// </summary>
    MinigameConfiguration Configuration { get; }
    
    /// <summary>
    /// Estatísticas do minigame
    /// </summary>
    MinigameStatistics Statistics { get; }
    
    /// <summary>
    /// Inicia a resolução do minigame
    /// </summary>
    /// <param name="fishingArea">Área de pesca na tela</param>
    /// <param name="configuration">Configuração do minigame</param>
    /// <returns>Task que representa a operação</returns>
    Task StartMinigameAsync(Rectangle fishingArea, MinigameConfiguration? configuration = null);
    
    /// <summary>
    /// Para a resolução do minigame
    /// </summary>
    /// <returns>Task que representa a operação</returns>
    Task StopMinigameAsync();
    
    /// <summary>
    /// Processa um frame do minigame
    /// </summary>
    /// <param name="frame">Frame capturado</param>
    /// <returns>Decisão tomada para o frame</returns>
    Task<MinigameDecision> ProcessFrameAsync(Bitmap frame);
    
    /// <summary>
    /// Determina se deve segurar o mouse baseado na posição do bobber
    /// </summary>
    /// <param name="bobberPosition">Posição do bobber</param>
    /// <param name="fishingAreaWidth">Largura da área de pesca</param>
    /// <returns>Decisão de mouse</returns>
    MouseDecision ShouldHoldMouse(BobberPosition bobberPosition, int fishingAreaWidth);
    
    /// <summary>
    /// Configura o serviço
    /// </summary>
    /// <param name="configuration">Nova configuração</param>
    Task ConfigureAsync(MinigameConfiguration configuration);
    
    /// <summary>
    /// Obtém estatísticas detalhadas
    /// </summary>
    /// <returns>Estatísticas do minigame</returns>
    MinigameStatistics GetDetailedStatistics();
    
    /// <summary>
    /// Reseta as estatísticas
    /// </summary>
    void ResetStatistics();
}

/// <summary>
/// Configuração do minigame
/// </summary>
public class MinigameConfiguration
{
    /// <summary>
    /// Modo de resolução do minigame
    /// </summary>
    public MinigameResolutionMode Mode { get; set; } = MinigameResolutionMode.VisualTracking;
    
    /// <summary>
    /// Threshold de confiança para detecção
    /// </summary>
    public double ConfidenceThreshold { get; set; } = 0.5;
    
    /// <summary>
    /// Sensibilidade de reação (0.0 - 1.0)
    /// </summary>
    public double ReactionSensitivity { get; set; } = 0.8;
    
    /// <summary>
    /// Duração mínima para segurar o mouse (ms)
    /// </summary>
    public int MinMouseHoldDuration { get; set; } = 100;
    
    /// <summary>
    /// Duração máxima para segurar o mouse (ms)
    /// </summary>
    public int MaxMouseHoldDuration { get; set; } = 2000;
    
    /// <summary>
    /// Intervalo entre frames (ms)
    /// </summary>
    public int FrameInterval { get; set; } = 30;
    
    /// <summary>
    /// Duração máxima do minigame (ms)
    /// </summary>
    public int MaxMinigameDuration { get; set; } = 20000;
    
    /// <summary>
    /// Habilita suavização de posição
    /// </summary>
    public bool EnablePositionSmoothing { get; set; } = true;
    
    /// <summary>
    /// Fator de suavização (0.0 - 1.0)
    /// </summary>
    public double SmoothingFactor { get; set; } = 0.3;
    
    /// <summary>
    /// Habilita predição de movimento
    /// </summary>
    public bool EnableMovementPrediction { get; set; } = true;
    
    /// <summary>
    /// Histórico de posições para predição
    /// </summary>
    public int PositionHistorySize { get; set; } = 5;
    
    /// <summary>
    /// Habilita análise de micro-movimentos
    /// </summary>
    public bool EnableMicroMotionAnalysis { get; set; } = true;
    
    /// <summary>
    /// Threshold para micro-movimentos
    /// </summary>
    public double MicroMotionThreshold { get; set; } = 2.0;
    
    /// <summary>
    /// Habilita análise de ondulação
    /// </summary>
    public bool EnableRippleAnalysis { get; set; } = true;
    
    /// <summary>
    /// Threshold para análise de ondulação
    /// </summary>
    public double RippleThreshold { get; set; } = 2.2;
    
    /// <summary>
    /// Habilita modo de debug
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;
    
    /// <summary>
    /// Caminho para salvar imagens de debug
    /// </summary>
    public string? DebugImagePath { get; set; }
    
    /// <summary>
    /// Configurações específicas do detector de visão
    /// </summary>
    public VisionDetectorConfiguration VisionConfiguration { get; set; } = new();
}

/// <summary>
/// Configuração do detector de visão
/// </summary>
public class VisionDetectorConfiguration
{
    /// <summary>
    /// Caminho do template do bobber
    /// </summary>
    public string TemplatePath { get; set; } = "data/images/bobber_in_water.png";
    
    /// <summary>
    /// Método de matching do template
    /// </summary>
    public string MatchMethod { get; set; } = "CCoeffNormed";
    
    /// <summary>
    /// Habilita filtros de cor
    /// </summary>
    public bool EnableColorFilters { get; set; } = true;
    
    /// <summary>
    /// Habilita análise HSV
    /// </summary>
    public bool EnableHsvAnalysis { get; set; } = true;
    
    /// <summary>
    /// Habilita análise de gradiente
    /// </summary>
    public bool EnableGradientAnalysis { get; set; } = true;
    
    /// <summary>
    /// Habilita análise multi-escala
    /// </summary>
    public bool EnableMultiScale { get; set; } = true;
    
    /// <summary>
    /// Escalas para análise multi-escala
    /// </summary>
    public double[] Scales { get; set; } = { 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2 };
    
    /// <summary>
    /// Habilita análise de sinal
    /// </summary>
    public bool EnableSignalAnalysis { get; set; } = true;
    
    /// <summary>
    /// Habilita análise de cinemática
    /// </summary>
    public bool EnableKinematics { get; set; } = true;
    
    /// <summary>
    /// Habilita análise de micro-movimento
    /// </summary>
    public bool EnableMicroMotion { get; set; } = true;
    
    /// <summary>
    /// Habilita heurística de gancho
    /// </summary>
    public bool EnableHookHeuristic { get; set; } = true;
}

/// <summary>
/// Modo de resolução do minigame
/// </summary>
public enum MinigameResolutionMode
{
    /// <summary>
    /// Reação padrão baseada em tempo
    /// </summary>
    StandardReaction,
    
    /// <summary>
    /// Tracking visual reativo
    /// </summary>
    VisualTracking,
    
    /// <summary>
    /// Controle por IA
    /// </summary>
    AIControlled,
    
    /// <summary>
    /// Modo híbrido (visual + IA)
    /// </summary>
    Hybrid
}

/// <summary>
/// Posição do bobber
/// </summary>
public class BobberPosition
{
    public float X { get; set; }
    public float Y { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsValid { get; set; }
    public string? DetectionMethod { get; set; }
}

/// <summary>
/// Decisão do minigame
/// </summary>
public class MinigameDecision
{
    public bool ShouldHoldMouse { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Duration { get; set; }
    public DateTime Timestamp { get; set; }
    public BobberPosition? BobberPosition { get; set; }
    public MinigameAnalysis? Analysis { get; set; }
}

/// <summary>
/// Análise do minigame
/// </summary>
public class MinigameAnalysis
{
    public bool HasMicroMotion { get; set; }
    public bool HasRipple { get; set; }
    public bool HasJerk { get; set; }
    public bool HasFlow { get; set; }
    public bool HasDiff { get; set; }
    public int Votes { get; set; }
    public double MicroMotionScore { get; set; }
    public double RippleScore { get; set; }
    public double JerkScore { get; set; }
    public double FlowScore { get; set; }
    public double DiffScore { get; set; }
    public double OverallScore { get; set; }
    public string? Method { get; set; }
}

/// <summary>
/// Decisão de mouse
/// </summary>
public class MouseDecision
{
    public bool ShouldHold { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Duration { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Estatísticas do minigame
/// </summary>
public class MinigameStatistics
{
    public int TotalMinigames { get; set; }
    public int SuccessfulMinigames { get; set; }
    public int FailedMinigames { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan AverageMinigameDuration { get; set; }
    public TimeSpan TotalMinigameTime { get; set; }
    public int TotalFramesProcessed { get; set; }
    public double AverageFrameProcessingTime { get; set; }
    public int TotalMouseDecisions { get; set; }
    public int CorrectMouseDecisions { get; set; }
    public double MouseDecisionAccuracy { get; set; }
    public double AverageConfidence { get; set; }
    public DateTime LastMinigameTime { get; set; }
    public string? LastError { get; set; }
    public int ErrorCount { get; set; }
}

/// <summary>
/// Argumentos do evento de início do minigame
/// </summary>
public class MinigameStartedEventArgs : EventArgs
{
    public Rectangle FishingArea { get; }
    public MinigameConfiguration Configuration { get; }
    public DateTime StartTime { get; }
    
    public MinigameStartedEventArgs(Rectangle fishingArea, MinigameConfiguration configuration)
    {
        FishingArea = fishingArea;
        Configuration = configuration;
        StartTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos do evento de conclusão do minigame
/// </summary>
public class MinigameCompletedEventArgs : EventArgs
{
    public bool WasSuccessful { get; }
    public TimeSpan Duration { get; }
    public MinigameStatistics Statistics { get; }
    public string? Error { get; }
    
    public MinigameCompletedEventArgs(bool wasSuccessful, TimeSpan duration, MinigameStatistics statistics, string? error = null)
    {
        WasSuccessful = wasSuccessful;
        Duration = duration;
        Statistics = statistics;
        Error = error;
    }
}

/// <summary>
/// Argumentos do evento de decisão de mouse
/// </summary>
public class MouseDecisionEventArgs : EventArgs
{
    public MouseDecision Decision { get; }
    public BobberPosition BobberPosition { get; }
    public MinigameAnalysis Analysis { get; }
    
    public MouseDecisionEventArgs(MouseDecision decision, BobberPosition bobberPosition, MinigameAnalysis analysis)
    {
        Decision = decision;
        BobberPosition = bobberPosition;
        Analysis = analysis;
    }
}
