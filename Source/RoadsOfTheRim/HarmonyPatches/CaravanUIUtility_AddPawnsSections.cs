﻿using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(CaravanUIUtility), nameof(CaravanUIUtility.AddPawnsSections))]
/*
 * Adds a Road equipment section to pawns & animals
 */
public static class CaravanUIUtility_AddPawnsSections
{
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