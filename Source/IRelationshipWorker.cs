using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public interface IRelationshipWorker {
    PawnRelationDef Def {
        get;
        set;
    }

    void CreateRelationship(Pawn source, Pawn target);
}
