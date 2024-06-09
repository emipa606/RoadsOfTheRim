using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim.HarmonyPatches;

[StaticConstructorOnStartup]
public class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmony = new Harmony("Loconeko.Rimworld.RoadsOfTheRim");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        // Initialise the list of terrains that are specific to built roads. Doing it here is hacky, but this is a quick way to use defs after they were loaded
        foreach (var thisDef in DefDatabase<RoadDef>.AllDefs)
        {
            //RoadsOfTheRim.DebugLog("initialising roadDef " + thisDef);
            if (!thisDef.HasModExtension<DefModExtension_RotR_RoadDef>() ||
                !thisDef.GetModExtension<DefModExtension_RotR_RoadDef>().built
               ) // Only add RoadDefs that are buildable, based on DefModExtension_RotR_RoadDef.built
            {
                continue;
            }

            foreach (var aStep in thisDef.roadGenSteps.OfType<RoadDefGenStep_Place>()
                    ) // Only get RoadDefGenStep_Place
            {
                var t = (TerrainDef)aStep.place; // Cast the buildableDef into a TerrainDef
                if (!RoadsOfTheRim.builtRoadTerrains.Contains(t))
                {
                    RoadsOfTheRim.builtRoadTerrains.Add(t);
                }
            }
        }
    }
}