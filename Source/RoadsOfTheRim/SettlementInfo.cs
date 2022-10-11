using RimWorld.Planet;

namespace RoadsOfTheRim;

public class SettlementInfo // Convenience class to store Settlements and their distance to the Site
{
    public int distance;
    public Settlement settlement;

    public SettlementInfo(Settlement s, int d)
    {
        settlement = s;
        distance = d;
    }
}