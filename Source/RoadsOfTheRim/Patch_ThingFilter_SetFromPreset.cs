using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim;

[HarmonyPatch(typeof(ThingFilter), "SetFromPreset")]
//Remove Road equipment from Item tab when forming caravans
public static class Patch_ThingFilter_SetFromPreset
{
    [HarmonyPostfix]
    public static void Postfix(ref ThingFilter __instance, StorageSettingsPreset preset)
    {
        if (preset != StorageSettingsPreset.DefaultStockpile)
        {
            return;
        }

        __instance.SetAllow(ThingCategoryDef.Named("RoadEquipment"), true);
    }
}