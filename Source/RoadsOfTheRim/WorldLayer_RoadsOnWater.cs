// RimWorld.Planet.WorldLayer_Roads

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RoadsOfTheRim;

public class WorldLayer_RoadsOnWater : WorldLayer_Paths
{
    private readonly ModuleBase roadDisplacementX = new Perlin(1.0, 2.0, 0.5, 3, 78951234, QualityMode.Medium);

    private readonly ModuleBase roadDisplacementY = new Perlin(1.0, 2.0, 0.5, 3, 12357896, QualityMode.Medium);

    private readonly ModuleBase roadDisplacementZ = new Perlin(1.0, 2.0, 0.5, 3, 14753698, QualityMode.Medium);

    public override IEnumerable Regenerate()
    {
        RoadsOfTheRim.DebugLog("Just regenerated Roads on Water");
        foreach (var item in base.Regenerate())
        {
            yield return item;
        }

        var subMesh = GetSubMesh(WorldMaterials.Roads);
        var grid = Find.WorldGrid;
        var roadLayerDefs = DefDatabase<RoadWorldLayerDef>.AllDefs.OrderBy(rwld => rwld.order).ToList();
        var i = 0;
        while (i < grid.TilesCount)
        {
            if (i % 1000 == 0)
            {
                yield return null;
            }

            if (subMesh.verts.Count > 60000)
            {
                subMesh = GetSubMesh(WorldMaterials.Roads);
            }

            var tile = grid[i];
            if (tile.WaterCovered)
            {
                var list = new List<OutputDirection>();
                if (tile.potentialRoads != null)
                {
                    RoadsOfTheRim.DebugLog($"Road on water on tile {i}");
                    var allowSmoothTransition = true;
                    for (var j = 0; j < tile.potentialRoads.Count - 1; j++)
                    {
                        if (tile.potentialRoads[j].road.worldTransitionGroup ==
                            tile.potentialRoads[j + 1].road.worldTransitionGroup)
                        {
                            continue;
                        }

                        allowSmoothTransition = false;
                    }

                    foreach (var roadWorldLayerDef in roadLayerDefs)
                    {
                        var layerWidthPositive = false;
                        list.Clear();
                        for (var l = 0; l < tile.potentialRoads.Count; l++)
                        {
                            var road = tile.potentialRoads[l].road;
                            var layerWidth = road.GetLayerWidth(roadWorldLayerDef);
                            if (layerWidth > 0f)
                            {
                                layerWidthPositive = true;
                            }

                            list.Add(new OutputDirection
                            {
                                neighbor = tile.potentialRoads[l].neighbor,
                                width = layerWidth,
                                distortionFrequency = road.distortionFrequency,
                                distortionIntensity = road.distortionIntensity
                            });
                        }

                        if (layerWidthPositive)
                        {
                            GeneratePaths(subMesh, i, list, roadWorldLayerDef.color, allowSmoothTransition);
                        }
                    }
                }
            }

            var num = i + 1;
            i = num;
        }

        FinalizeMesh(MeshParts.All);
    }

    public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
    {
        var coordinate = inp * distortionFrequency;
        var magnitude = inp.magnitude;
        var a = new Vector3(roadDisplacementX.GetValue(coordinate), roadDisplacementY.GetValue(coordinate),
            roadDisplacementZ.GetValue(coordinate));
        if (a.magnitude > 0.0001)
        {
            var d = ((1f / (1f + Mathf.Exp((0f - a.magnitude) / 1f * 2f)) * 2f) - 1f) * 1f;
            a = a.normalized * d;
        }

        inp = (inp + (a * distortionIntensity)).normalized * magnitude;
        return inp + (inp.normalized * 0.012f);
    }
}