using System;
using System.Drawing;
using System.Threading.Tasks;
using AlbionFishing.Vision.Analysis.Models;

namespace AlbionFishing.Vision.Analysis;

/// <summary>
/// Interface para o serviço de análise visual
/// </summary>
public interface IVisionAnalysisService
{
    /// <summary>
    /// Inicia uma nova sessão de análise visual
    /// </summary>
    VisionAnalysisSession StartAnalysisSession(string sessionName, string detectorType, double confidenceThreshold);

    /// <summary>
    /// Processa um frame de análise visual de forma assíncrona
    /// </summary>
    Task<VisionAnalysisFrame> ProcessVisionFrameAsync(Bitmap capturedImage, Rectangle detectionRegion);

    /// <summary>
    /// Finaliza a sessão de análise atual
    /// </summary>
    void EndAnalysisSession();

    /// <summary>
    /// Obtém a sessão de análise atual
    /// </summary>
    VisionAnalysisSession? GetCurrentSession();

    /// <summary>
    /// Evento disparado quando um frame é processado
    /// </summary>
    event EventHandler<VisionAnalysisFrame>? FrameProcessed;

    /// <summary>
    /// Evento disparado quando uma detecção é encontrada
    /// </summary>
    event EventHandler<VisionAnalysisFrame>? DetectionFound;

    /// <summary>
    /// Evento disparado quando a sessão é finalizada
    /// </summary>
    event EventHandler<VisionAnalysisSession>? SessionEnded;
} 