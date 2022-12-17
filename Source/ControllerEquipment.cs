namespace EdB.PrepareCarefully;

public class ControllerEquipment {
    private Randomizer randomizer = new();
    private State state;

    public ControllerEquipment(State state) {
        this.state = state;
    }

    public void AddEquipment(EquipmentRecord entry) {
        PrepareCarefully.Instance.AddEquipment(entry);
    }

    public void RemoveEquipment(EquipmentSelection equipment) {
        PrepareCarefully.Instance.RemoveEquipment(equipment);
    }

    public void UpdateEquipmentCount(EquipmentSelection equipment, int count) {
        if (count >= 0) {
            equipment.Count = count;
        }
    }
}
