using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelTitles : PanelModule {
    public static readonly float FieldPadding = 6;
    protected Dictionary<Trait, string> conflictingTraitList = new();
    protected HashSet<TraitDef> disallowedTraitDefs = new();
    public Rect FieldRect;
    protected List<Field> fields = new();
    protected List<RoyalTitle> itemsToRemove = new();

    protected ProviderTitles providerTitles = new();
    protected ScrollViewVertical scrollView = new();
    protected TipCache tipCache = new();

    public override void Resize(float width) {
        base.Resize(width);
        FieldRect = new Rect(FieldPadding, 0, width - (FieldPadding * 2), 28);
    }

    public float Measure() {
        return 0;
    }

    public override bool IsVisible(State state) {
        return ModsConfig.RoyaltyActive;
    }

    public float DrawDialogGroupHeader(ProviderTitles.TitleSet group, float y, float width) {
        float factionIconSize = 24;
        float iconPadding = 4;
        var availableWidth = width - factionIconSize - iconPadding;

        var groupTitle = group.Faction.Name;
        var labelHeight = Text.CalcHeight(groupTitle, availableWidth);
        var labelRect = new Rect(factionIconSize + iconPadding, y, width, Mathf.Max(factionIconSize, labelHeight));
        Text.Anchor = TextAnchor.LowerLeft;
        Widgets.Label(labelRect, groupTitle);
        Text.Anchor = TextAnchor.UpperLeft;

        var iconRect = new Rect(0, labelRect.yMax - factionIconSize, factionIconSize, factionIconSize);
        GUI.DrawTexture(iconRect, group.Faction.def.FactionIcon);

        return labelRect.height;
    }

    public float DrawDialogGroupContent(Dictionary<Faction, int> favorLookup, ProviderTitles.TitleSet set, float y,
        float width) {
        if (!favorLookup.ContainsKey(set.Faction)) {
            Logger.Debug("Drawing dialog group content, but faction not found: " + favorLookup.Count);
            return 0;
        }

        var favorValue = favorLookup[set.Faction];
        Logger.Debug("Drawing dialog group content, and value was " + favorValue);

        var top = y;

        y += 8;
        var savedFont = Text.Font;
        Text.Font = GameFont.Tiny;
        var favorLabel = set.Faction.def.royalFavorLabel.CapitalizeFirst();
        var sizeLabel = Text.CalcSize(favorLabel);
        var textToMeasure = new string('5', set.MaxFavor.ToString().Length);
        var sizeValue = Text.CalcSize(textToMeasure);
        Text.Font = savedFont;
        var labelHeight = Math.Max(sizeLabel.y, sizeValue.y);

        float fieldPadding = 16;
        var labelRect = new Rect(fieldPadding, y, sizeLabel.x, labelHeight);
        var valueRect = new Rect(width - sizeValue.x - fieldPadding, y + 1, sizeValue.x, labelHeight);

        var sliderHeight = 8f;
        float sliderPadding = 8;
        var sliderWidth = valueRect.xMin - labelRect.xMax - (sliderPadding * 2f);
        var sliderRect = new Rect(labelRect.xMax + sliderPadding,
            labelRect.yMin + (labelRect.height * 0.5f) - (sliderHeight * 0.5f) - 1,
            sliderWidth, sliderHeight);

        // Draw the certainty slider
        Text.Font = GameFont.Tiny;
        GUI.color = Style.ColorText;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(labelRect, favorLabel);
        Text.Anchor = TextAnchor.MiddleRight;
        Text.Font = GameFont.Tiny;
        Widgets.Label(valueRect, favorLookup[set.Faction].ToString());
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
        var value = GUI.HorizontalSlider(sliderRect, favorValue, 0, set.MaxFavor);
        y += labelRect.height;

        favorLookup[set.Faction] = Mathf.FloorToInt(value);

        y += 12;

        return y - top;
    }

    protected IEnumerable<ProviderTitles.Title> FactionTitlesWithNoneOption(ProviderTitles.TitleSet titleSet) {
        yield return new ProviderTitles.Title { Faction = titleSet.Faction, Def = null };
        foreach (var title in titleSet.Titles) {
            yield return title;
        }
    }

    public void OpenOptionsDialog(CustomPawn pawn) {
        var selectedTitles = new Dictionary<Faction, RoyalTitleDef>();
        var favorLookup = new Dictionary<Faction, int>();
        foreach (var title in pawn.Pawn.royalty.AllTitlesForReading) {
            if (!selectedTitles.ContainsKey(title.faction)) {
                selectedTitles.Add(title.faction, title.def);
            }
        }

        foreach (var faction in Find.World.factionManager.AllFactionsInViewOrder) {
            var favor = pawn.Pawn.royalty.GetFavor(faction);
            favorLookup.Add(faction, favor);
        }

        Find.WindowStack.Add(new DialogTitles<ProviderTitles.TitleSet, ProviderTitles.Title> {
            Header = "EdB.PC.Dialog.Titles.Header".Translate(),
            Groups = () => providerTitles.Titles,
            GroupTitle = g => g.Faction.Name,
            DrawGroupHeader = DrawDialogGroupHeader,
            DrawGroupContent = (g, y, width) => DrawDialogGroupContent(favorLookup, g, y, width),
            OptionsFromGroup = g => FactionTitlesWithNoneOption(g),
            OptionTitle = o => {
                return o.Def != null ? o.Def.GetLabelCapFor(pawn.Pawn) : "None".Translate().CapitalizeFirst().Resolve();
            },
            IsSelected = o => {
                if (o.Def != null) {
                    var def = selectedTitles.GetOrDefault(o.Faction);
                    if (def != null && def == o.Def) {
                        return true;
                    }

                    return false;
                }

                return selectedTitles.GetOrDefault(o.Faction) == null;
            },
            Select = o => {
                if (selectedTitles.ContainsKey(o.Faction)) {
                    selectedTitles.Remove(o.Faction);
                }

                if (o.Def != null) {
                    selectedTitles.Add(o.Faction, o.Def);
                }
            },
            Confirm = () => {
                ConfigureTitles(pawn, selectedTitles, favorLookup);
            }
        });
    }

    protected void ConfigureTitles(CustomPawn pawn, Dictionary<Faction, RoyalTitleDef> selectedTitles,
        Dictionary<Faction, int> favorLookup) {
        var toRemove = new List<RoyalTitle>();
        foreach (var title in pawn.Pawn.royalty.AllTitlesForReading) {
            var def = selectedTitles.GetOrDefault(title.faction);
            if (def == null) {
                toRemove.Add(title);
            }
        }

        foreach (var title in toRemove) {
            pawn.Pawn.royalty.SetTitle(title.faction, null, false, false, false);
        }

        foreach (var pair in selectedTitles) {
            pawn.Pawn.royalty.SetTitle(pair.Key, pair.Value, false, false, false);
        }

        foreach (var pair in favorLookup) {
            pawn.Pawn.royalty.SetFavor(pair.Key, pair.Value, false);
        }

        pawn.ResetCachedIncapableOf();
        pawn.ClearPawnCaches();
    }

    public override float Draw(State state, float y) {
        var top = y;
        y += Margin.y;

        y += DrawHeader(y, Width, "EdB.PC.Panel.Titles.Header".Translate().Resolve());

        var currentPawn = state.CurrentPawn;
        var index = 0;
        Action clickAction = null;
        foreach (var title in currentPawn.Pawn.royalty.AllTitlesForReading) {
            if (title == null) {
                continue;
            }

            if (index > 0) {
                y += FieldPadding;
            }

            if (index >= fields.Count) {
                fields.Add(new Field { IconSizeFunc = () => new Vector2(22, 22) });
            }

            var localTitle = title;
            var localTitleDef = title.def;
            var localIndex = index;

            var field = fields[index];
            var fieldRect = FieldRect.OffsetBy(0, y);
            field.Rect = fieldRect;
            var fieldClickRect = fieldRect;
            fieldClickRect.width -= 36;
            field.ClickRect = fieldClickRect;
            field.DrawIconFunc = rect => GUI.DrawTexture(rect, title.faction.def.FactionIcon);
            field.Label = title.def.GetLabelCapFor(currentPawn.Pawn);
            field.TipAction = rect => {
                if (Mouse.IsOver(rect)) {
                    var method = ReflectionUtil.Method(typeof(CharacterCardUtility), "GetTitleTipString");
                    var tipString = method.Invoke(null,
                        new object[] {
                            currentPawn.Pawn, localTitle.faction, localTitle,
                            currentPawn.Pawn.royalty.GetFavor(localTitle.faction)
                        }) as string;
                    tipString = tipString.Replace("\n\n" + "ClickToLearnMore".Translate(), "");
                    var tip = new TipSignal(() => tipString, (int)y * 37);
                    TooltipHandler.TipRegion(rect, tip);
                }
            };

            field.ClickAction = () => {
                OpenOptionsDialog(currentPawn);
            };
            field.Draw();

            // Remove trait button.
            //Rect deleteRect = new Rect(field.Rect.xMax - 32, field.Rect.y + field.Rect.HalfHeight() - 6, 12, 12);
            //if (deleteRect.Contains(Event.current.mousePosition)) {
            //    GUI.color = Style.ColorButtonHighlight;
            //}
            //else {
            //    GUI.color = Style.ColorButton;
            //}
            //GUI.DrawTexture(deleteRect, Textures.TextureButtonDelete);
            //if (Widgets.ButtonInvisible(deleteRect, false)) {
            //    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            //    traitsToRemove.Add(trait);
            //}

            index++;

            y += FieldRect.height;
        }

        tipCache.MakeReady();

        // If the index is still zero, then the pawn has no titles.  Draw the "none" label.
        if (index == 0) {
            GUI.color = Style.ColorText;
            Widgets.Label(FieldRect.InsetBy(6, 0).OffsetBy(0, y - 4), "EdB.PC.Panel.Titles.None".Translate());
            y += FieldRect.height - 4;
        }

        GUI.color = Color.white;

        // Fire any action that was triggered
        if (clickAction != null) {
            clickAction();
            clickAction = null;
        }

        // Add button.
        var addRect = new Rect(Width - 24, top + 12, 16, 16);
        Style.SetGUIColorForButton(addRect);
        GUI.DrawTexture(addRect, Textures.TextureButtonAdd);
        if (Widgets.ButtonInvisible(addRect, false)) {
            OpenOptionsDialog(currentPawn);
        }

        // Remove any traits that were marked for deletion
        if (itemsToRemove.Count > 0) {
            foreach (var item in itemsToRemove) {
                //TraitRemoved(item);
            }

            itemsToRemove.Clear();
            tipCache.Invalidate();
        }

        y += Margin.y;
        return y - top;
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
