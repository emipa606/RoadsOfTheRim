using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RoadsOfTheRim;

public class RoadConstructionLeg : WorldObject
{
    private RoadConstructionLeg next;

    private RoadConstructionLeg previous;
    private RoadConstructionSite site;

    public override Material Material =>
        next == null
            ?
            // This alternate Material : goal flag
            RotR_StaticConstructorOnStartup.ConstructionLegLast_Material
            : base.Material;

    public RoadConstructionLeg Previous
    {
        get => previous;
        set => previous = value;
    }

    public RoadConstructionLeg Next
    {
        get => next;
        set => next = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
        Scribe_References.Look(ref previous, "previous");
        Scribe_References.Look(ref next, "next");
    }

    public RoadConstructionSite GetSite()
    {
        return site;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (stringBuilder.Length != 0)
        {
            stringBuilder.AppendLine();
        }

        if (Next is null)
        {
            stringBuilder.Append("Goal");
        }
        else
        {
            stringBuilder.Append("RoadsOfTheRim_siteInspectString".Translate(GetSite().roadDef.label,
                $"{GetSite().roadDef.movementCostMultiplier:0.0}"));

            var totalCostModifier = 0f;
            stringBuilder.Append(
                WorldObjectComp_ConstructionSite.CostModifersDescription(Tile, Next.Tile, ref totalCostModifier));

            // Show costs
            var SiteComp = GetSite().GetComponent<WorldObjectComp_ConstructionSite>();
            foreach (var resourceName in DefModExtension_RotR_RoadDef.allResourcesAndWork)
            {
                if (SiteComp.GetCost(resourceName) <= 0)
                {
                    continue;
                }

                // The cost modifier doesn't affect some advanced resources, as defined in static DefModExtension_RotR_RoadDef.allResourcesWithoutModifiers
                // TO DO : COuld this be combined with WorldObjectComp_ConstructionSite.setCosts() ? shares a lot in common except rebates. Can we really calcualte rebate on a leg ?
                var costModifierForThisResource =
                    DefModExtension_RotR_RoadDef.allResourcesWithoutModifiers.Contains(resourceName)
                        ? 1
                        : totalCostModifier;
                stringBuilder.AppendLine();
                stringBuilder.Append(
                    $"{SiteComp.GetCost(resourceName) * costModifierForThisResource} {resourceName}");
            }
        }

        return stringBuilder.ToString();
    }


    // Here, test if we picked a tile that's already part of the chain for this construction site (different construction sites can cross each other's paths)
    // Yes -> 
    //      Was it the construction site itself ?
    //      Yes -> We are done creating the site
    //      No ->  delete this leg and all legs after it
    // No -> create a new Leg
    private static bool ActionOnTile(RoadConstructionSite site, int tile)
    {
        if (site.def != DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"))
        {
            Log.Error("[RotR] - The RoadConstructionSite given is somehow wrong");
            return true;
        }

        try
        {
            foreach (var o in Find.WorldObjects.ObjectsAt(tile))
            {
                // Action on the construction site = we're done
                if (o.def == DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite") &&
                    (RoadConstructionSite)o == site)
                {
                    return true;
                }

                // Action on a leg that's part of this chain = we should delete all legs after that & keep targetting
                if (o.def != DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionLeg") ||
                    ((RoadConstructionLeg)o).site != site)
                {
                    continue;
                }

                Remove((RoadConstructionLeg)o);
                Target(site);
                return false;
            }

            // Check whether we clicked on a neighbour
            var neighbouringTiles = new List<int>();
            Find.WorldGrid.GetTileNeighbors(tile, neighbouringTiles);
            // This is not a neighbour : do nothing
            if (!neighbouringTiles.Contains(site.LastLeg.Tile))
            {
                Target(site);
                return false;
            }

            // There can be no ConstructionLeg on a biome that doesn't allow roads
            if (!DefModExtension_RotR_RoadDef.BiomeAllowed(tile, site.roadDef, out var biomeHere))
            {
                Messages.Message(
                    "RoadsOfTheRim_BiomePreventsConstruction".Translate(site.roadDef.label, biomeHere.label),
                    MessageTypeDefOf.RejectInput);
                Target(site);
                return false;
            }

            if (!DefModExtension_RotR_RoadDef.ImpassableAllowed(tile, site.roadDef))
            {
                Messages.Message(
                    "RoadsOfTheRim_BiomePreventsConstruction".Translate(site.roadDef.label,
                        " impassable mountains"), MessageTypeDefOf.RejectInput);
                Target(site);
                return false;
            }

            var newLeg =
                (RoadConstructionLeg)WorldObjectMaker.MakeWorldObject(
                    DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionLeg"));
            newLeg.Tile = tile;
            newLeg.site = site;
            // This is not the first Leg
            if (site.LastLeg.def == DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionLeg"))
            {
                var l = site.LastLeg as RoadConstructionLeg;
                l?.SetNext(newLeg);
                newLeg.previous = l;
            }
            else
            {
                newLeg.previous = null;
            }

            newLeg.SetNext(null);
            Find.WorldObjects.Add(newLeg);
            site.LastLeg = newLeg;
            Target(site);
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"[RotR] Exception : {e}");
            return true;
        }
    }

    public override void Draw()
    {
        base.Draw();
        var worldGrid = Find.WorldGrid;
        var fromPos = worldGrid.GetTileCenter(Tile);
        var toPos = previous != null ? worldGrid.GetTileCenter(previous.Tile) : worldGrid.GetTileCenter(site.Tile);
        var d = 0.05f;
        fromPos += fromPos.normalized * d;
        toPos += toPos.normalized * d;
        GenDraw.DrawWorldLineBetween(fromPos, toPos);
        // Note : I override the material (see above) to display a goal flag if the leg is the last one, and a circle if it's not, so it looks like this :
        // Site---Leg---Leg---Leg---Leg---Goal
    }

    private void SetNext(RoadConstructionLeg nextLeg)
    {
        try
        {
            next = nextLeg;
        }
        catch (Exception e)
        {
            Log.Error($"[RotR] Exception : {e}");
        }
    }

    public static void Target(RoadConstructionSite site)
    {
        // Log.Warning("[RotR] - Target(site)");
        Find.WorldTargeter.BeginTargeting(
            target => ActionOnTile(site, target.Tile),
            true, RotR_StaticConstructorOnStartup.ConstructionLeg_MouseAttachment, false, null,
            delegate { return "RoadsOfTheRim_BuildToHere".Translate(); });
    }

    /*
     * Remove all legs up to and including the one passed in argument      
     */
    public static void Remove(RoadConstructionLeg leg)
    {
        var site = leg.site;
        var CurrentLeg = (RoadConstructionLeg)site.LastLeg;
        while (CurrentLeg != leg.previous)
        {
            if (CurrentLeg.previous != null)
            {
                var PreviousLeg = CurrentLeg.previous;
                PreviousLeg.SetNext(null);
                site.LastLeg = PreviousLeg;
                Find.WorldObjects.Remove(CurrentLeg);
                CurrentLeg = PreviousLeg;
            }
            else
            {
                Find.WorldObjects.Remove(CurrentLeg);
                site.LastLeg = site;
                break;
            }
        }
    }
}