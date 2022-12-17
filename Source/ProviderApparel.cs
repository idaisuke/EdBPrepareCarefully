using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderApparel {
    protected Dictionary<ThingDef, OptionsApparel> apparelLookup = new();
    protected OptionsApparel humanlikeApparel;
    protected OptionsApparel noApparel = new();

    public ProviderAlienRaces AlienRaceProvider {
        get;
        set;
    }

    protected OptionsApparel HumanlikeApparel {
        get {
            if (humanlikeApparel == null) {
                humanlikeApparel = InitializeHumanlikeApparel();
            }

            return humanlikeApparel;
        }
    }

    public List<ThingDef> GetApparel(CustomPawn pawn, PawnLayer layer) {
        return GetApparel(pawn.Pawn.def, layer);
    }

    public List<ThingDef> GetApparel(ThingDef raceDef, PawnLayer layer) {
        var apparel = GetApparelForRace(raceDef);
        return apparel.GetApparel(layer);
    }

    public OptionsApparel GetApparelForRace(CustomPawn pawn) {
        return GetApparelForRace(pawn.Pawn.def);
    }

    public OptionsApparel GetApparelForRace(ThingDef raceDef) {
        OptionsApparel apparel;
        if (apparelLookup.TryGetValue(raceDef, out apparel)) {
            return apparel;
        }

        apparel = InitializeApparel(raceDef);
        if (apparel == null) {
            if (raceDef != ThingDefOf.Human) {
                return GetApparelForRace(ThingDefOf.Human);
            }

            return null;
        }

        apparelLookup.Add(raceDef, apparel);
        return apparel;
    }

    protected PawnLayer LayerForApparel(ThingDef def) {
        if (def.apparel == null) {
            return null;
        }

        return PrepareCarefully.Instance.Providers.PawnLayers.FindLayerForApparel(def);
    }

    protected void AddApparelToOptions(OptionsApparel options, string defName) {
        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (def != null) {
            AddApparelToOptions(options, def);
        }
    }

    protected void AddApparelToOptions(OptionsApparel options, ThingDef def) {
        var layer = LayerForApparel(def);
        if (layer != null) {
            options.Add(layer, def);
        }
    }

    protected OptionsApparel InitializeApparel(ThingDef raceDef) {
        var alienRace = AlienRaceProvider.GetAlienRace(raceDef);
        if (alienRace == null) {
            return HumanlikeApparel;
        }

        var result = new OptionsApparel();
        var addedAlready = new HashSet<string>();
        // Add all race-specific apparel.
        foreach (var a in alienRace.RaceSpecificApparel ?? Enumerable.Empty<string>()) {
            if (!addedAlready.Contains(a)) {
                AddApparelToOptions(result, a);
                addedAlready.Add(a);
            }
        }

        // Even if we're only allowed to use race-specific apparel, we're also allowed to use anything in the allowed list.
        if (alienRace.RaceSpecificApparelOnly) {
            var allowed = alienRace.AllowedApparel ?? new HashSet<string>();
            foreach (var def in HumanlikeApparel.AllApparel ?? Enumerable.Empty<ThingDef>()) {
                if (allowed.Contains(def.defName) && !addedAlready.Contains(def.defName)) {
                    AddApparelToOptions(result, def);
                    addedAlready.Add(def.defName);
                }
            }
        }
        // Even if we're allowed to use more than just race-specific apparel, we can't use anything in the disallowed list.
        else {
            var disallowed = alienRace.DisallowedApparel ?? new HashSet<string>();
            foreach (var def in HumanlikeApparel.AllApparel ?? Enumerable.Empty<ThingDef>()) {
                if (!addedAlready.Contains(def.defName) && !disallowed.Contains(def.defName)) {
                    AddApparelToOptions(result, def);
                    addedAlready.Add(def.defName);
                }
            }
        }

        result.Sort();
        return result;
    }

    protected OptionsApparel InitializeHumanlikeApparel() {
        var nonHumanApparel = new HashSet<string>();
        var alienRaces = DefDatabase<ThingDef>.AllDefs.Where(def => {
            return def.race != null && ProviderAlienRaces.IsAlienRace(def);
        });
        foreach (var alienRaceDef in alienRaces) {
            var alienRace = AlienRaceProvider.GetAlienRace(alienRaceDef);
            if (alienRace == null) {
                continue;
            }

            if (alienRace?.ThingDef?.defName == "Human") {
                continue;
            }

            if (alienRace?.AllowedApparel != null) {
                foreach (var defName in alienRace.RaceSpecificApparel) {
                    nonHumanApparel.Add(defName);
                }
            }
        }

        var result = new OptionsApparel();
        foreach (var apparelDef in DefDatabase<ThingDef>.AllDefs) {
            if (apparelDef.apparel == null) {
                continue;
            }

            if (!nonHumanApparel.Contains(apparelDef.defName)) {
                AddApparelToOptions(result, apparelDef);
            }
        }

        result.Sort();
        return result;
    }
}
