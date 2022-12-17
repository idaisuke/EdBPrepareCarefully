using Verse;

namespace EdB.PrepareCarefully;

public class AnimalRecord {
    public double Cost = 0;
    public Gender Gender;
    public string label;
    public Thing Thing;
    public ThingDef ThingDef;

    public AnimalRecord() {
    }

    public AnimalRecord(ThingDef thingDef, Gender gender) {
        ThingDef = thingDef;
        Gender = gender;
    }

    public AnimalRecordKey Key => new(ThingDef, Gender);

    public string Label {
        get {
            if (label != null) {
                return label;
            }

            if (Gender == Gender.None) {
                return "EdB.PC.Animals.LabelWithoutGender".Translate(ThingDef.LabelCap);
            }

            return "EdB.PC.Animals.LabelWithGender".Translate(ThingDef.LabelCap, Gender.GetLabel());
        }
    }
}
