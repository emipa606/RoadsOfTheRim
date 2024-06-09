using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(WorldTargeter), nameof(WorldTargeter.StopTargeting))]
public static class WorldTargeter_StopTargeting
{
    public static void Prefix()
    {
        if (Current.ProgramState != ProgramState.Playing)
        {
            return;
        }

        if (RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting == null)
        {
            return;
        }

        //RoadsOfTheRim.DebugLog("StopTargeting");
        RoadsOfTheRim.FinaliseConstructionSite(RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting);
        RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting = null;
    }
}