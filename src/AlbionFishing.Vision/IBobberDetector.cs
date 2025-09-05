using System.Drawing;
using AlbionFishing.Vision.Models;

namespace AlbionFishing.Vision;

/// <summary>
/// Interface unificada para diferentes implementações de detecção de bobber
/// Permite intercambiar implementações (template/híbrido/ONNX) sem alterar o Engine
/// </summary>
public interface IBobberDetector
{
    /// <summary>
    /// Detecta o bobber em uma área específica da tela
    /// </summary>
    /// <param name="fishingArea">Área de detecção</param>
    /// <param name="confidenceThreshold">Threshold de confiança (0.0-1.0)</param>
    /// <returns>Resultado simples da detecção</returns>
    BobberInWaterDetectionResult DetectInArea(Rectangle fishingArea, double confidenceThreshold = 0.5);

    /// <summary>
    /// Versão verbosa da detecção com informações detalhadas
    /// </summary>
    /// <param name="fishingArea">Área de detecção</param>
    /// <param name="confidenceThreshold">Threshold de confiança (0.0-1.0)</param>
    /// <returns>Resultado detalhado da detecção</returns>
    BobberInWaterDetectionVerboseResult DetectInAreaVerbose(Rectangle fishingArea, double confidenceThreshold = 0.5);

    /// <summary>
    /// Configura o template a ser usado para detecção
    /// </summary>
    /// <param name="templatePath">Caminho para o arquivo do template</param>
    void SetTemplatePath(string templatePath);

    /// <summary>
    /// Nome identificador do detector
    /// </summary>
    string DetectorName { get; }

    /// <summary>
    /// Versão do detector
    /// </summary>
    string Version { get; }
}
