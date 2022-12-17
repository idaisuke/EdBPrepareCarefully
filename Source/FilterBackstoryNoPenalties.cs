using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstoryNoPenalties : Filter<BackstoryDef> {
    public FilterBackstoryNoPenalties() {
        LabelShort = LabelFull = "EdB.PC.Dialog.Backstory.Filter.NoSkillPenalties".Translate();
        FilterFunction = backstory => {
            if (backstory.skillGains.Count == 0) {
                return true;
            }

            foreach (var gain in backstory.skillGains.Values) {
                if (gain < 0) {
                    return false;
                }
            }

            return true;
        };
    }
}
