using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordSkillV4 : IExposable {
    public string name;
    public Passion passion;
    public int value;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref name, "name", null, true);
        Scribe_Values.Look(ref value, "value", 0, true);
        Scribe_Values.Look(ref passion, "passion", Passion.None, true);
    }
}
