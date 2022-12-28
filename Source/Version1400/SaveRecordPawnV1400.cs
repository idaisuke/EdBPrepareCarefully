using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordPawnV1400 {
    private readonly string id;
    public List<string> abilities = new();
    public string adulthood;
    public List<SaveRecordApparelV4> apparel = new();
    public List<Color> apparelColors = new();
    public List<int> apparelLayers = new();
    public List<string> apparelStuff = new();
    public string beard;
    public int biologicalAge;
    public string bodyTattoo;
    public string bodyType;
    public string childhood;
    public int chronologicalAge;
    public string compsXml = null;
    public string faceTattoo;
    public Color? favoriteColor;
    public string firstName;
    public Gender gender;
    public List<SaveRecordGeneV1400> genes = new();
    public Color hairColor;
    public string hairDef;
    public string headGraphicPath;
    public SaveRecordIdeoV5 ideo;
    public List<SaveRecordImplantV3> implants = new();
    public List<SaveRecordInjuryV3> injuries = new();
    public string lastName;
    public string nickName;
    public string originalFactionDef;
    public string pawnKindDef;
    public bool randomInjuries = true;
    public bool randomRelations = false;
    public List<string> savedComps = new();
    public List<SaveRecordSkillV4> skills = new();
    public Color skinColor;
    public string thingDef;
    public List<SaveRecordTraitV1400> traits = new();
    private string type;


    public SaveRecordPawnV1400(CustomPawn customPawn) {
        id = customPawn.Id;
        type = customPawn.Type.ToString();
    }

    public PawnKindDef PawnKindDef {
        get {
            var def = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKindDef);
            if (def != null) {
                return def;
            }

            Logger.Warning("Pawn kind definition for the saved character (" + pawnKindDef +
                           ") not found.  Picking the basic colony pawn kind definition.");

            return FactionDefOf.PlayerColony.basicMemberKind;
        }
    }

    public ThingDef? ThingDef {
        get {
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(thingDef);
            if (def == null) {
                Logger.Warning("Pawn's thing definition {" + thingDef + "} was not found.");
            }

            return def;
        }
    }
}
