using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnCompsLoader : IExposable {
    public List<ThingComp> Comps = new();

    public PawnCompsLoader(Pawn target, PawnCompRules rules) {
        TargetPawn = target;
        Rules = rules ?? new PawnCompInclusionRules();
    }

    public ThingWithComps TargetPawn { get; set; }
    public PawnCompRules Rules { get; set; }

    public void ExposeData() {
        if (Scribe.mode == LoadSaveMode.LoadingVars) {
            if (TargetPawn.AllComps != null) {
                foreach (var c in TargetPawn.AllComps) {
                    if (Rules.IsCompIncluded(c)) {
                        //Logger.Debug("Deserializing into " + c.GetType().FullName);
                        c.PostExposeData();
                    }
                }
            }
        }
    }
}
