using System;
using RimWorld;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionTattoo : PawnLayerOption {
    public override string Label {
        get => TattooDef.LabelCap;
        set => throw new NotImplementedException();
    }

    public TattooDef TattooDef {
        get;
        set;
    }
}
