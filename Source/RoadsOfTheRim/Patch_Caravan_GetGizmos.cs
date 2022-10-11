using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(Caravan), "GetGizmos")]
public static class Patch_Caravan_GetGizmos
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<Gizmo> __result, Caravan __instance)
    {
        var isThereAConstructionSiteHere =
            Find.WorldObjects.AnyWorldObjectOfDefAt(DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"),
                __instance.Tile);
        var isTheCaravanWorkingOnASite = true;
        try
        {
            isTheCaravanWorkingOnASite = __instance.GetComponent<WorldObjectComp_Caravan>().currentlyWorkingOnSite;
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog(null, e);
        }

        __result = __result.Concat(new Gizmo[] { RoadsOfTheRim.AddConstructionSite(__instance) })
            .Concat(new Gizmo[] { RoadsOfTheRim.RemoveConstructionSite(__instance.Tile) });
        if (isThereAConstructionSiteHere & !isTheCaravanWorkingOnASite &&
            RoadsOfTheRim.RoadBuildingState.CurrentlyTargeting == null)
        {
            __result = __result.Concat(new Gizmo[] { RoadsOfTheRim.WorkOnSite(__instance) });
        }

        if (isTheCaravanWorkingOnASite)
        {
            __result = __result.Concat(new Gizmo[] { RoadsOfTheRim.StopWorkingOnSite(__instance) });
        }
    }
}