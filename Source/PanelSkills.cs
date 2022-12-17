using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelSkills : PanelBase {
    public delegate void ClearSkillsHandler();

    public delegate void ResetSkillsHandler();

    public delegate void UpdateSkillLevelHandler(SkillDef skill, int level);

    public delegate void UpdateSkillPassionHandler(SkillDef skill, Passion level);

    private static readonly Color ColorSkillDisabled = new(1f, 1f, 1f, 0.5f);

    protected static Rect RectButtonClearSkills;
    protected static Rect RectButtonResetSkills;
    protected static Rect RectLabel;
    protected static Rect RectPassion;
    protected static Rect RectSkillBar;
    protected static Rect RectButtonDecrement;
    protected static Rect RectButtonIncrement;
    protected static Rect RectScrollFrame;
    protected static Rect RectScrollView;

    protected ScrollViewVertical scrollView = new();

    public override string PanelHeader => "Skills".Translate();

    public event ClearSkillsHandler SkillsCleared;
    public event ResetSkillsHandler SkillsReset;
    public event UpdateSkillLevelHandler SkillLevelUpdated;
    public event UpdateSkillPassionHandler SkillPassionUpdated;

    public override void Resize(Rect rect) {
        base.Resize(rect);

        float panelPaddingLeft = 12;
        float panelPaddingRight = 10;
        float panelPaddingBottom = 10;
        float panelPaddingTop = 4;
        var top = BodyRect.y + panelPaddingTop;

        var savedFont = Text.Font;
        Text.Font = GameFont.Small;
        var maxLabelSize = new Vector2(float.MinValue, float.MinValue);
        foreach (var current in DefDatabase<SkillDef>.AllDefs) {
            var labelSize = Text.CalcSize(current.skillLabel);
            // Need to add some padding because the "n" at the end of "Construction" gets cut off if we don't.
            labelSize += new Vector2(4, 0);
            maxLabelSize.x = Mathf.Max(labelSize.x, maxLabelSize.x);
            maxLabelSize.y = Mathf.Max(labelSize.y, maxLabelSize.y);
        }

        Text.Font = savedFont;

        float labelPadding = 4;
        var availableContentWidth = PanelRect.width - panelPaddingLeft - panelPaddingRight;
        var passionSize = new Vector2(24, 24);
        float passionPadding = 2;
        var arrowButtonSize = new Vector2(16, 16);
        float arrowsWidth = 32;
        var skillBarSize = new Vector2(availableContentWidth - passionSize.x - passionPadding
                                       - arrowsWidth - maxLabelSize.x - labelPadding, 22);

        RectButtonClearSkills = new Rect(PanelRect.width - 65, 9, 20, 20);
        RectButtonResetSkills = new Rect(PanelRect.width - 38, 8, 23, 21);
        RectLabel = new Rect(0, 0, maxLabelSize.x, maxLabelSize.y);
        RectPassion = new Rect(RectLabel.xMax + labelPadding, (maxLabelSize.y * 0.5f) - (passionSize.y * 0.5f),
            passionSize.x, passionSize.y);
        RectSkillBar = new Rect(RectPassion.xMax + passionPadding, (maxLabelSize.y * 0.5f) - (skillBarSize.y * 0.5f),
            skillBarSize.x, skillBarSize.y);
        RectButtonDecrement = new Rect(RectSkillBar.xMax, (maxLabelSize.y * 0.5f) - (arrowButtonSize.y * 0.5f),
            arrowButtonSize.x, arrowButtonSize.y);
        RectButtonIncrement = new Rect(RectButtonDecrement.xMax, (maxLabelSize.y * 0.5f) - (arrowButtonSize.y * 0.5f),
            arrowButtonSize.x, arrowButtonSize.y);
        RectScrollFrame = new Rect(panelPaddingLeft, top,
            availableContentWidth, BodyRect.height - panelPaddingTop - panelPaddingBottom);
        RectScrollView = new Rect(0, 0, RectScrollFrame.width, RectScrollFrame.height);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);

        var customPawn = state.CurrentPawn;

        // Clear button
        Style.SetGUIColorForButton(RectButtonClearSkills);
        GUI.DrawTexture(RectButtonClearSkills, Textures.TextureButtonClearSkills);
        if (Widgets.ButtonInvisible(RectButtonClearSkills, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            SkillsCleared();
        }

        TooltipHandler.TipRegion(RectButtonClearSkills, "EdB.PC.Panel.Skills.ClearTip".Translate());

        // Reset button
        Style.SetGUIColorForButton(RectButtonResetSkills);
        GUI.DrawTexture(RectButtonResetSkills, Textures.TextureButtonReset);
        if (Widgets.ButtonInvisible(RectButtonResetSkills, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            SkillsReset();
        }

        TooltipHandler.TipRegion(RectButtonResetSkills, "EdB.PC.Panel.Skills.ResetTip".Translate());

        var skillCount = customPawn.Pawn.skills.skills.Count;
        float rowHeight = 26;
        var height = rowHeight * skillCount;
        var willScroll = height > RectScrollView.height;

        float cursor = 0;
        GUI.BeginGroup(RectScrollFrame);
        try {
            scrollView.Begin(RectScrollView);

            Rect rect;
            Text.Font = GameFont.Small;
            foreach (var skill in customPawn.Pawn.skills.skills) {
                var def = skill.def;
                var disabled = skill.TotallyDisabled;

                // Draw the label.
                GUI.color = Style.ColorText;
                rect = RectLabel;
                rect.y = rect.y + cursor;
                Widgets.Label(rect, def.skillLabel.CapitalizeFirst());

                // Draw the passion.
                rect = RectPassion;
                rect.y = rect.y + cursor;
                if (!disabled) {
                    var passion = customPawn.currentPassions[skill.def];
                    Texture2D image;
                    if (passion == Passion.Minor) {
                        image = Textures.TexturePassionMinor;
                    }
                    else if (passion == Passion.Major) {
                        image = Textures.TexturePassionMajor;
                    }
                    else {
                        image = Textures.TexturePassionNone;
                    }

                    GUI.color = Color.white;
                    GUI.DrawTexture(rect, image);
                    if (Widgets.ButtonInvisible(rect, false)) {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        if (Event.current.button != 1) {
                            IncreasePassion(skill);
                        }
                        else {
                            DecreasePassion(skill);
                        }
                    }
                }

                // Draw the skill bar.
                rect = RectSkillBar;
                rect.y = rect.y + cursor;
                if (willScroll) {
                    rect.width = rect.width - 16;
                }

                DrawSkill(customPawn, skill, rect);

                // Handle the tooltip.
                // TODO: Should cover the whole row, not just the skill bar rect.
                TooltipHandler.TipRegion(rect, new TipSignal(GetSkillDescription(skill),
                    skill.def.GetHashCode() * 397945));

                if (!disabled) {
                    // Draw the decrement button.
                    rect = RectButtonDecrement;
                    rect.y = rect.y + cursor;
                    rect.x = rect.x - (willScroll ? 16 : 0);
                    if (rect.Contains(Event.current.mousePosition)) {
                        GUI.color = Style.ColorButtonHighlight;
                    }
                    else {
                        GUI.color = Style.ColorButton;
                    }

                    GUI.DrawTexture(rect, Textures.TextureButtonPrevious);
                    if (Widgets.ButtonInvisible(rect, false)) {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        DecreaseSkill(customPawn, skill);
                    }

                    // Draw the increment button.
                    rect = RectButtonIncrement;
                    rect.y = rect.y + cursor;
                    rect.x = rect.x - (willScroll ? 16 : 0);
                    if (rect.Contains(Event.current.mousePosition)) {
                        GUI.color = Style.ColorButtonHighlight;
                    }
                    else {
                        GUI.color = Style.ColorButton;
                    }

                    GUI.DrawTexture(rect, Textures.TextureButtonNext);
                    if (Widgets.ButtonInvisible(rect, false)) {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        IncreaseSkill(customPawn, skill);
                    }
                }

                cursor += rowHeight;
            }

            scrollView.End(cursor);
        }
        finally {
            GUI.EndGroup();
        }

        GUI.color = Color.white;
    }

    public static void FillableBar(Rect rect, float fillPercent, Texture2D fillTex) {
        rect.width *= fillPercent;
        GUI.DrawTexture(rect, fillTex);
    }

    private void DrawSkill(CustomPawn customPawn, SkillRecord skill, Rect rect) {
        var level = skill.Level;
        var disabled = skill.TotallyDisabled;
        if (!disabled) {
            var barSize = (level > 0 ? (float)level : 0) / 20f;
            FillableBar(rect, barSize, Textures.TextureSkillBarFill);

            var baseLevel = customPawn.GetSkillModifier(skill.def);
            var baseBarSize = (baseLevel > 0 ? (float)baseLevel : 0) / 20f;
            FillableBar(rect, baseBarSize, Textures.TextureSkillBarFill);

            GUI.color = new Color(0.25f, 0.25f, 0.25f);
            Widgets.DrawBox(rect);
            GUI.color = Style.ColorText;

            if (Widgets.ButtonInvisible(rect, false)) {
                var pos = Event.current.mousePosition;
                var x = pos.x - rect.x;
                var value = 0;
                if (Mathf.Floor(x / rect.width * 20f) == 0) {
                    if (x <= 1) {
                        value = 0;
                    }
                    else {
                        value = 1;
                    }
                }
                else {
                    value = Mathf.CeilToInt(x / rect.width * 20f);
                }

                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                SetSkillLevel(customPawn, skill, value);
            }
        }

        string label;
        if (disabled) {
            GUI.color = ColorSkillDisabled;
            label = "-";
        }
        else {
            label = level.ToStringCached();
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        rect.x = rect.x + 3;
        rect.y = rect.y + 1;
        Widgets.Label(rect, label);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    // EdB: Copy of private static SkillUI.GetSkillDescription().
    private static string GetSkillDescription(SkillRecord sk) {
        var stringBuilder = new StringBuilder();
        if (sk.TotallyDisabled) {
            stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
        }
        else {
            stringBuilder.AppendLine(string.Concat("Level".Translate(), " ", sk.Level, ": ", sk.LevelDescriptor));
            stringBuilder.Append("Passion".Translate() + ": ");
            switch (sk.passion) {
                case Passion.None:
                    stringBuilder.Append("PassionNone".Translate("0.3"));
                    break;
                case Passion.Minor:
                    stringBuilder.Append("PassionMinor".Translate("1.0"));
                    break;
                case Passion.Major:
                    stringBuilder.Append("PassionMajor".Translate("1.5"));
                    break;
            }
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine();
        stringBuilder.Append(sk.def.description);
        return stringBuilder.ToString();
    }

    protected void SetSkillLevel(CustomPawn pawn, SkillRecord record, int value) {
        pawn.SetSkillLevel(record.def, value);
    }

    protected void IncreaseSkill(CustomPawn pawn, SkillRecord record) {
        pawn.IncrementSkillLevel(record.def);
    }

    protected void DecreaseSkill(CustomPawn pawn, SkillRecord record) {
        pawn.DecrementSkillLevel(record.def);
    }

    protected void IncreasePassion(SkillRecord record) {
        if (record.passion == Passion.None) {
            SkillPassionUpdated(record.def, Passion.Minor);
        }
        else if (record.passion == Passion.Minor) {
            SkillPassionUpdated(record.def, Passion.Major);
        }
        else if (record.passion == Passion.Major) {
            SkillPassionUpdated(record.def, Passion.None);
        }
    }

    protected void DecreasePassion(SkillRecord record) {
        if (record.passion == Passion.None) {
            SkillPassionUpdated(record.def, Passion.Major);
        }
        else if (record.passion == Passion.Minor) {
            SkillPassionUpdated(record.def, Passion.None);
        }
        else if (record.passion == Passion.Major) {
            SkillPassionUpdated(record.def, Passion.Minor);
        }
    }

    public void ScrollToTop() {
        scrollView.ScrollToTop();
    }
}
