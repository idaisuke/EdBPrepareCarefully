using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class DialogTitles<G /*option group*/, O /*option*/> : Window where G : class where O : class {
    protected Rect BodyRect;
    protected Rect CancelButtonRect;
    protected Rect ConfirmButtonRect;

    protected Rect ContentRect;
    protected Rect HeaderRect;
    protected OptionRenderer<O> renderer = new();
    protected ScrollViewVertical ScrollView = new();

    public DialogTitles() {
        closeOnCancel = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        DrawGroupHeader = DefaultDrawGroupHeader;
    }

    public Func<IEnumerable<G>> Groups { get; set; }
    public Func<G, string> GroupTitle { get; set; }
    public Func<G, float, float, float> DrawGroupHeader { get; set; }
    public Func<G, float, float, float> DrawGroupContent { get; set; }
    public Func<G, IEnumerable<O>> OptionsFromGroup { get; set; }
    public Func<O, string> OptionTitle { get; set; }
    public Func<O, bool> IsSelected { get; set; }
    public Action<O> Select { get; set; }
    public Action Confirm { get; set; }
    public Action Cancel { get; set; } = () => { };

    public string Header { get; set; }

    public override void PreOpen() {
        base.PreOpen();
        Resize();

        if (renderer.TitleProvider == null) {
            renderer.TitleProvider = OptionTitle;
        }

        if (IsSelected != null) {
            renderer.Selected = IsSelected;
        }

        if (Select != null) {
            renderer.Clicked = Select;
        }
    }

    protected void Resize() {
        ContentRect = windowRect.InsetBy(StandardMargin).MoveTo(0, 0);
        if (!Header.NullOrEmpty()) {
            Text.Font = GameFont.Medium;
            var height = Text.CalcHeight(Header, ContentRect.width);
            HeaderRect = new Rect(ContentRect.xMin, ContentRect.yMin, ContentRect.width, height);
            BodyRect = new Rect(ContentRect.xMin, HeaderRect.yMax + 8, ContentRect.width,
                ContentRect.height - HeaderRect.height - 8);
            Text.Font = GameFont.Small;
        }
        else {
            BodyRect = ContentRect;
        }

        BodyRect = BodyRect.InsetBy(0, 0, 0, Style.DialogFooterHeight + Style.DialogFooterPadding);

        var footerRect = new Rect(BodyRect.x, BodyRect.yMax + Style.DialogFooterPadding, BodyRect.width,
            Style.DialogFooterHeight);
        CancelButtonRect = new Rect(footerRect.x, footerRect.yMax - Style.DialogButtonSize.y,
            Style.DialogButtonSize.x, Style.DialogButtonSize.y);
        ConfirmButtonRect = new Rect(footerRect.xMax - Style.DialogButtonSize.x,
            footerRect.yMax - Style.DialogButtonSize.y,
            Style.DialogButtonSize.x, Style.DialogButtonSize.y);
    }

    public static float DrawTextWithWidth(string textToMeasure, string textToDraw, Vector2 position, float width) {
        var labelHeight = Text.CalcHeight(textToMeasure, width);
        var labelRect = new Rect(position.x, position.y, width, labelHeight);
        Widgets.Label(labelRect, textToDraw);
        return labelHeight;
    }

    public float DefaultDrawGroupHeader(G group, float y, float width) {
        var top = y;

        var groupTitle = GroupTitle(group);
        y += DrawTextWithWidth(groupTitle, "<b>" + groupTitle + "</b>", new Vector2(0, y), width);

        return y - top;
    }

    public override void DoWindowContents(Rect rect) {
        GUI.color = Style.DialogHeaderColor;
        if (!Header.NullOrEmpty()) {
            Text.Font = GameFont.Medium;
            Widgets.Label(HeaderRect, Header);
            Text.Font = GameFont.Small;
        }

        float y = 0;
        GUI.BeginGroup(BodyRect);
        try {
            ScrollView.Begin(BodyRect.MoveTo(0, 0));
            var viewWidth = ScrollView.CurrentViewWidth;
            try {
                Text.Font = GameFont.Small;
                foreach (var group in Groups()) {
                    y += 8;
                    var groupTitle = GroupTitle(group);
                    y += DrawGroupHeader(group, y, viewWidth);

                    if (DrawGroupContent != null) {
                        y += DrawGroupContent(group, y, viewWidth);
                    }

                    var index = 0;
                    foreach (var option in OptionsFromGroup(group)) {
                        y += renderer.Draw(option, index++, new Vector2(0, y), viewWidth);
                    }
                }
            }
            finally {
                ScrollView.End(y);
            }
        }
        finally {
            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        if (Widgets.ButtonText(CancelButtonRect, "Cancel".Translate())) {
            Cancel();
            Close();
        }

        if (Widgets.ButtonText(ConfirmButtonRect, "Confirm".Translate())) {
            Confirm();
            Close();
        }
    }

    public class OptionRenderer<T> {
        protected bool checkbox;

        protected bool radioButton = true;
        public Func<T, string> TitleProvider { get; set; }
        public Func<T, string> SubtitleProvider { get; set; }
        public Func<T, string> IconProvider { get; set; }
        public Func<T, bool> Selected { get; set; }
        public Action<T> Clicked { get; set; }

        public bool RadioButton {
            get => radioButton;
            set {
                radioButton = value;
                checkbox = !value;
            }
        }

        public bool Checkbox {
            get => checkbox;
            set {
                radioButton = !value;
                checkbox = value;
            }
        }

        public bool DrawRadioButton(Rect rect, bool selected) {
            var position = new Vector2(rect.x + rect.HalfWidth() - (Style.RadioButtonSize * 0.5f),
                rect.y + rect.HalfHeight() - (Style.RadioButtonSize * 0.5f));
            return Widgets.RadioButton(position, selected);
        }

        public float Draw(T option, int index, Vector2 position, float width) {
            var selected = Selected != null ? Selected(option) : false;

            var rowRect = new Rect(position.x, position.y, width, 40);
            if (index % 2 == 0) {
                GUI.color = Style.DialogAlternatingRowColor;
                GUI.DrawTexture(rowRect, Textures.TextureWhite);
                GUI.color = Color.white;
            }

            var titleInset = Style.DialogAlternatingRowInset;
            float radioButtonMargin = 8;

            var radioRect = new Rect(rowRect.xMax - Style.RadioButtonSize - radioButtonMargin, rowRect.y,
                Style.RadioButtonSize, Style.RadioButtonSize);
            var titleRect = rowRect.InsetBy(0, 0, radioRect.width, 0);

            var title = TitleProvider(option);
            var titleWidth = titleRect.width - titleInset - (radioButtonMargin * 2);
            var titleHeight = Text.CalcHeight(title, titleWidth);
            titleRect = new Rect(titleRect.x + titleInset, titleRect.y, titleWidth,
                Mathf.Max(titleHeight, rowRect.height));

            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(titleRect, title);
            if (Widgets.ButtonInvisible(titleRect)) {
                if (Clicked != null) {
                    Clicked(option);
                }
            }

            Text.Font = font;
            Text.Anchor = anchor;


            if (DrawRadioButton(radioRect.ResizeTo(radioRect.width, titleRect.height), selected)) {
                if (Clicked != null) {
                    Clicked(option);
                }
            }

            return rowRect.height;
        }
    }
}
