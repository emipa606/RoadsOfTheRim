using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(WorldPathGrid), nameof(WorldPathGrid.CalculatedMovementDifficultyAt))]
internal static class WorldPathGrid_CalculatedMovementDifficultyAt
{
    public static void Postfix(ref float __result, PlanetTile tile)
    {
        if (__result <= 999f || !Find.WorldGrid.InBounds(tile))
        {
            return;
        }

        try
        {
            var tile2 = (SurfaceTile)Find.WorldGrid[tile];
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

            if ((!tile2.PrimaryBiome.impassable || !(roadDefExtension.biomeModifier > 0)) &&
                tile2.hilliness != Hilliness.Impassable)
            {
                return;
            }

            __result = 12f;
            RoadsOfTheRim.DebugLog(
                $"[RotR] - Impassable Tile {tile} of biome {tile2.PrimaryBiome.label} movement difficulty patched to 12");
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog(
                $"[RotR] - CalculatedMovementDifficultyAt Patch - Catastrophic failure for tile {Find.WorldGrid[tile]}",
                e);
        }
    }
}