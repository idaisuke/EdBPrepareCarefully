using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordImplantV3 : IExposable {
    public string bodyPart;
    public int? bodyPartIndex;
    public string recipe;

    public SaveRecordImplantV3() {
    }

    public SaveRecordImplantV3(Implant option) {
        bodyPart = option.BodyPartRecord.def.defName;
        recipe = option.recipe != null ? option.recipe.defName : null;
    }

    public void ExposeData() {
        Scribe_Values.Look<string>(ref bodyPart, "bodyPart");
        Scribe_Values.Look(ref bodyPartIndex, "bodyPartIndex");
        Scribe_Values.Look<string>(ref recipe, "recipe");
    }
}
