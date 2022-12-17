using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class OptionsBodyType {
    protected List<BodyTypeDef> femaleBodyTypes = new();
    protected List<BodyTypeDef> maleBodyTypes = new();
    protected List<BodyTypeDef> noGenderBodyTypes = new();

    public List<BodyTypeDef> MaleBodyTypes {
        get => maleBodyTypes;
        set => maleBodyTypes = value;
    }

    public List<BodyTypeDef> FemaleBodyTypes {
        get => femaleBodyTypes;
        set => femaleBodyTypes = value;
    }

    public List<BodyTypeDef> NoGenderBodyTypes {
        get => noGenderBodyTypes;
        set => noGenderBodyTypes = value;
    }

    public List<BodyTypeDef> GetBodyTypes(Gender gender) {
        if (gender == Gender.Male) {
            return maleBodyTypes;
        }

        if (gender == Gender.Female) {
            return femaleBodyTypes;
        }

        return noGenderBodyTypes;
    }
}
