using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterRimworlds.LuminescentWalls;

public class CompProperties_LuminescentLimestone : CompProperties
{
    public int reverseEngineerPointsMin = 1;
    public int reverseEngineerPointsMax = 8;
    public CompProperties_LuminescentLimestone() => compClass = typeof(CompLuminescentLimestone);
}

public class CompLuminescentLimestone : ThingComp
{
    public const string ResearchDefName = "BR_LuminescentWalls";
    public CompProperties_LuminescentLimestone Props => (CompProperties_LuminescentLimestone)props;

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        if (mode != DestroyMode.Deconstruct) return;
        var project = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(ResearchDefName);
        if (project == null || project.IsFinished || Find.ResearchManager == null) return;
        int min = Mathf.Max(0, Math.Min(Props.reverseEngineerPointsMin, Props.reverseEngineerPointsMax));
        int max = Math.Max(Props.reverseEngineerPointsMin, Props.reverseEngineerPointsMax);
        if (max <= 0) return;
        AddResearch(project, Rand.RangeInclusive(min, max));
    }

    private static void AddResearch(ResearchProjectDef project, float points)
    {
#if RIMWORLD15 || RIMWORLD16
        Find.ResearchManager.AddProgress(project, points);
#else
        var field = typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(Find.ResearchManager) is IDictionary dict)
        {
            float current = dict.Contains(project) ? Convert.ToSingle(dict[project]) : 0f;
            float next = Mathf.Min(current + points, project.baseCost);
            dict[project] = next;
            if (next >= project.baseCost)
                Find.ResearchManager.FinishProject(project, true);
        }
#endif
    }
}
