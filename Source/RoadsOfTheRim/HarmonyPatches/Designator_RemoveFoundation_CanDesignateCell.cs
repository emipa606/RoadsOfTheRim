using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(Designator_RemoveFoundation), nameof(Designator_RemoveFoundation.CanDesignateCell))]
public static class Designator_RemoveFoundation_CanDesignateCell
{
    public static void Postfix(ref AcceptanceReport __result, Designator_RemoveFoundation __instance, IntVec3 c)
    {
        if (!c.InBounds(__instance.Map) || c.GetTerrain(__instance.Map) != TerrainDefOf.ConcreteBridge)
        {
            return;
        }

        __result = true;
        RoadsOfTheRim.DebugLog(c.GetTerrain(__instance.Map).label);
    }
}