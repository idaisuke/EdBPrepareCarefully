using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public abstract class PanelPawnList : PanelBase {
    public delegate void AddFactionPawnHandler(FactionDef def, bool startingPawn);

    public delegate void AddPawnHandler(bool startingPawn);

    public delegate void AddPawnWithPawnKindHandler(PawnKindDef def, bool startingPawn);

    public delegate void DeletePawnHandler(CustomPawn pawn);

    public delegate void LoadCharacterHandler(string name);

    public delegate void MaximizeHandler();

    public delegate void MinimizeHandler();

    public delegate void SelectPawnHandler(CustomPawn pawn);

    public delegate void SwapPawnHandler(CustomPawn pawn);

    protected LabelTrimmer descriptionTrimmerNoScrollbar = new();
    protected LabelTrimmer descriptionTrimmerWithScrollbar = new();

    protected LabelTrimmer nameTrimmerNoScrollbar = new();
    protected LabelTrimmer nameTrimmerWithScrollbar = new();
    protected FactionDef previousFaction = null;
    protected Rect RectButtonAdvancedAdd;
    protected Rect RectButtonDelete;
    protected Rect RectButtonLoad;

    protected Rect RectButtonQuickAdd;
    protected Rect RectButtonSwap;
    protected Rect RectDescription;
    protected Rect RectEntry;
    protected Rect RectHeader;
    protected Rect RectMaximize;
    protected Rect RectMinimize;
    protected Rect RectName;
    protected Rect RectPawn;
    protected Rect RectPortrait;
    protected Rect RectPortraitClip;
    protected Rect RectScrollFrame;
    protected Rect RectScrollView;

    protected List<WidgetTable<PawnKindDef>.RowGroup> rowGroups = new();
    protected ScrollViewVertical scrollView = new();
    protected float SizeEntrySpacing = 8;

    public override Color ColorPanelBackground => Style.ColorPanelBackgroundDeep;

    public override string PanelHeader => base.PanelHeader;

    protected abstract bool StartingPawns {
        get;
    }

    protected abstract bool CanDeleteLastPawn {
        get;
    }

    public bool PawnKindRaceDiversificationEnabled =>
        ModsConfig.ActiveModsInLoadOrder?.FirstOrDefault(m =>
            m.PackageId == "solidwires.pawnkindracediversification") != null;

    public event SelectPawnHandler PawnSelected;
    public event DeletePawnHandler PawnDeleted;
    public event SwapPawnHandler PawnSwapped;
    public event AddPawnHandler AddingPawn;
    public event AddPawnWithPawnKindHandler AddingPawnWithPawnKind;
    public event MaximizeHandler Maximize;
    public event LoadCharacterHandler CharacterLoaded;

    public override void Resize(Rect rect) {
        base.Resize(rect);

        var panelPadding = new Vector2(6, 6);
        var entryPadding = new Vector2(4, 4);
        float buttonHeight = 22;

        var width = PanelRect.width - (panelPadding.x * 2);
        var height = PanelRect.height - (panelPadding.y * 2);

        float headerHeight = 36;
        RectScrollFrame = new Rect(panelPadding.x, headerHeight, width + 1,
            height - panelPadding.y - headerHeight - buttonHeight + 6);
        RectScrollView = new Rect(0, 0, RectScrollFrame.width, RectScrollFrame.height);

        var buttonWidth = (width / 2f) - (entryPadding.x / 2f);
        RectButtonQuickAdd = new Rect(PanelRect.width - 27, 10, 16, 16);
        RectButtonLoad = new Rect(panelPadding.x, height - buttonHeight + 6, buttonWidth, buttonHeight);
        RectButtonAdvancedAdd = new Rect(panelPadding.x + buttonWidth + entryPadding.x, RectButtonLoad.y, buttonWidth,
            buttonHeight);

        float portraitWidth = 68;
        var portraitHeight = Mathf.Floor(portraitWidth * 1.4f);
        RectPortrait = new Rect(-15, -14, portraitWidth, portraitHeight);
        RectPortrait = new Rect(-14, -13, 64, 90);

        RectEntry = new Rect(0, 0, width, 48);
        RectPortraitClip = new Rect(RectEntry.x, RectEntry.y - 8, RectEntry.width, RectEntry.height + 8);
        RectName = new Rect(44, 8, 92, 22);
        nameTrimmerNoScrollbar.Width = RectName.width;
        nameTrimmerWithScrollbar.Width = RectName.width - 16;
        RectDescription = new Rect(RectName.x, RectName.yMax - 6, RectName.width, 18);
        descriptionTrimmerNoScrollbar.Width = RectDescription.width;
        descriptionTrimmerWithScrollbar.Width = RectDescription.width - 16;
        RectButtonDelete = new Rect(RectEntry.xMax - 18, 6, 12, 12);
        RectButtonSwap = new Rect(RectEntry.xMax - 20, RectEntry.yMax - 20, 16, 16);

        var resizeButtonSize = new Vector2(18, 18);
        RectMinimize = new Rect(rect.width - 25, 4, resizeButtonSize.x, resizeButtonSize.y);
        RectMaximize = new Rect(rect.width - 25, 9, resizeButtonSize.x, resizeButtonSize.y);
        RectHeader = new Rect(0, 0, rect.width, headerHeight);
    }

    protected override void DrawPanelContent(State state) {
        /*
        // Test code for adjusting the size and position of the portrait.
        if (Event.current.type == EventType.KeyDown) {
            if (Event.current.shift) {
                if (Event.current.keyCode == KeyCode.LeftArrow) {
                    float portraitWidth = RectPortrait.width;
                    portraitWidth -= 1f;
                    float portraitHeight = Mathf.Floor(portraitWidth * 1.4f);
                    RectPortrait = new Rect(RectPortrait.x, RectPortrait.y, portraitWidth, portraitHeight);
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
                else if (Event.current.keyCode == KeyCode.RightArrow) {
                    float portraitWidth = RectPortrait.width;
                    portraitWidth += 1f;
                    float portraitHeight = Mathf.Floor(portraitWidth * 1.4f);
                    RectPortrait = new Rect(RectPortrait.x, RectPortrait.y, portraitWidth, portraitHeight);
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
            }
            else {
                if (Event.current.keyCode == KeyCode.LeftArrow) {
                    RectPortrait = RectPortrait.OffsetBy(new Vector2(-1, 0));
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
                else if (Event.current.keyCode == KeyCode.RightArrow) {
                    RectPortrait = RectPortrait.OffsetBy(new Vector2(1, 0));
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
                else if (Event.current.keyCode == KeyCode.UpArrow) {
                    RectPortrait = RectPortrait.OffsetBy(new Vector2(0, -1));
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
                else if (Event.current.keyCode == KeyCode.DownArrow) {
                    RectPortrait = RectPortrait.OffsetBy(new Vector2(0, 1));
                    Logger.Debug("RectPortrait = " + RectPortrait);
                }
            }
        }
        */
        base.DrawPanelContent(state);

        var currentPawn = state.CurrentPawn;
        CustomPawn pawnToSelect = null;
        CustomPawn pawnToSwap = null;
        CustomPawn pawnToDelete = null;
        var pawns = GetPawnListFromState(state);
        var colonistCount = pawns.Count();

        if (IsMinimized(state)) {
            // Count label.
            Text.Font = GameFont.Medium;
            var headerWidth = Text.CalcSize(PanelHeader).x;
            var countRect = new Rect(10 + headerWidth + 3, 3, 50, 27);
            GUI.color = Style.ColorTextPanelHeader;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(countRect, "EdB.PC.Panel.PawnList.PawnCount".Translate(colonistCount));
            GUI.color = Color.white;

            // Maximize button.
            if (RectHeader.Contains(Event.current.mousePosition)) {
                GUI.color = Style.ColorButtonHighlight;
            }
            else {
                GUI.color = Style.ColorButton;
            }

            GUI.DrawTexture(RectMaximize, IsTopPanel() ? Textures.TextureMaximizeDown : Textures.TextureMaximizeUp);
            if (Widgets.ButtonInvisible(RectHeader, false)) {
                SoundDefOf.ThingSelected.PlayOneShotOnCamera();
                Maximize();
            }

            return;
        }

        float cursor = 0;
        GUI.BeginGroup(RectScrollFrame);
        scrollView.Begin(RectScrollView);
        try {
            var nameTrimmer = scrollView.ScrollbarsVisible ? nameTrimmerWithScrollbar : nameTrimmerNoScrollbar;
            var descriptionTrimmer = scrollView.ScrollbarsVisible
                ? descriptionTrimmerWithScrollbar
                : descriptionTrimmerNoScrollbar;
            foreach (var pawn in pawns) {
                var selected = pawn == currentPawn;
                var rect = RectEntry;
                rect.y += cursor;
                rect.width -= scrollView.ScrollbarsVisible ? 16 : 0;

                GUI.color = Style.ColorPanelBackground;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                GUI.color = Color.white;

                if (selected || rect.Contains(Event.current.mousePosition)) {
                    if (selected) {
                        GUI.color = new Color(66f / 255f, 66f / 255f, 66f / 255f);
                        Widgets.DrawBox(rect);
                    }

                    GUI.color = Color.white;
                    var deleteRect = RectButtonDelete.OffsetBy(rect.position);
                    deleteRect.x = deleteRect.x - (scrollView.ScrollbarsVisible ? 16 : 0);
                    if (CanDeleteLastPawn || colonistCount > 1) {
                        Style.SetGUIColorForButton(deleteRect);
                        GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
                        // For some reason, this GUI.Button call is causing weirdness with text field focus (select
                        // text in one of the name fields and hover over the pawns in the pawn list to see what I mean).
                        // Replacing it with a mousedown event check fixes it for some reason.
                        //if (GUI.Button(deleteRect, string.Empty, Widgets.EmptyStyle)) {
                        if (Event.current.type == EventType.MouseDown &&
                            deleteRect.Contains(Event.current.mousePosition)) {
                            // Shift-click skips the confirmation dialog
                            if (Event.current.shift) {
                                // Delete after we've iterated and drawn everything
                                pawnToDelete = pawn;
                            }
                            else {
                                var localPawn = pawn;
                                Find.WindowStack.Add(
                                    new Dialog_Confirm("EdB.PC.Panel.PawnList.Delete.Confirm".Translate(),
                                        delegate {
                                            PawnDeleted(localPawn);
                                        },
                                        true, null, true)
                                );
                            }
                        }

                        GUI.color = Color.white;
                    }

                    if (rect.Contains(Event.current.mousePosition)) {
                        var swapRect = RectButtonSwap.OffsetBy(rect.position);
                        swapRect.x -= scrollView.ScrollbarsVisible ? 16 : 0;
                        if (CanDeleteLastPawn || colonistCount > 1) {
                            Style.SetGUIColorForButton(swapRect);
                            GUI.DrawTexture(swapRect,
                                pawn.Type == CustomPawnType.Colonist
                                    ? Textures.TextureButtonWorldPawn
                                    : Textures.TextureButtonColonyPawn);
                            if (Event.current.type == EventType.MouseDown &&
                                swapRect.Contains(Event.current.mousePosition)) {
                                pawnToSwap = pawn;
                            }

                            GUI.color = Color.white;
                        }
                    }
                }

                var pawnRect = RectPortrait.OffsetBy(rect.position);
                GUI.color = Color.white;
                var pawnTexture = pawn.GetPortrait(pawnRect.size);
                var clipRect = RectEntry.OffsetBy(rect.position);
                try {
                    GUI.BeginClip(clipRect);
                    GUI.DrawTexture(RectPortrait, pawnTexture);
                }
                finally {
                    GUI.EndClip();
                }

                GUI.color = new Color(238f / 255f, 238f / 255f, 238f / 255f);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.LowerLeft;
                var nameRect = RectName.OffsetBy(rect.position);
                nameRect.width = nameRect.width - (scrollView.ScrollbarsVisible ? 16 : 0);
                var nameSize = Text.CalcSize(pawn.Pawn.LabelShort);
                Widgets.Label(nameRect, nameTrimmer.TrimLabelIfNeeded(pawn.Pawn.LabelShort));

                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = new Color(184f / 255f, 184f / 255f, 184f / 255f);
                var professionRect = RectDescription.OffsetBy(rect.position);
                professionRect.width = professionRect.width - (scrollView.ScrollbarsVisible ? 16 : 0);
                string description = null;
                if (pawn.IsAdult) {
                    if (pawn.Adulthood != null) {
                        description = pawn.Adulthood.TitleShortFor(pawn.Gender).CapitalizeFirst();
                    }
                }
                else {
                    description = pawn.Childhood.TitleShortFor(pawn.Gender).CapitalizeFirst();
                }

                if (!description.NullOrEmpty()) {
                    Widgets.Label(professionRect, descriptionTrimmer.TrimLabelIfNeeded(description));
                }

                if (pawn != state.CurrentPawn && Event.current.type == EventType.MouseDown &&
                    rect.Contains(Event.current.mousePosition) && pawnToSwap == null) {
                    pawnToSelect = pawn;
                }

                cursor += rect.height + SizeEntrySpacing;
            }

            cursor -= SizeEntrySpacing;
        }
        finally {
            scrollView.End(cursor);
            GUI.EndGroup();
        }


        // Quick Add button.
        if (RectButtonQuickAdd.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(RectButtonQuickAdd, Textures.TextureButtonAdd);
        if (Widgets.ButtonInvisible(RectButtonQuickAdd, false)) {
            SoundDefOf.Click.PlayOneShotOnCamera();
            AddingPawn(StartingPawns);
        }

        GUI.color = Color.white;
        Text.Font = GameFont.Tiny;

        // Load button
        if (Widgets.ButtonText(RectButtonLoad, "EdB.PC.Panel.PawnList.Load".Translate(), true, false)) {
            Find.WindowStack.Add(new Dialog_LoadColonist(
                name => {
                    CharacterLoaded(name);
                }
            ));
        }

        // Advanced Add button
        if (Widgets.ButtonText(RectButtonAdvancedAdd, "EdB.PC.Panel.PawnList.Add".Translate(), true, false)) {
            ShowPawnKindDialog();
        }

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;

        if (pawnToDelete != null) {
            PawnDeleted(pawnToDelete);
        }
        else if (pawnToSwap != null) {
            PawnSwapped(pawnToSwap);
        }
        else if (pawnToSelect != null) {
            PawnSelected(pawnToSelect);
        }
    }

    protected abstract bool IsMaximized(State state);

    protected abstract bool IsMinimized(State state);

    protected abstract List<CustomPawn> GetPawnListFromState(State state);

    protected abstract bool IsTopPanel();

    public void SelectPawn(CustomPawn pawn) {
        PawnSelected(pawn);
    }

    public IEnumerable<PawnKindDef> ColonyKindDefs(PawnKindDef basicKind, IEnumerable<PawnKindDef> factionKinds) {
        if (basicKind != null) {
            yield return basicKind;
        }

        if (factionKinds != null) {
            foreach (var f in factionKinds) {
                if (f != basicKind) {
                    yield return f;
                }
            }
        }
    }

    public IEnumerable<PawnKindDef> AllPawnKinds(PawnKindDef basicKind, IEnumerable<PawnKindDef> factionKinds) {
        if (basicKind != null) {
            yield return basicKind;
        }

        if (factionKinds != null) {
            foreach (var f in factionKinds) {
                if (f != basicKind) {
                    yield return f;
                }
            }
        }
    }

    protected void ShowPawnKindDialog() {
        var disabled = new HashSet<PawnKindDef>();
        rowGroups.Clear();

        var selected = PrepareCarefully.Instance.State.LastSelectedPawnKindDef;

        var factionPawnKindsList =
            new List<ProviderPawnKinds.FactionPawnKinds>(PrepareCarefully.Instance.Providers.PawnKinds
                .PawnKindsByFaction);
        // Sort the pawn kinds to put the colony faction at the top.
        factionPawnKindsList.Sort((a, b) => {
            if (a.Faction == Find.FactionManager.OfPlayer.def && b.Faction != Find.FactionManager.OfPlayer.def) {
                return -1;
            }

            if (b.Faction == Find.FactionManager.OfPlayer.def && a.Faction != Find.FactionManager.OfPlayer.def) {
                return 1;
            }

            return string.Compare(a.Faction.LabelCap, b.Faction.LabelCap);
        });
        //Logger.Debug(String.Join("\n", factionPawnKindsList.Select(k => k.Faction.LabelCap + ", " + k.Faction.defName)));

        // If no pawn kind has been selected, select the colony's basic pawn kind by default.
        if (selected == null) {
            selected = factionPawnKindsList?.FirstOrDefault(f => f != null)?.PawnKinds?.FirstOrDefault(k => k != null);
        }

        foreach (var factionPawnKinds in factionPawnKindsList) {
            if (factionPawnKinds.PawnKinds.Count > 0) {
                rowGroups.Add(new WidgetTable<PawnKindDef>.RowGroup(
                    "<b>" + factionPawnKinds.Faction.LabelCap.ToString() + "</b>", factionPawnKinds.PawnKinds));
            }
        }

        if (!PrepareCarefully.Instance.Providers.PawnKinds.PawnKindsWithNoFaction.EnumerableNullOrEmpty()) {
            rowGroups.Add(new WidgetTable<PawnKindDef>.RowGroup("<b>Other</b>",
                PrepareCarefully.Instance.Providers.PawnKinds.PawnKindsWithNoFaction));
        }

        var dialog = new DialogPawnKinds {
            HeaderLabel = "EdB.PC.Panel.PawnList.SelectFaction".Translate(),
            SelectAction = pawnKind => { selected = pawnKind; },
            RowGroups = rowGroups,
            DisabledOptions = disabled,
            CloseAction = () => {
                SoundDefOf.Click.PlayOneShotOnCamera();
                if (selected != null) {
                    PrepareCarefully.Instance.State.LastSelectedPawnKindDef = selected;
                    AddingPawnWithPawnKind(selected, StartingPawns);
                }
            },
            Selected = selected,
            ShowRace = PrepareCarefully.Instance.Providers.PawnKinds.AnyNonHumanPawnKinds &&
                       !PawnKindRaceDiversificationEnabled
        };
        dialog.ScrollTo(PrepareCarefully.Instance.State.LastSelectedPawnKindDef);
        Find.WindowStack.Add(dialog);
    }

    public void ScrollToBottom() {
        scrollView.ScrollToBottom();
    }
}
