using System;
using System.Collections.Generic;

namespace AlbionFishing.Core;

public class BotConfig
{
    public AudioConfig Audio { get; set; } = new();
    public VisionConfig Vision { get; set; } = new();
    public FishingConfig Fishing { get; set; } = new();
    
    public void ValidateAndFix()
    {
        // Validação básica da configuração
        Audio ??= new AudioConfig();
        Vision ??= new VisionConfig();
        Fishing ??= new FishingConfig();
        
        Audio.ValidateAndFix();
        Vision.ValidateAndFix();
        Fishing.ValidateAndFix();
    }
}

public class AudioConfig
{
    public float Match_Threshold { get; set; } = 0.7f;
    public int Sample_Rate { get; set; } = 44100;
    public int Buffer_Size { get; set; } = 1024;
    public string Template_Path { get; set; } = "data/audio/fishing_sound.wav";
    
    public void ValidateAndFix()
    {
        Match_Threshold = Math.Max(0.1f, Math.Min(1.0f, Match_Threshold));
        Sample_Rate = Math.Max(8000, Math.Min(192000, Sample_Rate));
        Buffer_Size = Math.Max(256, Math.Min(4096, Buffer_Size));
    }
}

public class VisionConfig
{
    public double Confidence_Threshold { get; set; } = 0.5;
    public string Template_Path { get; set; } = "data/images/bobber.png";
    public int[] Bobber_Area { get; set; } = { 0, 0, 100, 100 };
    
    public void ValidateAndFix()
    {
        Confidence_Threshold = Math.Max(0.1, Math.Min(1.0, Confidence_Threshold));
        if (Bobber_Area == null || Bobber_Area.Length != 4)
        {
            Bobber_Area = new int[] { 0, 0, 100, 100 };
        }
    }
    
    public int[] GetBobberAreaArray()
    {
        return Bobber_Area;
    }
}

public class FishingConfig
{
    public List<int> Area { get; set; } = new() { 0, 0, 100, 100 };
    public double Cast_Delay_Min { get; set; } = 0.2;
    public double Cast_Delay_Max { get; set; } = 0.4;
    
    public void ValidateAndFix()
    {
        if (Area == null || Area.Count != 4)
        {
            Area = new List<int> { 0, 0, 100, 100 };
        }
        
        Cast_Delay_Min = Math.Max(0.1, Math.Min(1.0, Cast_Delay_Min));
        Cast_Delay_Max = Math.Max(Cast_Delay_Min, Math.Min(2.0, Cast_Delay_Max));
    }
}
