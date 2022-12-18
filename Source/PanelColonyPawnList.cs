using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelColonyPawnList : PanelPawnList {
    private readonly List<CustomPawn> pawns = new();

    public override string PanelHeader => "EdB.PC.Panel.ColonyPawnList.Title".Translate();

    protected override bool StartingPawns => true;

    protected override bool CanDeleteLastPawn => false;

    protected override bool IsMaximized(State state) {
        return state.PawnListMode != PawnListMode.WorldPawnsMaximized;
    }

    protected override bool IsMinimized(State state) {
        return state.PawnListMode == PawnListMode.WorldPawnsMaximized;
    }

    protected override List<CustomPawn> GetPawnListFromState(State state) {
        // Re-use the same list instead of instantiating a new one every frame.
        pawns.Clear();
        pawns.AddRange(PrepareCarefully.Instance.ColonyPawns);
        return pawns;
    }

    protected override bool IsTopPanel() {
        return true;
    }
}
