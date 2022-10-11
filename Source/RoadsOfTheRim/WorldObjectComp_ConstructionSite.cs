using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

public class WorldObjectComp_ConstructionSite : WorldObjectComp
{
    // TO DO : Make those 2 private
    private Dictionary<string, int> costs = new Dictionary<string, int>();

    // Used for ExposeData()
    private List<string> costs_Keys = new List<string>();
    private List<int> costs_Values = new List<int>();

    private Dictionary<string, float> left = new Dictionary<string, float>();
    private List<string> left_Keys = new List<string>();
    private List<float> left_Values = new List<float>();

    public CompProperties_RoadsOfTheRimConstructionSite Properties =>
        (CompProperties_RoadsOfTheRimConstructionSite)props;

    public int GetCost(string name)
    {
        return !costs.TryGetValue(name, out var value)
            ? 0
            : // TO DO : Throwing an excepion would be bettah
            value;
    }

    public float GetLeft(string name)
    {
        if (!left.TryGetValue(name, out var value))
        {
            return 0; // TO DO : Throwing an excepion would be bettah
        }

        return value;
    }

    public void ReduceLeft(string name, float amount)
    {
        if (!left.TryGetValue(name, out var value))
        {
            return;
        }

        left[name] -= amount > value ? value : amount;
    }

    private float GetPercentageDone(string name)
    {
        if (!costs.TryGetValue(name, out var costTotal) & !left.TryGetValue(name, out var leftTotal))
        {
            return 0;
        }

        return (costTotal - leftTotal) / costTotal;
    }

    /*
    Returns the cost modifiers for building a road from one tile to another, based on Elevation, Hilliness, Swampiness & river crossing
     */
    private static void GetCostsModifiers(int fromTile_int, int toTile_int, ref float elevationModifier,
        ref float hillinessModifier, ref float swampinessModifier, ref float bridgeModifier)
    {
        try
        {
            var settings = LoadedModManager.GetMod<RoadsOfTheRim>().GetSettings<RoadsOfTheRimSettings>();
            var fromTile = Find.WorldGrid[fromTile_int];
            var toTile = Find.WorldGrid[toTile_int];

            // Cost increase from elevation : if elevation is above {CostIncreaseElevationThreshold} (default 1000m), cost is doubled every {ElevationCostDouble} (default 2000m)
            elevationModifier = fromTile.elevation <= settings.CostIncreaseElevationThreshold
                ? 0
                : (fromTile.elevation - settings.CostIncreaseElevationThreshold) /
                  RoadsOfTheRimSettings.ElevationCostDouble;
            elevationModifier += toTile.elevation <= settings.CostIncreaseElevationThreshold
                ? 0
                : (toTile.elevation - settings.CostIncreaseElevationThreshold) /
                  RoadsOfTheRimSettings.ElevationCostDouble;

            // Hilliness and swampiness are the average between that of the from & to tiles
            // Hilliness is 0 on flat terrain, never negative. It's between 0 (flat) and 5(Impassable)
            var hilliness = Math.Max((((float)fromTile.hilliness + (float)toTile.hilliness) / 2) - 1, 0);
            var swampiness = (fromTile.swampiness + toTile.swampiness) / 2;

            // Hilliness and swampiness double the costs when they equal {HillinessCostDouble} (default 4) and {SwampinessCostDouble} (default 0.5)
            hillinessModifier = hilliness / RoadsOfTheRimSettings.HillinessCostDouble;
            swampinessModifier = swampiness / RoadsOfTheRimSettings.SwampinessCostDouble;

            bridgeModifier = 0f;
            /* TO DO : River crossing
            List<int> fromTileNeighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(parent.Tile, fromTileNeighbors);
            foreach (Tile.RiverLink aRiver in fromTile.Rivers )
            {
                Log.Message("River in FROM tile : neighbor="+aRiver.neighbor+", river="+aRiver.river.ToString());
            }
            */
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog(null, e);
        }
    }

    /*
     * For resources (including work) that are part of the cost of both the road to build and the best existing road, 
     * grant CostUpgradeRebate% (default 30%) of the best existing road build costs as a rebate on the costs of the road to be built
     * i.e. the exisitng road cost 300 stones, the new road cost 600 stones, the rebate is 300*30% = 90 stones
     */
    private static void GetUpgradeModifiers(int fromTile_int, int toTile_int, RoadDef roadToBuild,
        out Dictionary<string, int> rebate)
    {
        rebate = new Dictionary<string, int>();
        var bestExistingRoad = RoadsOfTheRim.BestExistingRoad(fromTile_int, toTile_int);
        if (bestExistingRoad == null)
        {
            return;
        }

        var bestExistingRoadDefModExtension = bestExistingRoad.GetModExtension<DefModExtension_RotR_RoadDef>();
        var roadToBuildRoadDefModExtension = roadToBuild.GetModExtension<DefModExtension_RotR_RoadDef>();
        if (bestExistingRoadDefModExtension == null || roadToBuildRoadDefModExtension == null ||
            !RoadsOfTheRim.IsRoadBetter(roadToBuild, bestExistingRoad))
        {
            return;
        }

        foreach (var resourceName in DefModExtension_RotR_RoadDef.allResourcesAndWork)
        {
            var existingCost = bestExistingRoadDefModExtension.GetCost(resourceName);
            var toBuildCost = roadToBuildRoadDefModExtension.GetCost(resourceName);
            if (existingCost == 0 || toBuildCost == 0)
            {
                continue;
            }

            if ((int)(existingCost * (float)RoadsOfTheRim.settings.CostUpgradeRebate / 100) > toBuildCost)
            {
                rebate[resourceName] = toBuildCost;
            }
            else
            {
                rebate[resourceName] =
                    (int)(existingCost * (float)RoadsOfTheRim.settings.CostUpgradeRebate / 100);
            }
        }
    }

    /* return a string describing modifiers to road costs building between two tiles, and sets totalCostModifer */
    public static string CostModifersDescription(int fromTile_int, int toTile_int, ref float totalCostModifier)
    {
        var result = new StringBuilder();
        var settings = LoadedModManager.GetMod<RoadsOfTheRim>().GetSettings<RoadsOfTheRimSettings>();
        // Show total cost modifiers
        var elevationModifier = 0f;
        var hillinessModifier = 0f;
        var swampinessModifier = 0f;
        var bridgeModifier = 0f;
        GetCostsModifiers(fromTile_int, toTile_int, ref elevationModifier, ref hillinessModifier,
            ref swampinessModifier, ref bridgeModifier);
        result.Append("RoadsOfTheRim_ConstructionSiteDescription_CostModifiers".Translate(
            $"{elevationModifier + hillinessModifier + swampinessModifier + bridgeModifier:P0}",
            $"{elevationModifier:P0}",
            $"{hillinessModifier:P0}",
            $"{swampinessModifier:P0}",
            $"{bridgeModifier:P0}"
        ));
        totalCostModifier = (1 + elevationModifier + hillinessModifier + swampinessModifier + bridgeModifier) *
                            ((float)settings.BaseEffort / 10);
        return result.ToString();
    }


    /*
     * Faction help must be handled here, since it's independent of whether or not a caravan is here.
     * Make it with a delay of 1/50 s compared to the CaravanComp so both functions end up playing nicely along each other
     * Don't work at night !
     */
    public override void CompTick()
    {
        try
        {
            if (((RoadConstructionSite)parent).helpFromFaction == null ||
                CaravanNightRestUtility.RestingNowAt(((RoadConstructionSite)parent).Tile) ||
                Find.TickManager.TicksGame % 100 != 50)
            {
                return;
            }

            ((RoadConstructionSite)parent).TryToSkipBetterRoads(); // No need to work if there's a better road here
            var amountOfWork = ((RoadConstructionSite)parent).FactionHelp();

            var percentOfWorkLeftToDoAfter = (GetLeft("Work") - amountOfWork) / GetCost("Work");
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
            {
                ReduceLeft(resourceName,
                    (int)Math.Round(GetLeft(resourceName) - (percentOfWorkLeftToDoAfter * GetCost(resourceName))));
            }

            UpdateProgress(amountOfWork);
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog($"Construction Site CompTick. parentsite = {(RoadConstructionSite)parent}", e);
        }
    }

    public string ResourcesAlreadyConsumed()
    {
        var resourceList = new List<string>();
        try
        {
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResourcesAndWork)
            {
                if (GetCost(resourceName) <= 0 || GetLeft(resourceName) >= GetCost(resourceName))
                {
                    continue;
                }

                resourceList.Add($"{GetCost(resourceName) - GetLeft(resourceName)} {resourceName}");
            }
        }
        catch
        {
            RoadsOfTheRim.DebugLog(
                "resourcesAlreadyConsumed failed. This will happen after upgrading to the 20190207 version");
        }

        return string.Join(", ", resourceList.ToArray());
    }

    public void SetCosts()
    {
        try
        {
            var settings = LoadedModManager.GetMod<RoadsOfTheRim>().GetSettings<RoadsOfTheRimSettings>();
            var parentSite = parent as RoadConstructionSite;

            var elevationModifier = 0f;
            var hillinessModifier = 0f;
            var swampinessModifier = 0f;
            var bridgeModifier = 0f;
            if (parentSite == null)
            {
                return;
            }

            GetCostsModifiers(parentSite.Tile, parentSite.GetNextLeg().Tile, ref elevationModifier,
                ref hillinessModifier, ref swampinessModifier, ref bridgeModifier);

            // Total cost modifier
            var totalCostModifier =
                (1 + elevationModifier + hillinessModifier + swampinessModifier + bridgeModifier) *
                ((float)settings.BaseEffort / 10);

            var roadDefExtension = parentSite.roadDef.GetModExtension<DefModExtension_RotR_RoadDef>();

            // Check existing roads for potential rebates when upgrading
            GetUpgradeModifiers(parentSite.Tile, parentSite.GetNextLeg().Tile, parentSite.roadDef,
                out var rebate);

            var s = new List<string>();
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResourcesAndWork)
            {
                if (roadDefExtension.GetCost(resourceName) <= 0)
                {
                    continue;
                }

                // The cost modifier doesn't affect some advanced resources, as defined in static DefModExtension_RotR_RoadDef.allResourcesWithoutModifiers
                var costModifierForThisResource =
                    DefModExtension_RotR_RoadDef.allResourcesWithoutModifiers.Contains(resourceName)
                        ? 1
                        : totalCostModifier;
                rebate.TryGetValue(resourceName, out var thisRebate);
                // Minimum cost of anything that's needed is 1
                costs[resourceName] =
                    Math.Max(
                        (int)((roadDefExtension.GetCost(resourceName) - thisRebate) *
                              costModifierForThisResource),
                        1);
                left[resourceName] = Math.Max(costs[resourceName], 1f);
                if (thisRebate > 0)
                {
                    s.Add("RoadsOfTheRim_UpgradeRebateDetail".Translate(
                        (int)(thisRebate * costModifierForThisResource), resourceName));
                }
            }

            if (s.Count > 0)
            {
                Messages.Message(
                    "RoadsOfTheRim_UpgradeRebate".Translate(parentSite.roadDef.label,
                        string.Join(", ", s.ToArray())), MessageTypeDefOf.PositiveEvent);
            }

            parentSite.UpdateProgressBarMaterial();
        }
        catch (Exception e)
        {
            Log.Error($"[RotR] : Exception when setting constructionSite costs = {e}");
        }
    }

    public bool UpdateProgress(float amountOfWork, Caravan caravan = null)
    {
        var parentSite = parent as RoadConstructionSite;

        ReduceLeft("Work", amountOfWork);

        parentSite?.UpdateProgressBarMaterial();

        // Work is done
        return GetLeft("Work") <= 0 && FinishWork(caravan);
    }

    /*
     * Build the road and move the construction site
     */
    public bool FinishWork(Caravan caravan = null)
    {
        var parentSite = parent as RoadConstructionSite;
        if (parentSite != null)
        {
            var fromTile_int = parentSite.Tile;
            var toTile_int = parentSite.GetNextLeg().Tile;
            var fromTile = Find.WorldGrid[fromTile_int];
            var toTile = Find.WorldGrid[toTile_int];

            // Remove lesser roads, they don't deserve to live
            if (fromTile.potentialRoads != null)
            {
                foreach (var aLink in fromTile.potentialRoads.ToArray())
                {
                    if ((aLink.neighbor == toTile_int) & RoadsOfTheRim.IsRoadBetter(parentSite.roadDef, aLink.road))
                    {
                        fromTile.potentialRoads.Remove(aLink);
                    }
                }
            }
            else
            {
                fromTile.potentialRoads = new List<Tile.RoadLink>();
            }

            if (toTile.potentialRoads != null)
            {
                foreach (var aLink in toTile.potentialRoads.ToArray())
                {
                    if ((aLink.neighbor == parentSite.Tile) &
                        RoadsOfTheRim.IsRoadBetter(parentSite.roadDef, aLink.road))
                    {
                        toTile.potentialRoads.Remove(aLink);
                    }
                }
            }
            else
            {
                toTile.potentialRoads = new List<Tile.RoadLink>();
            }

            // Add the road to fromTile & toTile
            fromTile.potentialRoads.Add(new Tile.RoadLink { neighbor = toTile_int, road = parentSite.roadDef });
            toTile.potentialRoads.Add(new Tile.RoadLink { neighbor = fromTile_int, road = parentSite.roadDef });
            try
            {
                Find.World.renderer.SetDirty<WorldLayer_Roads>();
                Find.World.renderer.SetDirty<WorldLayer_Paths>();
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(fromTile_int);
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(toTile_int);
            }
            catch (Exception e)
            {
                RoadsOfTheRim.DebugLog("[RotR] Exception : ", e);
            }
        }

        // The Construction site and the caravan can move to the next leg
        var nextLeg = parentSite?.GetNextLeg();
        if (nextLeg == null)
        {
            return true;
        }

        var CurrentTile = parentSite.Tile;
        parentSite.Tile = nextLeg.Tile;
        var nextNextLeg = nextLeg.Next;
        // TO DO Here : Check if there's an existing road that is the same or better as the one being built. If there is, skip the next leg
        if (nextNextLeg != null)
        {
            nextNextLeg.Previous = null;
            SetCosts();
            parentSite.MoveWorkersToNextLeg(
                CurrentTile); // Move any caravans working on this site to the next leg, and delay faction help if any
        }
        else
        {
            EndConstruction(caravan); // We have built the last leg. Notify & remove the site
        }

        Find.World.worldObjects.Remove(nextLeg);

        return true;
    }

    public void EndConstruction(Caravan caravan = null)
    {
        // On the last leg, send letter & remove the construction site
        if (!(parent is RoadConstructionSite parentSite))
        {
            return;
        }

        Find.LetterStack.ReceiveLetter(
            "RoadsOfTheRim_RoadBuilt".Translate(),
            "RoadsOfTheRim_RoadBuiltLetterText".Translate(parentSite.roadDef.label,
                caravan != null ? (TaggedString)caravan.Label : "RoadsOfTheRim_RoadBuiltByAlly".Translate()),
            LetterDefOf.PositiveEvent,
            new GlobalTargetInfo(parentSite.Tile)
        );
        Find.World.worldObjects.Remove(parentSite);
        if (parentSite.helpFromFaction != null)
        {
            RoadsOfTheRim.FactionsHelp.HelpFinished(parentSite.helpFromFaction);
        }
    }

    public string ProgressDescription()
    {
        var parentSite = parent as RoadConstructionSite;
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            "RoadsOfTheRim_ConstructionSiteDescription_Main".Translate($"{GetPercentageDone("Work"):P1}"));

        // Description of ally's help, if any
        if (parentSite?.helpFromFaction != null)
        {
            stringBuilder.Append("RoadsOfTheRim_ConstructionSiteDescription_Help".Translate(
                parentSite.helpFromFaction.Name, (int)parentSite.helpAmount,
                $"{parentSite.helpWorkPerTick:0.0}"));
            if (parentSite.helpFromTick > Find.TickManager.TicksGame)
            {
                stringBuilder.Append("RoadsOfTheRim_ConstructionSiteDescription_HelpStartsWhen".Translate(
                    $"{(parentSite.helpFromTick - Find.TickManager.TicksGame) / (float)GenDate.TicksPerDay:0.00}"));
            }
        }

        // Show total cost modifiers
        var totalCostModifier = 0f;
        if (parentSite == null)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.Append(CostModifersDescription(parentSite.Tile, parentSite.GetNextLeg().Tile,
            ref totalCostModifier));

        var AllCaravansHere = new List<Caravan>();
        Find.WorldObjects.GetPlayerControlledCaravansAt(parentSite.Tile, AllCaravansHere);
        var ISR2G = 0;
        foreach (var c in AllCaravansHere)
        {
            var caravanISR2G = c.GetComponent<WorldObjectComp_Caravan>().UseISR2G();
            if (caravanISR2G > ISR2G)
            {
                ISR2G = caravanISR2G;
            }
        }

        // Per resource : show costs & how much is left to do
        foreach (var resourceName in DefModExtension_RotR_RoadDef.allResourcesAndWork)
        {
            if (GetCost(resourceName) <= 0)
            {
                continue;
            }

            stringBuilder.AppendLine();
            var ISR2Gmsg = "";
            if (ISR2G > 0)
            {
                if (resourceName == "Work")
                {
                    ISR2Gmsg = ISR2G == 1
                        ? "RoadsOfTheRim_ConstructionSiteDescription_ISR2Gwork".Translate()
                        : "RoadsOfTheRim_ConstructionSiteDescription_AISR2Gwork".Translate();
                }
                else if (DefModExtension_RotR_RoadDef.GetInSituModifier(resourceName, ISR2G))
                {
                    ISR2Gmsg = ISR2G == 1
                        ? "RoadsOfTheRim_ConstructionSiteDescription_ISR2GFree".Translate()
                        : "RoadsOfTheRim_ConstructionSiteDescription_AISR2GFree".Translate();
                }
            }

            stringBuilder.Append("RoadsOfTheRim_ConstructionSiteDescription_Resource".Translate(
                GetNiceResourceName(resourceName),
                string.Format(resourceName == "Work" ? "{0:##.00}" : "{0:##}",
                    GetLeft(resourceName)), // Only Work should be shown with 2 decimals
                GetCost(resourceName),
                ISR2Gmsg
            ));
        }

        return stringBuilder.ToString();
    }

    private string GetNiceResourceName(string defName)
    {
        var defThing = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (defThing == null || string.IsNullOrEmpty(defThing.label))
        {
            return defName;
        }

        return defThing.label;
    }

    public float PercentageDone()
    {
        return GetPercentageDone("Work");
    }

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref costs, "RotR_site_costs", LookMode.Value, LookMode.Value, ref costs_Keys,
            ref costs_Values);
        Scribe_Collections.Look(ref left, "RotR_site_left", LookMode.Value, LookMode.Value, ref left_Keys,
            ref left_Values);
    }
}