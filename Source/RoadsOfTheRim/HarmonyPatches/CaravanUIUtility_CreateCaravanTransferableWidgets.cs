﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(CaravanUIUtility), nameof(CaravanUIUtility.CreateCaravanTransferableWidgets))]
//Remove Road equipment from Item tab when forming caravans
public static class CaravanUIUtility_CreateCaravanTransferableWidgets
{
    public static void Postfix(List<TransferableOneWay> transferables,
        ref TransferableOneWayWidget itemsTransfer, string thingCountTip,
        IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter,
        bool ignoreSpawnedCorpsesGearAndInventoryMass, PlanetTile tile)
    {
        var modifiedTransferables = transferables.Where(x => x.ThingDef.category != ThingCategory.Pawn).ToList();
        modifiedTransferables = modifiedTransferables
            .Where(x => !x.ThingDef.IsWithinCategory(ThingCategoryDef.Named("RoadEquipment"))).ToList();
        itemsTransfer = new TransferableOneWayWidget(modifiedTransferables, null, null, thingCountTip, true,
            ignorePawnInventoryMass, false, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile,
            true, false, false, true, false, true);
    }
}