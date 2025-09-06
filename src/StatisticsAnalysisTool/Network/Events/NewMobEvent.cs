﻿using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class NewMobEvent
{
    public NewMobEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.NewMob, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object mobIndex))
            {
                MobIndex = mobIndex.ObjectToInt();
            }

            if (parameters.TryGetValue(11, out object moveSpeed))
            {
                MoveSpeed = moveSpeed.ObjectToDouble();
            }

            if (parameters.TryGetValue(13, out object hitPoints))
            {
                HitPoints = hitPoints.ObjectToDouble();
            }

            if (parameters.TryGetValue(14, out object hitPointsMax))
            {
                HitPointsMax = hitPointsMax.ObjectToDouble();
            }

            if (parameters.TryGetValue(17, out object energy))
            {
                Energy = energy.ObjectToDouble();
            }

            if (parameters.TryGetValue(18, out object energyMax))
            {
                EnergyMax = energyMax.ObjectToDouble();
            }

            if (parameters.TryGetValue(19, out object energyRegeneration))
            {
                EnergyRegeneration = energyRegeneration.ObjectToDouble();
            }

            // Capturar posição atual (chave 7) - array de float[2]
            if (parameters.TryGetValue(7, out object position) && position is float[] positionArray && positionArray.Length >= 2)
            {
                Position = new float[] { positionArray[0], positionArray[1] };
            }

            // Capturar nova posição (chave 8) - array de float[2]  
            if (parameters.TryGetValue(8, out object newPosition) && newPosition is float[] newPositionArray && newPositionArray.Length >= 2)
            {
                NewPosition = new float[] { newPositionArray[0], newPositionArray[1] };
            }

            // Capturar dados adicionais se existirem
            if (parameters.TryGetValue(9, out object additionalData))
            {
                AdditionalData = additionalData.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public long? ObjectId { get; }
    public float[] Position { get; private set; } = Array.Empty<float>();
    public float[] NewPosition { get; private set; } = Array.Empty<float>();
    public int AdditionalData { get; }
    public int MobIndex { get; }
    public double MoveSpeed { get; }
    public double HitPoints { get; }
    public double HitPointsMax { get; }
    public double Energy { get; }
    public double EnergyMax { get; }
    public double EnergyRegeneration { get; }
}