using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class FishingCastingRequestHandler : RequestPacketHandler<FishingCastingRequest>
{
    private readonly TrackingController _trackingController;

    public FishingCastingRequestHandler(TrackingController trackingController) : base((int) OperationCodes.FishingCasting)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(FishingCastingRequest value)
    {
        // Log da operação de casting de pesca
        System.Diagnostics.Debug.WriteLine($"FishingCastingRequest: EventId={value.EventId}, ItemIndex={value.ItemIndex}");
        
        // Aqui você pode adicionar lógica específica para o casting de pesca
        // Por exemplo, notificar o GatheringController sobre o início do casting
        
        await Task.CompletedTask;
    }
}
