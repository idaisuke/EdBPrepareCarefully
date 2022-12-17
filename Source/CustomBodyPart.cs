using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public abstract class CustomBodyPart {
    public abstract BodyPartRecord BodyPartRecord {
        get;
        set;
    }

    public virtual string PartName => BodyPartRecord != null
        ? BodyPartRecord.LabelCap
        : "EdB.PC.BodyParts.WholeBody".Translate().Resolve();

    public abstract string ChangeName {
        get;
    }

    public abstract Color LabelColor {
        get;
    }

    public virtual bool HasTooltip => false;

    public abstract string Tooltip {
        get;
    }

    public abstract void AddToPawn(CustomPawn customPawn, Pawn pawn);
}
