using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderBeards {
    protected Dictionary<ThingDef, List<BeardDef>> beardsLookup = new();
    protected List<BeardDef> humanlikeBeards;
    protected List<BeardDef> noBeards = new();

    public ProviderAlienRaces AlienRaceProvider {
        get;
        set;
    }

    protected List<BeardDef> HumanlikeBeards {
        get {
            if (humanlikeBeards == null) {
                humanlikeBeards = InitializeHumanlikeBeards();
            }

            return humanlikeBeards;
        }
    }

    public List<BeardDef> GetBeards(CustomPawn pawn) {
        return GetBeards(pawn.Pawn.def, pawn.Gender);
    }

    public List<BeardDef> GetBeards(ThingDef raceDef, Gender gender) {
        var beards = GetBeardsForRace(raceDef);
        return beards;
    }

    public List<BeardDef> GetBeardsForRace(CustomPawn pawn) {
        return GetBeardsForRace(pawn.Pawn.def);
    }

    public List<BeardDef> GetBeardsForRace(ThingDef raceDef) {
        List<BeardDef> beards;
        if (beardsLookup.TryGetValue(raceDef, out beards)) {
            return beards;
        }

        beards = InitializeBeards(raceDef);
        if (beards == null) {
            if (raceDef != ThingDefOf.Human) {
                return GetBeardsForRace(ThingDefOf.Human);
            }

            return null;
        }

        beardsLookup.Add(raceDef, beards);
        return beards;
    }

    protected List<BeardDef> InitializeBeards(ThingDef raceDef) {
        var alienRace = AlienRaceProvider.GetAlienRace(raceDef);
        if (alienRace == null) {
            return HumanlikeBeards;
        }

        if (!alienRace.HasBeards) {
            return noBeards;
        }

        return HumanlikeBeards;
    }

    protected List<BeardDef> InitializeHumanlikeBeards() {
        var result = new List<BeardDef>();
        foreach (var beardDef in DefDatabase<BeardDef>.AllDefs.Where(def => {
                     return true;
                 })) {
            result.Add(beardDef);
        }

        return result;
    }
}
