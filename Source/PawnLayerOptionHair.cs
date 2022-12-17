using System;
using RimWorld;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionHair : PawnLayerOption {
    public override string Label {
        get => HairDef.LabelCap;
        set => throw new NotImplementedException();
    }

    public HairDef HairDef {
        get;
        set;
    }
}
