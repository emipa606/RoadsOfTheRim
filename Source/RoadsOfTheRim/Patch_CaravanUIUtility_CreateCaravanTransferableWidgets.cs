using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(CaravanUIUtility), "CreateCaravanTransferableWidgets")]
//Remove Road equipment from Item tab when forming caravans
public static class Patch_CaravanUIUtility_CreateCaravanTransferableWidgets
{
    [HarmonyPostfix]
    public static void Postfix(List<TransferableOneWay> transferables, ref TransferableOneWayWidget pawnsTransfer,
        ref TransferableOneWayWidget itemsTransfer, string thingCountTip,
        IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter,
        bool ignoreSpawnedCorpsesGearAndInventoryMass, int tile, bool playerPawnsReadOnly)
    {
        var modifiedTransferables = transferables.Where(x => x.ThingDef.category != ThingCategory.Pawn).ToList();
        modifiedTransferables = modifiedTransferables
            .Where(x => !x.ThingDef.IsWithinCategory(ThingCategoryDef.Named("RoadEquipment"))).ToList();
        itemsTransfer = new TransferableOneWayWidget(modifiedTransferables, null, null, thingCountTip, true,
            ignorePawnInventoryMass, false, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile,
            true, false, false, true, false, true);
    }
}