using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class InjurySeverity {
    public static readonly List<InjurySeverity> PermanentInjurySeverities = new() {
        new InjurySeverity(2),
        new InjurySeverity(3),
        new InjurySeverity(4),
        new InjurySeverity(5),
        new InjurySeverity(6)
    };

    protected HediffStage stage;
    protected float value;
    protected int? variant;

    public InjurySeverity(float value) {
        this.value = value;
    }

    public InjurySeverity(float value, HediffStage stage) {
        this.value = value;
        this.stage = stage;
    }

    public float Value => value;

    public HediffStage Stage => stage;

    public int? Variant {
        get => variant;
        set => variant = value;
    }

    public bool SeverityRepresentsLevel {
        get;
        set;
    }

    public string Label {
        get {
            if (stage != null) {
                if (SeverityRepresentsLevel) {
                    return "Level".Translate().CapitalizeFirst() + " " + (int)stage.minSeverity;
                }

                if (variant == null) {
                    return stage.label.CapitalizeFirst();
                }

                return "EdB.PC.Dialog.Severity.Stage.Label.".Translate(stage.label.CapitalizeFirst(), variant.Value);
            }

            return ("EdB.PC.Dialog.Severity.OldInjury.Label." + value).Translate();
        }
    }
}
