using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingGiveLineRequestHandler : RequestPacketHandler<FishingGiveLineRequest>
{
    private readonly TrackingController _trackingController;

    public FishingGiveLineRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingGiveLine)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingGiveLineRequest value)
    {
        // Log da operação de give line de pesca
        System.Diagnostics.Debug.WriteLine($"FishingGiveLineRequest: EventId={value.EventId}, ItemIndex={value.ItemIndex}");
        
        // Aqui você pode adicionar lógica específica para o give line de pesca
        // Por exemplo, notificar o GatheringController sobre o give line
        
        await Task.CompletedTask;
    }
}
