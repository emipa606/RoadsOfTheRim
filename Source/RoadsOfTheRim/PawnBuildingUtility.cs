using RimWorld;
using Verse;

namespace RoadsOfTheRim;

public static class PawnBuildingUtility
{
    public static bool HealthyColonist(Pawn p)
    {
        return p.IsFreeColonist && p.health.State == PawnHealthState.Mobile;
    }

    public static bool HealthyPackAnimal(Pawn p)
    {
        return p.RaceProps.packAnimal && p.health.State == PawnHealthState.Mobile;
    }

    public static float ConstructionValue(Pawn p)
    {
        return p.GetStatValue(StatDefOf.ConstructionSpeed) * p.GetStatValue(StatDefOf.ConstructSuccessChance);
    }

    public static int ConstructionLevel(Pawn p)
    {
        return p.skills.GetSkill(SkillDefOf.Construction).levelInt;
    }

    public static string ShowConstructionValue(Pawn p)
    {
        if (HealthyColonist(p))
        {
            return $"{ConstructionValue(p):0.##}";
        }

        return HealthyPackAnimal(p) ? $"+{ConstructionValue(p):0.##}" : "-";
    }

    public static string ShowSkill(Pawn p)
    {
        return HealthyColonist(p) ? $"{ConstructionLevel(p):0}" : "-";
    }

    public static string ShowBestRoad(Pawn p)
    {
        RoadDef BestRoadDef = null;
        if (!HealthyColonist(p))
        {
            return "-";
        }

        foreach (var thisDef in DefDatabase<RoadDef>.AllDefs)
        {
            if (!thisDef.HasModExtension<DefModExtension_RotR_RoadDef>() ||
                !thisDef.GetModExtension<DefModExtension_RotR_RoadDef>().built
               ) // Only add RoadDefs that are buildable, based on DefModExtension_RotR_RoadDef.built
            {
                continue;
            }

            var RoadDefMod = thisDef.GetModExtension<DefModExtension_RotR_RoadDef>();
            if (ConstructionLevel(p) < RoadDefMod.minConstruction)
            {
                continue;
            }

            if (BestRoadDef != null && thisDef.movementCostMultiplier >= BestRoadDef.movementCostMultiplier)
            {
                continue;
            }

            BestRoadDef = thisDef;
        }

        return BestRoadDef == null ? "-" : BestRoadDef.label;
    }
}