using Verse;

namespace RoadsOfTheRim;

public class RoadsOfTheRimSettings : ModSettings
{
    // Constants
    public const int MinBaseEffort = 1;
    public const int DefaultBaseEffort = 10;
    public const int MaxBaseEffort = 10;
    public const float ElevationCostDouble = 2000f;
    public const float HillinessCostDouble = 4f;
    public const float SwampinessCostDouble = 0.5f;
    public int BaseEffort = DefaultBaseEffort;
    public float CostIncreaseElevationThreshold = 1000f;
    public int CostUpgradeRebate = 30;
    public bool OverrideCosts = true;
    public bool useISR2G = true;

    public override void ExposeData()
    {
        base.ExposeData();
        // Costs are always 100% when using ISR2G
        if (useISR2G)
        {
            BaseEffort = MaxBaseEffort;
        }

        Scribe_Values.Look(ref BaseEffort, "BaseEffort", DefaultBaseEffort, true);
        Scribe_Values.Look(ref OverrideCosts, "OverrideCosts", true, true);
        Scribe_Values.Look(ref CostIncreaseElevationThreshold, "CostIncreaseElevationThreshold", 1000f, true);
        Scribe_Values.Look(ref CostUpgradeRebate, "CostUpgradeRebate", 30, true);
        Scribe_Values.Look(ref useISR2G, "useISR2G", true, true);
    }
}