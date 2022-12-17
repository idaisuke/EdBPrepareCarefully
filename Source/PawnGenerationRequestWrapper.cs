using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class PawnGenerationRequestWrapper {
    private PawnGenerationContext context = PawnGenerationContext.PlayerStarter;
    private Faction faction = Faction.OfPlayer;
    private float? fixedBiologicalAge;
    private float? fixedChronologicalAge;
    private Gender? fixedGender;
    private Ideo fixedIdeology;
    private PawnKindDef kindDef = Faction.OfPlayer.def.basicMemberKind;
    private bool mustBeCapableOfViolence;
    private bool worldPawnFactionDoesntMatter;

    public PawnGenerationRequest Request => CreateRequest();

    public PawnKindDef KindDef {
        set => kindDef = value;
    }

    public Faction Faction {
        set => faction = value;
    }

    public PawnGenerationContext Context {
        set => context = value;
    }

    public bool WorldPawnFactionDoesntMatter {
        set => worldPawnFactionDoesntMatter = value;
    }

    public float? FixedBiologicalAge {
        set => fixedBiologicalAge = value;
    }

    public float? FixedChronologicalAge {
        set => fixedChronologicalAge = value;
    }

    public Gender? FixedGender {
        set => fixedGender = value;
    }

    public bool MustBeCapableOfViolence {
        set => mustBeCapableOfViolence = value;
    }

    public Ideo FixedIdeology {
        set => fixedIdeology = value;
    }

    private PawnGenerationRequest CreateRequest() {
        return new PawnGenerationRequest(
            kindDef, // kind
            faction, // faction
            context, // context
            -1, // tile
            true, // forceGenerateNewPawn
            false, // allowDead
            false, // allowDowned
            false, // canGeneratePawnRelations
            mustBeCapableOfViolence, // mustBeCapableOfViolence
            0f, // colonistRelationChanceFactor
            false, // forceAddFreeWarmLayerIfNeeded
            true, // allowGay
            true, // allowPregnant
            false, // allowFood
            true, // allowAddictions
            false, // inhabitant
            false, // certainlyBeenInCryptosleep
            false, // forceRedressWorldPawnIfFormerColonist
            worldPawnFactionDoesntMatter, // worldPawnFactionDoesntMatter
            0f, // biocodeWeaponChance
            0f, // biocodeApparelChance
            null, // extraPawnForExtraRelationChance
            1f, // relationWithExtraPawnChanceFactor
            null, // validatorPreGear
            null, // validatorPostGear
            null, // forcedTraits
            null, // prohibitedTraits
            null, // minChanceToRedressWorldPawn
            fixedBiologicalAge, // fixedBiologicalAge
            fixedChronologicalAge, // fixedChronologicalAge
            fixedGender, // fixedGender
            null, // fixedLastName
            null, // fixedBirthName
            null, // fixedTitle
            fixedIdeology, // fixedIdeo
            false, // forceNoIdeo
            false, // forceNoBackstory
            true // forceRecruitable
        );
    }
}
