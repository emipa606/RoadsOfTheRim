using Verse;

namespace RoadsOfTheRim;

// I had to take into account the old defs of ISR2G that used to be buildings, and replace them with new ISR2G defs that are craftable items
// So I use this comp and add it to the old Defs
public class OldDefsCleanupComp : ThingComp
{
    public override void CompTick()
    {
        Thing oldISR2G = parent;
        var level = 0;
        switch (oldISR2G.def.defName)
        {
            case "RotR_ISR2G":
                level = 1;
                break;
            case "RotR_AISR2G":
                level = 2;
                break;
        }

        if (level <= 0)
        {
            return;
        }

        var newThingDefName = level == 1 ? "RotR_ISR2GNew" : "RotR_AISR2GNew";
        var newThing = ThingMaker.MakeThing(ThingDef.Named(newThingDefName));
        var position = oldISR2G.Position;
        var map = oldISR2G.MapHeld;
        RoadsOfTheRim.DebugLog($"Replacing a ISR2G level {level} at position {position}");
        oldISR2G.Destroy();
        GenPlace.TryPlaceThing(newThing, position, map, ThingPlaceMode.Near);
    }
}