using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderTraits {
    protected List<Trait> sortedTraits = new();
    protected List<Trait> traits = new();

    public ProviderTraits() {
        // Get all trait options.  If a traits has multiple degrees, create a separate trait for each degree.
        foreach (var def in DefDatabase<TraitDef>.AllDefs) {
            List<TraitDegreeData> degreeData = def.degreeDatas;
            var count = degreeData.Count;
            if (count > 0) {
                for (var i = 0; i < count; i++) {
                    var trait = new Trait(def, degreeData[i].degree, true);
                    traits.Add(trait);
                }
            }
            else {
                traits.Add(new Trait(def, 0, true));
            }
        }

        // Create a sorted version of the trait list.
        sortedTraits = new List<Trait>(traits);
        sortedTraits.Sort((t1, t2) => t1.LabelCap.CompareTo(t2.LabelCap));
    }

    public List<Trait> Traits => sortedTraits;
}
