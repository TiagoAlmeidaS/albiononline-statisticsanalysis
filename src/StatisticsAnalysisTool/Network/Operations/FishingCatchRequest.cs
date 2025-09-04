using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Operations;

public class FishingCatchRequest
{
    public readonly long EventId;
    public readonly int ItemIndex;

    public FishingCatchRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object eventId))
            {
                EventId = eventId.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(2, out object itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}
