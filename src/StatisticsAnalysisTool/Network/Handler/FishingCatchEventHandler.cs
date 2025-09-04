using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCatchEventHandler(TrackingController trackingController) : EventPacketHandler<FishingCatchEvent>((int) EventCodes.NewFloatObject)
{
    protected override async Task OnActionAsync(FishingCatchEvent value)
    {
        try
        {
            Log.Information("FishingCatchEventHandler: Evento recebido - EventId={EventId}, ItemIndex={ItemIndex}, IsSuccessful={IsSuccessful}, Timestamp={Timestamp}", 
                value.EventId, value.ItemIndex, value.IsSuccessful, value.Timestamp);
            
            DebugConsole.WriteInfo(typeof(FishingCatchEventHandler), 
                $"FishingCatchEventHandler: Evento recebido - EventId={value.EventId}, ItemIndex={value.ItemIndex}, IsSuccessful={value.IsSuccessful}, Timestamp={value.Timestamp}", 
                "#5196A6");

            var isTrackingAllowed = trackingController.IsTrackingAllowedByMainCharacter();
            Log.Debug("FishingCatchEventHandler: Tracking permitido: {IsTrackingAllowed}", isTrackingAllowed);
            
            if (isTrackingAllowed)
            {
                Log.Information("FishingCatchEventHandler: Processando evento de pesca no GatheringController");
                DebugConsole.WriteInfo(typeof(FishingCatchEventHandler), 
                    "FishingCatchEventHandler: Processando evento de pesca no GatheringController", 
                    "#5196A6");
                
                trackingController.GatheringController.OnFishingCatch(value);
                
                Log.Information("FishingCatchEventHandler: Evento de pesca processado com sucesso");
                DebugConsole.WriteInfo(typeof(FishingCatchEventHandler), 
                    "FishingCatchEventHandler: Evento de pesca processado com sucesso", 
                    "#5196A6");
            }
            else
            {
                Log.Warning("FishingCatchEventHandler: Tracking não permitido para o personagem principal - evento ignorado");
                DebugConsole.WriteWarn(typeof(FishingCatchEventHandler), 
                    new Exception("Tracking não permitido para o personagem principal - evento ignorado"));
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "FishingCatchEventHandler: Erro ao processar evento de pesca");
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
        
        await Task.CompletedTask;
    }
}
