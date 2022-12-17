using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class CarefullyPawnRelationDef : Def {
    public bool animal = false;

    public List<String> conflicts = null;
    public string inverse = null;

    public bool needsCompatibility = false;

    [Unsaved] private PawnRelationWorker worker;

    public Type workerClass = null;

    public PawnRelationWorker Worker {
        get {
            if (workerClass != null && worker == null) {
                var pawnRelationDef = DefDatabase<PawnRelationDef>.GetNamedSilentFail(defName);
                if (pawnRelationDef != null) {
                    worker = (PawnRelationWorker)Activator.CreateInstance(workerClass);
                    worker.def = pawnRelationDef;
                }
            }

            return worker;
        }
    }
}
