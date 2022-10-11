using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(Alert_CaravanIdle), "GetReport")]
public static class Patch_Alert_CaravanIdle_GetReport
{
    [HarmonyPostfix]
    public static void Postfix(ref AlertReport __result)
    {
        var newList = new List<Caravan>();
        foreach (var caravan in Find.WorldObjects.Caravans)
        {
            var caravanComp = caravan.GetComponent<WorldObjectComp_Caravan>();
            if (!caravan.Spawned || !caravan.IsPlayerControlled || caravan.pather.MovingNow || caravan.CantMove ||
                caravanComp.currentlyWorkingOnSite)
            {
                continue;
            }

            newList.Add(caravan);
        }

        __result = AlertReport.CulpritsAre(newList);
    }
}