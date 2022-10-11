using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(GenConstruct), "CanPlaceBlueprintAt")]
public static class Patch_GenConstruct_CanPlaceBlueprintAt
{
    [HarmonyPostfix]
    public static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Rot4 rot,
        Map map, bool godMode = false, Thing thingToIgnore = null, Thing thing = null, ThingDef stuffDef = null)
    {
        if (entDef != TerrainDefOf.ConcreteBridge || !map.terrainGrid.TerrainAt(center).affordances
                .Contains(TerrainAffordanceDefOf.Bridgeable)) // ConcreteBridge on normal water (bridgeable)
        {
            return;
        }

        __result = AcceptanceReport.WasAccepted;
    }
}