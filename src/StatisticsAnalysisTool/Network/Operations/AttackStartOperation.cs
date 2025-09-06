using System.Collections.Generic;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Operations;

public class AttackStartOperation
{
    public AttackStartOperation(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out object targetId))
        {
            TargetId = targetId.ObjectToLong();
        }

        if (parameters.TryGetValue(1, out object attackType))
        {
            AttackType = attackType.ObjectToInt();
        }

        if (parameters.TryGetValue(2, out object weaponType))
        {
            WeaponType = weaponType.ObjectToInt();
        }
    }

    public long? TargetId { get; }
    public int AttackType { get; }
    public int WeaponType { get; }
}
