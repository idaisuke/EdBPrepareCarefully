using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordParentChildGroupV3 : IExposable {
    public List<string> children;
    public List<string> parents;

    public void ExposeData() {
        Scribe_Collections.Look(ref parents, "parents", LookMode.Value, null);
        Scribe_Collections.Look(ref children, "children", LookMode.Value, null);
    }
}
