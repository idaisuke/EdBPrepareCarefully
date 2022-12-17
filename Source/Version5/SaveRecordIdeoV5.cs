using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordIdeoV5 : IExposable {
    public float certainty;
    public string culture;
    public List<string> memes;
    public string name;
    public bool sameAsColony;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref name, "name");
        Scribe_Values.Look(ref certainty, "certainty", 0.85f);
        Scribe_Values.Look(ref sameAsColony, "sameAsColony", true);
        Scribe_Values.Look<string>(ref culture, "culture");
        Scribe_Collections.Look(ref memes, "memes");
    }
}
