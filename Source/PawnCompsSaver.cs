using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnCompsSaver : IExposable {
    private readonly List<ThingComp> comps = new();
    public List<string> savedComps = new();

    // The constructor needs to take the target pawn as an argument--the pawn to which the comps will be copied.
    public PawnCompsSaver(Pawn source, PawnCompRules rules) {
        comps = source.AllComps ?? new List<ThingComp>();
        Rules = rules ?? new PawnCompExclusionRules();
    }

    public PawnCompsSaver(IEnumerable<ThingComp> comps, PawnCompRules rules) {
        this.comps = new List<ThingComp>(comps);
        Rules = rules;
    }

    public ThingWithComps SourcePawn { get; set; }
    public PawnCompRules Rules { get; set; }

    public void ExposeData() {
        if (Scribe.mode == LoadSaveMode.Saving && comps != null) {
            for (var i = 0; i < comps.Count; i++) {
                var comp = comps[i];
                if (Rules == null || Rules.IsCompIncluded(comp)) {
                    comp.PostExposeData();
                    savedComps.Add(comp.GetType().FullName);
                }
                //Logger.Debug("Excluded comp: " + comp.GetType().FullName);
            }
        }
    }
}
