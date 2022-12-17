using UnityEngine;

namespace EdB.PrepareCarefully;

public static class Style {
    public static Color ColorText = new(0.80f, 0.80f, 0.80f);
    public static Color ColorTextSecondary = new(0.50f, 0.50f, 0.50f);
    public static Color ColorTextPanelHeader = new(207f / 255f, 207f / 255f, 207f / 255f);

    public static Color ColorPanelBackground = new(36f / 255f, 37f / 255f, 38f / 255f);
    public static Color ColorPanelBackgroundDeep = new(24f / 255f, 24f / 255f, 29f / 255f);
    public static Color ColorPanelBackgroundItem = new(43f / 255f, 44f / 255f, 45f / 255f);
    public static Color ColorPanelBackgroundScrollView = new(30f / 255f, 31f / 255f, 32f / 255f);

    public static Color ColorButton = new(0.623529f, 0.623529f, 0.623529f);
    public static Color ColorButtonHighlight = new(0.97647f, 0.97647f, 0.97647f);
    public static Color ColorButtonDisabled = new(0.27647f, 0.27647f, 0.27647f);
    public static Color ColorButtonSelected = new(1, 1, 1);

    public static Color ColorControlDisabled = new(1, 1, 1, 0.27647f);

    public static Color ColorTableHeader = new(30f / 255f, 31f / 255f, 32f / 255f);
    public static Color ColorTableHeaderBorder = new(63f / 255f, 64f / 255f, 65f / 255f);
    public static Color ColorTableRow1 = new(47f / 255f, 49f / 255f, 50f / 255f);
    public static Color ColorTableRow2 = new(54f / 255f, 56f / 255f, 57f / 255f);
    public static Color ColorTableRowSelected = new(12f / 255f, 12f / 255f, 12f / 255f);
    public static Color ColorTabViewBackground = new(42f / 255f, 43f / 255f, 44f / 255f);

    public static Color ColorWindowBackground = new(21f / 255f, 25f / 255f, 29f / 255f);

    public static readonly float DialogHeaderHeight = 36;
    public static readonly Vector2 DialogMargin = new(10, 18);
    public static readonly Color DialogTextColor = Color.white;
    public static readonly Color DialogHeaderColor = DialogTextColor;
    public static readonly Color DialogGroupColor = DialogTextColor;
    public static readonly Color DialogAlternatingRowColor = new(28f / 255f, 32f / 255f, 36f / 255f);
    public static readonly float DialogAlternatingRowInset = 16;
    public static readonly Vector2 DialogButtonSize = new(140f, 40f);
    public static readonly float DialogFooterHeight = 40;
    public static readonly float DialogFooterPadding = 16;

    public static Vector2 SizePanelMargin = new(12, 12);
    public static Vector2 SizePanelPadding = new(12, 12);

    public static float RadioButtonSize = 24f;

    public static Vector2 SizeTextFieldArrowMargin = new(4, 0);

    public static float FieldHeight = 22;

    public static void SetGUIColorForButton(Rect rect) {
        if (rect.Contains(Event.current.mousePosition)) {
            GUI.color = ColorButtonHighlight;
        }
        else {
            GUI.color = ColorButton;
        }
    }

    public static void SetGUIColorForButton(Rect rect, bool selected) {
        if (selected) {
            GUI.color = ColorButtonSelected;
        }
        else {
            if (rect.Contains(Event.current.mousePosition)) {
                GUI.color = ColorButtonHighlight;
            }
            else {
                GUI.color = ColorButton;
            }
        }
    }

    public static void SetGUIColorForButton(Rect rect, bool selected, Color color, Color hoverColor,
        Color selectedColor) {
        if (selected) {
            GUI.color = selectedColor;
        }
        else {
            if (rect.Contains(Event.current.mousePosition)) {
                GUI.color = hoverColor;
            }
            else {
                GUI.color = color;
            }
        }
    }
}
