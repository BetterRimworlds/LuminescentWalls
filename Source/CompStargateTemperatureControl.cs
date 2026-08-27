using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterRimworlds.LuminescentWalls;

public sealed class CompProperties_StargateTemperatureControl : CompProperties
{
    public CompProperties_StargateTemperatureControl() => compClass = typeof(CompStargateTemperatureControl);
}

public sealed class CompStargateTemperatureControl : ThingComp
{
    private const float ColdThresholdCelsius = 10f;
    private const float HotThresholdCelsius = 25.444f;
    private const float TargetTemperatureCelsius = 21f;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Log.Warning($"[Luminescent Walls] Temperature control component spawned on {parent.def.defName} at {parent.Position}; loadingSave={respawningAfterLoad}.");
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        if (parent.MapHeld == null)
            return;

        bool logDiagnostics = parent.IsHashIntervalTick(1000);
        CompPowerTrader power = parent.TryGetComp<CompPowerTrader>();
        if (power != null && !power.PowerOn)
        {
            if (logDiagnostics)
                Log.Warning($"[Luminescent Walls] Temperature control skipped on {parent.def.defName} at {parent.Position}: unpowered.");
            return;
        }

        if (logDiagnostics)
            Log.Warning($"[Luminescent Walls] Temperature control rare tick on {parent.def.defName} at {parent.Position}; power={(power == null ? "no CompPowerTrader" : "on")}.");

        var rooms = new HashSet<Room>();
        foreach (IntVec3 offset in GenAdj.CardinalDirections)
        {
            Room room = (parent.Position + offset).GetRoom(parent.Map);
            if (room == null)
            {
                if (logDiagnostics)
                    Log.Warning($"[Luminescent Walls] Temperature control found no room {offset} of {parent.Position}.");
                continue;
            }

            if (room.UsesOutdoorTemperature)
            {
                if (logDiagnostics)
                    Log.Warning($"[Luminescent Walls] Temperature control skipped outdoor room {offset} of {parent.Position}.");
                continue;
            }

            if (!rooms.Add(room))
                continue;

            float temperature = room.Temperature;
            if (temperature >= ColdThresholdCelsius && temperature <= HotThresholdCelsius)
            {
                if (logDiagnostics)
                    Log.Warning($"[Luminescent Walls] Temperature control observed adjacent room at {temperature:F2}C; within {ColdThresholdCelsius:F2}C-{HotThresholdCelsius:F2}C range, no adjustment.");
                continue;
            }

            float newTemperature =
                System.Math.Abs(TargetTemperatureCelsius - temperature) < 0.05f
                    ? TargetTemperatureCelsius
                    : temperature + (TargetTemperatureCelsius - temperature) * 0.25f;

#if RIMWORLD12
            if (room.Group?.TempTracker == null)
            {
                if (logDiagnostics)
                    Log.Warning("[Luminescent Walls] Temperature control could not adjust room: no group temperature tracker.");
                continue;
            }
            room.Group.TempTracker.Temperature = newTemperature;
#else
            if (room.TempTracker == null)
            {
                if (logDiagnostics)
                    Log.Warning("[Luminescent Walls] Temperature control could not adjust room: no temperature tracker.");
                continue;
            }
            room.TempTracker.Temperature = newTemperature;
#endif

            if (logDiagnostics)
                Log.Warning($"[Luminescent Walls] Temperature control adjusted adjacent room from {temperature:F2}C to {newTemperature:F2}C.");
        }
    }
}
