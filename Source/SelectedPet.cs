using Verse;

namespace EdB.PrepareCarefully;

public class SelectedPet {
    public CustomPawn BondedPawn;
    public string Id;
    public string Name;
    public Pawn Pawn;
    public AnimalRecord Record;

    public AnimalRecordKey Key => Record.Key;
}
