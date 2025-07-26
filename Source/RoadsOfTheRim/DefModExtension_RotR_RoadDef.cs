using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

public class DefModExtension_RotR_RoadDef : DefModExtension
{
    public static readonly string[] allResources =
        ["WoodLog", "Stone", "Steel", "Chemfuel", "Plasteel", "Uranium", "ComponentIndustrial", "ComponentSpacer"];

    public static readonly string[] allResourcesAndWork =
    [
        "Work", "WoodLog", "Stone", "Steel", "Chemfuel", "Plasteel", "Uranium", "ComponentIndustrial",
        "ComponentSpacer"
    ];

    public static readonly string[] allResourcesWithoutModifiers =
        ["Uranium", "ComponentIndustrial", "ComponentSpacer"];
    // Base roads (DirtPath, DirtRoad, StoneRoad, AncientAsphaltRoad, AncientAsphaltHighway) will have this set to false, 
    // Built roads (DirtPath+, DirtRoad+, StoneRoad+, AsphaltRoad+, GlitterRoad) will have this set to true
    // Built roads will prevent rocks from being generated on top of them on maps

    public readonly float biomeModifier = 0f;
    public readonly bool built = false; // Whether this road is built or generated

    private readonly bool canBuildOnImpassable = false;

    private readonly bool canBuildOnWater = false;

    private readonly List<RotR_cost> costs = [];

    public readonly float hillinessModifier = 0f;

    public readonly float minConstruction = 0f;

    public readonly float percentageOfminConstruction = 0f;

    public readonly TechLevel techlevelToBuild = TechLevel.Neolithic;

    public readonly ResearchProjectDef techNeededToBuild = null;

    public readonly float winterModifier = 0f;

    public string GetCosts()
    {
        var s = new StringBuilder();
        s.Append($"The road is {(built ? "" : "not")} built. Costs : ");
        foreach (var c in costs)
        {
            s.Append($"{c.count} {c.name}, ");
        }

        return s.ToString();
    }

    public int GetCost(string name)
    {
        var aCost = costs.Find(c => c.name == name);
        return aCost?.count ?? 0;
    }

    public static bool GetInSituModifier(string resourceName, int ISR2G)
    {
        var result = false;
        switch (resourceName)
        {
            case "WoodLog":
            case "Stone":
                result = ISR2G > 0;
                break;
            case "Steel":
            case "Chemfuel":
            case "Plasteel":
            case "Uranium":
                result = ISR2G > 1;
                break;
        }

        return result;
    }

    public static bool BiomeAllowed(int tile, RoadDef roadDef, out BiomeDef biomeHere)
    {
        var RoadDefMod = roadDef.GetModExtension<DefModExtension_RotR_RoadDef>();
        biomeHere = Find.WorldGrid.Surface.Tiles[tile].PrimaryBiome;
        if (RoadDefMod.canBuildOnWater && biomeHere is { defName: "Ocean" } or { defName: "Lake" })
        {
            return true;
        }

        return biomeHere is { allowRoads: true };
    }

    public static bool ImpassableAllowed(int tile, RoadDef roadDef)
    {
        var RoadDefMod = roadDef.GetModExtension<DefModExtension_RotR_RoadDef>();
        var hillinnessHere = Find.WorldGrid.Surface.Tiles[tile].hilliness;
        if (RoadDefMod.canBuildOnImpassable && hillinnessHere == Hilliness.Impassable)
        {
            return true;
        }

        return hillinnessHere != Hilliness.Impassable;
    }
}