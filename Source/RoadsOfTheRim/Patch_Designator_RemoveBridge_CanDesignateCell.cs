using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(Designator_RemoveBridge), "CanDesignateCell")]
public static class Patch_Designator_RemoveBridge_CanDesignateCell
{
    [HarmonyPostfix]
    public static void Postfix(ref AcceptanceReport __result, Designator_RemoveBridge __instance, IntVec3 c)
    {
        if (!c.InBounds(__instance.Map) || c.GetTerrain(__instance.Map) != TerrainDefOf.ConcreteBridge)
        {
            return;
        }

        __result = true;
        RoadsOfTheRim.DebugLog(c.GetTerrain(__instance.Map).label);
    }
}