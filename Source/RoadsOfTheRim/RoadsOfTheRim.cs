using System;
using System.Collections.Generic;
using System.Linq;
using Mlie;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RoadsOfTheRim;

public class RoadsOfTheRim : Mod
{
    public static RoadsOfTheRimSettings settings;

    public static readonly List<TerrainDef> builtRoadTerrains = new List<TerrainDef>();
    private static string currentVersion;

    public RoadsOfTheRim(ModContentPack content) : base(content)
    {
        settings = GetSettings<RoadsOfTheRimSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }


    /***********************************
      * Static links to WorldComponents *       
      ***********************************/

    public static WorldComponent_FactionRoadConstructionHelp FactionsHelp
    {
        get
        {
            if (Find.World.GetComponent(typeof(WorldComponent_FactionRoadConstructionHelp)) is
                WorldComponent_FactionRoadConstructionHelp f)
            {
                return f;
            }

            Log.Warning("[RotR] - ERROR, couldn't find WorldComponent_FactionRoadConstructionHelp");
            return null;
        }
    }

    public static WorldComponent_RoadBuildingState RoadBuildingState
    {
        get
        {
            if (Find.World.GetComponent(typeof(WorldComponent_RoadBuildingState)) is
                WorldComponent_RoadBuildingState f)
            {
                return f;
            }

            Log.Message("[RotR] - ERROR, couldn't find WorldComponent_RoadBuildingState");
            return null;
        }
    }

    public static void DebugLog(string message = null, Exception e = null)
    {
#if DEBUG
            if (message != null)
            {
                Log.Warning("[RotR] DEBUG - " + message);
            }
            if (e != null)
            {
                Log.Error(
                "[RotR] Exception :\n" + e + "\n=====\n" +
                "Stack trace :\n" + e.StackTrace + "\n=====\n" +
                "Data : " + e.Data
                );
            }
#endif
    }

    public static float CalculateRoadModifier(RoadDef roadDef, float BiomeMovementDifficulty, float HillinessOffset,
        float WinterOffset, out float BiomeModifier, out float HillModifier, out float WinterModifier)
    {
        BiomeModifier = 0f;
        HillModifier = 0f;
        WinterModifier = 0f;
        if (roadDef.HasModExtension<DefModExtension_RotR_RoadDef>())
        {
            BiomeModifier = roadDef.GetModExtension<DefModExtension_RotR_RoadDef>().biomeModifier;
            HillModifier = roadDef.GetModExtension<DefModExtension_RotR_RoadDef>().hillinessModifier;
            WinterModifier = roadDef.GetModExtension<DefModExtension_RotR_RoadDef>().winterModifier;
        }

        var BiomeCoef = (1 + ((BiomeMovementDifficulty - 1) * (1 - BiomeModifier))) / BiomeMovementDifficulty;
        //RoadsOfTheRim.DebugLog("calculateRoadModifier: BiomeCoef=" +BiomeCoef+ ", BiomeMovementDifficulty="+ BiomeMovementDifficulty+ ", HillModifier"+ HillModifier+ ", HillinessOffset="+ HillinessOffset+ ", WinterModifier="+ WinterModifier+ ", WinterOffset="+ WinterOffset);
        return ((BiomeCoef * BiomeMovementDifficulty) + ((1 - HillModifier) * HillinessOffset) +
                ((1 - WinterModifier) * WinterOffset)) / (BiomeMovementDifficulty + HillinessOffset + WinterOffset);
    }


    /*
    Based on the Caravan's resources, Pawns & the road's cost (modified by terrain) :
    - Determine the amount of work done in a tick
    - Consume the caravan's resources
    - Return whether or not the Caravan must now stop because it ran out of resources
    - NOTE : Does this need to be here ? Maybe better in Mod.cs
    * Returns TRUE if work finished
    * CALLED FROM : CompTick() of WorldObjectComp_Caravan        
    */
    public static bool DoSomeWork(Caravan caravan, RoadConstructionSite site, out bool noMoreResources)
    {
        var caravanComp = caravan.GetComponent<WorldObjectComp_Caravan>();
        var siteComp = site.GetComponent<WorldObjectComp_ConstructionSite>();
        _ = site.roadDef.GetModExtension<DefModExtension_RotR_RoadDef>();
        noMoreResources = false;
        var useISR2G = caravanComp.UseISR2G();
        var available = new Dictionary<string, int>();
        var needed = new Dictionary<string, int>();
        var ratio = new Dictionary<string, float>();
        float ratio_final = 1;
        //RoadsOfTheRim.DebugLog("[RotR] DEBUG ========== doSomeWork() ==========");
        //RoadsOfTheRim.DebugLog("[RotR] DEBUG ISR2G set to "+useISR2G);

        if (DebugSettings.godMode)
        {
            return siteComp.FinishWork(caravan);
        }

        if (caravanComp.CaravanCurrentState() != CaravanState.ReadyToWork)
        {
            DebugLog("[RotR] DEBUG : doSomeWork() failed because the caravan can't work.");
            return false;
        }

        // Percentage of total work that can be done in this batch, might be 0 if no pawn was found with enough skill
        var amountOfWork = caravanComp.AmountOfWork(true);

        // Work was 0 (not enough skill)
        if (Math.Abs(amountOfWork) < double.Epsilon)
        {
            Messages.Message("RoadsOfTheRim_CaravanNoWork".Translate(caravan.Name, site.roadDef.label),
                MessageTypeDefOf.RejectInput);
            caravanComp.StopWorking();
            return false;
        }

        // calculate material present in the caravan
        foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
        {
            available[resourceName] = 0;
        }

        foreach (var aThing in CaravanInventoryUtility.AllInventoryItems(caravan))
        {
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
            {
                if (IsThis(aThing.def, resourceName))
                {
                    available[resourceName] += aThing.stackCount;
                }
            }
        }

        // What percentage of work will remain after amountOfWork is done ?
        var percentOfWorkLeftToDoAfter = (siteComp.GetLeft("Work") - amountOfWork) / siteComp.GetCost("Work");

        // The amount of each resource left to spend in total is : percentOfWorkLeftToDoAfter * {this resource cost}
        // Materials that would be needed to do that much work
        foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
        {
            needed[resourceName] = (int)Math.Round(siteComp.GetLeft(resourceName) -
                                                   (percentOfWorkLeftToDoAfter * siteComp.GetCost(resourceName)));
            // Check if there's enough material to go through this batch. Materials with a cost of 0 are always OK
            // Don't check when ISR2G is in use for this resource, don't check for work
            if (DefModExtension_RotR_RoadDef.GetInSituModifier(resourceName, useISR2G) || resourceName == "Work")
            {
                continue;
            }

            ratio[resourceName] = needed[resourceName] == 0
                ? 1f
                : Math.Min(available[resourceName] / (float)needed[resourceName], 1f);
            if (ratio[resourceName] < ratio_final)
            {
                ratio_final = ratio[resourceName];
            }
        }

        // The caravan didn't have enough resources for a full batch of work. Use as much as we can then stop working
        if (ratio_final < 1f)
        {
            Messages.Message("RoadsOfTheRim_CaravanNoResource".Translate(caravan.Name, site.roadDef.label),
                MessageTypeDefOf.RejectInput);
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
            {
                needed[resourceName] = (int)(needed[resourceName] * ratio_final);
            }

            caravanComp.StopWorking();
        }
        //RoadsOfTheRim.DebugLog("[RotR] ISR2G DEBUG ratio final = " + ratio_final);

        // Consume resources from the caravan 
        _ = site.roadDef.defName ==
            "DirtPathBuilt"; // Always consider resources have been consumed when the road is a dirt path
        foreach (var aThing in CaravanInventoryUtility.AllInventoryItems(caravan))
        {
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResources)
            {
                if (!DefModExtension_RotR_RoadDef.GetInSituModifier(resourceName, useISR2G))
                {
                    if (needed[resourceName] <= 0 || !IsThis(aThing.def, resourceName))
                    {
                        continue;
                        //RoadsOfTheRim.DebugLog("[RotR] ISR2G consumption DEBUG =" + resourceName + " Qty consumed = " + amountUsed);
                    }

                    var amountUsed = aThing.stackCount > needed[resourceName]
                        ? needed[resourceName]
                        : aThing.stackCount;
                    aThing.stackCount -= amountUsed;
                    // Reduce how much of this resource is needed
                    needed[resourceName] -= amountUsed;
                    siteComp.ReduceLeft(resourceName, amountUsed);
                }
                else
                {
                    if (needed[resourceName] <= 0)
                    {
                        continue;
                    }

                    //RoadsOfTheRim.DebugLog("[RotR] ISR2G consumption DEBUG =" + resourceName + " Qty freely awarded = " + needed[resourceName]);
                    siteComp.ReduceLeft(resourceName, needed[resourceName]);
                    needed[resourceName] = 0;
                }
            }

            if (aThing.stackCount == 0)
            {
                aThing.Destroy();
            }
        }

        caravanComp.TeachPawns(ratio_final); // Pawns learn some construction
        // HARDCODED : ISR2G divides work done by 4 , AISR2G by 2 for all roads except dirt path
        if (useISR2G > 0 && site.roadDef.defName != "DirtPathBuilt")
        {
            amountOfWork = amountOfWork * 0.25f * useISR2G;
        }

        // Update amountOfWork based on the actual ratio worked & finally reducing the work & resources left
        amountOfWork = ratio_final * amountOfWork;
        return siteComp.UpdateProgress(amountOfWork, caravan);
    }


    /***********************************
     * Settings                        *       
     ***********************************/

    public override string SettingsCategory()
    {
        return "RoadsOfTheRimSettingsCategoryLabel".Translate();
    }

    public override void DoSettingsWindowContents(Rect rect)
    {
        var CurrentOverOverrideCosts = settings.OverrideCosts;
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.CheckboxLabeled("RoadsOfTheRimSettingsOverrideCosts".Translate() + ": ",
            ref settings.OverrideCosts);
        listing_Standard.Gap();
        listing_Standard.Label("RoadsOfTheRimSettingsElevationThreshold".Translate() + ": " +
                               settings.CostIncreaseElevationThreshold);
        listing_Standard.Gap();
        settings.CostIncreaseElevationThreshold =
            listing_Standard.Slider(settings.CostIncreaseElevationThreshold, 0f, 5000f);
        listing_Standard.Gap();
        listing_Standard.Label(
            $"{"RoadsOfTheRimSettingsUpgradeRebate".Translate() + ": "}{settings.CostUpgradeRebate}%");
        listing_Standard.Gap();
        settings.CostUpgradeRebate = (int)listing_Standard.Slider(settings.CostUpgradeRebate, 0, 100);
        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("RoadsOfTheRimSettingsUseISR2G".Translate() + ": ", ref settings.useISR2G);
        // Always make sure to set costs to 100% when using ISR2G
        if (settings.useISR2G)
        {
            settings.BaseEffort = RoadsOfTheRimSettings.MaxBaseEffort;
        }
        else
        {
            listing_Standard.Gap();
            listing_Standard.Label("RoadsOfTheRimSettingsBaseEffort".Translate() + ": " +
                                   $"{(float)settings.BaseEffort / 10:0%}");
            listing_Standard.Gap();
            settings.BaseEffort = (int)listing_Standard.Slider(settings.BaseEffort,
                RoadsOfTheRimSettings.MinBaseEffort, RoadsOfTheRimSettings.MaxBaseEffort);
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("RoadsOfTheRim_ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();

        settings.Write();
        if (CurrentOverOverrideCosts == settings.OverrideCosts)
        {
            return;
        }

        try
        {
            Find.WorldPathGrid.RecalculateAllPerceivedPathCosts();
        }
        catch
        {
            // Ugly. I should just check if the WorldPathGrid exists.
        }
    }

    /********************************
     * Gizmos commands              *       
     ********************************/

    public static Command AddConstructionSite(Caravan caravan)
    {
        var command_Action = new Command_Action
        {
            defaultLabel = "RoadsOfTheRimAddConstructionSite".Translate(),
            defaultDesc = "RoadsOfTheRimAddConstructionSiteDescription".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/AddConstructionSite"),
            action = delegate
            {
                var constructionSite =
                    (RoadConstructionSite)WorldObjectMaker.MakeWorldObject(
                        DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"));
                constructionSite.Tile = caravan.Tile;
                Find.WorldObjects.Add(constructionSite);

                var menu = new ConstructionMenu(constructionSite, caravan);

                if (menu.CountBuildableRoads() == 0)
                {
                    Find.WorldObjects.Remove(constructionSite);
                    Messages.Message("RoadsOfTheRim_NoBetterRoadCouldBeBuilt".Translate(),
                        MessageTypeDefOf.RejectInput);
                }
                else
                {
                    menu.closeOnClickedOutside = true;
                    menu.forcePause = true;
                    Find.WindowStack.Add(menu);
                }
            }
        };

        // Disable if there's already a construction site here
        if (Find.WorldObjects.AnyWorldObjectOfDefAt(DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"),
                caravan.Tile))
        {
            command_Action.Disable("RoadsOfTheRimBuildConstructionSiteAlreadyHere".Translate());
        }

        // disable if the caravan can't work OR if the site is not ready
        if (caravan.GetComponent<WorldObjectComp_Caravan>().CaravanCurrentState() != CaravanState.ReadyToWork)
        {
            command_Action.Disable("RoadsOfTheRimBuildWorkOnSiteCantWork".Translate());
        }

        // Disable on biomes that don't allow roads
        // TO DO : did that ever belong here ? testing for each leg should be enough except if somehow a caravan is in an ocean (glitter road ?) building a lower tier road towards the shore ?
        // In which case, problem is easy to resolve : limit the roads that can be chosen in the menu
        /*
        BiomeDef biomeHere = Find.WorldGrid.tiles[caravan.Tile].biome ;
        if (!biomeHere.allowRoads)
        {
            command_Action.Disable("RoadsOfTheRim_BiomePreventsConstruction".Translate(biomeHere.label));
        }
        */
        return command_Action;
    }

    public static void FinaliseConstructionSite(RoadConstructionSite site)
    {
        if (site.GetNextLeg() != null)
        {
            site.GetComponent<WorldObjectComp_ConstructionSite>().SetCosts();
            RoadBuildingState.Caravan.GetComponent<WorldObjectComp_Caravan>().StartWorking();
        }
        else
        {
            RoadConstructionSite.DeleteSite(site);
        }
    }

    /*
    Work on  Site
     */
    public static Command WorkOnSite(Caravan caravan)
    {
        var command_Action = new Command_Action
        {
            defaultLabel = "RoadsOfTheRimWorkOnSite".Translate(),
            defaultDesc = "RoadsOfTheRimWorkOnSiteDescription".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/AddConstructionSite"),
            action = delegate
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                caravan.GetComponent<WorldObjectComp_Caravan>().StartWorking();
            }
        };
        // disable if the caravan can't work OR if the site is not ready
        if (caravan.GetComponent<WorldObjectComp_Caravan>().CaravanCurrentState() != CaravanState.ReadyToWork)
        {
            command_Action.Disable("RoadsOfTheRimBuildWorkOnSiteCantWork".Translate(caravan.GetDescription()));
        }

        return command_Action;
    }

    /*
    Stop working on  Site
     */
    public static Command StopWorkingOnSite(Caravan caravan)
    {
        var command_Action = new Command_Action
        {
            defaultLabel = "RoadsOfTheRimStopWorkingOnSite".Translate(),
            defaultDesc = "RoadsOfTheRimStopWorkingOnSiteDescription".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RemoveConstructionSite"),
            action = delegate
            {
                SoundDefOf.CancelMode.PlayOneShotOnCamera();
                caravan.GetComponent<WorldObjectComp_Caravan>().StopWorking();
            }
        };
        return command_Action;
    }

    /*
    Remove Construction Site
     */
    public static Command RemoveConstructionSite(int tile)
    {
        // TO DO : Refactor this so we find the site first, to pass it to Deleteconstructionsite directly, or even get rid of that function all together
        var command_Action = new Command_Action
        {
            defaultLabel = "RoadsOfTheRimRemoveConstructionSite".Translate(),
            defaultDesc = "RoadsOfTheRimRemoveConstructionSiteDescription".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Commands/RemoveConstructionSite"),
            action = delegate
            {
                SoundDefOf.CancelMode.PlayOneShotOnCamera();
                DeleteConstructionSite(tile);
            }
        };
        // Test when the RemoveConstructionSite action should be disabled (i.e. there's no construction site here)
        var ConstructionSiteAlreadyHere = false;
        try
        {
            ConstructionSiteAlreadyHere =
                Find.WorldObjects.AnyWorldObjectOfDefAt(
                    DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"), tile);
        }
        catch
        {
            // ignored
        }

        if (!ConstructionSiteAlreadyHere)
        {
            command_Action.Disable("RoadsOfTheRimBuildConstructionSiteNotAlreadyHere".Translate());
        }

        return command_Action;
    }

    /*Delete Construction Site    */
    public static void DeleteConstructionSite(int tile)
    {
        var ConstructionSite =
            (RoadConstructionSite)Find.WorldObjects.WorldObjectOfDefAt(
                DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"), tile);
        if (ConstructionSite == null)
        {
            return;
        }

        // Confirm construction site deletion if resources were already consumed
        var s = ConstructionSite.ResourcesAlreadyConsumed();
        if (!s.NullOrEmpty())
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "RoadsOfTheRim_ConfirmDestroyResourcesAlreadyConsumed".Translate(s),
                delegate { DeleteConstructionSiteConfirmed(ConstructionSite); }));
        }
        else
        {
            DeleteConstructionSiteConfirmed(ConstructionSite);
        }
    }

    /*Delete Cosntruction Site once it's been confirmed, or no confirmation was necessary */
    private static void DeleteConstructionSiteConfirmed(RoadConstructionSite ConstructionSite)
    {
        if (ConstructionSite.helpFromFaction != null)
        {
            FactionsHelp.HelpFinished(ConstructionSite.helpFromFaction);
        }

        RoadConstructionSite.DeleteSite(ConstructionSite);
    }

    public static DiaOption HelpRoadConstruction(Faction faction, Pawn negotiator)
    {
        var dialog = new DiaOption("RoadsOfTheRim_commsAskHelp".Translate());

        // Find all construction sites on the world map
        IEnumerable<WorldObject> constructionSites = Find.WorldObjects.AllWorldObjects
            .Where(site => site.def == DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite")).ToArray();
        // If none : option should be disabled
        if (!constructionSites.Any())
        {
            dialog.Disable("RoadsOfTheRim_commsNoSites".Translate());
        }

        var diaNode = new DiaNode("RoadsOfTheRim_commsSitesList".Translate());
        foreach (var o in constructionSites)
        {
            var site = (RoadConstructionSite)o;
            var diaOption = new DiaOption(site.FullName())
            {
                action = delegate { FactionsHelp.StartHelping(faction, site, negotiator); }
            };
            // Disable sites that do not have a settlement of this faction close enough (as defined by ConstructionSite.maxTicksToNeighbour)
            if (site.ClosestSettlementOfFaction(faction) == null)
            {
                diaOption = new DiaOption("Invalid site");
                diaOption.Disable("RoadsOfTheRim_commsNotClose".Translate(faction.Name));
            }

            if (site.helpFromFaction != null)
            {
                diaOption = new DiaOption("Invalid site");
                diaOption.Disable("RoadsOfTheRim_commsAnotherFactionIsHelping".Translate(site.helpFromFaction));
            }

            if (!FactionsHelp.IsDeveloppedEnough(faction,
                    site.roadDef.GetModExtension<DefModExtension_RotR_RoadDef>()))
            {
                diaOption = new DiaOption("Invalid site");
                diaOption.Disable(
                    "RoadsOfTheRim_commsNotDevelopedEnough".Translate(faction.Name, site.roadDef.label));
            }

            diaNode.options.Add(diaOption);
            diaOption.resolveTree = true;
        }

        // If the faction is already helping, it must be disabled
        if (FactionsHelp.GetCurrentlyHelping(faction))
        {
            dialog = new DiaOption("Can't help build roads");
            dialog.Disable("RoadsOfTheRim_commsAlreadyHelping".Translate());
        }

        // If the faction is in construction cooldown, it must be disabled
        if (FactionsHelp.InCooldown(faction))
        {
            dialog = new DiaOption("Can't help build roads");
            dialog.Disable("RoadsOfTheRim_commsHasHelpedRecently".Translate(
                $"{FactionsHelp.DaysBeforeFactionCanHelp(faction):0.0}"));
        }

        // Cancel option (needed when all sites are disabled for one of the above reason)
        var cancelOption = new DiaOption("(" + "RoadsOfTheRim_commsCancel".Translate() + ")");
        diaNode.options.Add(cancelOption);
        cancelOption.resolveTree = true;

        dialog.link = diaNode;
        return dialog;
    }


    /********************************
     * Convenience static functions *       
     ********************************/

    // Compares the movement cost multiplier of 2 roaddefs, returns TRUE if roadA is better or roadB is null. returns FALSE in all other cases
    public static bool IsRoadBetter(RoadDef roadA, RoadDef roadB)
    {
        if (roadA == null)
        {
            return false;
        }

        if (roadB == null)
        {
            return true;
        }

        return roadA.movementCostMultiplier < roadB.movementCostMultiplier;
    }

    /*
    Tells me whether or not a ThingDef is what I want
    */
    private static bool IsThis(ThingDef def, string name)
    {
        if (name == "Stone" && def.IsWithinCategory(ThingCategoryDefOf.StoneBlocks))
        {
            return true;
        }

        try
        {
            return def.Equals(DefDatabase<ThingDef>.GetNamed(name, false));
        }
        catch
        {
            return false;
        }
    }

    /*
    Returns the road with the best movement cost multiplier between 2 neighbouring tiles.
    returns null if there's no road or if the tiles are not neighbours
     */
    public static RoadDef BestExistingRoad(int fromTile_int, int toTile_int)
    {
        RoadDef bestExistingRoad = null;
        try
        {
            var worldGrid = Find.WorldGrid;
            var fromTile = worldGrid[fromTile_int];
            var toTile = worldGrid[toTile_int];

            if (fromTile.potentialRoads != null)
            {
                foreach (var aLink in fromTile.potentialRoads)
                {
                    if ((aLink.neighbor == toTile_int) & IsRoadBetter(aLink.road, bestExistingRoad))
                    {
                        bestExistingRoad = aLink.road;
                    }
                }
            }

            if (toTile.potentialRoads != null)
            {
                foreach (var aLink in toTile.potentialRoads)
                {
                    if ((aLink.neighbor == fromTile_int) & IsRoadBetter(aLink.road, bestExistingRoad))
                    {
                        bestExistingRoad = aLink.road;
                    }
                }
            }
        }
        catch (Exception e)
        {
            DebugLog(null, e);
        }

        return bestExistingRoad;
    }
}