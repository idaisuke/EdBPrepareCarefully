using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordFactionV4 : IExposable {
    public string def;
    public int? index;
    public bool leader;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref def, "def");
        Scribe_Values.Look(ref index, "index");
        Scribe_Values.Look(ref leader, "leader");
    }
}
