using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstoryNoDisabledWorkTypes : Filter<BackstoryDef> {
    public FilterBackstoryNoDisabledWorkTypes() {
        LabelShort = LabelFull = "EdB.PC.Dialog.Backstory.Filter.NoDisabledWorkTypes".Translate();
        FilterFunction = backstory => {
            return backstory.DisabledWorkTypes.Count == 0;
        };
    }
}
