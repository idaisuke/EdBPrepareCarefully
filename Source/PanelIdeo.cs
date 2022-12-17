using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelIdeo : PanelModule {
    public static readonly Vector2 FieldPadding = new(6, 6);
    public Rect CertaintyLabelRect;
    public Rect CertaintySliderRect;
    protected Field FieldIdeo = new();

    public Rect FieldRect;
    protected LabelTrimmer labelTrimmer = new();
    public Rect PercentLabelRect;

    public override void Resize(float width) {
        base.Resize(width);
        float fieldPadding = 8;
        FieldRect = new Rect(FieldPadding.x, 0, width - (FieldPadding.x * 2), 36);

        var savedFont = Text.Font;
        Text.Font = GameFont.Small;
        var sizeCertainty = Text.CalcSize("Certainty".Translate().CapitalizeFirst());
        Text.Font = GameFont.Tiny;
        var sizePercent = Text.CalcSize("100%");
        Text.Font = savedFont;
        var labelHeight = Math.Max(sizeCertainty.y, sizePercent.y);
        CertaintyLabelRect = new Rect(Margin.x, 0, sizeCertainty.x, labelHeight);
        PercentLabelRect = new Rect(width - Margin.x - sizePercent.x, 1, sizePercent.x, labelHeight);

        var sliderHeight = 8f;
        var sliderWidth = PercentLabelRect.xMin - CertaintyLabelRect.xMax - (fieldPadding * 2f);
        CertaintySliderRect = new Rect(CertaintyLabelRect.xMax + fieldPadding,
            CertaintyLabelRect.yMin + (CertaintyLabelRect.height * 0.5f) - (sliderHeight * 0.5f) - 1,
            sliderWidth, sliderHeight);
    }

    public float Measure() {
        return 0;
    }

    public override bool IsVisible(State state) {
        return ModsConfig.IdeologyActive;
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "Ideo".Translate().CapitalizeFirst().Resolve());

        var pawn = state.CurrentPawn;
        var ideoTracker = pawn.Pawn.ideo;
        var ideo = ideoTracker?.Ideo;

        FieldIdeo.Rect = FieldRect.OffsetBy(0, y);
        labelTrimmer.Rect = FieldIdeo.Rect.InsetBy(8, 0);

        if (ideo != null) {
            FieldIdeo.Label = labelTrimmer.TrimLabelIfNeeded(ideo.name);
            FieldIdeo.Tip = ideo.description;
        }

        FieldIdeo.Enabled = true;
        FieldIdeo.ClickAction = () => {
            var dialog = new DialogIdeos { Pawn = pawn };
            Find.WindowStack.Add(dialog);
        };
        FieldIdeo.DrawIconFunc = rect => {
            ideo.DrawIcon(rect);
        };
        FieldIdeo.IconSizeFunc = () => new Vector2(32, 32);

        FieldIdeo.Draw();

        y += FieldRect.height;
        y += FieldPadding.y;

        // Draw the certainty slider
        Text.Font = GameFont.Small;
        GUI.color = Style.ColorText;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(CertaintyLabelRect.OffsetBy(0, y), "Certainty".Translate().CapitalizeFirst());
        Text.Anchor = TextAnchor.MiddleRight;
        Text.Font = GameFont.Tiny;
        Widgets.Label(PercentLabelRect.OffsetBy(0, y), (int)(ideoTracker.Certainty * 100f) + "%");
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        var certainty = GUI.HorizontalSlider(CertaintySliderRect.OffsetBy(0, y), ideoTracker.Certainty, 0, 1);
        y += CertaintyLabelRect.height;

        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;

        y += Margin.y;

        // Update the certainty based on the slider value
        pawn.Certainty = certainty;

        // Randomize button
        var randomizeRect = new Rect(Width - 32, top + 9, 22, 22);
        if (randomizeRect.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(randomizeRect, Textures.TextureButtonRandom);
        if (Widgets.ButtonInvisible(randomizeRect, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            SelectRandomIdeo(state);
        }

        return y - top;
    }

    protected void SelectRandomIdeo(State state) {
        var pawn = state.CurrentPawn;
        var ideo = pawn.Pawn?.ideo;
        if (ideo != null) {
            var currentIdeo = ideo.Ideo;
            var certainty = ideo.Certainty;
            var newIdeo = Find.IdeoManager.IdeosInViewOrder.Where(i => i != currentIdeo).RandomElement();
            ideo.SetIdeo(newIdeo);
            pawn.Certainty = certainty;
        }
    }
}
