using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(RoadDefGenStep_Place), "Place")]
public static class Patch_RoadDefGenStep_Place_Place
{
    public static bool IsGoodTerrain(TerrainDef terrain)
    {
        return terrain == TerrainDefOf.Mud || terrain == TerrainDefOf.MarshyTerrain;
    }

    [HarmonyPostfix]
    public static void Postfix(ref RoadDefGenStep_Place __instance, Map map, IntVec3 position)
    {
        if (__instance.place == TerrainDefOf.ConcreteBridge && position.GetTerrain(map).IsWater)
        {
            map.terrainGrid.SetTerrain(position, TerrainDefOf.ConcreteBridge);
        }

        if (__instance.place == TerrainDefOf.GlitterRoad &&
            (IsGoodTerrain(position.GetTerrain(map)) || position.GetTerrain(map).IsWater))
        {
            map.terrainGrid.SetTerrain(position, TerrainDefOf.GlitterRoad);
        }

        if (__instance.place == TerrainDefOf.AsphaltRecent && IsGoodTerrain(position.GetTerrain(map)))
        {
            map.terrainGrid.SetTerrain(position, TerrainDefOf.AsphaltRecent);
        }

        if (__instance.place == TerrainDefOf.StoneRecent && IsGoodTerrain(position.GetTerrain(map)))
        {
            map.terrainGrid.SetTerrain(position, TerrainDefOf.StoneRecent);
        }
    }
}