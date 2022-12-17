using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class RelationshipDefinitionHelper {
    protected Randomizer randomizer = new();

    public RelationshipDefinitionHelper() {
        AllowedRelationships = InitializeAllowedRelationships();
        InverseRelationships = InitializeInverseRelationships();
    }

    public List<PawnRelationDef> AllowedRelationships { get; set; } = new();
    public Dictionary<PawnRelationDef, PawnRelationDef> InverseRelationships { get; set; } = new();

    public PawnRelationDef FindInverseRelationship(PawnRelationDef def) {
        PawnRelationDef inverse;
        if (InverseRelationships.TryGetValue(def, out inverse)) {
            return inverse;
        }

        return null;
    }

    protected List<PawnRelationDef> InitializeAllowedRelationships() {
        return DefDatabase<PawnRelationDef>.AllDefs.Where(def => {
            if (def.familyByBloodRelation) {
                return false;
            }

            var extended = DefDatabase<CarefullyPawnRelationDef>.GetNamedSilentFail(def.defName);
            if (extended != null && extended.animal) {
                return false;
            }

            var info = def.workerClass.GetMethod("CreateRelation",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (info == null) {
                return false;
            }

            return true;
        }).ToList();
    }

    protected Dictionary<PawnRelationDef, PawnRelationDef> InitializeInverseRelationships() {
        var result = new Dictionary<PawnRelationDef, PawnRelationDef>();
        foreach (var def in DefDatabase<PawnRelationDef>.AllDefs) {
            var extended = DefDatabase<CarefullyPawnRelationDef>.GetNamedSilentFail(def.defName);
            PawnRelationDef inverse;
            if (extended != null && extended.inverse != null) {
                inverse = DefDatabase<PawnRelationDef>.GetNamedSilentFail(extended.inverse);
            }
            else {
                inverse = TryToComputeInverseRelationship(def);
            }

            if (inverse != null) {
                result[def] = inverse;
            }
        }

        return result;
    }

    // We try to determine the inverse of a relationship by adding the relationship between two pawns.  If we're able to add
    // the relationship, the target pawn should have the inverse relationship.
    public PawnRelationDef TryToComputeInverseRelationship(PawnRelationDef def) {
        var source = randomizer.GenerateKindOfPawn(Find.FactionManager.OfPlayer.def.basicMemberKind);
        var target = randomizer.GenerateKindOfPawn(Find.FactionManager.OfPlayer.def.basicMemberKind);
        var info = def.workerClass.GetMethod("CreateRelation",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        if (info == null) {
            return null;
        }

        var worker = FindPawnRelationWorker(def);
        var req = new PawnGenerationRequest();
        worker.CreateRelation(source, target, ref req);
        foreach (var d in target.GetRelations(source)) {
            return d;
        }

        return null;
    }

    public PawnRelationWorker FindPawnRelationWorker(PawnRelationDef def) {
        var carefullyDef = DefDatabase<CarefullyPawnRelationDef>.GetNamedSilentFail(def.defName);
        if (carefullyDef == null || carefullyDef.workerClass == null) {
            return def.Worker;
        }

        var worker = carefullyDef.Worker;
        if (worker != null) {
            //Logger.Debug("Returned carefully worker for " + def.defName + ", " + worker.GetType().FullName);
            return carefullyDef.Worker;
        }

        return def.Worker;
    }
}
