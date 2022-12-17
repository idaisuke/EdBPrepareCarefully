namespace EdB.PrepareCarefully;

public class SelectedAnimal {
    public int Count;
    public AnimalRecord Record;

    public AnimalRecordKey Key => Record.Key;
}
