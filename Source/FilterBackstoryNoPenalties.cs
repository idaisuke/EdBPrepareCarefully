using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstoryNoPenalties : Filter<Backstory> {
    public FilterBackstoryNoPenalties() {
        LabelShort = LabelFull = "EdB.PC.Dialog.Backstory.Filter.NoSkillPenalties".Translate();
        FilterFunction = backstory => {
            if (backstory.skillGainsResolved.Count == 0) {
                return true;
            }

            foreach (var gain in backstory.skillGainsResolved.Values) {
                if (gain < 0) {
                    return false;
                }
            }

            return true;
        };
    }
}
