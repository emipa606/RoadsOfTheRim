using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadsOfTheRim;

[StaticConstructorOnStartup]
public class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmony = new Harmony("Loconeko.Rimworld.RoadsOfTheRim");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        /* How I found the hidden methods :
        var methods = typeof(Tile).GetMethods();
        foreach (var method in methods)
        {
            Log.Message(method.Name);
        }
        */

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

// TO DO : Ideally, this should be a transpiler. But should I bother ? The code below does the job
/*
 * Patching roads so they cancel all or part of the Tile.biome.movementDifficulty and Hilliness
 * The actual rates are stored in static method RoadsOfTheRim.calculateRoadModifier
 */

// All Tiles can now have roads

// When WorldLayer_Paths.AddPathEndPoint calls WaterCovered, it should return 1, not 0.5
/*
 * NOT EVEN SURE THIS IS NECESSARY
[HarmonyPatch(typeof(WorldLayer_Paths))]
[HarmonyPatch("AddPathEndpoint")]
public static class Patch_WorldLayer_Paths_AddPathEndpoint
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        RoadsOfTheRim.DebugLog("TRANSPILING");
        int index = -1;
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            //RoadsOfTheRim.DebugLog("Transpiler operand =" + codes[i].operand.ToStringSafe());
            if (codes[i].operand is float && (float)codes[i].operand == 0.5)
            {
                index = i;
                break;
            }
        }
        if (index != -1)
        {
            codes[index].operand = 1f;
            RoadsOfTheRim.DebugLog("Transpiler found 0.5 in AddPathEndPoint: " + codes[index].ToString());
        }
        return codes.AsEnumerable();
    }
}
*/