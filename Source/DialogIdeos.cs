using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class DialogIdeos : Window {
    private Vector2 scrollPosition_ideoDetails;

    private Vector2 scrollPosition_ideoList;

    private float scrollViewHeight_ideoDetails;

    private float scrollViewHeight_ideoList;

    public DialogIdeos() {
        doCloseButton = true;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public CustomPawn Pawn { get; set; }

    public override Vector2 InitialSize => new(1010f, Mathf.Min(1000f, UI.screenHeight));

    public override void DoWindowContents(Rect inRect) {
        IdeoUIUtility.DoIdeoListAndDetails(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - CloseButSize.y),
            ref scrollPosition_ideoList, //ref Vector2 scrollPosition_list
            ref scrollViewHeight_ideoList, //ref float scrollViewHeight_list
            ref scrollPosition_ideoDetails, //ref Vector2 scrollPosition_details
            ref scrollViewHeight_ideoDetails, //ref float scrollViewHeight_details
            false, //bool editMode = false
            false, //bool showCreateIdeoButton = false
            null, //List<Pawn> pawns = null
            null, //Ideo onlyEditIdeo = null
            null, //Action createCustomBtnActOverride = null
            false, //bool forArchonexusRestart = false
            null, //(Pawn p) => Pawn.Pawn.ideo?.Ideo,//Func<Pawn, Ideo> pawnIdeoGetter = null
            null, //Action<Ideo> ideoLoadedFromFile = null
            false, //bool showLoadExistingIdeoBtn = false
            false //Action createFluidBtnAct = null
        );
    }

    public override void PreOpen() {
        base.PreOpen();
        IdeoUIUtility.selected = Pawn.Pawn.ideo?.Ideo;
    }

    public override void PostClose() {
        base.PostClose();
        Pawn.Pawn.ideo.SetIdeo(IdeoUIUtility.selected);
    }
}
