using RimWorld.Planet;

namespace RoadsOfTheRim;

public class WorldComponent_RoadBuildingState(World world) : WorldComponent(world)
{
    public RoadConstructionSite CurrentlyTargeting { get; set; }

    public Caravan Caravan { get; set; }
}