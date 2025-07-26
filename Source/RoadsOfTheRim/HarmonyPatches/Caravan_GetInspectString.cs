using System;
using System.Text;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[HarmonyPatch(typeof(Caravan), nameof(Caravan.GetInspectString))]
public static class Caravan_GetInspectString
{
    public static void Postfix(ref string __result, Caravan __instance)
    {
        try
        {
            var CaravanComp = __instance.GetComponent<WorldObjectComp_Caravan>();
            if (CaravanComp is not { currentlyWorkingOnSite: true })
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(__result);
            // remove "waiting"
            var waitingIndex = stringBuilder.ToString().IndexOf("CaravanWaiting".Translate());
            if (waitingIndex >= 0)
            {
                stringBuilder.Remove(waitingIndex, "CaravanWaiting".Translate().Length);
            }

            // remove "resting (using x bedrolls)"
            var usedBedCount = __instance.beds.GetUsedBedCount();
            var stringToFind =
                // remove singular version
                usedBedCount == 1
                    ? $" ({(string)"UsingBedroll".Translate()})"
                    :
                    // remove plural version
                    $" ({(string)"UsingBedrolls".Translate(usedBedCount)})";

            var bedrollIndex = stringBuilder.ToString().IndexOf(stringToFind, StringComparison.Ordinal);

            if (bedrollIndex >= 0)
            {
                stringBuilder.Remove(bedrollIndex, stringToFind.Length);
                var restingIndex = stringBuilder.ToString().IndexOf("CaravanResting".Translate());
                if (restingIndex >= 0)
                {
                    stringBuilder.Remove(restingIndex, "CaravanResting".Translate().Length);
                }
            }

            // Appending "working on road"
            stringBuilder.Replace("\n", "");
            stringBuilder.Replace("\r", "");
            stringBuilder.AppendLine();
            stringBuilder.Append("RoadsOfTheRim_CaravanInspectStringWorkingOn".Translate(
                CaravanComp.GetSite().FullName(), $"{CaravanComp.AmountOfWork():0.00}"));
            __result = stringBuilder.ToString();
        }
        catch
        {
            // lazy way out : the caravan can, on occasions (mainly debug teleport, though...), not have a site linked to the comp
        }
    }
}