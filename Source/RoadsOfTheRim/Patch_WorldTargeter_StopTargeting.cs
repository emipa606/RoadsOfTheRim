using HarmonyLib;
using RimWorld;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(WorldTargeter), "StopTargeting")]
public static class Patch_WorldTargeter_StopTargeting
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        if (RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting == null)
        {
            return;
        }

        //RoadsOfTheRim.DebugLog("StopTargeting");
        RoadsOfTheRim.FinaliseConstructionSite(RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting);
        RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting = null;
    }
}