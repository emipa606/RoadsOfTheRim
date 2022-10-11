using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

public class WorldComponent_FactionRoadConstructionHelp : WorldComponent
{
    private const int
        helpCooldownTicks =
            5 * GenDate
                .TicksPerDay; // A faction can only help on a construction site 5 days after it's finished helping on another one

    private const float helpRequestFailChance = 0.1f;

    private const float helpBaseAmount = 600f;

    private const float helpPerTickMedian = 25f;

    private const float helpPerTickVariance = 10f;

    private const float helpPerTickMin = 5f;
    private List<bool> boolList_currentlyHelping = new List<bool>();

    private Dictionary<Faction, int> canHelpAgainAtTick = new Dictionary<Faction, int>();

    private Dictionary<Faction, bool> currentlyHelping = new Dictionary<Faction, bool>();

    // those lists are used for ExposeData() to load & save correctly
    private List<Faction> factionList_canHelpAgainAtTick = new List<Faction>();
    private List<Faction> factionList_currentlyHelping = new List<Faction>();
    private List<int> intList_canHelpAgainAtTick = new List<int>();

    public WorldComponent_FactionRoadConstructionHelp(World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref canHelpAgainAtTick, "RotR_canHelpAgainAtTick", LookMode.Reference,
            LookMode.Value, ref factionList_canHelpAgainAtTick, ref intList_canHelpAgainAtTick);
        Scribe_Collections.Look(ref currentlyHelping, "RotR_currentlyHelping", LookMode.Reference, LookMode.Value,
            ref factionList_currentlyHelping, ref boolList_currentlyHelping);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        if (canHelpAgainAtTick == null)
        {
            canHelpAgainAtTick = new Dictionary<Faction, int>();
        }

        if (currentlyHelping == null)
        {
            currentlyHelping = new Dictionary<Faction, bool>();
        }
    }

    private void SetHelpAgainTick(Faction faction, int tick)
    {
        canHelpAgainAtTick[faction] = tick;
    }

    private int? GetHelpAgainTick(Faction faction)
    {
        if (canHelpAgainAtTick != null && canHelpAgainAtTick.TryGetValue(faction, out var result))
        {
            return result;
        }

        return null;
    }

    public bool GetCurrentlyHelping(Faction faction)
    {
        return currentlyHelping.TryGetValue(faction, out var result) && result;
    }

    private void SetCurrentlyHelping(Faction faction, bool value = true)
    {
        currentlyHelping[faction] = value;
    }

    public void StartHelping(Faction faction, RoadConstructionSite site, Pawn negotiator)
    {
        // Test success or failure of the negotiator, plus amount of help obtained (based on negotiation value & roll)
        var negotiationValue = negotiator.GetStatValue(StatDefOf.NegotiationAbility);
        _ = helpRequestFailChance / negotiationValue;
        var roll = Rand.Value;
        var amountOfHelp = helpBaseAmount * (1 + (negotiationValue * roll * 5));
        //Log.Message(String.Format("[RotR] - Negotiation for road construction help : negotiation value = {0:0.00} , fail chance = {1:P} , roll = {2:0.00} , help = {3:0.00}", negotiationValue , failChance, roll , amountOfHelp));

        // Calculate how long the faction needs to start helping
        var closestSettlement = site.ClosestSettlementOfFaction(faction);
        var tick = Find.TickManager.TicksGame + closestSettlement.distance;

        // Determine amount of help per tick
        var amountPerTick = Math.Max(Rand.Gaussian(helpPerTickMedian, helpPerTickVariance), helpPerTickMin);

        SetCurrentlyHelping(faction);
        site.InitiateFactionHelp(faction, tick, amountOfHelp, amountPerTick);
    }

    public void HelpFinished(Faction faction)
    {
        faction.TryAffectGoodwillWith(Faction.OfPlayer, -10);
        faction.Notify_GoodwillSituationsChanged(Faction.OfPlayer, true,
            "Help with road construction cost 10 goodwill", GlobalTargetInfo.Invalid);
        SetCurrentlyHelping(faction, false);
        SetHelpAgainTick(faction, Find.TickManager.TicksGame + helpCooldownTicks);
    }

    public bool InCooldown(Faction faction)
    {
        var helpAgainTick = GetHelpAgainTick(faction);
        return helpAgainTick != null && !(Find.TickManager.TicksGame >= GetHelpAgainTick(faction));
    }

    public bool IsDeveloppedEnough(Faction faction, DefModExtension_RotR_RoadDef RoadDefModExtension)
    {
        return faction.def.techLevel >= RoadDefModExtension.techlevelToBuild;
    }

    public float DaysBeforeFactionCanHelp(Faction faction)
    {
        try
        {
            var tick = GetHelpAgainTick(faction);
            if (tick == null)
            {
                return 0;
            }

            return (float)(tick - Find.TickManager.TicksGame) / GenDate.TicksPerDay;
        }
        catch
        {
            return 0;
        }
    }
}