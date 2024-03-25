using RimWorld.Planet;

namespace RoadsOfTheRim;

public class SettlementInfo // Convenience class to store Settlements and their distance to the Site
    (Settlement s, int d)
{
    public readonly int distance = d;
    public readonly Settlement settlement = s;
}