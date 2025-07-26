using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt))]
public static class GenConstruct_CanPlaceBlueprintAt
{
    public static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Map map)
    {
        if (entDef != TerrainDefOf.ConcreteBridge || !map.terrainGrid.TerrainAt(center).affordances
                .Contains(TerrainAffordanceDefOf.Bridgeable)) // ConcreteBridge on normal water (bridgeable)
        {
            return;
        }

        __result = AcceptanceReport.WasAccepted;
    }
}