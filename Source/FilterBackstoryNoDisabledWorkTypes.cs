using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstoryNoDisabledWorkTypes : Filter<Backstory> {
    public FilterBackstoryNoDisabledWorkTypes() {
        LabelShort = LabelFull = "EdB.PC.Dialog.Backstory.Filter.NoDisabledWorkTypes".Translate();
        FilterFunction = backstory => {
            return backstory.DisabledWorkTypes.FirstOrDefault() == null;
        };
    }
}
