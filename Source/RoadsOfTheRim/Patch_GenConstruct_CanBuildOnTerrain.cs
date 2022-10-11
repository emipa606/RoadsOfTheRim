using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(GenConstruct), "CanBuildOnTerrain")]
public static class Patch_GenConstruct_CanBuildOnTerrain
{
    [HarmonyPostfix]
    public static void Postfix(ref bool __result, BuildableDef entDef, IntVec3 c, Map map)
    {
        if (entDef != TerrainDefOf.ConcreteBridge && entDef != TerrainDefOf.AsphaltRecent &&
            entDef != TerrainDefOf.GlitterRoad)
        {
            return;
        }

        if (!map.terrainGrid.TerrainAt(c).IsWater)
        {
            return;
        }

        __result = true;
    }
}