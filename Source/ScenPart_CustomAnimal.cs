using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class ScenPart_CustomAnimal : ScenPart {
    public PawnKindDef animalKindDef;
    private float bondToRandomPlayerPawnChance = 0.5f;
    public int count = 1;
    protected Gender gender = Gender.None;

    public ScenPart_CustomAnimal() {
        // Set the def to match the standard starting animal that we'll be replacing with this one.
        // Doing so makes sure that this part gets sorted as expected when building the scenario description
        def = ScenPartDefOf.StartingAnimal;
    }

    public PawnKindDef KindDef {
        get => animalKindDef;
        set => animalKindDef = value;
    }

    public Gender Gender {
        get => gender;
        set => gender = value;
    }

    public int Count {
        get => count;
        set => count = value;
    }

    public override IEnumerable<Thing> PlayerStartingThings() {
        var result = new List<Thing>();
        if (animalKindDef == null) {
            return result;
        }

        for (var i = 0; i < count; i++) {
            var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequestWrapper {
                FixedGender = gender,
                Faction = Faction.OfPlayer,
                KindDef = animalKindDef,
                Context = PawnGenerationContext.NonPlayer
            }.Request);
            if (pawn.Name == null || pawn.Name.Numerical) {
                pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
            }

            if (Rand.Value < bondToRandomPlayerPawnChance) {
                var bonded = Find.GameInitData.startingAndOptionalPawns.RandomElement<Pawn>();
                if (!bonded.story.traits.HasTrait(TraitDefOf.Psychopath)) {
                    bonded.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn);
                }
            }

            result.Add(pawn);
        }

        return result;
    }

    public override string Summary(Scenario scen) {
        return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith",
            ScenPart_StartingThing_Defined.PlayerStartWithIntro);
    }

    public override IEnumerable<string> GetSummaryListEntries(string tag) {
        if (tag == "PlayerStartsWith") {
            var label = new StringBuilder();
            var entries = new List<string>();
            if (KindDef.RaceProps.hasGenders) {
                label.Append("PawnMainDescGendered".Translate(gender.GetLabel(), KindDef.label).CapitalizeFirst());
            }
            else {
                label.Append(KindDef.label.CapitalizeFirst());
            }

            label.Append(" x");
            label.Append(count.ToString());
            entries.Add(label.ToString());
            return entries;
        }

        return Enumerable.Empty<string>();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref animalKindDef, "animalKind");
        Scribe_Values.Look(ref count, "count");
        Scribe_Values.Look(ref bondToRandomPlayerPawnChance, "bondToRandomPlayerPawnChance");
    }
}
