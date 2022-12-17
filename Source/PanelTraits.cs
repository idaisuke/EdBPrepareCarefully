using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class PanelTraits : PanelModule {
    public delegate void AddTraitHandler(Trait trait);

    public delegate void RandomizeTraitsHandler();

    public delegate void RemoveTraitHandler(Trait trait);

    public delegate void UpdateTraitHandler(int index, Trait trait);

    public static readonly float FieldPadding = 6;

    private readonly ProviderTraits providerTraits = new();
    protected Dictionary<Trait, string> conflictingTraitList = new();
    protected HashSet<TraitDef> disallowedTraitDefs = new();
    public Rect FieldRect;
    protected List<Field> fields = new();
    protected ScrollViewVertical scrollView = new();
    protected TipCache tipCache = new();
    protected List<Trait> traitsToRemove = new();

    public event RandomizeTraitsHandler TraitsRandomized;
    public event AddTraitHandler TraitAdded;
    public event UpdateTraitHandler TraitUpdated;
    public event RemoveTraitHandler TraitRemoved;

    public override void Resize(float width) {
        base.Resize(width);
        FieldRect = new Rect(FieldPadding, 0, width - (FieldPadding * 2), Style.FieldHeight);
    }

    public float Measure() {
        return 0;
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "Traits".Translate().Resolve());

        var currentPawn = state.CurrentPawn;
        var index = 0;
        Action clickAction = null;
        foreach (var trait in currentPawn.Traits) {
            if (index > 0) {
                y += FieldPadding;
            }

            if (index >= fields.Count) {
                fields.Add(new Field());
            }

            var field = fields[index];

            var fieldRect = FieldRect.OffsetBy(0, y);
            field.Rect = fieldRect;
            var fieldClickRect = fieldRect;
            fieldClickRect.width -= 36;
            field.ClickRect = fieldClickRect;

            if (trait != null) {
                field.Label = trait.LabelCap;
                field.Tip = GetTraitTip(trait, currentPawn);
            }
            else {
                field.Label = null;
                field.Tip = null;
            }

            var localTrait = trait;
            var localIndex = index;
            field.ClickAction = () => {
                var originalTrait = localTrait;
                var selectedTrait = originalTrait;
                ComputeDisallowedTraits(currentPawn, originalTrait);
                var dialog = new Dialog_Options<Trait>(providerTraits.Traits) {
                    NameFunc = t => {
                        return t.LabelCap;
                    },
                    DescriptionFunc = t => {
                        return GetTraitTip(t, currentPawn);
                    },
                    SelectedFunc = t => {
                        if ((selectedTrait == null || t == null) && selectedTrait != t) {
                            return false;
                        }

                        return selectedTrait.def == t.def && selectedTrait.Label == t.Label;
                    },
                    SelectAction = t => {
                        selectedTrait = t;
                    },
                    EnabledFunc = t => {
                        return !disallowedTraitDefs.Contains(t.def);
                    },
                    CloseAction = () => {
                        TraitUpdated(localIndex, selectedTrait);
                    },
                    NoneSelectedFunc = () => {
                        return selectedTrait == null;
                    },
                    SelectNoneAction = () => {
                        selectedTrait = null;
                    }
                };
                Find.WindowStack.Add(dialog);
            };
            field.PreviousAction = () => {
                var capturedIndex = index;
                clickAction = () => {
                    SelectPreviousTrait(currentPawn, capturedIndex);
                };
            };
            field.NextAction = () => {
                var capturedIndex = index;
                clickAction = () => {
                    SelectNextTrait(currentPawn, capturedIndex);
                };
            };
            field.Draw();

            // Remove trait button.
            var deleteRect = new Rect(field.Rect.xMax - 32, field.Rect.y + field.Rect.HalfHeight() - 6, 12, 12);
            if (deleteRect.Contains(Event.current.mousePosition)) {
                GUI.color = Style.ColorButtonHighlight;
            }
            else {
                GUI.color = Style.ColorButton;
            }

            GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
            if (Widgets.ButtonInvisible(deleteRect, false)) {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                traitsToRemove.Add(trait);
            }

            index++;

            y += FieldRect.height;
        }

        tipCache.MakeReady();

        // If the index is still zero, then the pawn has no traits.  Draw the "none" label.
        if (index == 0) {
            GUI.color = Style.ColorText;
            Widgets.Label(FieldRect.InsetBy(6, 0).OffsetBy(0, y - 4), "EdB.PC.Panel.Traits.None".Translate());
            y += FieldRect.height - 4;
        }

        GUI.color = Color.white;

        // Fire any action that was triggered
        if (clickAction != null) {
            clickAction();
            clickAction = null;
        }

        // Randomize traits button.
        var randomizeRect = new Rect(Width - 32, top + 9, 22, 22);
        if (randomizeRect.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(randomizeRect, Textures.TextureButtonRandom);
        if (Widgets.ButtonInvisible(randomizeRect, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            tipCache.Invalidate();
            TraitsRandomized();
        }

        // Add trait button.
        var addRect = new Rect(randomizeRect.x - 24, top + 12, 16, 16);
        Style.SetGUIColorForButton(addRect);
        var traitCount = state.CurrentPawn.Traits.Count();
        var addButtonEnabled = state.CurrentPawn != null && traitCount < Constraints.MaxTraits;
        if (!addButtonEnabled) {
            GUI.color = Style.ColorButtonDisabled;
        }

        GUI.DrawTexture(addRect, Textures.TextureButtonAdd);
        if (addButtonEnabled && Widgets.ButtonInvisible(addRect, false)) {
            ComputeDisallowedTraits(currentPawn, null);
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            Trait selectedTrait = null;
            var dialog = new Dialog_Options<Trait>(providerTraits.Traits) {
                ConfirmButtonLabel = "EdB.PC.Common.Add".Translate(),
                NameFunc = t => {
                    return t.LabelCap;
                },
                DescriptionFunc = t => {
                    return GetTraitTip(t, state.CurrentPawn);
                },
                SelectedFunc = t => {
                    return selectedTrait == t;
                },
                SelectAction = t => {
                    selectedTrait = t;
                },
                EnabledFunc = t => {
                    return !disallowedTraitDefs.Contains(t.def);
                },
                CloseAction = () => {
                    if (selectedTrait != null) {
                        TraitAdded(selectedTrait);
                        tipCache.Invalidate();
                    }
                }
            };
            Find.WindowStack.Add(dialog);
        }

        // Remove any traits that were marked for deletion
        if (traitsToRemove.Count > 0) {
            foreach (var trait in traitsToRemove) {
                TraitRemoved(trait);
            }

            traitsToRemove.Clear();
            tipCache.Invalidate();
        }

        y += Margin.y;
        return y - top;
    }


    protected string GetTraitTip(Trait trait, CustomPawn pawn) {
        if (!tipCache.Ready || !tipCache.Lookup.ContainsKey(trait)) {
            var value = GenerateTraitTip(trait, pawn);
            tipCache.Lookup.Add(trait, value);
            return value;
        }

        return tipCache.Lookup[trait];
    }

    protected string GenerateTraitTip(Trait trait, CustomPawn pawn) {
        try {
            var baseTip = trait.TipString(pawn.Pawn);
            string conflictingNames = null;
            if (!conflictingTraitList.TryGetValue(trait, out conflictingNames)) {
                var conflictingTraits = providerTraits.Traits.Where(t => {
                    return trait.def.conflictingTraits.Contains(t.def) ||
                           (t.def == trait.def && t.Label != trait.Label);
                }).ToList();
                if (conflictingTraits.Count == 0) {
                    conflictingTraitList.Add(trait, null);
                }
                else {
                    conflictingNames = "";
                    if (conflictingTraits.Count == 1) {
                        conflictingNames =
                            "EdB.PC.Panel.Traits.Tip.Conflict.List.1".Translate(conflictingTraits[0].LabelCap);
                    }
                    else if (conflictingTraits.Count == 2) {
                        conflictingNames =
                            "EdB.PC.Panel.Traits.Tip.Conflict.List.2".Translate(conflictingTraits[0].LabelCap,
                                conflictingTraits[1].LabelCap);
                    }
                    else {
                        var c = conflictingTraits.Count;
                        conflictingNames =
                            "EdB.PC.Panel.Traits.Tip.Conflict.List.Last".Translate(conflictingTraits[c - 2].LabelCap,
                                conflictingTraits[c - 1].LabelCap);
                        for (var i = c - 3; i >= 0; i--) {
                            conflictingNames =
                                "EdB.PC.Panel.Traits.Tip.Conflict.List.Many".Translate(conflictingTraits[i].LabelCap,
                                    conflictingNames);
                        }
                    }

                    conflictingTraitList.Add(trait, conflictingNames);
                }
            }

            if (conflictingNames == null) {
                return baseTip;
            }

            return "EdB.PC.Panel.Traits.Tip.Conflict".Translate(baseTip, conflictingNames).Resolve();
        }
        catch (Exception e) {
            Logger.Warning("There was an error when trying to generate a mouseover tip for trait {" +
                           (trait?.LabelCap ?? "null") + "}\n" + e);
            return null;
        }
    }

    protected void ComputeDisallowedTraits(CustomPawn customPawn, Trait traitToReplace) {
        disallowedTraitDefs.Clear();
        foreach (var t in customPawn.Traits) {
            if (t == traitToReplace) {
                continue;
            }

            disallowedTraitDefs.Add(t.def);
            if (t.def.conflictingTraits != null) {
                foreach (var c in t.def.conflictingTraits) {
                    disallowedTraitDefs.Add(c);
                }
            }
        }
    }

    protected void SelectNextTrait(CustomPawn customPawn, int traitIndex) {
        var currentTrait = customPawn.GetTrait(traitIndex);
        ComputeDisallowedTraits(customPawn, currentTrait);
        var index = -1;
        if (currentTrait != null) {
            index = providerTraits.Traits.FindIndex(t => {
                return t.Label.Equals(currentTrait.Label);
            });
        }

        var count = 0;
        do {
            index++;
            if (index >= providerTraits.Traits.Count) {
                index = 0;
            }

            if (++count > providerTraits.Traits.Count + 1) {
                index = -1;
                break;
            }
        } while (index != -1 && (customPawn.HasTrait(providerTraits.Traits[index]) ||
                                 disallowedTraitDefs.Contains(providerTraits.Traits[index].def)));

        Trait newTrait = null;
        if (index > -1) {
            newTrait = providerTraits.Traits[index];
        }

        TraitUpdated(traitIndex, newTrait);
    }

    protected void SelectPreviousTrait(CustomPawn customPawn, int traitIndex) {
        var currentTrait = customPawn.GetTrait(traitIndex);
        ComputeDisallowedTraits(customPawn, currentTrait);
        var index = -1;
        if (currentTrait != null) {
            index = providerTraits.Traits.FindIndex(t => {
                return t.Label.Equals(currentTrait.Label);
            });
        }

        var count = 0;
        do {
            index--;
            if (index < 0) {
                index = providerTraits.Traits.Count - 1;
            }

            if (++count > providerTraits.Traits.Count + 1) {
                index = -1;
                break;
            }
        } while (index != -1 && (customPawn.HasTrait(providerTraits.Traits[index]) ||
                                 disallowedTraitDefs.Contains(providerTraits.Traits[index].def)));

        Trait newTrait = null;
        if (index > -1) {
            newTrait = providerTraits.Traits[index];
        }

        TraitUpdated(traitIndex, newTrait);
    }

    protected void ClearTrait(CustomPawn customPawn, int traitIndex) {
        TraitUpdated(traitIndex, null);
        tipCache.Invalidate();
    }

    public class TipCache {
        public Dictionary<Trait, string> Lookup = new();
        private CustomPawn pawn;

        public bool Ready { get; private set; }

        public void CheckPawn(CustomPawn pawn) {
            if (this.pawn != pawn) {
                this.pawn = pawn;
                Invalidate();
            }
        }

        public void Invalidate() {
            Ready = false;
            Lookup.Clear();
        }

        public void MakeReady() {
            Ready = true;
        }
    }
}
