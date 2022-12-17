using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderFaceTattoos {
    protected List<TattooDef> empty = new();
    protected List<TattooDef> humanlike;
    protected Dictionary<ThingDef, List<TattooDef>> lookup = new();

    public ProviderAlienRaces AlienRaceProvider {
        get;
        set;
    }

    protected List<TattooDef> Humanlike {
        get {
            if (humanlike == null) {
                humanlike = InitializeForHumanlike();
            }

            return humanlike;
        }
    }

    public List<TattooDef> GetTattoos(CustomPawn pawn) {
        return GetTattoos(pawn.Pawn.def, pawn.Gender);
    }

    public List<TattooDef> GetTattoos(ThingDef raceDef, Gender gender) {
        var defs = GetTattoosForRace(raceDef);
        return defs;
    }

    public List<TattooDef> GetTattoosForRace(CustomPawn pawn) {
        return GetTattoosForRace(pawn.Pawn.def);
    }

    public List<TattooDef> GetTattoosForRace(ThingDef raceDef) {
        if (lookup.TryGetValue(raceDef, out var defs)) {
            return defs;
        }

        defs = InitializeForRace(raceDef);
        if (defs == null) {
            if (raceDef != ThingDefOf.Human) {
                return GetTattoosForRace(ThingDefOf.Human);
            }

            return null;
        }

        lookup.Add(raceDef, defs);
        return defs;
    }

    // TODO: Handle tattoos for alien races
    protected List<TattooDef> InitializeForRace(ThingDef raceDef) {
        var alienRace = AlienRaceProvider.GetAlienRace(raceDef);
        if (alienRace == null) {
            return Humanlike;
        }

        return Humanlike;
    }

    protected List<TattooDef> InitializeForHumanlike() {
        var result = new List<TattooDef>();
        foreach (var TattooDef in DefDatabase<TattooDef>.AllDefs.Where(def => def.tattooType == TattooType.Face)) {
            result.Add(TattooDef);
        }

        return result;
    }
}
