using Verse;

namespace zed_0xff.VNPE;

public class ConnectedBedSettings : ModSettings
{
    public TypeSettings colonists = new TypeSettings(0.9f);

    public GeneralSettings general = new GeneralSettings();
    public TypeSettings others = new TypeSettings();
    public TypeSettings prisoners = new TypeSettings();
    public TypeSettings slaves = new TypeSettings();

    public override void ExposeData()
    {
        general.ExposeData();
        colonists.ExposeData("colonists", 0.9f);
        prisoners.ExposeData("prisoners");
        slaves.ExposeData("slaves");
        others.ExposeData("others");
        base.ExposeData();
    }

    public sealed class TypeSettings(float defaultFillUpTo = 0.5f)
    {
        public bool draw = true;
        public float fillUpTo = defaultFillUpTo;
        public bool transfuse = true;
        public float transfuseIfLess = 0.2f;

        public void ExposeData(string prefix, float defaultFillUpTo = 0.5f)
        {
            Scribe_Values.Look(ref draw, $"{prefix}.draw", true);
            Scribe_Values.Look(ref transfuse, $"{prefix}.transfuse", true);
            Scribe_Values.Look(ref transfuseIfLess, $"{prefix}.transfuseIfLess", 0.2f);
            Scribe_Values.Look(ref fillUpTo, $"{prefix}.fillUpTo", defaultFillUpTo);
        }
    }

    public sealed class GeneralSettings
    {
        public bool debugLog;
        public float maxFillRate = 0.8f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref maxFillRate, "maxFillRate", 0.8f);
            Scribe_Values.Look(ref debugLog, "debugLog");
        }
    }
}