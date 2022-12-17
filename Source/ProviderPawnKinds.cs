using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderPawnKinds {
    private readonly List<PawnKindDef> otherPawnKinds = new();
    private readonly List<PawnKindDef> pawnKindDefs = new();
    private readonly List<FactionPawnKinds> pawnKindsByFaction = new();

    public ProviderPawnKinds() {
        var uniquePawnKindsByFaction = new Dictionary<FactionDef, HashSet<PawnKindDef>>();
        foreach (var factionDef in DefDatabase<FactionDef>.AllDefs) {
            uniquePawnKindsByFaction.Add(factionDef, new HashSet<PawnKindDef>());
        }

        foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefs) {
            //Logger.Debug("pawnKindDef {0}, {1}", kindDef.defName, kindDef.LabelCap);
            if (kindDef == null) {
                //Logger.Debug("Excluding pawnKindDef because it was null");
                continue;
            }

            if (kindDef.RaceProps == null) {
                //Logger.Debug("Excluding pawnKindDef because its RaceProps was null {0}, {1}", kindDef.defName, kindDef.LabelCap);
                continue;
            }

            // Exclude animals, mechanoids and other non-human pawn kinds.
            if (!kindDef.RaceProps.Humanlike) {
                //Logger.Debug("Excluding pawnKindDef because it's non-human {0}, {1}", kindDef.defName, kindDef.LabelCap);
                continue;
            }

            if (kindDef.LabelCap.NullOrEmpty()) {
                continue;
            }

            if (kindDef.defaultFactionType != null) {
                if (uniquePawnKindsByFaction.ContainsKey(kindDef.defaultFactionType)) {
                    uniquePawnKindsByFaction[kindDef.defaultFactionType].Add(kindDef);
                }
            }
            else {
                otherPawnKinds.Add(kindDef);
            }

            pawnKindDefs.Add(kindDef);

            if (kindDef?.race?.defName != "Human") {
                AnyNonHumanPawnKinds = true;
            }
        }

        foreach (var pair in uniquePawnKindsByFaction) {
            var faction = pair.Key;
            var pawnKinds = new List<PawnKindDef>(pair.Value);
            pawnKinds.Sort((a, b) => {
                return a.LabelCap.ToString().CompareTo(b.LabelCap.ToString());
            });
            pawnKindsByFaction.Add(new FactionPawnKinds { Faction = faction, PawnKinds = pawnKinds });
        }

        pawnKindsByFaction.Sort((a, b) => {
            return a.Faction.LabelCap.ToString().CompareTo(b.Faction.LabelCap.ToString());
        });
        otherPawnKinds.Sort((a, b) => {
            return a.LabelCap.ToString().CompareTo(b.LabelCap.ToString());
        });
    }

    public Dictionary<FactionDef, List<PawnKindDef>> PawnKindByFaction { get; set; } = new();

    public IEnumerable<PawnKindDef> AllPawnKinds => pawnKindDefs;

    public IEnumerable<PawnKindDef> PawnKindsWithNoFaction => otherPawnKinds;

    public IEnumerable<FactionPawnKinds> PawnKindsByFaction => pawnKindsByFaction;

    public bool AnyNonHumanPawnKinds { get; }

    public class FactionPawnKinds {
        public FactionDef Faction { get; set; }
        public List<PawnKindDef> PawnKinds { get; set; } = new();
    }
}
