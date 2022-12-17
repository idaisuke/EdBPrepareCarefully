using System;
using Verse;

namespace EdB.PrepareCarefully;

public struct EquipmentKey {
    public ThingDef ThingDef { get; set; }

    public ThingDef StuffDef { get; set; }

    public Gender Gender { get; set; }

    public EquipmentKey(ThingDef thingDef, ThingDef stuffDef, Gender gender) {
        ThingDef = thingDef;
        StuffDef = stuffDef;
        Gender = gender;
    }

    public EquipmentKey(ThingDef thingDef, ThingDef stuffDef) {
        ThingDef = thingDef;
        StuffDef = stuffDef;
        Gender = Gender.None;
    }

    public EquipmentKey(ThingDef thingDef) {
        ThingDef = thingDef;
        StuffDef = null;
        Gender = Gender.None;
    }

    public EquipmentKey(ThingDef thingDef, Gender gender) {
        ThingDef = thingDef;
        StuffDef = null;
        Gender = gender;
    }

    public override bool Equals(Object o) {
        if (o == null) {
            return false;
        }

        if (!(o is EquipmentKey)) {
            return false;
        }

        var pair = (EquipmentKey)o;
        return ThingDef == pair.ThingDef && StuffDef == pair.StuffDef;
    }

    public override int GetHashCode() {
        unchecked {
            var a = ThingDef != null ? ThingDef.GetHashCode() : 0;
            var b = StuffDef != null ? StuffDef.GetHashCode() : 0;
            return (((31 * a) + b) * 31) + Gender.GetHashCode();
        }
    }

    public override string ToString() {
        return string.Format("[EquipmentKey: def = {0}, stuffDef = {1}, gender = {2}]",
            ThingDef != null ? ThingDef.defName : "null",
            StuffDef != null ? StuffDef.defName : "null",
            Gender);
    }
}
