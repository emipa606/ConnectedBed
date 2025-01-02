using DubsBadHygiene;
using Verse;

namespace zed_0xff.VNPE;

public class DBH
{
    private const float waterPerDay = 2.7f;

    public static void ProcessPawn(Pawn pawn, ThingWithComps parent)
    {
        var pipeComp = parent.TryGetComp<CompPipe>();
        if (pipeComp == null)
        {
            return;
        }

        checkThirst(pawn, pipeComp);
        checkBladder(pawn, pipeComp);
    }

    private static void checkBladder(Pawn pawn, CompPipe pipeComp)
    {
        var bladder = pawn.needs.TryGetNeed<Need_Bladder>();
        if (bladder == null || bladder.CurLevelPercentage >= 0.5)
        {
            return;
        }

        var urineAmount = (1.0f - bladder.CurLevelPercentage) * waterPerDay;
        if (pipeComp.pipeNet.PushSewage(urineAmount * 2))
        {
            bladder.dump();
            bladder.CurLevel = 1;
        }

        pipeComp.pipeNet.PullWater(urineAmount, out _); // flush
    }

    private static void checkThirst(Pawn pawn, CompPipe pipeComp)
    {
        var thirst = pawn.needs.TryGetNeed<Need_Thirst>();
        if (thirst == null || thirst.CurLevelPercentage >= 0.5)
        {
            return;
        }

        if (!pipeComp.pipeNet.PullWater(waterPerDay * thirst.CurLevelPercentage, out var WaterTaint))
        {
            return;
        }

        thirst.Drink(100.0f);
        SanitationUtil.ContaminationCheckDrinking(pawn, WaterTaint);
    }
}