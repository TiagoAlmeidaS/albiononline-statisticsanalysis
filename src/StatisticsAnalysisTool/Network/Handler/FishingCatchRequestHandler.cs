using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCatchRequestHandler : RequestPacketHandler<FishingCatchRequest>
{
    private readonly TrackingController _trackingController;

    public FishingCatchRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingCatch)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingCatchRequest value)
    {
        // Log da operação de catch de pesca
        System.Diagnostics.Debug.WriteLine($"FishingCatchRequest: EventId={value.EventId}, ItemIndex={value.ItemIndex}");
        
        // Aqui você pode adicionar lógica específica para o catch de pesca
        // Por exemplo, notificar o GatheringController sobre a tentativa de catch
        
        await Task.CompletedTask;
    }
}
