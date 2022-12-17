using Verse;

namespace EdB.PrepareCarefully;

internal class RelatedPawn {
    private Gender gender = Gender.None;
    public CustomPawn Pawn = null;

    public Gender Gender {
        get => Pawn != null ? Pawn.Gender : gender;
        set => gender = value;
    }
}
