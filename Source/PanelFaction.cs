using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelFaction : PanelModule {
    public static readonly float FieldPadding = 6;
    protected Field FieldFaction = new();

    public Rect FieldRect;
    protected LabelTrimmer labelTrimmer = new();
    protected ProviderFactions providerFactions = PrepareCarefully.Instance.Providers.Factions;

    protected List<WidgetTable<CustomFaction>.RowGroup> rowGroups = new();

    public override void Resize(float width) {
        base.Resize(width);
        FieldRect = new Rect(FieldPadding, 0, width - (FieldPadding * 2), Style.FieldHeight);
    }

    public float Measure() {
        return 0;
    }

    public override bool IsVisible(State state) {
        return state.CurrentPawn.Type != CustomPawnType.Colonist;
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "Faction".Translate().Resolve());

        var pawn = state.CurrentPawn;
        FieldFaction.Rect = FieldRect.OffsetBy(0, y);
        labelTrimmer.Rect = FieldFaction.Rect.InsetBy(8, 0);
        if (pawn.Type == CustomPawnType.Colonist) {
            FieldFaction.Label = "Colony".Translate();
            FieldFaction.Enabled = false;
            FieldFaction.ClickAction = () => { };
        }
        else {
            FieldFaction.Label =
                labelTrimmer.TrimLabelIfNeeded(pawn.Faction != null
                    ? pawn.Faction.Name
                    : providerFactions.RandomFaction.Name);
            FieldFaction.Enabled = true;
            FieldFaction.ClickAction = () => {
                ShowFactionDialog(pawn);
            };
        }

        FieldFaction.Draw();
        y += FieldRect.height;

        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;


        y += Margin.y;
        return y - top;
    }

    protected void ShowFactionDialog(CustomPawn pawn) {
        var selectedFaction = pawn.Faction != null
            ? pawn.Faction
            : PrepareCarefully.Instance.Providers.Factions.RandomFaction;
        var disabled = new HashSet<CustomFaction>();
        rowGroups.Clear();
        rowGroups.Add(new WidgetTable<CustomFaction>.RowGroup(
            "<b>" + "EdB.PC.Dialog.Faction.SelectRandomFaction".Translate() + "</b>",
            providerFactions.RandomCustomFactions));
        rowGroups.Add(new WidgetTable<CustomFaction>.RowGroup(
            "<b>" + "EdB.PC.Dialog.Faction.SelectSpecificFaction".Translate() + "</b>",
            providerFactions.SpecificCustomFactions));
        rowGroups.Add(new WidgetTable<CustomFaction>.RowGroup(
            "<b>" + "EdB.PC.Dialog.Faction.SelectLeaderFaction".Translate() + "</b>",
            providerFactions.LeaderCustomFactions));
        var factionDialog = new DialogFactions {
            HeaderLabel = "EdB.PC.Dialog.Faction.ChooseFaction".Translate(),
            SelectAction = faction => { selectedFaction = faction; },
            RowGroups = rowGroups,
            DisabledFactions = disabled,
            CloseAction = () => {
                pawn.Faction = selectedFaction;
            },
            SelectedFaction = selectedFaction
        };
        factionDialog.ScrollTo(selectedFaction);
        Find.WindowStack.Add(factionDialog);
    }
}
