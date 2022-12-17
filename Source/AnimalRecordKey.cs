using System;
using Verse;

namespace EdB.PrepareCarefully;

public struct AnimalRecordKey {
    public ThingDef ThingDef;
    public Gender Gender;

    public AnimalRecordKey(ThingDef thingDef, Gender gender) {
        ThingDef = thingDef;
        Gender = gender;
    }

    public override bool Equals(Object o) {
        if (o == null) {
            return false;
        }

        if (!(o is AnimalRecordKey)) {
            return false;
        }

        var pair = (AnimalRecordKey)o;
        return ThingDef == pair.ThingDef && Gender == pair.Gender;
    }

    public override int GetHashCode() {
        unchecked {
            var a = ThingDef != null ? ThingDef.GetHashCode() : 0;
            return (31 * a * 31) + Gender.GetHashCode();
        }
    }
}
