using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelAge : PanelBase {
    public delegate void UpdateAgeHandler(int age);

    private static Rect _rectBiologicalAgeLabel;
    private static Rect _rectBiologicalAgeField;
    private static Rect _rectChronologicalAgeLabel;
    private static Rect _rectChronologicalAgeField;

    private readonly WidgetNumberField biologicalField;
    private readonly WidgetNumberField chronologicalField;

    private readonly ProviderAgeLimits providerAgeLimits = PrepareCarefully.Instance.Providers.AgeLimits;

    public PanelAge() {
        biologicalField = new WidgetNumberField {
            DragSlider = new DragSlider(0.4f, 20, 100), MinValue = 14, MaxValue = 99, UpdateAction = UpdateBiologicalAge
        };
        chronologicalField = new WidgetNumberField {
            DragSlider = new DragSlider(0.4f, 15, 100),
            MinValue = 14,
            MaxValue = Constraints.AgeChronologicalMax,
            UpdateAction = UpdateChronologicalAge
        };
    }

    public event UpdateAgeHandler? BiologicalAgeUpdated;
    public event UpdateAgeHandler? ChronologicalAgeUpdated;

    private void UpdateBiologicalAge(int value) {
        BiologicalAgeUpdated?.Invoke(value);
    }

    private void UpdateChronologicalAge(int value) {
        ChronologicalAgeUpdated?.Invoke(value);
    }

    public override void Resize(Rect rect) {
        base.Resize(rect);

        const float arrowPadding = 1;
        float arrowWidth = Textures.TextureButtonNext.width;
        const float bioWidth = 32;
        const float chronoWidth = 48;

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

        const float fieldHeight = 28;

        var saveFont = Text.Font;
        Text.Font = GameFont.Tiny;
        var bioLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Biological".Translate());
        var chronoLabelSize = Text.CalcSize("EdB.PC.Panel.Age.Chronological".Translate());
        Text.Font = saveFont;

        var labelHeight = Mathf.Max(bioLabelSize.y, chronoLabelSize.y);
        var contentHeight = labelHeight + fieldHeight;
        var top = PanelRect.HalfHeight() - (contentHeight * 0.5f);
        var fieldTop = top + labelHeight;

        _rectBiologicalAgeField =
            new Rect(spacing + extendedArrowSize, fieldTop, bioWidth + extraFieldWidth, fieldHeight);
        _rectChronologicalAgeField = new Rect(_rectBiologicalAgeField.xMax + extendedArrowSize +
                                              spacing + extendedArrowSize, fieldTop, chronoWidth + extraFieldWidth,
            fieldHeight);

        _rectBiologicalAgeLabel = new Rect(_rectBiologicalAgeField.MiddleX() - (bioLabelSize.x / 2),
            _rectBiologicalAgeField.y - bioLabelSize.y, bioLabelSize.x, bioLabelSize.y);
        _rectChronologicalAgeLabel = new Rect(_rectChronologicalAgeField.MiddleX() - (chronoLabelSize.x / 2),
            _rectChronologicalAgeField.y - chronoLabelSize.y, chronoLabelSize.x, chronoLabelSize.y);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);

        var customPawn = state.CurrentPawn;
        if (customPawn == null) {
            return;
        }

        // Update field values.
        var maxAge = providerAgeLimits.MaxAgeForPawn(customPawn.Pawn);
        var minAge = providerAgeLimits.MinAgeForPawn(customPawn.Pawn);
        chronologicalField.MinValue = customPawn.BiologicalAge;
        biologicalField.MinValue = minAge;
        biologicalField.MaxValue = customPawn.ChronologicalAge < maxAge ? customPawn.ChronologicalAge : maxAge;

        // Age labels.
        Text.Font = GameFont.Tiny;
        GUI.color = Style.ColorText;
        Widgets.Label(_rectBiologicalAgeLabel, "EdB.PC.Panel.Age.Biological".Translate());
        Widgets.Label(_rectChronologicalAgeLabel, "EdB.PC.Panel.Age.Chronological".Translate());
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        // Biological age field.
        var fieldRect = _rectBiologicalAgeField;
        biologicalField.Draw(fieldRect, customPawn.BiologicalAge);

        // Chronological age field.
        fieldRect = _rectChronologicalAgeField;
        chronologicalField.Draw(fieldRect, customPawn.ChronologicalAge);
    }
}
