using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PagePrepareCarefully : Page {
    public delegate void PresetHandler(string name);

    public delegate void StartGameHandler();

    private readonly List<TabRecord> tabRecords = new();
    private readonly TabViewEquipment tabViewEquipment;

    private readonly TabViewPawns tabViewPawns;
    private readonly TabViewRelationships tabViewRelationships;
    private readonly List<ITabView> tabViews = new();

    private Controller controller;

    private float? costLabelWidth;
    private bool pawnListActionThisFrame;

    public PagePrepareCarefully(State state) {
        closeOnCancel = false;
        closeOnAccept = false;
        closeOnClickedOutside = false;
        doCloseButton = false;
        doCloseX = false;

        LargeUI = false;

        tabViewPawns = new TabViewPawns(LargeUI);
        tabViewEquipment = new TabViewEquipment();
        tabViewRelationships = new TabViewRelationships();

        // Add the tab views to the tab view list.
        tabViews.Add(tabViewPawns);
        tabViews.Add(tabViewRelationships);
        tabViews.Add(tabViewEquipment);

        // Create a tab record UI widget for each tab view.
        foreach (var tab in tabViews) {
            var currentTab = tab;
            var tabRecord = new TabRecord(currentTab.Name, delegate {
                // When a new tab is selected, mark the previously selected TabRecord as unselected and the current one as selected.
                // Also, update the State to reflected the newly selected ITabView.
                if (State.CurrentTab != null) {
                    State.CurrentTab.TabRecord.selected = false;
                }

                State.CurrentTab = currentTab;
                currentTab.TabRecord.selected = true;
            }, false);
            currentTab.TabRecord = tabRecord;
            tabRecords.Add(tabRecord);
        }

        controller = new Controller(state);
        GameStarted += controller.StartGame;
        PresetLoaded += controller.LoadPreset;
        PresetSaved += controller.SavePreset;
    }

    private bool LargeUI { get; }

    private static State State => PrepareCarefully.Instance.State;

    private static Configuration Config => PrepareCarefully.Instance.Config;

    public override string PageTitle => "EdB.PC.Page.Title".Translate();

    public event StartGameHandler GameStarted;
    public event PresetHandler PresetLoaded;
    public event PresetHandler PresetSaved;

    public override void OnAcceptKeyPressed() {
        // Don't close the window if the user clicks the "enter" key.
    }

    public override void OnCancelKeyPressed() {
        // Confirm that the user wants to quit if they click the escape key.
        ConfirmExit();
    }

    public override void PreOpen() {
        base.PreOpen();
        //Logger.Debug("windowRect: " + windowRect);

        // Set the default tab view to the first tab and the selected pawn to the first pawn.
        State.CurrentTab = tabViews[0];
        State.CurrentColonyPawn = PrepareCarefully.Instance.ColonyPawns.FirstOrDefault();
        State.CurrentWorldPawn = PrepareCarefully.Instance.WorldPawns.FirstOrDefault();

        costLabelWidth = null;
        controller = new Controller(State);
        InstrumentPanels();
    }


    public override void DoWindowContents(Rect inRect) {
        pawnListActionThisFrame = false;
        DrawPageTitle(inRect);
        var mainRect = GetMainRect(inRect, 30f);
        Widgets.DrawMenuSection(mainRect);
        TabDrawer.DrawTabs(mainRect, tabRecords);

        // Determine the size of the tab view and draw the current tab.
        var sizePageMargins = new Vector2(16, 16);
        var tabViewRect = new Rect(mainRect.x + sizePageMargins.x, mainRect.y + sizePageMargins.y,
            mainRect.width - (sizePageMargins.x * 2), mainRect.height - (sizePageMargins.y * 2));
        State.CurrentTab?.Draw(State, tabViewRect);

        // Display any pending messages.
        if (State.Messages.Any()) {
            foreach (var message in State.Messages) {
                Messages.Message(message, MessageTypeDefOf.NeutralEvent);
            }

            State.ClearMessages();
        }

        // Display any pending errors.
        if (State.Errors.Any()) {
            foreach (var message in State.Errors) {
                Messages.Message(message, MessageTypeDefOf.RejectInput);
            }

            State.ClearErrors();
        }

        // Draw other controls.
        DrawPresetButtons(inRect);
        DrawPoints(mainRect);
        DoNextBackButtons(inRect, "Start".Translate(),
            delegate {
                if (controller.CanDoNext()) {
                    ShowStartConfirmation();
                }
            },
            ConfirmExit
        );

        PrepareCarefully.Instance.EquipmentDatabase.LoadFrame();
    }

    private void ConfirmExit() {
        Find.WindowStack.Add(new Dialog_Confirm("EdB.PC.Page.ConfirmExit".Translate(), delegate {
            PrepareCarefully.Instance.Clear();
            PrepareCarefully.ClearVanillaFriendlyScenario();
            Close();
        }, true, "", true));
    }

    private static void DoNextBackButtons(Rect innerRect, string nextLabel, Action nextAct, Action backAct) {
        var top = innerRect.height - 38;
        Text.Font = GameFont.Small;

        var rect = new Rect(0, top, BottomButSize.x, BottomButSize.y);
        if (Widgets.ButtonText(rect, "Back".Translate(), true, false)) {
            backAct();
        }

        var rect2 = new Rect(innerRect.width - BottomButSize.x, top, BottomButSize.x, BottomButSize.y);
        if (Widgets.ButtonText(rect2, nextLabel, true, false)) {
            nextAct();
        }
    }

    private void ShowStartConfirmation() {
        // Show the missing required work dialog if necessary.  Otherwise, just show the standard confirmation.
        if (State.MissingWorkTypes.Count > 0) {
            var stringBuilder = new StringBuilder();
            foreach (var current in State.MissingWorkTypes) {
                if (stringBuilder.Length > 0) {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append("  - " + current.CapitalizeFirst());
            }

            string text = "ConfirmRequiredWorkTypeDisabledForEveryone".Translate(stringBuilder.ToString());
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate {
                GameStarted();
            }));
        }
        else {
            Find.WindowStack.Add(new Dialog_Confirm("EdB.PC.Page.ConfirmStart".Translate(), delegate {
                GameStarted();
            }, false, "あああ", true));
        }
    }

    private void DrawPresetButtons(Rect rect) {
        GUI.color = Color.white;
        var middle = rect.width / 2f;
        var top = rect.height - 38;
        //float middle = this.windowRect.width / 2f;
        const float buttonWidth = 150;
        const float buttonSpacing = 24;
        if (Widgets.ButtonText(new Rect(middle - buttonWidth - (buttonSpacing / 2), top, buttonWidth, 38),
                "EdB.PC.Page.Button.LoadPreset".Translate(), true, false)) {
            Find.WindowStack.Add(new Dialog_LoadPreset(name => {
                PresetLoaded(name);
            }));
        }

        if (Widgets.ButtonText(new Rect(middle + (buttonSpacing / 2), top, buttonWidth, 38),
                "EdB.PC.Page.Button.SavePreset".Translate(), true, false)) {
            Find.WindowStack.Add(new Dialog_SavePreset(name => {
                PresetSaved(name);
            }));
        }

        GUI.color = Color.white;
    }

    private void DrawPoints(Rect parentRect) {
        Text.Anchor = TextAnchor.UpperRight;
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        try {
            if (costLabelWidth == null) {
                var max = Int32.MaxValue.ToString();
                string translated1 = "EdB.PC.Page.Points.Spent".Translate(max);
                string translated2 = "EdB.PC.Page.Points.Remaining".Translate(max);
                costLabelWidth = Mathf.Max(Text.CalcSize(translated1).x, Text.CalcSize(translated2).x);
            }

            var cost = PrepareCarefully.Instance.Cost;
            string label;
            if (Config.pointsEnabled) {
                var points = PrepareCarefully.Instance.PointsRemaining;
                GUI.color = points < 0 ? Color.yellow : Style.ColorText;

                label = "EdB.PC.Page.Points.Remaining".Translate(points);
            }
            else {
                var points = cost.total;
                GUI.color = Style.ColorText;
                label = "EdB.PC.Page.Points.Spent".Translate(points);
            }

            var rect = new Rect(parentRect.width - costLabelWidth.Value, 2, costLabelWidth.Value, 32);
            Widgets.Label(rect, label);

            var tooltipText = "";
            tooltipText += "EdB.PC.Page.Points.ScenarioPoints".Translate(PrepareCarefully.Instance.StartingPoints);
            tooltipText += "\n\n";
            foreach (var c in cost.colonistDetails) {
                tooltipText +=
                    "EdB.PC.Page.Points.CostSummary.Colonist".Translate(c.name, c.total - c.apparel - c.bionics) +
                    "\n";
            }

            tooltipText += "\n" + "EdB.PC.Page.Points.CostSummary.Apparel".Translate(cost.colonistApparel) + "\n"
                           + "EdB.PC.Page.Points.CostSummary.Implants".Translate(cost.colonistBionics) + "\n"
                           + "EdB.PC.Page.Points.CostSummary.Equipment".Translate(cost.equipment) + "\n\n"
                           + "EdB.PC.Page.Points.CostSummary.Total".Translate(cost.total);
            var tip = new TipSignal(() => tooltipText, tooltipText.GetHashCode());
            TooltipHandler.TipRegion(rect, tip);

            GUI.color = Style.ColorText;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            var optionTop = rect.y;
            string optionLabel = "EdB.PC.Page.Points.UsePoints".Translate();
            var size = Text.CalcSize(optionLabel);
            var optionRect = new Rect(parentRect.width - costLabelWidth.Value - size.x - 100, optionTop,
                size.x + 10, 32);
            Widgets.Label(optionRect, optionLabel);
            GUI.color = Color.white;
            TooltipHandler.TipRegion(optionRect, "EdB.PC.Page.Points.UsePoints.Tip".Translate());
            Widgets.Checkbox(new Vector2(optionRect.x + optionRect.width, optionRect.y - 3),
                ref Config.pointsEnabled);
        }
        finally {
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }
    }

    private void SelectPawn(CustomPawn pawn) {
        if (pawnListActionThisFrame) {
            return;
        }

        pawnListActionThisFrame = true;
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        controller.SubcontrollerCharacters.SelectPawn(pawn);
        tabViewPawns.PanelName.ClearSelection();
        tabViewPawns.PanelColumn1?.ScrollToTop();
        tabViewPawns.PanelColumn2?.ScrollToTop();
        tabViewPawns.PanelSkills.ScrollToTop();
        tabViewPawns.PanelAppearance.UpdatePawnLayers();
    }

    private void SwapPawn(CustomPawn pawn) {
        if (!pawnListActionThisFrame) {
            pawnListActionThisFrame = true;
            SoundDefOf.ThingSelected.PlayOneShotOnCamera();
            controller.SubcontrollerCharacters.SwapPawn(pawn);
            var newMode = PrepareCarefully.Instance.State.PawnListMode == PawnListMode.ColonyPawnsMaximized
                ? PawnListMode.WorldPawnsMaximized
                : PawnListMode.ColonyPawnsMaximized;
            PrepareCarefully.Instance.State.PawnListMode = newMode;
            tabViewPawns.ResizeTabView();
            if (newMode == PawnListMode.ColonyPawnsMaximized) {
                tabViewPawns.PanelColonyPawns.ScrollToBottom();
            }
            else {
                tabViewPawns.PanelWorldPawns.ScrollToBottom();
            }
        }
    }

    private void InstrumentPanels() {
        var state = PrepareCarefully.Instance.State;

        // Instrument the characters tab view.
        var pawnController = controller.SubcontrollerCharacters;

        tabViewPawns.PanelAge.BiologicalAgeUpdated += pawnController.UpdateBiologicalAge;
        tabViewPawns.PanelAge.ChronologicalAgeUpdated += pawnController.UpdateChronologicalAge;

        tabViewPawns.PanelAppearance.RandomizeAppearance += pawnController.RandomizeAppearance;
        tabViewPawns.PanelAppearance.GenderUpdated += gender => {
            pawnController.UpdateGender(gender);
            tabViewPawns.PanelAppearance.UpdatePawnLayers();
        };

        tabViewPawns.PanelBackstory.BackstoryUpdated += pawnController.UpdateBackstory;
        tabViewPawns.PanelBackstory.BackstoryUpdated += (_, _) => {
            pawnController.CheckPawnCapabilities();
        };
        tabViewPawns.PanelBackstory.BackstoriesRandomized += pawnController.RandomizeBackstories;
        tabViewPawns.PanelBackstory.BackstoriesRandomized += () => { pawnController.CheckPawnCapabilities(); };

        tabViewPawns.PanelColonyPawns.PawnSelected += SelectPawn;
        tabViewPawns.PanelColonyPawns.AddingPawn += pawnController.AddingPawn;
        tabViewPawns.PanelColonyPawns.AddingPawnWithPawnKind += pawnController.AddFactionPawn;
        tabViewPawns.PanelColonyPawns.PawnDeleted += pawnController.DeletePawn;
        tabViewPawns.PanelColonyPawns.PawnDeleted += _ => {
            pawnController.CheckPawnCapabilities();
        };
        tabViewPawns.PanelColonyPawns.PawnSwapped += SwapPawn;
        tabViewPawns.PanelColonyPawns.CharacterLoaded += pawnController.LoadCharacter;
        tabViewPawns.PanelColonyPawns.CharacterLoaded += _ => {
            pawnController.CheckPawnCapabilities();
        };

        tabViewPawns.PanelWorldPawns.PawnSelected += SelectPawn;
        tabViewPawns.PanelWorldPawns.AddingPawn += pawnController.AddingPawn;
        tabViewPawns.PanelWorldPawns.AddingPawnWithPawnKind += pawnController.AddFactionPawn;
        tabViewPawns.PanelWorldPawns.PawnDeleted += pawnController.DeletePawn;
        tabViewPawns.PanelWorldPawns.PawnDeleted += _ => {
            pawnController.CheckPawnCapabilities();
        };
        tabViewPawns.PanelWorldPawns.PawnSwapped += SwapPawn;
        tabViewPawns.PanelWorldPawns.CharacterLoaded += pawnController.LoadCharacter;
        tabViewPawns.PanelWorldPawns.CharacterLoaded += _ => {
            pawnController.CheckPawnCapabilities();
        };

        tabViewPawns.PanelColonyPawns.Maximize += () => {
            state.PawnListMode = PawnListMode.ColonyPawnsMaximized;
            tabViewPawns.ResizeTabView();
        };
        tabViewPawns.PanelWorldPawns.Maximize += () => {
            state.PawnListMode = PawnListMode.WorldPawnsMaximized;
            tabViewPawns.ResizeTabView();
        };

        pawnController.PawnAdded += pawn => {
            PanelPawnList pawnList;
            if (pawn.Type == CustomPawnType.Colonist) {
                pawnList = tabViewPawns.PanelColonyPawns;
            }
            else {
                pawnList = tabViewPawns.PanelWorldPawns;
            }

            pawnList.ScrollToBottom();
            pawnList.SelectPawn(pawn);
        };
        pawnController.PawnAdded += _ => { pawnController.CheckPawnCapabilities(); };
        pawnController.PawnReplaced += _ => { pawnController.CheckPawnCapabilities(); };

        tabViewPawns.PanelHealth.InjuryAdded += pawnController.AddInjury;
        tabViewPawns.PanelHealth.ImplantAdded += pawnController.AddImplant;
        tabViewPawns.PanelHealth.HediffRemoved += pawnController.RemoveHediff;

        tabViewPawns.PanelName.FirstNameUpdated += pawnController.UpdateFirstName;
        tabViewPawns.PanelName.NickNameUpdated += pawnController.UpdateNickName;
        tabViewPawns.PanelName.LastNameUpdated += pawnController.UpdateLastName;
        tabViewPawns.PanelName.NameRandomized += pawnController.RandomizeName;

        tabViewPawns.PanelRandomize.RandomizeAllClicked += pawnController.RandomizeAll;
        tabViewPawns.PanelRandomize.RandomizeAllClicked += () => { pawnController.CheckPawnCapabilities(); };

        tabViewPawns.PanelFavoriteColor.FavoriteColorUpdated += pawnController.UpdateFavoriteColor;

        //tabViewPawns.PanelSaveLoad.CharacterLoaded += pawnController.LoadCharacter;
        //tabViewPawns.PanelSaveLoad.CharacterLoaded += (string filename) => { pawnController.CheckPawnCapabilities(); };
        tabViewPawns.PanelSaveLoad.CharacterSaved += pawnController.SaveCharacter;

        tabViewPawns.PanelSkills.SkillLevelUpdated += pawnController.UpdateSkillLevel;
        tabViewPawns.PanelSkills.SkillPassionUpdated += pawnController.UpdateSkillPassion;
        tabViewPawns.PanelSkills.SkillsReset += pawnController.ResetSkills;
        tabViewPawns.PanelSkills.SkillsCleared += pawnController.ClearSkills;

        tabViewPawns.PanelTraits.TraitAdded += pawnController.AddTrait;
        tabViewPawns.PanelTraits.TraitUpdated += pawnController.UpdateTrait;
        tabViewPawns.PanelTraits.TraitRemoved += pawnController.RemoveTrait;
        tabViewPawns.PanelTraits.TraitsRandomized += pawnController.RandomizeTraits;

        tabViewPawns.PanelAbilities.AbilityAdded += pawnController.AddAbility;
        tabViewPawns.PanelAbilities.AbilityRemoved += pawnController.RemoveAbility;
        tabViewPawns.PanelAbilities.AbilitiesSet += pawnController.SetAbilities;

        // Instrument the equipment tab view.
        var equipment = controller.SubcontrollerEquipment;

        tabViewEquipment.PanelAvailable.EquipmentAdded += equipment.AddEquipment;
        tabViewEquipment.PanelAvailable.EquipmentAdded += tabViewEquipment.PanelSelected.EquipmentAdded;

        tabViewEquipment.PanelSelected.EquipmentRemoved += equipment.RemoveEquipment;
        tabViewEquipment.PanelSelected.EquipmentCountUpdated += equipment.UpdateEquipmentCount;

        // Instrument the relationships tab view.
        var relationships = controller.SubcontrollerRelationships;
        tabViewRelationships.PanelRelationshipsOther.RelationshipAdded += relationships.AddRelationship;
        tabViewRelationships.PanelRelationshipsOther.RelationshipRemoved += relationships.RemoveRelationship;
        tabViewRelationships.PanelRelationshipsParentChild.ParentAddedToGroup +=
            relationships.AddParentToParentChildGroup;
        tabViewRelationships.PanelRelationshipsParentChild.ChildAddedToGroup +=
            relationships.AddChildToParentChildGroup;
        tabViewRelationships.PanelRelationshipsParentChild.ParentRemovedFromGroup +=
            relationships.RemoveParentFromParentChildGroup;
        tabViewRelationships.PanelRelationshipsParentChild.ChildRemovedFromGroup +=
            relationships.RemoveChildFromParentChildGroup;
        tabViewRelationships.PanelRelationshipsParentChild.GroupAdded += relationships.AddParentChildGroup;
        tabViewPawns.PanelColonyPawns.PawnDeleted += relationships.DeleteAllPawnRelationships;
        pawnController.PawnAdded += relationships.AddPawn;
        pawnController.PawnReplaced += relationships.ReplacePawn;
    }
}
