using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class Field {
    public Action<Rect> DrawIconFunc = null;
    public Func<Vector2> IconSizeFunc = null;
    private Rect rect;

    public Rect Rect {
        get => rect;
        set => rect = value;
    }

    public Rect? ClickRect {
        get;
        set;
    }

    public string Label { get; set; }

    public string Tip { get; set; }

    public Action ClickAction { get; set; }

    public Action PreviousAction { get; set; }

    public Action NextAction { get; set; }

    public Action<Rect> TipAction { get; set; }

    public Color Color { get; set; } = Style.ColorText;

    public bool Enabled { get; set; } = true;

    public void Draw() {
        var saveAnchor = Text.Anchor;
        var saveColor = GUI.color;
        var saveFont = Text.Font;
        try {
            // Adjust the width of the rectangle if the field has next and previous buttons.
            var fieldRect = rect;
            if (PreviousAction != null) {
                fieldRect.x += 12;
                fieldRect.width -= 12;
            }

            if (NextAction != null) {
                fieldRect.width -= 12;
            }

            // Draw the field background.
            if (Enabled) {
                GUI.color = Color.white;
            }
            else {
                GUI.color = Style.ColorControlDisabled;
            }

            Widgets.DrawAtlas(fieldRect, Textures.TextureFieldAtlas);

            // Draw the label.
            Text.Anchor = TextAnchor.MiddleCenter;
            var iconRect = new Rect();
            var fullRect = new Rect(rect.x, rect.y + 1, rect.width, rect.height);
            var textRect = fullRect;
            var drawIcon = DrawIconFunc != null && IconSizeFunc != null;
            if (drawIcon) {
                var iconSize = IconSizeFunc();
                textRect = textRect.InsetBy(iconSize.x * 2f, 0).OffsetBy(4, 0);
                var textSize = Text.CalcSize(Label);
                iconRect = new Rect(fullRect.x + (fullRect.width / 2f) - (textSize.x / 2f) - iconSize.x - 4,
                    fullRect.y + (fullRect.height / 2) - (iconSize.y / 2), iconSize.x, iconSize.y);
            }

            if (!Enabled) {
                GUI.color = Style.ColorControlDisabled;
            }
            else if (ClickAction != null && fieldRect.Contains(Event.current.mousePosition)) {
                GUI.color = Color.white;
            }
            else {
                GUI.color = Color;
            }

            if (drawIcon) {
                DrawIconFunc(iconRect);
            }

            if (Label != null) {
                Widgets.Label(textRect, Label);
            }

            GUI.color = Color.white;

            if (!Enabled) {
                return;
            }

            // Handle the tooltip.
            if (Tip != null) {
                TooltipHandler.TipRegion(ClickRect.HasValue ? ClickRect.Value : fieldRect, Tip);
            }

            if (TipAction != null) {
                TipAction(ClickRect.HasValue ? ClickRect.Value : fieldRect);
            }

            // Draw the previous button and handle any click events on it.
            if (PreviousAction != null) {
                var buttonRect = new Rect(fieldRect.x - 17, fieldRect.MiddleY() - 8, 16, 16);
                if (buttonRect.Contains(Event.current.mousePosition)) {
                    GUI.color = Style.ColorButtonHighlight;
                }
                else {
                    GUI.color = Style.ColorButton;
                }

                GUI.DrawTexture(buttonRect, Textures.TextureButtonPrevious);
                if (Widgets.ButtonInvisible(buttonRect, false)) {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    PreviousAction();
                }
            }

            // Draw the next button and handle any click events on it.
            if (NextAction != null) {
                var buttonRect = new Rect(fieldRect.xMax + 1, fieldRect.MiddleY() - 8, 16, 16);
                if (buttonRect.Contains(Event.current.mousePosition)) {
                    GUI.color = Style.ColorButtonHighlight;
                }
                else {
                    GUI.color = Style.ColorButton;
                }

                GUI.DrawTexture(buttonRect, Textures.TextureButtonNext);
                if (Widgets.ButtonInvisible(buttonRect, false)) {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    NextAction();
                }
            }

            // Handle any click event on the field.
            if (ClickAction != null) {
                if (ClickRect == null) {
                    if (Widgets.ButtonInvisible(fieldRect, false)) {
                        ClickAction();
                    }
                }
                else {
                    if (Widgets.ButtonInvisible(ClickRect.Value, false)) {
                        ClickAction();
                    }
                }
            }
        }
        finally {
            Text.Anchor = saveAnchor;
            GUI.color = saveColor;
            Text.Font = saveFont;
        }
    }
}
