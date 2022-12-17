using System;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionHead : PawnLayerOption {
    public override string Label {
        get => HeadType.defName;
        set => throw new NotImplementedException();
    }

    public HeadTypeDef HeadType {
        get;
        set;
    }
}
