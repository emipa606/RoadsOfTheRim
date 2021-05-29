using System.Collections.Generic;
using Verse;

namespace RoadsOfTheRim
{
    /*
    This GenStep should be called at the very end when generating a map
    It aims at pushing away things that spawned on recently built roads and have no place there : Rocks, walls, ruins...
    This should make the roads much more believable
     */
    public class GenStep_CleanBuiltRoads : GenStep
    {
        public override int SeedPart => 314159265;

        public override void Generate(Map map, GenStepParams parms)
        {
            //RoadsOfTheRim.DebugLog("Cleaning up roads if I can");
            var terrainGrid = map.terrainGrid;
            foreach (var current in map.AllCells)
            {
                var thingList = current.GetThingList(map);
                var terrainDefHere = terrainGrid.TerrainAt(current);
                if (!IsBuiltRoad(terrainDefHere))
                {
                    continue;
                }

                map.roofGrid.SetRoof(current, null); // remove any roof
                if (map.fogGrid.IsFogged(current))
                {
                    map.fogGrid.Unfog(current); // no fog on road
                }

                if (thingList.Count > 0)
                {
                    //RoadsOfTheRim.DebugLog("Placed " + thingList.Count + " things on top of " + terrainDefHere.label);
                    MoveThings(map, current);
                }

                /*
                 * Quick and dirty hack because classes in the Bridge.cs file do not handle all cases properly. Terrain needs to be set again over water & MarshyTerrain as below.
                 */
                if (map.terrainGrid.UnderTerrainAt(current).IsWater)
                {
                    if (terrainDefHere == TerrainDefOf.GlitterRoad)
                    {
                        map.terrainGrid.SetTerrain(current, TerrainDefOf.GlitterRoad);
                    }

                    if (terrainDefHere == TerrainDefOf.AsphaltRecent)
                    {
                        map.terrainGrid.SetTerrain(current, TerrainDefOf.ConcreteBridge);
                    }

                    if (terrainDefHere == TerrainDefOf.StoneRecent)
                    {
                        map.terrainGrid.SetTerrain(current, TerrainDefOf.ConcreteBridge);
                    }
                }

                if (map.terrainGrid.UnderTerrainAt(current) != TerrainDefOf.MarshyTerrain)
                {
                    continue;
                }

                if (terrainDefHere == TerrainDefOf.GlitterRoad)
                {
                    map.terrainGrid.SetTerrain(current, TerrainDefOf.GlitterRoad);
                }

                if (terrainDefHere == TerrainDefOf.AsphaltRecent)
                {
                    map.terrainGrid.SetTerrain(current, TerrainDefOf.AsphaltRecent);
                }

                if (terrainDefHere == TerrainDefOf.StoneRecent)
                {
                    map.terrainGrid.SetTerrain(current, TerrainDefOf.StoneRecent);
                }
            }
        }

        private static bool IsBuiltRoad(TerrainDef def)
        {
            return RoadsOfTheRim.builtRoadTerrains.Contains(def);
        }

        /*
        Moves all things in a cell to the closest cell that is empty and not a built road
         */
        private static void MoveThings(Map map, IntVec3 cell)
        {
            var thingList = cell.GetThingList(map);
            var terrainGrid = map.terrainGrid;
            //thingList.RemoveAll(item => item !=null);
            foreach (var thingToMove in thingList) // Go through all things on that cell
            {
                //RoadsOfTheRim.DebugLog("Trying to move " + thingToMove.Label);
                var cellChecked = new List<IntVec3>
                {
                    cell
                };
                var goodCellFound = false;
                while (!goodCellFound) // Keep doing this as long as I haven't found a good cell (empty, and not a road)
                {
                    var newCells = cellChecked;
                    ExpandNeighbouringCells(ref newCells, map);
                    foreach (var c in newCells)
                    {
                        var terrainDefHere = terrainGrid.TerrainAt(c);
                        var thingList2 = c.GetThingList(map);
                        if (IsBuiltRoad(terrainDefHere) || thingList2.Count != 0)
                        {
                            continue;
                        }

                        //RoadsOfTheRim.DebugLog("Moved "+thingToMove.Label);
                        thingToMove.SetPositionDirect(c);
                        goodCellFound = true;
                        break;
                    }

                    if (newCells.Count <= cellChecked.Count) // break out of the loop if we couldn't find any new cells
                    {
                        break;
                    }

                    cellChecked = newCells;
                }
            }
        }

        private static void ExpandNeighbouringCells(ref List<IntVec3> cells, Map map)
        {
            var expandedCells = new List<IntVec3>();
            foreach (var c in cells)
            {
                if (!expandedCells.Contains(c) && !cells.Contains(c)) // Add the current cell
                {
                    expandedCells.Add(c);
                }

                foreach (var c2 in GenAdjFast.AdjacentCells8Way(c)) // Add all the current cell's neighbours
                {
                    if (!expandedCells.Contains(c2) && !cells.Contains(c2) && c.InBounds(map))
                    {
                        expandedCells.Add(c2);
                    }
                }
            }

            cells = expandedCells;
        }
    }
}