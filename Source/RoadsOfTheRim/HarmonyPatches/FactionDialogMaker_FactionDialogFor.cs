﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(FactionDialogMaker), nameof(FactionDialogMaker.FactionDialogFor))]
public static class FactionDialogMaker_FactionDialogFor
{
    [HarmonyPostfix]
    public static void Postfix(ref DiaNode __result, Pawn negotiator, Faction faction)
    {
        // Allies can help build roads
        if (faction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return;
        }

        __result.options.Insert(0, RoadsOfTheRim.HelpRoadConstruction(faction, negotiator));
    }
}