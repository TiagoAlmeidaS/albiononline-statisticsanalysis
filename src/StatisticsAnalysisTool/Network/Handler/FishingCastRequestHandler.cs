using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCastRequestHandler : RequestPacketHandler<FishingCastRequest>
{
    private readonly TrackingController _trackingController;

    public FishingCastRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingCast)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingCastRequest value)
    {
        // Log da operação de cast de pesca
        System.Diagnostics.Debug.WriteLine($"FishingCastRequest: EventId={value.EventId}, ItemIndex={value.ItemIndex}");
        
        // Aqui você pode adicionar lógica específica para o cast de pesca
        // Por exemplo, notificar o GatheringController sobre o cast
        
        await Task.CompletedTask;
    }
}
