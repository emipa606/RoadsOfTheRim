using RimWorld;
using Verse;

/*
 * This file contains all C# related to placing & removing Concrete bridges
 */
namespace RoadsOfTheRim;

[DefOf]
public static class TerrainDefOf
{
    public static TerrainDef StoneRecent;
    public static TerrainDef AsphaltRecent;
    public static TerrainDef GlitterRoad;
    public static TerrainDef ConcreteBridge;
    public static TerrainDef MarshyTerrain;
    public static TerrainDef Mud;
}

/*
 * Both Concrete bridges, Stone Roads, and Asphalt roads must check the terrain they're placed on and :
 * - Change it (Marsh & marshy soil to be removed when a "good" road was placed
 * - Be placed despite affordance (Concrete bridges on top of normal bridgeable water)    
 */