using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelWorldPawnList : PanelPawnList {
    private readonly List<CustomPawn> pawns = new();

    public override string PanelHeader => "EdB.PC.Panel.WorldPawnList.Title".Translate();

    protected override bool StartingPawns => false;

    protected override bool CanDeleteLastPawn => true;

    protected override bool IsMaximized(State state) {
        return state.PawnListMode != PawnListMode.ColonyPawnsMaximized;
    }

    protected override bool IsMinimized(State state) {
        return state.PawnListMode == PawnListMode.ColonyPawnsMaximized;
    }

    protected override List<CustomPawn> GetPawnListFromState(State state) {
        // Re-use the same list instead of instantiating a new one every frame.
        pawns.Clear();
        pawns.AddRange(PrepareCarefully.Instance.WorldPawns);
        return pawns;
    }

    protected override bool IsTopPanel() {
        return false;
    }
}
