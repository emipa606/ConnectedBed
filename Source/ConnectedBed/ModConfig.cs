using Mlie;
using UnityEngine;
using Verse;

namespace zed_0xff.VNPE;

public class ModConfig : Mod
{
    public static bool DBH_Loaded;

    private static Vector2 scrollPosition = Vector2.zero;
    private static string currentVersion;

    public ModConfig(ModContentPack content) : base(content)
    {
        Settings = GetSettings<ConnectedBedSettings>();

        if (ModLister.HasActiveModWithName("Dubs Bad Hygiene"))
        {
            DBH_Loaded = true;
            Log.Message("[ConnectedBed]: Adding support for Dubs Bad Hygiene");
        }

        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public static ConnectedBedSettings Settings { get; private set; }


    private void drawBlock(Listing_Standard l, string title, ref ConnectedBedSettings.GeneralSettings s)
    {
        l.Label(title);
        l.GapLine();

        l.Label("VNPE.StorageSetting".Translate(s.maxFillRate.ToStringPercent()));
        s.maxFillRate = l.Slider(s.maxFillRate, 0.1f, 1.0f);

        l.Gap();
    }

    private void drawBlock(Listing_Standard l, string title, ref ConnectedBedSettings.TypeSettings s)
    {
        l.Label(title);
        l.GapLine();

        if (title == "Prisoners")
        {
            l.CheckboxLabeled("VNPE.AutoBlood".Translate(), ref s.draw);
        }

        l.CheckboxLabeled("VNPE.Transfuse".Translate(), ref s.transfuse);

        l.Label("VNPE.TransfuseSetting".Translate(s.transfuseIfLess.ToStringPercent()));
        s.transfuseIfLess = l.Slider(s.transfuseIfLess, 0.1f, 0.99f);

        l.Label("VNPE.FillUp".Translate(s.fillUpTo.ToStringPercent()));
        s.fillUpTo = l.Slider(s.fillUpTo, 0.4f, 1.0f);

        l.Gap();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var l = new Listing_Standard();

        var scrollContainer = inRect.ContractedBy(10);
        scrollContainer.height -= l.CurHeight;
        scrollContainer.y += l.CurHeight;
        Widgets.DrawBoxSolid(scrollContainer, Color.grey);
        var innerContainer = scrollContainer.ContractedBy(1);
        Widgets.DrawBoxSolid(innerContainer, new ColorInt(42, 43, 44).ToColor);
        var frameRect = innerContainer.ContractedBy(5);
        frameRect.y += 15;
        frameRect.height -= 15;
        var contentRect = frameRect;
        contentRect.x = 0;
        contentRect.y = 0;
        contentRect.width -= 20;
        contentRect.height = 950f;

        Widgets.BeginScrollView(frameRect, ref scrollPosition, contentRect);
        l.Begin(contentRect.AtZero());

        drawBlock(l, "General", ref Settings.general);
        drawBlock(l, "Prisoners", ref Settings.prisoners);
        drawBlock(l, "Colonists", ref Settings.colonists);
        drawBlock(l, "Slaves", ref Settings.slaves);
        drawBlock(l, "Others", ref Settings.others);

        l.Label("VNPE.HemogenInfo".Translate());
        l.Label("VNPE.FarmInfo".Translate());
        if (currentVersion != null)
        {
            l.Gap();
            GUI.contentColor = Color.gray;
            l.Label("VNPE.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        l.End();
        Widgets.EndScrollView();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Connected Bed";
    }
}