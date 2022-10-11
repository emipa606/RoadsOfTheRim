using System;
using RimWorld.Planet;
using Verse;

namespace RoadsOfTheRim;

public class CaravanArrivalAction_StartWorkingOnRoad : CaravanArrivalAction
{
    public override string Label => "Start working, you lazy bastards";

    public override string ReportString => "Work for your rich masters";

    public override void Arrived(Caravan caravan)
    {
        try
        {
            var CaravanComp = caravan.GetComponent<WorldObjectComp_Caravan>();
            CaravanComp.StartWorking();
        }
        catch (Exception e)
        {
            Log.Error($"[Roads of the Rim] : Exception upon caravan arrival = {e}");
        }
    }
}