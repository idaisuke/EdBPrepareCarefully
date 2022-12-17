using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordTraitV5 : IExposable {
    public string def;
    public int degree;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref def, "def");
        Scribe_Values.Look(ref degree, "degree");
    }
}
