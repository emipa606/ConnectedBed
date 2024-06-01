using RimWorld;
using Verse;

namespace zed_0xff.VNPE;

[StaticConstructorOnStartup]
public class Init
{
    static Init()
    {
        var connected_bed = VThingDefOf.VNPE_ConnectedBed;

        if (connected_bed?.GetCompProperties<CompProperties_AffectedByFacilities>() is not { } connected_props)
        {
            return;
        }

        if (!ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded"))
        {
            return;
        }

        // Unlink dripper from ConnectedBed
        var dripper = DefDatabase<ThingDef>.GetNamed("VNPE_NutrientPasteDripper");
        if (connected_props.linkableFacilities.Contains(dripper))
        {
            connected_props.linkableFacilities.Remove(dripper);
        }
    }
}