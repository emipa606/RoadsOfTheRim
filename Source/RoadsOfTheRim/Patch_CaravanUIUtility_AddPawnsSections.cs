using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
/*
* Adds a Road equipment section to pawns & animals
*/
public static class Patch_CaravanUIUtility_AddPawnsSections
{
    [HarmonyPostfix]
    public static void Postfix(ref TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
    {
        RoadsOfTheRim.DebugLog("DEBUG AddPawnsSection: ");
        var source = new List<TransferableOneWay>();
        foreach (var tow in transferables)
        {
            if (!tow.ThingDef.IsWithinCategory(ThingCategoryDef.Named("RoadEquipment")))
            {
                continue;
            }

            source.Add(tow);
            RoadsOfTheRim.DebugLog("Found an ISR2G");
        }

        widget.AddSection("RoadsOfTheRim_RoadEquipment".Translate(), source);
    }
}