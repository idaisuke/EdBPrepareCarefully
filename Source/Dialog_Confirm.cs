using System;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class Dialog_Confirm : Window {
    private const float TitleHeight = 40f;
    private readonly Action confirmedAction;
    private readonly float createRealTime;
    private readonly bool destructiveAction;
    private readonly string text;
    private readonly string title;
    public string confirmLabel;
    public string goBackLabel;
    public float interactionDelay;
    private Vector2 scrollPos;
    private float scrollViewHeight;
    public bool showGoBack;

    public Dialog_Confirm(string text, Action confirmedAction) {
        this.text = text;
        this.confirmedAction = confirmedAction;
        destructiveAction = false;
        title = null;
        showGoBack = true;
        confirmLabel = "EdB.PC.Common.Confirm".Translate();
        goBackLabel = "EdB.PC.Common.Cancel".Translate();
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnCancel = showGoBack;
        createRealTime = Time.realtimeSinceStartup;
    }


    public Dialog_Confirm(string text, Action confirmedAction, bool destructive) {
        this.text = text;
        this.confirmedAction = confirmedAction;
        destructiveAction = destructive;
        title = null;
        showGoBack = true;
        confirmLabel = "EdB.PC.Common.Confirm".Translate();
        goBackLabel = "EdB.PC.Common.Cancel".Translate();
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnCancel = showGoBack;
        createRealTime = Time.realtimeSinceStartup;
    }

    public Dialog_Confirm(string text, Action confirmedAction, bool destructive, string title) {
        this.text = text;
        this.confirmedAction = confirmedAction;
        destructiveAction = destructive;
        this.title = title;
        showGoBack = true;
        confirmLabel = "EdB.PC.Common.Confirm".Translate();
        goBackLabel = "EdB.PC.Common.Cancel".Translate();
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnCancel = showGoBack;
        createRealTime = Time.realtimeSinceStartup;
    }

    public Dialog_Confirm(string text, Action confirmedAction, bool destructive, string title, bool showGoBack) {
        this.text = text;
        this.confirmedAction = confirmedAction;
        destructiveAction = destructive;
        this.title = title;
        this.showGoBack = showGoBack;
        confirmLabel = "EdB.PC.Common.Confirm".Translate();
        goBackLabel = "EdB.PC.Common.Cancel".Translate();
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnCancel = showGoBack;
        createRealTime = Time.realtimeSinceStartup;
    }

    public override Vector2 InitialSize {
        get {
            var y = 300f;
            if (title != null) {
                y += 40f;
            }

            return new Vector2(500f, y);
        }
    }

    private float TimeUntilInteractive => interactionDelay - (Time.realtimeSinceStartup - createRealTime);

    private bool InteractionDelayExpired => TimeUntilInteractive <= 0.0;

    public override void DoWindowContents(Rect inRect) {
        var y = inRect.y;
        if (!title.NullOrEmpty()) {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0.0f, y, inRect.width, 40f), title);
            y += 40f;
        }

        Text.Font = GameFont.Small;
        var outRect = new Rect(0.0f, y, inRect.width, inRect.height - 45f - y);
        var viewRect = new Rect(0.0f, 0.0f, inRect.width - 16f, scrollViewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
        Widgets.Label(new Rect(0.0f, 0.0f, viewRect.width, scrollViewHeight), text);
        if (Event.current.type == EventType.Layout) {
            scrollViewHeight = Text.CalcHeight(text, viewRect.width);
        }

        Widgets.EndScrollView();
        if (destructiveAction) {
            GUI.color = new Color(1f, 0.3f, 0.35f);
        }

        var label = !InteractionDelayExpired
            ? confirmLabel + "(" + Mathf.Ceil(TimeUntilInteractive).ToString("F0") + ")"
            : confirmLabel;
        if (Widgets.ButtonText(
                new Rect((float)((inRect.width / 2.0) + 20.0), inRect.height - 35f,
                    (float)((inRect.width / 2.0) - 20.0), 35f), label, true, false) && InteractionDelayExpired) {
            confirmedAction();
            Close();
        }

        GUI.color = Color.white;
        if (!showGoBack ||
            !Widgets.ButtonText(new Rect(0.0f, inRect.height - 35f, (float)((inRect.width / 2.0) - 20.0), 35f),
                goBackLabel, true, false)) {
            return;
        }

        Close();
    }
}
