using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderBackstories {
    protected List<BackstoryDef> adulthoodBackstories = new();
    protected Dictionary<string, List<BackstoryDef>> adulthoodBackstoryLookup = new();
    protected Dictionary<string, HashSet<BackstoryDef>> backstoryHashSetLookup = new();
    protected List<BackstoryDef> childhoodBackstories = new();

    protected Dictionary<string, List<BackstoryDef>> childhoodBackstoryLookup = new();
    protected List<BackstoryDef> sortedAdulthoodBackstories;
    protected List<BackstoryDef> sortedChildhoodBackstories;

    public ProviderBackstories() {
        // Go through all of the backstories and mark them as childhood or adult.
        var backstories = DefDatabase<BackstoryDef>.AllDefsListForReading;
        foreach (var backstory in backstories) {
            if (backstory.slot == BackstorySlot.Childhood) {
                childhoodBackstories.Add(backstory);
            }
            else {
                adulthoodBackstories.Add(backstory);
            }
        }

        // Create sorted versions of the backstory lists
        sortedChildhoodBackstories = new List<BackstoryDef>(childhoodBackstories);
        sortedChildhoodBackstories.Sort((b1, b2) => b1.TitleCapFor(Gender.Male).CompareTo(b2.TitleCapFor(Gender.Male)));
        sortedAdulthoodBackstories = new List<BackstoryDef>(adulthoodBackstories);
        sortedAdulthoodBackstories.Sort((b1, b2) => b1.TitleCapFor(Gender.Male).CompareTo(b2.TitleCapFor(Gender.Male)));
    }

    public List<BackstoryDef> AllChildhookBackstories => sortedChildhoodBackstories;

    public List<BackstoryDef> AllAdulthookBackstories => sortedAdulthoodBackstories;

    public List<BackstoryDef> GetChildhoodBackstoriesForPawn(CustomPawn pawn) {
        return GetChildhoodBackstoriesForPawnKindDef(pawn.OriginalKindDef);
    }

    public List<BackstoryDef> GetAdulthoodBackstoriesForPawn(CustomPawn pawn) {
        return GetAdulthoodBackstoriesForPawnKindDef(pawn.OriginalKindDef);
    }

    public List<BackstoryDef> GetChildhoodBackstoriesForPawnKindDef(PawnKindDef kindDef) {
        if (!backstoryHashSetLookup.ContainsKey(kindDef.defName)) {
            InitializeBackstoriesForPawnKind(kindDef);
        }

        return childhoodBackstoryLookup[kindDef.defName];
    }

    public List<BackstoryDef> GetAdulthoodBackstoriesForPawnKindDef(PawnKindDef kindDef) {
        if (!backstoryHashSetLookup.ContainsKey(kindDef.defName)) {
            InitializeBackstoriesForPawnKind(kindDef);
        }

        return adulthoodBackstoryLookup[kindDef.defName];
    }

    public HashSet<BackstoryDef> BackstoriesForPawnKindDef(PawnKindDef kindDef) {
        if (!backstoryHashSetLookup.ContainsKey(kindDef.defName)) {
            InitializeBackstoriesForPawnKind(kindDef);
        }

        return backstoryHashSetLookup[kindDef.defName];
    }

    private void InitializeBackstoriesForPawnKind(PawnKindDef def) {
        var categories = BackstoryCategoriesForPawnKindDef(def);
        var childhood = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(b => {
            if (b.slot != BackstorySlot.Childhood) {
                return false;
            }

            foreach (var c in b.spawnCategories) {
                if (categories.Contains(c)) {
                    return true;
                }
            }

            return false;
        }).ToList();
        childhood.Sort((b1, b2) => b1.TitleCapFor(Gender.Male).CompareTo(b2.TitleCapFor(Gender.Male)));
        childhoodBackstoryLookup[def.defName] = childhood;

        var adulthood = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(b => {
            if (b.slot != BackstorySlot.Adulthood) {
                return false;
            }

            foreach (var c in b.spawnCategories) {
                if (categories.Contains(c)) {
                    return true;
                }
            }

            return false;
        }).ToList();
        adulthood.Sort((b1, b2) => b1.TitleCapFor(Gender.Male).CompareTo(b2.TitleCapFor(Gender.Male)));
        adulthoodBackstoryLookup[def.defName] = adulthood;

        HashSet<BackstoryDef> backstorySet = new(childhood);
        backstorySet.AddRange(adulthood);
        backstoryHashSetLookup[def.defName] = backstorySet;
    }

    public HashSet<string> BackstoryCategoriesForPawnKindDef(PawnKindDef kindDef) {
        var faction = PrepareCarefully.Instance.Providers.Factions.GetFaction(kindDef);
        var filters = GetBackstoryCategoryFiltersFor(kindDef, faction != null ? faction.def : null);
        return AllBackstoryCategoriesFromFilterList(filters);
    }

    // EVERY RELEASE:
    // Evaluate to make sure the logic in PawnBioAndNameGenerator.GetBackstoryCategoryFiltersFor() has not changed in a way
    // that invalidates this rewrite. This is a modified version of that method but with the first argument a PawnKindDef
    // instead of a Pawn and with logging removed.
    private List<BackstoryCategoryFilter> GetBackstoryCategoryFiltersFor(PawnKindDef kindDef, FactionDef faction) {
        if (!kindDef.backstoryFiltersOverride.NullOrEmpty<BackstoryCategoryFilter>()) {
            return kindDef.backstoryFiltersOverride;
        }

        var list = new List<BackstoryCategoryFilter>();
        if (kindDef.backstoryFilters != null) {
            list.AddRange(kindDef.backstoryFilters);
        }

        if (faction != null && !faction.backstoryFilters.NullOrEmpty<BackstoryCategoryFilter>()) {
            for (var i = 0; i < faction.backstoryFilters.Count; i++) {
                var item = faction.backstoryFilters[i];
                if (!list.Contains(item)) {
                    list.Add(item);
                }
            }
        }

        if (!list.NullOrEmpty()) {
            return list;
        }

        return new List<BackstoryCategoryFilter> {
            new() { categories = new List<string> { "Civil" }, commonality = 1f }
        };
    }

    private HashSet<string> AllBackstoryCategoriesFromFilterList(List<BackstoryCategoryFilter> filterList) {
        var result = new HashSet<string>();
        foreach (var filter in filterList) {
            foreach (var category in filter.categories) {
                result.Add(category);
            }
        }

        return result;
    }
}
