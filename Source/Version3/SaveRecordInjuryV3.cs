using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordInjuryV3 : IExposable {
    public string bodyPart;
    public int? bodyPartIndex;
    public string hediffDef;
    public string painFactor;
    public string severity;

    public SaveRecordInjuryV3() {
    }

    public SaveRecordInjuryV3(Injury injury) {
        bodyPart = injury.BodyPartRecord != null ? injury.BodyPartRecord.def.defName : null;
        hediffDef = injury.Option.HediffDef != null ? injury.Option.HediffDef.defName : null;
        if (injury.Severity != 0) {
            severity = injury.Severity.ToString();
        }

        if (injury.PainFactor != null) {
            painFactor = injury.PainFactor.Value.ToString();
        }
    }

    public float Severity => float.Parse(severity);

    public float PainFactor => float.Parse(painFactor);

    public void ExposeData() {
        Scribe_Values.Look<string>(ref hediffDef, "hediffDef");
        Scribe_Values.Look<string>(ref bodyPart, "bodyPart");
        Scribe_Values.Look(ref bodyPartIndex, "bodyPartIndex");
        Scribe_Values.Look<string>(ref severity, "severity");
        Scribe_Values.Look<string>(ref painFactor, "painFactor");
    }
}
