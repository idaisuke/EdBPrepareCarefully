using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class Injury : CustomBodyPart {
    protected BodyPartRecord bodyPartRecord;

    protected Hediff hediff;

    protected InjuryOption option;

    protected float? painFactor;

    protected float severity;

    protected string stageLabel;

    protected string tooltip;

    public InjuryOption Option {
        get => option;
        set {
            option = value;
            tooltip = null;
            stageLabel = ComputeStageLabel();
        }
    }

    public Hediff Hediff {
        get => hediff;
        set => hediff = value;
    }

    public float Severity {
        get => severity;
        set {
            tooltip = null;
            severity = value;
            stageLabel = ComputeStageLabel();
        }
    }

    public float? PainFactor {
        get => painFactor;
        set => painFactor = value;
    }

    public override BodyPartRecord BodyPartRecord {
        get => bodyPartRecord;
        set => bodyPartRecord = value;
    }

    public override string ChangeName {
        get {
            if (stageLabel != null) {
                return stageLabel;
            }

            if (Option != null) {
                return Option.Label;
            }

            return "?";
        }
    }

    public override Color LabelColor {
        get {
            if (Option != null && Option.HediffDef != null) {
                return Option.IsOldInjury ? Color.gray : Option.HediffDef.defaultLabelColor;
            }

            return Style.ColorText;
        }
    }

    protected HediffStage CurStage => !option.HediffDef.stages.NullOrEmpty<HediffStage>()
        ? option.HediffDef.stages[CurStageIndex]
        : null;

    protected int CurStageIndex {
        get {
            if (option.HediffDef.stages == null) {
                return 0;
            }

            List<HediffStage> stages = option.HediffDef.stages;
            for (var i = stages.Count - 1; i >= 0; i--) {
                if (Severity >= stages[i].minSeverity) {
                    return i;
                }
            }

            return 0;
        }
    }

    public override bool HasTooltip => hediff != null;

    public override string Tooltip {
        get {
            if (tooltip == null) {
                InitializeTooltip();
            }

            return tooltip;
        }
    }

    public override void AddToPawn(CustomPawn customPawn, Pawn pawn) {
        if (Option.Giver != null) {
            //Logger.Debug("Adding injury {" + Option.Label + "} to part {" + BodyPartRecord?.LabelCap + "} using giver {" + Option.Giver.GetType().FullName + "}");
            var hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, BodyPartRecord);
            hediff.Severity = Severity;
            pawn.health.AddHediff(hediff, BodyPartRecord);
            this.hediff = hediff;
        }
        else if (Option.IsOldInjury) {
            var hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn);
            hediff.Severity = Severity;

            var getsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
            if (getsPermanent != null) {
                getsPermanent.IsPermanent = true;
                Reflection.HediffComp_GetsPermanent.SetPainCategory(getsPermanent,
                    PainCategoryForFloat(painFactor == null ? 0 : painFactor.Value));
                //ReflectionUtil.SetNonPublicField(getsPermanent, "painFactor", painFactor == null ? 0 : painFactor.Value);
            }

            pawn.health.AddHediff(hediff, BodyPartRecord);
            this.hediff = hediff;
        }
        else if (Option.HediffDef.defName == "MissingBodyPart") {
            //Logger.Debug("Adding {" + Option.Label + "} to part {" + BodyPartRecord?.LabelCap);
            var hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, BodyPartRecord);
            hediff.Severity = Severity;
            pawn.health.AddHediff(hediff, BodyPartRecord);
            this.hediff = hediff;
        }
        else {
            var hediff = HediffMaker.MakeHediff(Option.HediffDef, pawn, bodyPartRecord);
            hediff.Severity = Severity;
            if (hediff is Hediff_Level hediffLevel) {
                // The default psylink behavior adds a random ability when you gain a level.  We don't want to do that, so we
                // just set the level field directly.  For any other level-based hediff, we call SetLevelTo() in case the behavior
                // defined in that method is required.
                if (hediff is Hediff_Psylink) {
                    hediffLevel.level = (int)Mathf.Clamp(Severity, hediff.def.minSeverity, hediff.def.maxSeverity);
                }
                else {
                    hediffLevel.SetLevelTo((int)Severity);
                }
            }

            pawn.health.AddHediff(hediff);
            this.hediff = hediff;
        }
    }

    // EVERY RELEASE:
    // Check the PainCategory enum to verify that we still only have 4 values and that their int values match the logic here.
    // This method converts a float value into a PainCategory.  It's here because we don't quite remember where that float
    // value comes from and if it contain a value that won't map to one of the PainCategory enum values.
    protected PainCategory PainCategoryForFloat(float value) {
        var intValue = Mathf.FloorToInt(value);
        if (intValue == 2) {
            intValue = 1;
        }
        else if (intValue > 3 && intValue < 6) {
            intValue = 3;
        }
        else if (intValue > 6) {
            intValue = 6;
        }

        return (PainCategory)intValue;
    }

    protected string ComputeStageLabel() {
        if (Option.HasStageLabel) {
            return "EdB.PC.Panel.Health.InjuryLabel.Stage".Translate(option.Label, CurStage.label);
        }

        if (Option.IsOldInjury) {
            return "EdB.PC.Panel.Health.InjuryLabel.Severity".Translate(option.Label, (int)severity);
        }

        return null;
    }

    protected void InitializeTooltip() {
        var stringBuilder = new StringBuilder();
        if (hediff.Part != null) {
            stringBuilder.Append(hediff.Part.def.LabelCap + ": ");
            stringBuilder.Append(" " + hediff.pawn.health.hediffSet.GetPartHealth(hediff.Part) + " / " +
                                 hediff.Part.def.GetMaxHealth(hediff.pawn));
        }
        else {
            stringBuilder.Append("WholeBody".Translate());
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("------------------");
        var hediff_Injury = hediff as Hediff_Injury;
        var damageLabel = hediff.SeverityLabel;
        if (!hediff.Label.NullOrEmpty() || !damageLabel.NullOrEmpty() ||
            !hediff.CapMods.NullOrEmpty<PawnCapacityModifier>()) {
            stringBuilder.Append(hediff.LabelCap);
            if (!damageLabel.NullOrEmpty()) {
                stringBuilder.Append(": " + damageLabel);
            }

            stringBuilder.AppendLine();
            var tipStringExtra = hediff.TipStringExtra;
            if (!tipStringExtra.NullOrEmpty()) {
                stringBuilder.AppendLine(tipStringExtra.TrimEndNewlines().Indented());
            }
        }

        tooltip = stringBuilder.ToString();
    }
}
