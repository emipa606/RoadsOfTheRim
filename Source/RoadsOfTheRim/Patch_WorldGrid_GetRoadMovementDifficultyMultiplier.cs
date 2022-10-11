using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(WorldGrid), "GetRoadMovementDifficultyMultiplier")]
public static class Patch_WorldGrid_GetRoadMovementDifficultyMultiplier
{
    private static readonly MethodInfo HillinessMovementDifficultyOffset = AccessTools.Method(typeof(WorldPathGrid),
        "HillinessMovementDifficultyOffset", new[] { typeof(Hilliness) });

    [HarmonyPostfix]
    public static void Postifx(ref float __result, WorldGrid __instance, ref int fromTile, ref int toTile,
        ref StringBuilder explanation)
    {
        var roads = __instance.tiles[fromTile].Roads;
        if (roads == null)
        {
            return;
        }

        if (toTile == -1)
        {
            toTile = __instance.FindMostReasonableAdjacentTileForDisplayedPathCost(fromTile);
        }

        for (var i = 0; i < roads.Count; i++)
        {
            if (roads[i].neighbor != toTile)
            {
                continue;
            }

            var ToTileAsTile = Find.WorldGrid[toTile];
            var HillinessOffset =
                (float)HillinessMovementDifficultyOffset.Invoke(null, new object[] { ToTileAsTile.hilliness });
            if (HillinessOffset > 12f)
            {
                HillinessOffset = 12f;
            }

            // If the tile has an impassable biome, set the biomemovement difficulty to 12, as per the patch for CalculatedMovementDifficultyAt
            var biomeMovementDifficulty =
                ToTileAsTile.biome.impassable ? 12f : ToTileAsTile.biome.movementDifficulty;

            // Calculate biome, Hillines & winter modifiers, update explanation &  multiply result by biome modifier
            var RoadModifier = RoadsOfTheRim.CalculateRoadModifier(
                roads[i].road,
                biomeMovementDifficulty,
                HillinessOffset,
                WorldPathGrid.GetCurrentWinterMovementDifficultyOffset(toTile),
                out var BiomeModifier,
                out var HillModifier,
                out var WinterModifier
            );
            var resultBefore = __result;
            __result *= RoadModifier;
            if (explanation == null)
            {
                return;
            }

            explanation.AppendLine();
            explanation.Append(string.Format(
                "The road cancels {0:P0} of the biome ({3:##.###}), {1:P0} of the hills ({4:##.###}) & {2:P0} of winter movement costs. Total modifier={5} applied to {6}",
                BiomeModifier, HillModifier, WinterModifier,
                biomeMovementDifficulty, HillinessOffset, RoadModifier, resultBefore
            ));

            return;
        }
    }
}