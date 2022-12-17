using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class Dialog_SaveColonist : Dialog_Colonist {
    protected const float NewColonistNameButtonSpace = 20;
    protected const float NewColonistHeight = 35;
    protected const float NewColonistNameWidth = 400;
    protected static string Filename = "";
    protected Action<string> action;

    private bool focusedColonistNameArea;

    public Dialog_SaveColonist(string name, Action<string> action) {
        interactButLabel = "OverwriteButton".Translate();
        bottomAreaHeight = 85;
        this.action = action;
        Filename = name;
    }

    protected override void DoMapEntryInteraction(string colonistName) {
        if (string.IsNullOrEmpty(colonistName)) {
            return;
        }

        Filename = colonistName;
        if (action != null) {
            action(Filename);
        }

        Close();
    }

    protected override void DoSpecialSaveLoadGUI(Rect inRect) {
        GUI.BeginGroup(inRect);
        var flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
        var top = inRect.height - 52;
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.SetNextControlName("ColonistNameField");
        var rect = new Rect(5, top, 400, 35);
        var text = Widgets.TextField(rect, Filename);
        if (GenText.IsValidFilename(text)) {
            Filename = text;
        }

        if (!focusedColonistNameArea) {
            GUI.FocusControl("ColonistNameField");
            focusedColonistNameArea = true;
        }

        var butRect = new Rect(420, top, inRect.width - 400 - 20, 35);
        if (Widgets.ButtonText(butRect, "EdB.PC.Dialog.PawnPreset.Button.Save".Translate(), true, false) || flag) {
            if (Filename.Length == 0) {
                Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput);
            }
            else {
                if (action != null) {
                    action(Filename);
                }

                Close();
            }
        }

        Text.Anchor = TextAnchor.UpperLeft;
        GUI.EndGroup();
    }
}
