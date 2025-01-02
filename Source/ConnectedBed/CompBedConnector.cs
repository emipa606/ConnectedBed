using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using VNPE;

namespace zed_0xff.VNPE;

// code partially copied from Vanilla Nutrient Paste Expanded (c) Oskar Potocki
// https://steamcommunity.com/sharedfiles/filedetails/?id=2920385763

public class CompBedConnector : ThingComp
{
    private CompResource hemogenComp;
    private CompResource pasteComp;
    private CompPowerTrader powerComp;
    private CompProperties_BedConnector Props => (CompProperties_BedConnector)props;

    private IEnumerable<Pawn> CurOccupants
    {
        get
        {
            if (parent is Building_Bed bed)
            {
                // I'm a part of connected bed
                foreach (var pawn in bed.CurOccupants)
                {
                    yield return pawn;
                }
            }
            else
            {
                // I'm a separate building, built over a bed or smth
                foreach (var t in parent.Map.thingGrid.ThingsListAt(parent.Position))
                {
                    switch (t)
                    {
                        case Pawn { Dead: false } p when p.GetPosture().Laying():
                            yield return p;
                            break;
                        case Building_Enterable { SelectedPawn: { } p2 } be when p2.ParentHolder == be:
                            yield return p2;
                            break;
                    }
                }
            }
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (parent is MinifiedThing or not { Spawned: true })
        {
            return;
        }

        powerComp = parent.GetComp<CompPowerTrader>();

        foreach (var comp in parent.GetComps<CompResource>())
        {
            if (comp?.Props?.pipeNet?.defName == "VRE_HemogenNet")
            {
                hemogenComp = comp;
            }

            if (comp?.Props?.pipeNet?.defName == "VNPE_NutrientPasteNet")
            {
                pasteComp = comp;
            }
        }
    }

    public override void PostDeSpawn(Map map)
    {
        powerComp = null;
        pasteComp = null;
        hemogenComp = null;
    }

    private void FeedOccupant()
    {
        var net = pasteComp.PipeNet;
        if (net.Stored < 1)
        {
            return;
        }

        var occupants = CurOccupants.ToList();
        foreach (var pawn in occupants)
        {
            var foodNeed = pawn.needs.TryGetNeed<Need_Food>();
            if (foodNeed == null || foodNeed.CurLevelPercentage > 0.4)
            {
                continue;
            }

            net.DrawAmongStorage(1, net.storages);
            var meal = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
            if (meal.TryGetComp<CompIngredients>() is { } ingredients)
            {
                foreach (var storage in net.storages)
                {
                    var storageParent = storage.parent;
                    if (storageParent.TryGetComp<CompRegisterIngredients>() is not { } storageIngredients)
                    {
                        continue;
                    }

                    foreach (var thingDef in storageIngredients.ingredients)
                    {
                        ingredients.RegisterIngredient(thingDef);
                    }
                }
            }

            var ingestedNum = meal.Ingested(pawn, foodNeed.NutritionWanted);
            foodNeed.CurLevel += ingestedNum;
            pawn.records.AddTo(RecordDefOf.NutritionEaten, ingestedNum);
        }
    }

    private ref ConnectedBedSettings.TypeSettings GetSettingForPawn(Pawn pawn)
    {
        if (pawn.IsPrisonerOfColony)
        {
            return ref ModConfig.Settings.prisoners;
        }

        if (pawn.IsSlaveOfColony)
        {
            return ref ModConfig.Settings.slaves;
        }

        if (pawn.IsColonist)
        {
            return ref ModConfig.Settings.colonists;
        }

        return ref ModConfig.Settings.others;
    }

    private void TransfuseBlood()
    {
        var net = hemogenComp.PipeNet;
        if (net.Stored < 1)
        {
            return;
        }

        var stored = net.Stored;

        var occupants = CurOccupants.ToList();
        foreach (var pawn in occupants)
        {
            var bloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLossHediff == null)
            {
                continue;
            }

            var ts = GetSettingForPawn(pawn);

            if (bloodLossHediff.Severity < 1.0f - ts.transfuseIfLess)
            {
                continue;
            }

            while (bloodLossHediff.Severity > 1.0f - ts.fillUpTo && stored >= 1)
            {
                if (ModConfig.Settings.general.debugLog)
                {
                    Log.Message(
                        $"[d] ConnectedBed: transfuse {pawn} {ts.fillUpTo}");
                }

                net.DrawAmongStorage(1, net.storages);
                bloodLossHediff.Severity -= 0.35f; // see RimWorld/Recipe_BloodTransfusion.cs
                if (pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null)
                {
                    GeneUtility.OffsetHemogen(pawn, JobGiver_GetHemogen.HemogenPackHemogenGain);
                }

                stored -= 1;
            }

            if (stored < 1)
            {
                break;
            }
        }
    }

    private void DrawBlood()
    {
        if (!ModConfig.Settings.prisoners.draw)
        {
            return;
        }

        var net = hemogenComp.PipeNet;

        var occupants = CurOccupants.ToList();
        foreach (var pawn in occupants)
        {
            if (!pawn.IsPrisonerOfColony)
            {
                continue;
            }

            if (pawn.guest.ExclusiveInteractionMode != PrisonerInteractionModeDefOf.HemogenFarm)
            {
                continue;
            }

            if (!RecipeDefOf.ExtractHemogenPack.Worker.AvailableOnNow(pawn))
            {
                continue;
            }

            if (pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
            {
                continue;
            }

            // should be created by Pawn_GuestTracker::GuestTrackerTick
            if (!pawn.BillStack.Bills.Any(x => x.recipe == RecipeDefOf.ExtractHemogenPack))
            {
                continue;
            }

            var cap = net.AvailableCapacity;
            if (cap > 1)
            {
                var fillRate = net.Stored / (net.Stored + cap);
                if (fillRate >= ModConfig.Settings.general.maxFillRate)
                {
                    return;
                }

                if (ModConfig.Settings.general.debugLog)
                {
                    Log.Message($"[d] ConnectedBed: draw {pawn}");
                }

                var hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                hediff.Severity = 0.59f; // 0.6 pops up unwanted health alert
                pawn.health.AddHediff(hediff);
                net.DistributeAmongStorage(1, out _);
                if (IsViolationOnPawn(pawn, Faction.OfPlayer))
                {
                    ReportViolation(pawn, pawn.HomeFaction, -1, HistoryEventDefOf.ExtractedHemogenPack);
                }
            }

            if (!net.storages.Any())
            {
                continue;
            }

            // remove pawn's all ExtractHemogenPack bills, if any
            var bills = pawn.BillStack.Bills.Where(b => b.recipe == RecipeDefOf.ExtractHemogenPack).ToList();
            foreach (var b in bills)
            {
                pawn.BillStack.Delete(b);
            }
        }
    }

    private bool IsViolationOnPawn(Pawn pawn, Faction billDoerFaction)
    {
        return pawn.Faction != billDoerFaction || pawn.IsQuestLodger();
    }

    private void ReportViolation(Pawn pawn, Faction factionToInform, int goodwillImpact,
        HistoryEventDef overrideEventDef = null)
    {
        if (factionToInform == null)
        {
            return;
        }

        Faction.OfPlayer.TryAffectGoodwillWith(factionToInform, goodwillImpact, true, !factionToInform.temporary,
            overrideEventDef ?? HistoryEventDefOf.PerformedHarmfulSurgery);
        QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
    }

    public override void CompTickRare()
    {
        if (!powerComp.PowerOn)
        {
            return;
        }

        if (pasteComp != null)
        {
            FeedOccupant();
        }

        if (hemogenComp != null)
        {
            TransfuseBlood();
            DrawBlood();
        }

        if (ModConfig.DBH_Loaded)
        {
            foreach (var t in CurOccupants.ToList())
            {
                if (t != null)
                {
                    DBH.ProcessPawn(t, parent);
                }
            }
        }

        base.CompTickRare();
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Props.graphicData == null)
        {
            return;
        }

        var mesh = Props.graphicData.Graphic.MeshAt(parent.Rotation);
        var quat = Quaternion.Euler(Vector3.up * parent.Rotation.AsAngle);
        var loc = parent.DrawPos;
        loc += Props.graphicData.Graphic.DrawOffset(parent.Rotation);
        var mat = Props.graphicData.Graphic.MatAt(parent.Rotation);
        var matrix = Matrix4x4.TRS(loc, quat, new Vector3(1, 1, 1));
        Graphics.DrawMesh(mesh, matrix, mat, 0);
    }

    public override string CompInspectStringExtra()
    {
        if (parent is MinifiedThing || parent is null || !parent.Spawned)
        {
            return null;
        }

        return "VNPE.Occupants".Translate(CurOccupants.Count());
    }
}