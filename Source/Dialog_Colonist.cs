using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public abstract class Dialog_Colonist : Window {
    protected const float DeleteButtonSpace = 5;
    protected const float MapDateExtraLeftMargin = 220;

    protected const float MapEntrySpacing = 8;
    protected const float BoxMargin = 20;
    protected const float MapNameExtraLeftMargin = 15;
    protected const float MapEntryMargin = 6;

    private static readonly Color ManualSaveTextColor = new(1, 1, 0.6f);
    private static readonly Color AutosaveTextColor = new(0.75f, 0.75f, 0.75f);
    protected float bottomAreaHeight;

    protected string interactButLabel = "Error";

    private Vector2 scrollPosition = Vector2.zero;

    public Dialog_Colonist() {
        closeOnCancel = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
    }

    public override Vector2 InitialSize => new(600, 700);

    protected abstract void DoMapEntryInteraction(string mapName);

    protected virtual void DoSpecialSaveLoadGUI(Rect inRect) {
    }

    public override void PostClose() {
        GUI.FocusControl(null);
    }

    public override void DoWindowContents(Rect inRect) {
        var vector = new Vector2(inRect.width - 16, 36);
        var vector2 = new Vector2(100, vector.y - 6);
        inRect.height -= 45;
        var list = ColonistFiles.AllFiles.ToList();
        var num = vector.y + 3;
        var height = list.Count * num;
        var viewRect = new Rect(0, 0, inRect.width - 16, height);
        var outRect = new Rect(inRect.AtZero());
        outRect.height -= bottomAreaHeight;
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        try {
            float num2 = 0;
            var num3 = 0;
            foreach (var current in list) {
                var rect = new Rect(0, num2, vector.x, vector.y);
                if (num3 % 2 == 0) {
                    GUI.DrawTexture(rect, Textures.TextureAlternateRow);
                }

                var innerRect = new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6);
                GUI.BeginGroup(innerRect);
                try {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.Name);
                    GUI.color = ManualSaveTextColor;
                    var rect2 = new Rect(15, 0, innerRect.width, innerRect.height);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Text.Font = GameFont.Small;
                    Widgets.Label(rect2, fileNameWithoutExtension);
                    GUI.color = Color.white;
                    var rect3 = new Rect(250, 0, innerRect.width, innerRect.height);
                    Text.Font = GameFont.Tiny;
                    GUI.color = new Color(1, 1, 1, 0.5f);
                    Widgets.Label(rect3, current.LastWriteTime.ToString("g"));
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    var num4 = vector.x - 6 - vector2.x - vector2.y;
                    var butRect = new Rect(num4, 0, vector2.x, vector2.y);
                    if (Widgets.ButtonText(butRect, interactButLabel, true, false)) {
                        DoMapEntryInteraction(Path.GetFileNameWithoutExtension(current.Name));
                    }

                    var rect4 = new Rect(num4 + vector2.x + 5, 0, vector2.y, vector2.y);
                    if (Widgets.ButtonImage(rect4, Textures.TextureDeleteX)) {
                        var localFile = current;
                        Find.UIRoot.windows.Add(new Dialog_Confirm(
                            "EdB.PC.Dialog.PawnPreset.ConfirmDelete".Translate(localFile.Name), delegate {
                                localFile.Delete();
                            }, true, null, true));
                    }

                    TooltipHandler.TipRegion(rect4, "EdB.PC.Dialog.PawnPreset.DeleteTooltip".Translate());
                }
                finally {
                    GUI.EndGroup();
                }

                num2 += vector.y + 3;
                num3++;
            }
        }
        finally {
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        DoSpecialSaveLoadGUI(inRect.AtZero());
    }
}
