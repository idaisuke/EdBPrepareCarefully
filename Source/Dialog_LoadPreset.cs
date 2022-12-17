using System;
using Verse;

namespace EdB.PrepareCarefully;

public class Dialog_LoadPreset : Dialog_Preset {
    private readonly Action<string> action;

    public Dialog_LoadPreset(Action<string> action) {
        this.action = action;
        interactButLabel = "EdB.PC.Dialog.Preset.Button.Load".Translate();
    }

    protected override void DoMapEntryInteraction(string mapName) {
        if (action != null) {
            action(mapName);
        }

        Close();
    }
}
