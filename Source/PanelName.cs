using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelName : PanelBase {
    public delegate void RandomizeNameHandler();

    public delegate void UpdateNameHandler(string name);

    protected Rect RectFirstName;
    protected Rect RectInfo;
    protected Rect RectLastName;
    protected Rect RectNickName;
    protected Rect RectRandomize;

    public event UpdateNameHandler FirstNameUpdated;
    public event UpdateNameHandler NickNameUpdated;
    public event UpdateNameHandler LastNameUpdated;
    public event RandomizeNameHandler NameRandomized;

    public override void Resize(Rect rect) {
        base.Resize(rect);

        float panelPadding = 12;
        float fieldPadding = 4;
        float fieldHeight = 28;
        var SizeRandomize = new Vector2(22, 22);
        var SizeInfo = new Vector2(24, 24);
        RectRandomize = new Rect(PanelRect.width - panelPadding - SizeRandomize.y,
            PanelRect.HalfHeight() - (SizeRandomize.y * 0.5f), SizeRandomize.x, SizeRandomize.y);
        RectInfo = new Rect(panelPadding, PanelRect.HalfHeight() - SizeInfo.HalfY(), SizeInfo.x, SizeInfo.y);

        var availableSpace = PanelRect.width - (panelPadding * 2) - RectInfo.width - RectRandomize.width - fieldPadding;

        float firstMinWidth = 80;
        float nickMinWidth = 90;
        float lastMinWidth = 90;
        var fieldsMinWidth = firstMinWidth + nickMinWidth + lastMinWidth + (fieldPadding * 2);
        var extraSpace = availableSpace - fieldsMinWidth;
        var extraForField = Mathf.Floor(extraSpace / 3);
        var top = PanelRect.HalfHeight() - (fieldHeight * 0.5f);

        RectFirstName = new Rect(RectInfo.xMax, top, firstMinWidth + extraForField, fieldHeight);
        RectNickName = new Rect(RectFirstName.xMax + fieldPadding, top, nickMinWidth + extraForField, fieldHeight);
        RectLastName = new Rect(RectNickName.xMax + fieldPadding, top, lastMinWidth + extraForField, fieldHeight);

        // Shift the info button to the left a bit to making the spacing look better.
        RectInfo.x -= 6;
    }

    protected override void DrawPanelContent(State state) {
        var customPawn = state.CurrentPawn;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        Style.SetGUIColorForButton(RectInfo);
        GUI.DrawTexture(RectInfo, Textures.TextureButtonInfo);
        if (Widgets.ButtonInvisible(RectInfo)) {
            Find.WindowStack.Add(new Dialog_InfoCard(customPawn.Pawn));
        }

        GUI.color = Color.white;

        var first = customPawn.FirstName;
        var nick = customPawn.NickName;
        var last = customPawn.LastName;
        string text;
        GUI.SetNextControlName("PrepareCarefullyFirst");
        text = Widgets.TextField(RectFirstName, first);
        if (text != first) {
            FirstNameUpdated(text);
        }

        if (nick == first || nick == last) {
            GUI.color = new Color(1, 1, 1, 0.5f);
        }

        GUI.SetNextControlName("PrepareCarefullyNick");
        text = Widgets.TextField(RectNickName, nick);
        if (text != nick) {
            NickNameUpdated(text);
        }

        GUI.color = Color.white;
        GUI.SetNextControlName("PrepareCarefullyLast");
        text = Widgets.TextField(RectLastName, last);
        if (text != last) {
            LastNameUpdated(text);
        }

        TooltipHandler.TipRegion(RectFirstName, "FirstNameDesc".Translate());
        TooltipHandler.TipRegion(RectNickName, "ShortIdentifierDesc".Translate());
        TooltipHandler.TipRegion(RectLastName, "LastNameDesc".Translate());

        // Random button
        Style.SetGUIColorForButton(RectRandomize);
        GUI.DrawTexture(RectRandomize, Textures.TextureButtonRandom);
        if (Widgets.ButtonInvisible(RectRandomize, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            GUI.FocusControl(null);
            NameRandomized();
        }
    }

    public void ClearSelection() {
        GUI.FocusControl(null);
    }
}
