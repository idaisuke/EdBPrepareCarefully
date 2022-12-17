using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordEquipmentV3 : IExposable {
    public int count;
    public string def;
    public string gender;
    public string stuffDef;

    public SaveRecordEquipmentV3() {
    }

    public SaveRecordEquipmentV3(EquipmentSelection equipment) {
        count = equipment.Count;
        def = equipment.Record.def.defName;
        stuffDef = equipment.Record.stuffDef != null ? equipment.Record.stuffDef.defName : null;
        gender = equipment.Record.gender == Gender.None ? null : equipment.Record.gender.ToString();
    }

    public void ExposeData() {
        Scribe_Values.Look<string>(ref def, "def");
        Scribe_Values.Look<string>(ref stuffDef, "stuffDef");
        Scribe_Values.Look<string>(ref gender, "gender");
        Scribe_Values.Look(ref count, "count");
    }
}
