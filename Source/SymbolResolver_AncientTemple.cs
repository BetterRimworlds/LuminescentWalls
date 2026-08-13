using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace BetterRimworlds.LuminescentWalls;

/// <summary>Converts walls created by the Ancient Danger resolver into research samples.</summary>
public class SymbolResolver_AncientTemple : RimWorld.BaseGen.SymbolResolver_AncientTemple
{
    public override void Resolve(ResolveParams rp)
    {
        Map map = BaseGen.globalSettings?.map;
        var preExistingWalls = new HashSet<Thing>();
        if (map != null)
        {
            foreach (IntVec3 cell in rp.rect)
            {
                foreach (Thing thing in map.thingGrid.ThingsAt(cell))
                {
                    if (thing.def == ThingDefOf.Wall)
                        preExistingWalls.Add(thing);
                }
            }
        }

        base.Resolve(rp);

        if (map == null) return;
        var wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("BR_IlluminescentLimestoneWall");
        if (wallDef == null) return;

        foreach (IntVec3 cell in rp.rect)
        {
            // Copy the list because replacing a thing mutates the cell's thing list.
            var things = new List<Thing>(map.thingGrid.ThingsAt(cell));
            foreach (Thing wall in things)
            {
                if (wall.def != ThingDefOf.Wall || preExistingWalls.Contains(wall)) continue;

                IntVec3 position = wall.Position;
                Rot4 rotation = wall.Rotation;
                Faction faction = wall.Faction;
                var replacement = ThingMaker.MakeThing(wallDef);
                replacement.SetFaction(faction);
                wall.Destroy(DestroyMode.Vanish);
                GenSpawn.Spawn(replacement, position, map, rotation, WipeMode.Vanish);
            }
        }
    }
}
