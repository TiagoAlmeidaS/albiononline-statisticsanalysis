using System;
using System.Drawing;

namespace AlbionFishing.Vision.Reporting;

/// <summary>
/// Reporter de visão para o detector Bobber-In-Water.
/// Responsável por registrar métricas frame a frame e um resumo da sessão.
/// </summary>
public interface IBobberVisionReporter
{
    void StartSession(string? sessionName = null, string? templatePath = null, Rectangle? detectionRegion = null, double? confidenceThreshold = null);
    void LogFrame(BobberVisionFrame frame);
    void EndSession();
}

/// <summary>
/// DTO com os campos de log por frame para o Bobber-In-Water.
/// </summary>
public class BobberVisionFrame
{
    public DateTime TimestampUtc { get; set; }
    public bool Detected { get; set; }
    public double Score { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public int PatchW { get; set; }
    public int PatchH { get; set; }
        // Bounding box (top-left) do match dentro da ROI, quando disponível
        public int? BBoxX { get; set; }
        public int? BBoxY { get; set; }
        // Tamanho do template utilizado (após possíveis resizes)
        public int? TemplateW { get; set; }
        public int? TemplateH { get; set; }
        public double? PatchStdDev { get; set; }
        public bool? TemplateOnly { get; set; }

    // Dinâmica / micro-movimento
    public double ZDy { get; set; }
    public double ZRipple { get; set; }
    public double DyPixels { get; set; }
    public double RippleEnergy { get; set; }
    public bool MicroMove { get; set; }
        public bool HookDetected { get; set; }

    // Splash/Jerk sinais
    public bool Jerk { get; set; }
    public bool Flow { get; set; }
    public bool Diff { get; set; }
    public int Votes { get; set; }

    // Opcionais/diagnóstico
    public double? ScaleUsed { get; set; }
    public string? TemplateId { get; set; }
    public bool? ColorGateOk { get; set; }
    public double? MatchV1 { get; set; } // score de intensidade (CCorrNormed com máscara)
    public double? MatchV2 { get; set; } // score de gradiente (Canny + CCoeffNormed)
    public double? MaskOnRatio { get; set; } // fração de pixels ligados na máscara (após resize)
        public string? Method { get; set; } // descrição do método de matching
        public bool? UsedMask { get; set; } // se máscara do template foi aplicada
    public string? Reason { get; set; } // motivo/diagnóstico quando não detecta ou em erros
}


