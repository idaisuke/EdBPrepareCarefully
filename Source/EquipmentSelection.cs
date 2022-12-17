using Verse;

namespace EdB.PrepareCarefully;

public class EquipmentSelection {
    public int count = 1;
    public EquipmentRecord record;

    public EquipmentSelection() {
    }

    public EquipmentSelection(EquipmentRecord entry) {
        count = 1;
        record = entry;
    }

    public EquipmentSelection(EquipmentRecord entry, int count) {
        this.count = count;
        record = entry;
    }

    public EquipmentRecord Record => record;

    public int Count {
        get => count;
        set => count = value;
    }

    public ThingDef ThingDef {
        get {
            if (record == null) {
                return null;
            }

            return record.def;
        }
    }

    public ThingDef StuffDef => record.stuffDef;

    public Gender Gender => record.gender;

    public EquipmentKey Key {
        get {
            if (record == null) {
                return new EquipmentKey();
            }

            return record.EquipmentKey;
        }
    }
}
