using RimWorld;
using Verse;

namespace zed_0xff.VNPE;

[StaticConstructorOnStartup]
public class Init
{
    static Init()
    {
        var connectedBed = VThingDefOf.VNPE_ConnectedBed;

        if (connectedBed?.GetCompProperties<CompProperties_AffectedByFacilities>() is not { } connectedProps)
        {
            return;
        }

        if (!ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded"))
        {
            return;
        }

        // Unlink dripper from ConnectedBed
        var dropper = DefDatabase<ThingDef>.GetNamed("VNPE_NutrientPasteDripper");
        if (connectedProps.linkableFacilities.Contains(dropper))
        {
            connectedProps.linkableFacilities.Remove(dropper);
        }
    }
}