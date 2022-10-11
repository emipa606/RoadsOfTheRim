using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(WorldPathGrid), "CalculatedMovementDifficultyAt")]
internal static class Patch_WorldPathGrid_CalculatedMovementDifficultyAt
{
    [HarmonyPostfix]
    public static void PostFix(ref float __result, int tile, bool perceivedStatic, int? ticksAbs,
        StringBuilder explanation)
    {
        if (__result <= 999f || !Find.WorldGrid.InBounds(tile))
        {
            return;
        }

        try
        {
            var tile2 = Find.WorldGrid[tile];
            if (tile2.Roads == null)
            {
                return;
            }

            RoadDef BestRoad = null;
            foreach (var roadLink in tile2.Roads)
            {
                var currentRoad = roadLink.road;
                if (currentRoad == null)
                {
                    continue;
                }

                if (BestRoad == null)
                {
                    BestRoad = currentRoad;
                    continue;
                }

                if (BestRoad.movementCostMultiplier < currentRoad.movementCostMultiplier)
                {
                    BestRoad = roadLink.road;
                }
            }

            var roadDefExtension = BestRoad?.GetModExtension<DefModExtension_RotR_RoadDef>();
            if (roadDefExtension == null)
            {
                return;
            }

            if ((!tile2.biome.impassable || !(roadDefExtension.biomeModifier > 0)) &&
                tile2.hilliness != Hilliness.Impassable)
            {
                return;
            }

            __result = 12f;
            RoadsOfTheRim.DebugLog(
                $"[RotR] - Impassable Tile {tile} of biome {tile2.biome.label} movement difficulty patched to 12");
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog(
                $"[RotR] - CalculatedMovementDifficultyAt Patch - Catastrophic failure for tile {Find.WorldGrid[tile]}",
                e);
        }
    }
}