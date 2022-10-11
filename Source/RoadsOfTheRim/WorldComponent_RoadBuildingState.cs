using RimWorld.Planet;

namespace RoadsOfTheRim;

public class WorldComponent_RoadBuildingState : WorldComponent
{
    public WorldComponent_RoadBuildingState(World world) : base(world)
    {
        CurrentlyTargeting = null;
    }

    public RoadConstructionSite CurrentlyTargeting { get; set; }

    public Caravan Caravan { get; set; }
}