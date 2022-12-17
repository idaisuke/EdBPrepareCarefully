using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class Dialog_Options<T> : Window {
    protected Vector2 ButtonSize = new(140f, 40f);

    public string CancelButtonLabel = null;
    protected Rect CancelButtonRect;
    public Action CloseAction = () => { };
    protected bool confirmButtonClicked;
    public string ConfirmButtonLabel = "EdB.PC.Common.Close".Translate();
    protected Rect ConfirmButtonRect;

    public Func<string> ConfirmValidation = () => {
        return null;
    };

    protected Vector2 ContentMargin = new(10f, 18f);
    protected Rect ContentRect;
    protected Vector2 ContentSize;
    public Func<T, string> DescriptionFunc;

    public Func<Rect, float> DrawHeader = rect => {
        return 0;
    };

    public Action<T, Rect> DrawIconFunc = null;

    public Func<T, bool> EnabledFunc = T => {
        return true;
    };

    public Func<IEnumerable<T>, IEnumerable<T>> FilterOptions = options => {
        return options;
    };

    protected float FooterHeight = 40f;
    protected Rect FooterRect;
    protected float HeaderHeight = 32;

    protected string headerLabel;
    protected Rect HeaderRect;
    public Func<Vector2> IconSizeFunc = null;

    public bool IncludeNone = false;

    public Action Initialize = () => { };

    public Func<T, string> NameFunc = T => {
        return "";
    };

    public Func<bool> NoneEnabledFunc = () => {
        return true;
    };

    public Func<bool> NoneSelectedFunc = () => {
        return false;
    };

    protected IEnumerable<T> options;
    protected Rect ScrollRect;
    protected ScrollViewVertical ScrollView = new();
    public Action<T> SelectAction = T => { };

    public Func<T, bool> SelectedFunc = T => {
        return false;
    };

    public Action SelectNoneAction = () => { };
    protected Rect SingleButtonRect;
    protected float WindowPadding = 18;
    protected Vector2 WindowSize = new(440f, 584f);


    public Dialog_Options(IEnumerable<T> options) {
        closeOnCancel = true;
        //this.doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;

        this.options = options;

        ComputeSizes();
    }

    public float? InitialPositionX { get; set; } = null;
    public float? InitialPositionY { get; set; } = null;

    public string HeaderLabel {
        get => headerLabel;
        set {
            headerLabel = value;
            ComputeSizes();
        }
    }

    public IEnumerable<T> Options {
        get => options;
        set => options = value;
    }


    public override Vector2 InitialSize => new(WindowSize.x, WindowSize.y);

    public void ScrollToTop() {
        ScrollView.ScrollToTop();
    }

    protected override void SetInitialSizeAndPosition() {
        var initialSize = InitialSize;
        var x = InitialPositionX.HasValue ? InitialPositionX.Value : (UI.screenWidth - initialSize.x) / 2f;
        var y = InitialPositionY.HasValue ? InitialPositionY.Value : (UI.screenHeight - initialSize.y) / 2f;
        windowRect = new Rect(x, y, initialSize.x, initialSize.y);
        windowRect = windowRect.Rounded();
    }

    protected void ComputeSizes() {
        float headerSize = 0;
        if (HeaderLabel != null) {
            headerSize = HeaderHeight;
        }

        ContentSize = new Vector2(WindowSize.x - (WindowPadding * 2) - (ContentMargin.x * 2),
            WindowSize.y - (WindowPadding * 2) - (ContentMargin.y * 2) - FooterHeight - headerSize);

        ContentRect = new Rect(ContentMargin.x, ContentMargin.y + headerSize, ContentSize.x, ContentSize.y);

        ScrollRect = new Rect(0, 0, ContentRect.width, ContentRect.height);

        HeaderRect = new Rect(ContentMargin.x, ContentMargin.y, ContentSize.x, HeaderHeight);

        FooterRect = new Rect(ContentMargin.x, ContentRect.y + ContentSize.y + 20,
            ContentSize.x, FooterHeight);

        SingleButtonRect = new Rect((ContentSize.x / 2) - (ButtonSize.x / 2),
            (FooterHeight / 2) - (ButtonSize.y / 2),
            ButtonSize.x, ButtonSize.y);

        CancelButtonRect = new Rect(0,
            (FooterHeight / 2) - (ButtonSize.y / 2),
            ButtonSize.x, ButtonSize.y);
        ConfirmButtonRect = new Rect(ContentSize.x - ButtonSize.x,
            (FooterHeight / 2) - (ButtonSize.y / 2),
            ButtonSize.x, ButtonSize.y);
    }

    public override void DoWindowContents(Rect inRect) {
        var cursor = DrawHeader(new Rect(0, 0, inRect.width, inRect.height));

        GUI.color = Color.white;
        var headerRect = HeaderRect.InsetBy(0, 0, 0, cursor).OffsetBy(0, cursor);
        if (HeaderLabel != null) {
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, HeaderLabel);
        }

        var contentRect = ContentRect.InsetBy(0, 0, 0, cursor).OffsetBy(0, cursor);
        var scrollRect = new Rect(0, 0, contentRect.width, contentRect.height);

        Text.Font = GameFont.Small;
        GUI.BeginGroup(contentRect);
        ScrollView.Begin(scrollRect);

        cursor = 0;

        if (IncludeNone) {
            var height = Text.CalcHeight("EdB.PC.Common.NoOptionSelected".Translate(), ContentSize.x - 32);
            if (height < 30) {
                height = 30;
            }

            var isEnabled = NoneEnabledFunc();
            var isSelected = NoneSelectedFunc();
            if (Widgets.RadioButtonLabeled(new Rect(0, cursor, ContentSize.x - 32, height),
                    "EdB.PC.Common.NoOptionSelected".Translate(), isSelected)) {
                SelectNoneAction();
            }

            cursor += height;
            cursor += 2;
        }

        var drawIcon = DrawIconFunc != null && IconSizeFunc != null;
        var iconSize = new Vector2(0, 0);
        if (IconSizeFunc != null) {
            iconSize = IconSizeFunc();
        }

        var itemRect = new Rect(0, cursor, ContentSize.x - 32, 0);
        var filteredOptions = FilterOptions(options);
        foreach (var option in filteredOptions) {
            var name = NameFunc(option);
            var selected = SelectedFunc(option);
            var enabled = EnabledFunc != null ? EnabledFunc(option) : true;

            var height = Text.CalcHeight(name, ContentSize.x - 32);
            if (height < 30) {
                height = 30;
            }

            itemRect.height = height;
            var size = Text.CalcSize(name);
            if (size.x > ContentSize.x - 32) {
                size.x = ContentSize.x;
            }

            size.y = height;

            if (cursor + height >= ScrollView.Position.y && cursor <= ScrollView.Position.y + ScrollView.ViewHeight) {
                GUI.color = Color.white;
                if (!enabled) {
                    GUI.color = new Color(0.65f, 0.65f, 0.65f);
                }

                var labelRect = new Rect(0, cursor + 2, ContentSize.x - 32, height);
                if (drawIcon) {
                    var iconRect = new Rect(0, cursor + 2 + (height / 2) - (iconSize.y / 2), iconSize.x, iconSize.y);
                    DrawIconFunc(option, iconRect);
                    labelRect = labelRect.InsetBy(iconSize.x + 4, 0);
                }

                Widgets.Label(labelRect, name);
                var radioButtonPosition = new Vector2(itemRect.x + itemRect.width - 24,
                    itemRect.y + (itemRect.height / 2) - 12);
                if (!enabled) {
                    var image = Textures.TextureRadioButtonOff;
                    GUI.color = new Color(1, 1, 1, 0.28f);
                    GUI.DrawTexture(new Rect(radioButtonPosition.x, radioButtonPosition.y, 24, 24), image);
                    GUI.color = Color.white;
                }
                else {
                    if (Widgets.RadioButton(radioButtonPosition, selected) || Widgets.ButtonInvisible(labelRect)) {
                        SelectAction(option);
                    }
                }

                if (DescriptionFunc != null) {
                    var tipRect = new Rect(itemRect.x, itemRect.y, size.x, size.y);
                    TooltipHandler.TipRegion(tipRect, DescriptionFunc(option));
                }
            }

            cursor += height;
            cursor += 2;
            itemRect.y = cursor;
        }

        ScrollView.End(cursor);
        GUI.EndGroup();
        GUI.color = Color.white;

        GUI.BeginGroup(FooterRect);
        var buttonRect = SingleButtonRect;
        if (CancelButtonLabel != null) {
            if (Widgets.ButtonText(CancelButtonRect, CancelButtonLabel)) {
                Close();
            }

            buttonRect = ConfirmButtonRect;
        }

        if (Widgets.ButtonText(buttonRect, ConfirmButtonLabel)) {
            var validationMessage = ConfirmValidation();
            if (validationMessage != null) {
                Messages.Message(validationMessage.Translate(), MessageTypeDefOf.RejectInput);
            }
            else {
                Confirm();
            }
        }

        GUI.EndGroup();
    }

    protected void Confirm() {
        confirmButtonClicked = true;
        Close();
    }

    public override void PostClose() {
        if (ConfirmButtonLabel != null) {
            if (confirmButtonClicked && CloseAction != null) {
                CloseAction();
            }
        }
        else {
            if (CloseAction != null) {
                CloseAction();
            }
        }
    }
}
