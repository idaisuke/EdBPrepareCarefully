using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstoryMatchesFaction : Filter<BackstoryDef> {
    public FilterBackstoryMatchesFaction() {
        LabelShort = LabelFull = "EdB.PC.Dialog.Backstory.Filter.MatchesFaction".Translate();
        FilterFunction = backstory => {
            var pawn = PrepareCarefully.Instance.State.CurrentPawn;
            var kindDef = pawn.OriginalKindDef;
            if (kindDef == null) {
                kindDef = PawnKindDefOf.Colonist;
            }

            var set = PrepareCarefully.Instance.Providers.Backstories.BackstoriesForPawnKindDef(kindDef);
            if (set == null) {
                return false;
            }

            return set.Contains(backstory);
        };
    }
}
