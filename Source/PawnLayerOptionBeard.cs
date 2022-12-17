using System;
using RimWorld;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionBeard : PawnLayerOption {
    public override string Label {
        get => BeardDef.LabelCap;
        set => throw new NotImplementedException();
    }

    public BeardDef BeardDef {
        get;
        set;
    }
}
