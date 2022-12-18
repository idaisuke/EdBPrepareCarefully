using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class DialogInitializationError : Window {
    public DialogInitializationError() {
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = true;
        doCloseButton = true;
    }

    public override Vector2 InitialSize => new(500f, 400f);

    public override void DoWindowContents(Rect inRect) {
        Text.Font = GameFont.Small;
        if (VersionControl.CurrentVersion < PrepareCarefully.MinimumGameVersion) {
            Widgets.Label(inRect,
                "EdB.PC.Error.GameVersion".Translate(VersionControl.CurrentVersionString,
                    PrepareCarefully.MinimumGameVersion.ToString()));
        }
        else {
            Widgets.Label(inRect, "EdB.PC.Error.Initialization".Translate());
        }
    }
}
