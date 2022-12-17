using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class ScenPart_CustomScatterThingsNearPlayerStart : ScenPart_ScatterThings {
    protected int radius = 4;

    public ScenPart_CustomScatterThingsNearPlayerStart() {
        // Set the def to match the standard scatter part that we'll be replacing with this one.
        // Doing so makes sure that this part gets sorted as expected when building the scenario description
        def = ScenPartDefOf.ScatterThingsNearPlayerStart;
    }

    public ThingDef ThingDef {
        get => thingDef;
        set => thingDef = value;
    }

    public ThingDef StuffDef {
        get => stuff;
        set => stuff = value;
    }

    public int Count {
        get => count;
        set => count = value;
    }

    protected override bool NearPlayerStart => true;

    public int Radius {
        get => radius;
        set => radius = value;
    }

    public override void GenerateIntoMap(Map map) {
        if (Find.GameInitData == null) {
            return;
        }

        new GenStep_CustomScatterThings {
            nearPlayerStart = NearPlayerStart,
            thingDef = thingDef,
            stuff = stuff,
            count = count,
            spotMustBeStandable = true,
            minSpacing = 5f,
            clusterSize = thingDef.category != ThingCategory.Building ? 4 : 1,
            radius = 4 + radius
        }.Generate(map, new GenStepParams());
    }

    public override string Summary(Scenario scen) {
        return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith",
            ScenPart_StartingThing_Defined.PlayerStartWithIntro);
    }

    public override IEnumerable<string> GetSummaryListEntries(string tag) {
        if (tag == "PlayerStartsWith") {
            var entries = new List<string>();
            entries.Add(GenLabel.ThingLabel(thingDef, stuff, count).CapitalizeFirst());
            return entries;
        }

        return Enumerable.Empty<string>();
    }
}
