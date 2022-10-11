using System;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

public class WorldObjectComp_Caravan : WorldObjectComp
{
    public bool currentlyWorkingOnSite;

    private RoadConstructionSite site;

    // workOnWakeUp must be more than just working when waking up, it must tell the caravan to work as long as the site is not finished
    private bool workOnWakeUp;

    private Caravan GetCaravan()
    {
        return (Caravan)parent;
    }

    private bool IsThereAConstructionSiteHere()
    {
        return Find.WorldObjects.AnyWorldObjectOfDefAt(DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"),
            GetCaravan().Tile);
    }

    public RoadConstructionSite GetSite()
    {
        return site;
    }

    private void SetSiteFromTile()
    {
        try
        {
            site = (RoadConstructionSite)Find.WorldObjects.WorldObjectOfDefAt(
                DefDatabase<WorldObjectDef>.GetNamed("RoadConstructionSite"), GetCaravan().Tile);
        }
        catch (Exception e)
        {
            RoadsOfTheRim.DebugLog("", e);
        }
    }

    private void UnsetSite()
    {
        site = null;
    }

    public CaravanState CaravanCurrentState()
    {
        var caravan = GetCaravan();
        if (caravan.pather.MovingNow)
        {
            return CaravanState.Moving;
        }

        if (caravan.AllOwnersDowned)
        {
            return CaravanState.AllOwnersDowned;
        }

        if (caravan.AllOwnersHaveMentalBreak)
        {
            return CaravanState.AllOwnersHaveMentalBreak;
        }

        return caravan.NightResting ? CaravanState.NightResting : CaravanState.ReadyToWork;
    }

    public override void CompTick()
    {
        OldDefsCleanup();
        if (Find.TickManager.TicksGame % 100 != 0)
        {
            return;
        }

        var caravan = GetCaravan();
        // Wake up the caravan if it's ready to work
        if (workOnWakeUp && CaravanCurrentState() == CaravanState.ReadyToWork)
        {
            workOnWakeUp = false;
            currentlyWorkingOnSite = true;
            Messages.Message("RotR_CaravanWakesUp".Translate(caravan.Label, site.roadDef.label),
                MessageTypeDefOf.NeutralEvent);
        }

        // Do some work & stop working if finished
        // Caravan is working AND there's a site here AND caravan can work AND the site is indeed the same the caravan was working on
        if (currentlyWorkingOnSite & IsThereAConstructionSiteHere() &
            (CaravanCurrentState() == CaravanState.ReadyToWork) && GetCaravan().Tile == GetSite().Tile)
        {
            base.CompTick();
            site.TryToSkipBetterRoads(caravan); // No need to work if there's a better road here
            if (RoadsOfTheRim.DoSomeWork(caravan, GetSite(), out _))
            {
                StopWorking();
                UnsetSite();
            }
        }

        // Site tile and Caravan tile mismatch 
        if (GetSite() != null && GetCaravan().Tile != GetSite().Tile)
        {
            StopWorking();
            UnsetSite();
        }

        // Stop working as soon as the caravan moves, or rests, or is downed
        if (currentlyWorkingOnSite & (CaravanCurrentState() != CaravanState.ReadyToWork))
        {
            StopWorking();
            var stoppedReason = "";
            // More general use of workOnWakeUp : set it to true if the caravan was working on a site but stopped working for any reason listed in CaravanState
            workOnWakeUp = true;
            if (CaravanCurrentState() == CaravanState.AllOwnersDowned)
            {
                stoppedReason = "RotR_EveryoneDown".Translate();
            }

            if (CaravanCurrentState() == CaravanState.AllOwnersHaveMentalBreak)
            {
                stoppedReason = "RotR_EveryoneCrazy".Translate();
            }

            // I decided to remove this (Issue #38) so code should never reach here
            if (CaravanCurrentState() == CaravanState.ImmobilizedByMass)
            {
                stoppedReason = "RotR_TooHeavy".Translate();
            }

            if (CaravanCurrentState() == CaravanState.NightResting)
            {
                stoppedReason = "RotR_RestingAtNight".Translate();
            }

            if (stoppedReason != "")
            {
                Messages.Message("RotR_CaravanStopped".Translate(caravan.Label, site.roadDef.label) + stoppedReason,
                    MessageTypeDefOf.RejectInput);
            }

            // This should not happen ?
            else
            {
                workOnWakeUp = false;
            }
        }

        if (!IsThereAConstructionSiteHere())
        {
            StopWorking();
        }
    }

    //Start working on a Construction Site.
    public void StartWorking()
    {
        if (CaravanCurrentState() == CaravanState.ReadyToWork)
        {
            var caravan = GetCaravan();
            caravan.pather.StopDead();
            SetSiteFromTile();
            currentlyWorkingOnSite = true;
        }
        else
        {
            Log.Warning("[RotR] : Caravan was given the order to start working but can't work.");
        }
    }

    //Stop working on a Construction Site. No need to check anything, just stop
    public void StopWorking()
    {
        currentlyWorkingOnSite = false;
        // TO DO : A quick message (with a reason) would be nice
    }

    /*
    * Amount of work :
    * - Construction speed (0.5 + 0.15 per level) times the construct success chance (0.75 to 1.13 - lvl 8 is 1)
    * - Pack animals help as well (see below)
    */
    public float AmountOfWork(bool verbose = false)
    {
        var pawns = GetCaravan().PawnsListForReading;
        DefModExtension_RotR_RoadDef roadDefModExtension = null;
        try
        {
            roadDefModExtension = site.roadDef.GetModExtension<DefModExtension_RotR_RoadDef>();
        }
        catch
        {
            /* Either there's no site, no roaddef, or no modextension. In any case, not much to do here */
        }

        //site.roadDef.GetModExtension<DefModExtension_RotR_RoadDef>().minConstruction ;
        var totalConstruction = 0f;
        var totalConstructionAboveMinLevel = 0f;
        var animalConstruction = 0f;
        foreach (var pawn in pawns)
        {
            /*
            if (pawn.IsFreeColonist && pawn.health.State == PawnHealthState.Mobile)
            {
                totalConstruction += pawn.GetStatValue(StatDefOf.ConstructionSpeed) * pawn.GetStatValue(StatDefOf.ConstructSuccessChance);

                if (roadDefModExtension!=null && pawn.skills.GetSkill(SkillDefOf.Construction).levelInt >= roadDefModExtension.minConstruction)
                {
                    totalConstructionAboveMinLevel += pawn.GetStatValue(StatDefOf.ConstructionSpeed) * pawn.GetStatValue(StatDefOf.ConstructSuccessChance);
                }
            }
            else if (pawn.RaceProps.packAnimal  && pawn.health.State == PawnHealthState.Mobile)
            {
                animalConstruction += pawn.GetStatValue(StatDefOf.ConstructionSpeed) * pawn.GetStatValue(StatDefOf.ConstructSuccessChance);
            }
            */
            var PawnConstructionValue = PawnBuildingUtility.ConstructionValue(pawn);

            if (PawnBuildingUtility.HealthyColonist(pawn))
            {
                totalConstruction += PawnConstructionValue;

                if (roadDefModExtension != null && PawnBuildingUtility.ConstructionLevel(pawn) >=
                    roadDefModExtension.minConstruction)
                {
                    totalConstructionAboveMinLevel += PawnConstructionValue;
                }

                continue;
            }

            if (PawnBuildingUtility.HealthyPackAnimal(pawn))
            {
                animalConstruction += PawnConstructionValue;
            }
        }

        if (roadDefModExtension != null)
        {
            var ratioOfConstructionAboveMinLevel = totalConstructionAboveMinLevel / totalConstruction;
            if (ratioOfConstructionAboveMinLevel < roadDefModExtension.percentageOfminConstruction)
            {
                // Check minimum construction level requirements if needed
                var ratioActuallyWorked = ratioOfConstructionAboveMinLevel /
                                          roadDefModExtension.percentageOfminConstruction;
                totalConstruction *= ratioActuallyWorked;
                if (verbose)
                {
                    Messages.Message(
                        "RoadsOfTheRim_InsufficientConstructionMinLevel".Translate(totalConstruction,
                            roadDefModExtension.percentageOfminConstruction.ToString("P0"),
                            roadDefModExtension.minConstruction), MessageTypeDefOf.NegativeEvent);
                }
            }
        }

        // Pack animals can only add as much work as humans (i.e. : at best, pack animals double the amount of work)
        if (animalConstruction > totalConstruction)
        {
            animalConstruction = totalConstruction;
        }

        totalConstruction += animalConstruction;
        return totalConstruction;
    }

    public void TeachPawns(float ratio) // The pawns learn a little construction
    {
        ratio = Math.Max(Math.Min(1, ratio), 0);
        var pawns = GetCaravan().PawnsListForReading;
        //RoadsOfTheRim.DebugLog("Teaching Construction to pawns");
        foreach (var pawn in pawns)
        {
            if (!pawn.IsFreeColonist || pawn.health.State != PawnHealthState.Mobile || pawn.RaceProps.packAnimal)
            {
                continue;
                //RoadsOfTheRim.DebugLog(pawn.Name+" learned " + ratio + " Xp = "+pawn.skills.GetSkill(SkillDefOf.Construction).XpTotalEarned);
            }

            pawn.skills.Learn(SkillDefOf.Construction, 3f * ratio);
        }
    }

    public int UseISR2G()
    {
        var result = 0;
        var settings = LoadedModManager.GetMod<RoadsOfTheRim>().GetSettings<RoadsOfTheRimSettings>();
        // Setting the caravan to use ISR2G or AISR2G if present and settings allow it
        // TO DO : I can do better than hardcode
        if (!settings.useISR2G)
        {
            return result;
        }

        foreach (var aThing in CaravanInventoryUtility.AllInventoryItems(GetCaravan()))
        {
            if (result < 1 && aThing.GetInnerIfMinified().def.defName == "RotR_ISR2GNew")
            {
                result = 1;
            }

            if (aThing.GetInnerIfMinified().def.defName != "RotR_AISR2GNew")
            {
                continue;
            }

            result = 2;
            return result;
        }

        return result;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref currentlyWorkingOnSite, "RoadsOfTheRim_Caravan_currentlyWorkingOnSite", false, true);
        Scribe_Values.Look(ref workOnWakeUp, "RoadsOfTheRim_Caravan_workOnWakeUp", false, true);
        Scribe_References.Look(ref site, "RoadsOfTheRim_Caravan_RoadConstructionSite");
    }

    // I had to take into account the old defs of ISR2G that used to be buildings, and replace them with new ISR2G defs that are craftable items
    private void OldDefsCleanup()
    {
        var newISRG2 = 0;
        var newAISRG2 = 0;
        var caravan = GetCaravan();
        foreach (var aThing in CaravanInventoryUtility.AllInventoryItems(caravan))
        {
            switch (aThing.GetInnerIfMinified().def.defName)
            {
                case "RotR_ISR2G":
                    newISRG2++;
                    aThing.Destroy();
                    break;
                case "RotR_AISR2G":
                    newAISRG2++;
                    aThing.Destroy();
                    break;
            }
        }

        for (var i = newISRG2; i > 0; i--)
        {
            var newThing = ThingMaker.MakeThing(ThingDef.Named("RotR_ISR2GNew"));
            CaravanInventoryUtility.GiveThing(caravan, newThing);
            RoadsOfTheRim.DebugLog($"Replacing an ISR2G in caravan {caravan}");
        }

        for (var j = newAISRG2; j > 0; j--)
        {
            var newThing = ThingMaker.MakeThing(ThingDef.Named("RotR_AISR2GNew"));
            CaravanInventoryUtility.GiveThing(caravan, newThing);
            RoadsOfTheRim.DebugLog($"Replacing an AISR2G in caravan {caravan}");
        }
    }
}