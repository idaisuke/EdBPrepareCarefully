using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Enumerable = System.Linq.Enumerable;

namespace EdB.PrepareCarefully;

public class PanelBackstory : PanelModule {
    public delegate void RandomizeBackstoriesHandler();

    public delegate void UpdateBackstoryHandler(BackstorySlot slot, Backstory backstory);

    protected List<Filter<Backstory>> activeFilters = new();
    protected List<Filter<Backstory>> availableFilters = new();
    protected Field FieldAdulthood = new();
    protected Field FieldChildhood = new();
    protected ProviderBackstories providerBackstories = PrepareCarefully.Instance.Providers.Backstories;

    public PanelBackstory() {
        availableFilters.Add(new FilterBackstoryMatchesFaction());
        availableFilters.Add(new FilterBackstoryNoDisabledWorkTypes());
        availableFilters.Add(new FilterBackstoryNoPenalties());
        foreach (var s in DefDatabase<SkillDef>.AllDefs) {
            availableFilters.Add(new FilterBackstorySkillAdjustment(s, 1));
            availableFilters.Add(new FilterBackstorySkillAdjustment(s, 3));
            availableFilters.Add(new FilterBackstorySkillAdjustment(s, 5));
        }
    }

    protected Rect LabelRect { get; set; }
    protected Rect FieldRect { get; set; }

    public event UpdateBackstoryHandler BackstoryUpdated;
    public event RandomizeBackstoriesHandler BackstoriesRandomized;

    public override void Resize(float width) {
        base.Resize(width);
        float panelPadding = 12;
        float fieldPadding = 8;

        // The width of the label is the widest of the childhood/adulthood text
        var savedFont = Text.Font;
        Text.Font = GameFont.Small;
        var sizeChildhood = Text.CalcSize("Childhood".Translate());
        var sizeAdulthood = Text.CalcSize("Adulthood".Translate());
        Text.Font = savedFont;
        var labelWidth = Mathf.Max(sizeChildhood.x, sizeAdulthood.x);

        LabelRect = new Rect(panelPadding, 0, labelWidth, Style.FieldHeight);
        FieldRect = new Rect(LabelRect.xMax + fieldPadding, 0, width - LabelRect.xMax - (fieldPadding * 2),
            Style.FieldHeight);
    }

    public float Measure() {
        return 0;
    }

    protected float DrawChildhood(CustomPawn pawn, float y, float width) {
        // Draw the label
        Text.Font = GameFont.Small;
        GUI.color = Style.ColorText;
        Text.Anchor = TextAnchor.MiddleCenter;
        var labelRect = LabelRect.OffsetBy(0, y);
        Widgets.Label(labelRect, "Childhood".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        // Draw the field
        FieldChildhood.Rect = FieldRect.OffsetBy(0, y);
        if (pawn.Childhood != null) {
            FieldChildhood.Label = pawn.Childhood.TitleCapFor(pawn.Gender);
        }
        else {
            FieldChildhood.Label = null;
        }

        FieldChildhood.Tip = pawn.Childhood.CheckedDescriptionFor(pawn.Pawn);
        FieldChildhood.ClickAction = () => {
            ShowBackstoryDialog(pawn, BackstorySlot.Childhood);
        };
        FieldChildhood.PreviousAction = () => {
            NextBackstory(pawn, BackstorySlot.Childhood, -1);
        };
        FieldChildhood.NextAction = () => {
            NextBackstory(pawn, BackstorySlot.Childhood, 1);
        };
        FieldChildhood.Draw();

        return FieldRect.height;
    }

    protected float DrawAdulthood(CustomPawn pawn, float y, float width) {
        Text.Font = GameFont.Small;
        GUI.color = Style.ColorText;
        Text.Anchor = TextAnchor.MiddleCenter;
        if (!pawn.HasAdulthoodBackstory) {
            GUI.color = Style.ColorControlDisabled;
        }

        var labelRect = LabelRect.OffsetBy(0, y);
        Widgets.Label(labelRect, "Adulthood".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        // Draw the field
        FieldAdulthood.Rect = FieldRect.OffsetBy(0, y);
        FieldAdulthood.Enabled = pawn.HasAdulthoodBackstory;
        if (FieldAdulthood.Enabled) {
            FieldAdulthood.Label = pawn.Adulthood.TitleCapFor(pawn.Gender);
            FieldAdulthood.Tip = pawn.Adulthood.CheckedDescriptionFor(pawn.Pawn);
            FieldAdulthood.ClickAction = () => {
                ShowBackstoryDialog(pawn, BackstorySlot.Adulthood);
            };
            FieldAdulthood.PreviousAction = () => {
                NextBackstory(pawn, BackstorySlot.Adulthood, -1);
            };
            FieldAdulthood.NextAction = () => {
                NextBackstory(pawn, BackstorySlot.Adulthood, 1);
            };
        }
        else {
            FieldAdulthood.Label = null;
            FieldAdulthood.Tip = null;
            FieldAdulthood.ClickAction = null;
            FieldAdulthood.PreviousAction = () => { };
            FieldAdulthood.NextAction = () => { };
        }

        FieldAdulthood.Draw();

        return FieldRect.height;
    }

    public void DrawRandomizeButton(float y, float width) {
        // Randomize button.
        var randomizeRect = new Rect(width - 32, y + 9, 22, 22);
        if (randomizeRect.Contains(Event.current.mousePosition)) {
            GUI.color = Style.ColorButtonHighlight;
        }
        else {
            GUI.color = Style.ColorButton;
        }

        GUI.DrawTexture(randomizeRect, Textures.TextureButtonRandom);
        if (Widgets.ButtonInvisible(randomizeRect, false)) {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            BackstoriesRandomized();
        }

        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    // Deprecated
    // Leave here for compatibility with any patches that used the old method for drawing
    protected override void DrawPanelContent(State state) {
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        var pawn = state.CurrentPawn;
        DrawRandomizeButton(y, Width);
        y += DrawHeader(y, Width, "Backstory".Translate().Resolve());
        y += DrawChildhood(pawn, y, Width);
        y += 6;
        y += DrawAdulthood(pawn, y, Width);

        y += Margin.y;

        // For backwards compatibility with any patches that used the old method for drawing
        GUI.BeginGroup(new Rect(0, top, Width, y - top));
        try {
            DrawPanelContent(state);
        }
        finally {
            GUI.EndGroup();
        }

        return y - top;
    }

    protected void ShowBackstoryDialog(CustomPawn customPawn, BackstorySlot slot) {
        Filter<Backstory> filterToRemove = null;
        var originalBackstory = slot == BackstorySlot.Childhood ? customPawn.Childhood : customPawn.Adulthood;
        var selectedBackstory = originalBackstory;
        var filterListDirtyFlag = true;
        var fullOptionsList = slot == BackstorySlot.Childhood
            ? providerBackstories.AllChildhookBackstories
            : providerBackstories.AllAdulthookBackstories;
        List<Backstory> filteredBackstories = new(fullOptionsList.Count);
        Dialog_Options<Backstory> dialog = new(filteredBackstories) {
            NameFunc = backstory => {
                return backstory.TitleCapFor(customPawn.Gender);
            },
            DescriptionFunc = backstory => {
                return backstory.CheckedDescriptionFor(customPawn.Pawn);
            },
            SelectedFunc = backstory => {
                return selectedBackstory == backstory;
            },
            SelectAction = backstory => {
                selectedBackstory = backstory;
            },
            CloseAction = () => {
                if (slot == BackstorySlot.Childhood) {
                    BackstoryUpdated(BackstorySlot.Childhood, selectedBackstory);
                }
                else {
                    BackstoryUpdated(BackstorySlot.Adulthood, selectedBackstory);
                }
            }
        };
        dialog.DrawHeader = rect => {
            if (filterToRemove != null) {
                activeFilters.Remove(filterToRemove);
                filterToRemove = null;
                filterListDirtyFlag = true;
            }

            if (filterListDirtyFlag) {
                filteredBackstories.Clear();
                filteredBackstories.AddRange(Enumerable.Where(fullOptionsList, p => {
                    foreach (var f in activeFilters) {
                        if (f.FilterFunction(p) == false) {
                            return false;
                        }
                    }

                    return true;
                }));
                filterListDirtyFlag = false;
                dialog.ScrollToTop();
            }

            float filterHeight = 18;
            float filterPadding = 4;
            var maxWidth = rect.width - 32;
            var cursor = new Vector2(0, 0);

            string addFilterLabel = "EdB.PC.Dialog.Backstory.Filter.Add".Translate();
            var width = Text.CalcSize(addFilterLabel).x;
            var addFilterRect = new Rect(rect.x, rect.y, width + 30, filterHeight);
            Widgets.DrawAtlas(addFilterRect, Textures.TextureFilterAtlas1);
            Text.Font = GameFont.Tiny;
            if (addFilterRect.Contains(Event.current.mousePosition)) {
                GUI.color = Style.ColorButtonHighlight;
            }
            else {
                GUI.color = Style.ColorText;
            }

            Widgets.Label(addFilterRect.InsetBy(10, 0, 20, 0).OffsetBy(0, 1), addFilterLabel);
            GUI.DrawTexture(new Rect(addFilterRect.xMax - 20, addFilterRect.y + 6, 11, 8),
                Textures.TextureDropdownIndicator);

            if (Widgets.ButtonInvisible(addFilterRect)) {
                var list = new List<FloatMenuOption>();
                foreach (var filter in availableFilters) {
                    if (activeFilters.FirstOrDefault(f => {
                            if (f == filter || f.ConflictsWith(filter)) {
                                return true;
                            }

                            return false;
                        }) == null) {
                        list.Add(new FloatMenuOption(filter.LabelFull, () => {
                            activeFilters.Add(filter);
                            filterListDirtyFlag = true;
                        }));
                    }
                }

                Find.WindowStack.Add(new FloatMenu(list, null));
            }

            cursor.x += addFilterRect.width + filterPadding;
            Text.Font = GameFont.Tiny;
            foreach (var filter in activeFilters) {
                GUI.color = Style.ColorText;
                var labelWidth = Text.CalcSize(filter.LabelShort).x;
                if (cursor.x + labelWidth > maxWidth) {
                    cursor.x = 0;
                    cursor.y += filterHeight + filterPadding;
                }

                var filterRect = new Rect(cursor.x, cursor.y, labelWidth + 30, filterHeight);
                Widgets.DrawAtlas(filterRect, Textures.TextureFilterAtlas2);
                var closeButtonRect = new Rect(filterRect.xMax - 15, filterRect.y + 5, 9, 9);
                if (filterRect.Contains(Event.current.mousePosition)) {
                    GUI.color = Style.ColorButtonHighlight;
                }
                else {
                    GUI.color = Style.ColorText;
                }

                Widgets.Label(filterRect.InsetBy(10, 0, 20, 0).OffsetBy(0, 1), filter.LabelShort);
                GUI.DrawTexture(closeButtonRect, Textures.TextureButtonCloseSmall);
                if (Widgets.ButtonInvisible(filterRect)) {
                    filterToRemove = filter;
                    filterListDirtyFlag = true;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                cursor.x += filterRect.width + filterPadding;
            }

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            return cursor.y + filterHeight + 4;
        };
        Find.WindowStack.Add(dialog);
    }

    protected void NextBackstory(CustomPawn pawn, BackstorySlot slot, int direction) {
        Backstory backstory;
        List<Backstory> backstories;
        PopulateBackstoriesFromSlot(pawn, slot, out backstories, out backstory);

        var currentIndex = FindBackstoryIndex(pawn, slot);
        currentIndex += direction;
        if (currentIndex >= backstories.Count) {
            currentIndex = 0;
        }
        else if (currentIndex < 0) {
            currentIndex = backstories.Count - 1;
        }

        BackstoryUpdated(slot, backstories[currentIndex]);
    }

    protected int FindBackstoryIndex(CustomPawn pawn, BackstorySlot slot) {
        Backstory backstory;
        List<Backstory> backstories;
        PopulateBackstoriesFromSlot(pawn, slot, out backstories, out backstory);
        return backstories.IndexOf(backstory);
    }

    protected void PopulateBackstoriesFromSlot(CustomPawn pawn, BackstorySlot slot, out List<Backstory> backstories,
        out Backstory backstory) {
        backstory = slot == BackstorySlot.Childhood ? pawn.Childhood : pawn.Adulthood;
        backstories = slot == BackstorySlot.Childhood
            ? providerBackstories.GetChildhoodBackstoriesForPawn(pawn)
            : providerBackstories.GetAdulthoodBackstoriesForPawn(pawn);
    }
}
