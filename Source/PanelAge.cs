using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelAge : PanelBase {
    public delegate void UpdateAgeHandler(int age);

    protected static Rect RectBiologicalAgeLabel;
    protected static Rect RectBiologicalAgeField;
    protected static Rect RectChronologicalAgeLabel;
    protected static Rect RectChronologicalAgeField;

    private readonly WidgetNumberField biologicalField;
    private readonly WidgetNumberField chronologicalField;

    private readonly ProviderAgeLimits providerAgeLimits = PrepareCarefully.Instance.Providers.AgeLimits;

    public PanelAge() {
        biologicalField = new WidgetNumberField {
            DragSlider = new DragSlider(0.4f, 20, 100),
            MinValue = 14,
            MaxValue = 99,
            UpdateAction = value => {
                UpdateBiologicalAge(value);
            }
        };
        chronologicalField = new WidgetNumberField {
            DragSlider = new DragSlider(0.4f, 15, 100),
            MinValue = 14,
            MaxValue = Constraints.AgeChronologicalMax,
            UpdateAction = value => {
                UpdateChronologicalAge(value);
            }
        };
    }

    public event UpdateAgeHandler BiologicalAgeUpdated;
    public event UpdateAgeHandler ChronologicalAgeUpdated;

    protected void UpdateBiologicalAge(int value) {
        BiologicalAgeUpdated(value);
    }

    protected void UpdateChronologicalAge(int value) {
        ChronologicalAgeUpdated(value);
    }

    public override void Resize(Rect rect) {
        base.Resize(rect);

        var available = PanelRect.size - Style.SizePanelPadding;

        float arrowPadding = 1;
        float arrowWidth = Textures.TextureButtonNext.width;
        float bioWidth = 32;
        float chronoWidth = 48;

        var extendedArrowSize = arrowPadding + arrowWidth;
        var extendedFieldSize = extendedArrowSize * 2;

        var usedSpace = (extendedFieldSize * 2) + bioWidth + chronoWidth;

        var availableSpace = PanelRect.width - usedSpace;
        var spacing = availableSpace / 3;

        float idealSpace = 15;
        float extraFieldWidth = 0;
        if (spacing > idealSpace) {
            var extra = (spacing - idealSpace) * 3;
            extraFieldWidth += Mathf.Floor(extra / 2);
            spacing = idealSpace;
        }
        else {
            spacing = Mathf.Floor(spacing);
        }

        float fieldHeight = 28;

        var saveFont = Text.Font;
        Text.Font = GameFont.Tiny;
        var bioLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Biological".Translate());
        var chronoLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Chronological".Translate());
        Text.Font = saveFont;

        var labelHeight = Mathf.Max(bioLabelSize.y, chronoLabelSize.y);
        var contentHeight = labelHeight + fieldHeight;
        var top = PanelRect.HalfHeight() - (contentHeight * 0.5f);
        var fieldTop = top + labelHeight;

        RectBiologicalAgeField =
            new Rect(spacing + extendedArrowSize, fieldTop, bioWidth + extraFieldWidth, fieldHeight);
        RectChronologicalAgeField = new Rect(RectBiologicalAgeField.xMax + extendedArrowSize +
                                             spacing + extendedArrowSize, fieldTop, chronoWidth + extraFieldWidth,
            fieldHeight);

        RectBiologicalAgeLabel = new Rect(RectBiologicalAgeField.MiddleX() - (bioLabelSize.x / 2),
            RectBiologicalAgeField.y - bioLabelSize.y, bioLabelSize.x, bioLabelSize.y);
        RectChronologicalAgeLabel = new Rect(RectChronologicalAgeField.MiddleX() - (chronoLabelSize.x / 2),
            RectChronologicalAgeField.y - chronoLabelSize.y, chronoLabelSize.x, chronoLabelSize.y);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);

        // Update field values.
        var customPawn = state.CurrentPawn;
        var maxAge = providerAgeLimits.MaxAgeForPawn(customPawn.Pawn);
        var minAge = providerAgeLimits.MinAgeForPawn(customPawn.Pawn);
        chronologicalField.MinValue = customPawn.BiologicalAge;
        biologicalField.MinValue = minAge;
        biologicalField.MaxValue = customPawn.ChronologicalAge < maxAge ? customPawn.ChronologicalAge : maxAge;

        // Age labels.
        Text.Font = GameFont.Tiny;
        GUI.color = Style.ColorText;
        Widgets.Label(RectBiologicalAgeLabel, "EdB.PC.Panel.Age.Biological".Translate());
        Widgets.Label(RectChronologicalAgeLabel, "EdB.PC.Panel.Age.Chronological".Translate());
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        // Biological age field.
        var fieldRect = RectBiologicalAgeField;
        biologicalField.Draw(fieldRect, customPawn.BiologicalAge);

        // Chronological age field.
        fieldRect = RectChronologicalAgeField;
        chronologicalField.Draw(fieldRect, customPawn.ChronologicalAge);
    }
}
