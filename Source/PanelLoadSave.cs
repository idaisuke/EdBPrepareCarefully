using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelLoadSave : PanelBase {
    public delegate void SaveCharacterHandler(CustomPawn pawn, string name);

    protected Rect RectButtonSave;

    public event SaveCharacterHandler CharacterSaved;

    public override void Resize(Rect rect) {
        base.Resize(rect);

        float panelPadding = 12;
        var buttonWidth = PanelRect.width - (panelPadding * 2);
        float buttonHeight = 38;
        var top = (PanelRect.height * 0.5f) - (buttonHeight * 0.5f);
        RectButtonSave = new Rect(panelPadding, top, buttonWidth, buttonHeight);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);
        var currentPawn = state.CurrentPawn;
        if (Widgets.ButtonText(RectButtonSave, "EdB.PC.Panel.LoadSave.Save".Translate(), true, false)) {
            Find.WindowStack.Add(new Dialog_SaveColonist(state.CurrentPawn.Pawn.LabelShort,
                file => {
                    CharacterSaved(currentPawn, file);
                }
            ));
        }
    }
}
