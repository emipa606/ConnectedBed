using RimWorld;
using Verse;

namespace zed_0xff.VNPE;

public class PlaceWorker_BedConnector : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        // LoftBed is not an Edifice!

        var wasBed = false;
        foreach (var thingHere in map.thingGrid.ThingsListAtFast(loc))
        {
            if (thingHere == thingToIgnore)
            {
                continue;
            }

            if (thingHere is Building_Bed or Building_Enterable)
            {
                wasBed = true;
            }

            if (thingHere.def.defName.Contains("BedConnector") || thingHere.def.defName == "VNPE_ConnectedBed")
            {
                return "IdenticalThingExists".Translate();
            }

            if (!(thingHere.def.entityDefToBuild?.defName?.Contains("BedConnector") ?? false))
            {
                continue;
            }

            return thingHere is Blueprint
                ? new AcceptanceReport("IdenticalBlueprintExists".Translate())
                : new AcceptanceReport("IdenticalThingExists".Translate());
        }

        if (!wasBed)
        {
            return "VNPE.MustOverBed".Translate();
        }

        return true;
    }
}