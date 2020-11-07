using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim
{
    public class WorldComponent_FactionRoadConstructionHelp : WorldComponent
    {
        public const int helpCooldownTicks = 5 * GenDate.TicksPerDay; // A faction can only help on a construction site 5 days after it's finished helping on another one

        public const float helpRequestFailChance = 0.1f;

        public const float helpBaseAmount = 600f;

        public const float helpPerTickMedian = 25f;

        public const float helpPerTickVariance = 10f;

        public const float helpPerTickMin = 5f;

        private Dictionary<Faction, int> canHelpAgainAtTick = new Dictionary<Faction, int>();

        private Dictionary<Faction, bool> currentlyHelping = new Dictionary<Faction, bool>();

        public WorldComponent_FactionRoadConstructionHelp(World world) : base(world)
        {
        }

        // those lists are used for ExposeData() to load & save correctly
        private List<Faction> factionList_canHelpAgainAtTick = new List<Faction>();
        private List<Faction> factionList_currentlyHelping = new List<Faction>();
        private List<int> intList_canHelpAgainAtTick = new List<int>();
        private List<bool> boolList_currentlyHelping = new List<bool>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref canHelpAgainAtTick, "RotR_canHelpAgainAtTick", LookMode.Reference, LookMode.Value, ref factionList_canHelpAgainAtTick, ref intList_canHelpAgainAtTick);
            Scribe_Collections.Look(ref currentlyHelping, "RotR_currentlyHelping", LookMode.Reference, LookMode.Value, ref factionList_currentlyHelping, ref boolList_currentlyHelping);
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

        public void SetHelpAgainTick(Faction faction, int tick)
        {
            canHelpAgainAtTick[faction] = tick;
        }

        public int? GetHelpAgainTick(Faction faction)
        {
            if (canHelpAgainAtTick != null && canHelpAgainAtTick.TryGetValue(faction, out var result))
            {
                return result;
            }
            return null;
        }

        public bool GetCurrentlyHelping(Faction faction)
        {
            if (currentlyHelping.TryGetValue(faction, out var result))
            {
                return result;
            }
            return false;
        }

        public void SetCurrentlyHelping(Faction faction, bool value = true)
        {
            currentlyHelping[faction] = value;
        }

        public void StartHelping(Faction faction, RoadConstructionSite site, Pawn negotiator)
        {
            // Test success or failure of the negotiator, plus amount of help obtained (based on negotiation value & roll)
            var negotiationValue = negotiator.GetStatValue(StatDefOf.NegotiationAbility, true);
            _ = helpRequestFailChance / negotiationValue;
            var roll = Rand.Value;
            var amountOfHelp = helpBaseAmount * (1 + (negotiationValue * roll * 5));
            //Log.Message(String.Format("[RotR] - Negotiation for road construction help : negotiation value = {0:0.00} , fail chance = {1:P} , roll = {2:0.00} , help = {3:0.00}", negotiationValue , failChance, roll , amountOfHelp));

            // Calculate how long the faction needs to start helping
            SettlementInfo closestSettlement = site.ClosestSettlementOfFaction(faction);
            var tick = Find.TickManager.TicksGame + closestSettlement.distance;

            // Determine amount of help per tick
            var amountPerTick = Math.Max(Rand.Gaussian(helpPerTickMedian, helpPerTickVariance), helpPerTickMin);

            SetCurrentlyHelping(faction);
            site.InitiateFactionHelp(faction, tick, amountOfHelp, amountPerTick);
        }

        public void HelpFinished(Faction faction)
        {
            faction.TryAffectGoodwillWith(Faction.OfPlayer, -10, true, true, "Help with road construction cost 10 goodwill");
            SetCurrentlyHelping(faction, false);
            SetHelpAgainTick(faction, Find.TickManager.TicksGame + helpCooldownTicks);
        }

        public bool InCooldown(Faction faction)
        {
            var helpAgainTick = GetHelpAgainTick(faction);
            if ((helpAgainTick == null) || (Find.TickManager.TicksGame >= GetHelpAgainTick(faction)))
            {
                return false;
            }
            return true;
        }

        public bool IsDeveloppedEnough(Faction faction, DefModExtension_RotR_RoadDef RoadDefModExtension)
        {
            return faction.def.techLevel >= RoadDefModExtension.techlevelToBuild;
        }

        public float DaysBeforeFactionCanHelp(Faction faction)
        {
            int? tick;
            try
            {
                tick = GetHelpAgainTick(faction);
                if (tick == null)
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
            return (float)(GetHelpAgainTick(faction) - Find.TickManager.TicksGame) / GenDate.TicksPerDay;
        }

    }
}