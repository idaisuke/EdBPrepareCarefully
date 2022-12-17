using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelAbilities : PanelModule {
    public delegate void AddAbilityHandler(AbilityDef abilityDef);

    public delegate void RemoveAbilityHandler(Ability ability);

    public delegate void SetAbilitiesHandler(IEnumerable<AbilityDef> abilityDefs);

    public static readonly Vector2 FieldPadding = new(6, 6);
    public Rect CertaintyLabelRect;
    public Rect CertaintySliderRect;
    protected HashSet<AbilityDef> disallowed = new();
    protected Field FieldFaction = new();

    public Rect FieldRect;
    public Rect IconRect;
    protected List<Ability> itemsToRemove = new();
    protected LabelTrimmer labelTrimmer = new();
    public Rect NoneRect;

    public event AddAbilityHandler AbilityAdded;
    public event SetAbilitiesHandler AbilitiesSet;
    public event RemoveAbilityHandler AbilityRemoved;

    public override void Resize(float width) {
        base.Resize(width);
        //FieldRect = new Rect(FieldPadding.x, 0, width - FieldPadding.x * 2, 36);
        float iconPadding = 2;
        float extraPaddingForDeleteButton = 16;
        IconRect = new Rect(iconPadding, iconPadding, 48, 48);
        FieldRect = new Rect(0, 0, (iconPadding * 2) + extraPaddingForDeleteButton + IconRect.width,
            (iconPadding * 2) + IconRect.height);
        NoneRect = new Rect(12, 0, width - 24, 32);
    }

    public float Measure() {
        return 0;
    }

    public override bool IsVisible(State state) {
        return base.IsVisible(state);
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "Abilities".Translate().CapitalizeFirst().Resolve());

        var pawn = state.CurrentPawn;
        var abilityTracker = pawn.Pawn.abilities;

        Action clickAction = null;
        var index = 0;
        var x = FieldPadding.x;
        foreach (var ability in abilityTracker.abilities) {
            if (ability == null) {
                continue;
            }

            GUI.color = Color.white;

            if (x + FieldRect.width > Width - FieldPadding.x) {
                x = FieldPadding.x;
                y += FieldRect.height + FieldPadding.y;
            }

            var fieldRect = FieldRect.OffsetBy(x, y);
            Widgets.DrawAtlas(fieldRect, Textures.TextureFieldAtlas);

            var iconRect = new Rect(fieldRect.x + IconRect.x, fieldRect.y + IconRect.y, IconRect.width,
                IconRect.height);
            if (Mouse.IsOver(iconRect)) {
                Widgets.DrawHighlight(iconRect);
            }

            GUI.DrawTexture(iconRect, ability.def.uiIcon);
            if (Widgets.ButtonInvisible(iconRect, false)) {
                clickAction = () => Find.WindowStack.Add(new Dialog_InfoCard(ability.def));
            }

            TooltipHandler.TipRegion(iconRect, ability.Tooltip + "\n\n" + "ClickToLearnMore".Translate());

            // Remove ability button.
            var deleteRect = new Rect(fieldRect.xMax - 15, fieldRect.y + 4, 12, 12);
            if (deleteRect.Contains(Event.current.mousePosition)) {
                GUI.color = Style.ColorButtonHighlight;
            }
            else {
                GUI.color = Style.ColorButton;
            }

            GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
            if (Widgets.ButtonInvisible(deleteRect, false)) {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                itemsToRemove.Add(ability);
            }

            GUI.color = Color.white;

            index++;
            x += FieldRect.width + FieldPadding.x;
        }

        // If the index is still zero, then the pawn has no abilities.  Draw the "none" label.
        if (index == 0) {
            GUI.color = Style.ColorText;
            Widgets.Label(NoneRect.OffsetBy(0, y - 4), "EdB.PC.Panel.Abilities.None".Translate());
            y += NoneRect.height;
        }
        else {
            y += FieldRect.height + FieldPadding.y;
        }

        GUI.color = Color.white;

        // Fire any action that was triggered
        if (clickAction != null) {
            clickAction();
            clickAction = null;
        }

        // Add button
        var addRect = new Rect(Width - 24, top + 12, 16, 16);
        Style.SetGUIColorForButton(addRect);
        var traitCount = state.CurrentPawn.Traits.Count();
        GUI.DrawTexture(addRect, Textures.TextureButtonAdd);
        if (Widgets.ButtonInvisible(addRect, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            disallowed.Clear();
            if (pawn.Pawn?.abilities?.abilities != null) {
                disallowed.AddRange(pawn.Pawn.abilities.abilities.Select(a => a.def));
            }

            var dialog = new DialogAbilities(pawn) {
                HeaderLabel = "EdB.PC.Dialog.Abilities.Header".Translate(),
                CloseAction = abilities => {
                    AbilitiesSet(abilities);
                }
            };
            Find.WindowStack.Add(dialog);
        }

        // Remove any items that were marked for deletion
        if (itemsToRemove.Count > 0) {
            foreach (var ability in itemsToRemove) {
                AbilityRemoved(ability);
            }

            itemsToRemove.Clear();
        }

        y += Margin.y;
        return y - top;
    }
}
