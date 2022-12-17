using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelFavoriteColor : PanelBase {
    public delegate void RandomizeFavoriteColorHandler();

    public delegate void UpdateFavoriteColorHandler(Color? color);

    public override string PanelHeader => null;

    public event UpdateFavoriteColorHandler FavoriteColorUpdated;

    public override void Resize(Rect rect) {
        base.Resize(rect);
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);

        var rect = new Rect(8, 8, PanelRect.width - 16, PanelRect.height - 16);
        var favoriteColor = state.CurrentPawn.Pawn.story.favoriteColor.HasValue
            ? state.CurrentPawn.Pawn.story.favoriteColor.Value
            : new Color(0.5f, 0.5f, 0.5f);

        if (rect.Contains(Event.current.mousePosition)) {
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        GUI.color = favoriteColor;
        GUI.DrawTexture(rect.ContractedBy(1), BaseContent.WhiteTex);

        if (Widgets.ButtonInvisible(rect, false)) {
            var dialog = new DialogFavoriteColor(favoriteColor) {
                ConfirmAction = color => FavoriteColorUpdated(color)
            };
            Find.WindowStack.Add(dialog);
        }

        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}
