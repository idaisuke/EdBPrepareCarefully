using System;
using RimWorld;

namespace EdB.PrepareCarefully;

public class PawnLayerOptionBody : PawnLayerOption {
    private BodyTypeDef bodyTypeDef;
    private string label;

    public override string Label {
        get => label;
        set => throw new NotImplementedException();
    }

    public BodyTypeDef BodyTypeDef {
        get => bodyTypeDef;
        set {
            bodyTypeDef = value;
            label = PrepareCarefully.Instance.Providers.BodyTypes.GetBodyTypeLabel(value);
        }
    }
}
