using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(Tile), nameof(Tile.Roads), MethodType.Getter)]
public static class Tile_Roads
{
    public static void Postfix(Tile __instance, ref List<Tile.RoadLink> __result)
    {
        __result = __instance.potentialRoads;
    }
}