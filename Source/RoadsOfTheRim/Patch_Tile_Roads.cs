using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(Tile), "Roads", MethodType.Getter)]
public static class Patch_Tile_Roads
{
    [HarmonyPostfix]
    public static void Postfix(Tile __instance, ref List<Tile.RoadLink> __result)
    {
        __result = __instance.potentialRoads;
    }
}