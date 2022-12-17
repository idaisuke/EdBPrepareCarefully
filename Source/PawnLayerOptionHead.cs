using System;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionHead : PawnLayerOption {
    public override string Label {
        get => HeadType.Label;
        set => throw new NotImplementedException();
    }

    public CustomHeadType HeadType {
        get;
        set;
    }
}
