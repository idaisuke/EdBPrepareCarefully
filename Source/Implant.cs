using System.Text;
using UnityEngine;
using Verse;
using Object = System.Object;

namespace EdB.PrepareCarefully;

public class Implant : CustomBodyPart {
    protected BodyPartRecord bodyPartRecord;
    protected Hediff hediff;

    public string label = "";
    public RecipeDef recipe;

    protected string tooltip;

    public Implant() {
    }

    public Implant(BodyPartRecord bodyPartRecord, RecipeDef recipe) {
        BodyPartRecord = bodyPartRecord;
        this.recipe = recipe;
    }

    public override BodyPartRecord BodyPartRecord {
        get => bodyPartRecord;
        set {
            bodyPartRecord = value;
            tooltip = null;
        }
    }

    public Hediff Hediff {
        get => hediff;
        set => hediff = value;
    }

    public override string ChangeName => Label;

    public override Color LabelColor {
        get {
            if (recipe.addsHediff != null) {
                return recipe.addsHediff.defaultLabelColor;
            }

            return Style.ColorText;
        }
    }

    public RecipeDef Recipe {
        get => recipe;
        set {
            recipe = value;
            tooltip = null;
        }
    }

    public string Label {
        get {
            if (recipe == null) {
                return "";
            }

            return recipe.addsHediff.LabelCap;
        }
    }

    public bool ReplacesPart {
        get {
            if (Recipe != null && Recipe.addsHediff != null
                               && (typeof(Hediff_AddedPart).IsAssignableFrom(Recipe.addsHediff.hediffClass)
                                   || typeof(Hediff_MissingPart).IsAssignableFrom(Recipe.addsHediff.hediffClass))) {
                return true;
            }

            return false;
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

    public override bool Equals(Object obj) {
        if (obj == null) {
            return false;
        }

        var option = obj as Implant;
        if ((Object)option == null) {
            return false;
        }

        return BodyPartRecord == option.BodyPartRecord && recipe == option.recipe;
    }

    public bool Equals(Implant option) {
        if (option == null) {
            return false;
        }

        return BodyPartRecord == option.BodyPartRecord && recipe == option.recipe;
    }

    public override int GetHashCode() {
        unchecked {
            var a = BodyPartRecord != null ? BodyPartRecord.GetHashCode() : 0;
            var b = recipe != null ? recipe.GetHashCode() : 0;
            return (31 * a) + b;
        }
    }

    public override void AddToPawn(CustomPawn customPawn, Pawn pawn) {
        if (recipe != null && BodyPartRecord != null) {
            hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn, BodyPartRecord);
            pawn.health.AddHediff(hediff, BodyPartRecord);
        }
    }

    protected void InitializeTooltip() {
        var stringBuilder = new StringBuilder();
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
