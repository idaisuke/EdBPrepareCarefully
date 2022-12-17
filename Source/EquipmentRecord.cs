using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class EquipmentRecord {
    public bool animal = false;
    public Color color = Color.white;
    public double cost = 0;
    public ThingDef def;
    public bool gear = false;
    public Gender gender = Gender.None;
    public bool hideFromPortrait = false;
    protected string label = null;
    public bool stacks = true;
    public int stackSize;

    public ThingDef stuffDef = null;

    //public Thing thing = null;
    public EquipmentType type;

    public bool Minifiable => def.Minifiable && def.building != null;

    public string Label {
        get {
            if (label == null) {
                if (animal) {
                    return LabelForAnimal;
                }

                return GenLabel.ThingLabel(def, stuffDef, stackSize).CapitalizeFirst();
            }

            return label;
        }
    }

    public string LabelNoCount {
        get {
            if (label == null) {
                if (animal) {
                    return LabelForAnimal;
                }

                return GenLabel.ThingLabel(def, stuffDef).CapitalizeFirst();
            }

            return label;
        }
    }

    public string LabelForAnimal {
        get {
            if (def.race.hasGenders) {
                return "EdB.PC.Equipment.AnimalLabel".Translate(gender.GetLabel(), def.label).CapitalizeFirst();
            }

            return GenLabel.ThingLabel(def, null).CapitalizeFirst();
        }
    }

    public EquipmentKey EquipmentKey => new(def, stuffDef, gender);

    public override string ToString() {
        return string.Format("[EquipmentDatabaseEntry: def = {0}, stuffDef = {1}, gender = {2}]",
            def != null ? def.defName : "null",
            stuffDef != null ? stuffDef.defName : "null",
            gender);
    }
}
