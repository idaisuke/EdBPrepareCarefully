using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordPawnV5 : IExposable {
    public List<string> abilities = new();
    public string adulthood;
    public int age;
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
    public string compsXml;
    public string faceTattoo;
    public SaveRecordFactionV4 faction;
    public Color? favoriteColor;
    public string firstName;
    public Gender gender;
    public Color hairColor;
    public string hairDef;
    public string headGraphicPath;
    public string id;
    public SaveRecordIdeoV5 ideo;
    public List<SaveRecordImplantV3> implants = new();
    public List<SaveRecordInjuryV3> injuries = new();
    public string lastName;
    public float melanin;
    public string nickName;
    public string originalFactionDef;

    public PawnCompsSaver pawnCompsSaver;
    public string pawnKindDef;
    public bool randomInjuries = true;
    public bool randomRelations = false;
    public List<string> savedComps = new();
    public List<SaveRecordSkillV4> skills = new();
    public Color skinColor;
    public string thingDef;
    public List<int> traitDegrees = new();

    // Deprecated.  Here for backwards compatibility with V4
    public List<string> traitNames = new();
    public List<SaveRecordTraitV5> traits = new();
    public string type;

    public SaveRecordPawnV5() {
    }

    public SaveRecordPawnV5(CustomPawn pawn) {
        id = pawn.Id;
        thingDef = pawn.Pawn.def.defName;
        type = pawn.Type.ToString();
        if (pawn.Type == CustomPawnType.World && pawn.Faction != null) {
            faction = new SaveRecordFactionV4 {
                def = pawn.Faction?.Def?.defName, index = pawn.Faction.Index, leader = pawn.Faction.Leader
            };
        }

        pawnKindDef = pawn.OriginalKindDef?.defName ?? pawn.Pawn.kindDef.defName;
        originalFactionDef = pawn.OriginalFactionDef?.defName;
        gender = pawn.Gender;
        adulthood = pawn.Adulthood?.identifier ?? pawn.LastSelectedAdulthoodBackstory?.identifier;
        childhood = pawn.Childhood?.identifier;
        skinColor = pawn.SkinColor;
        melanin = pawn.MelaninLevel;
        hairDef = pawn.HairDef.defName;
        hairColor = pawn.Pawn.story.HairColor;
        // headGraphicPath = pawn.HeadGraphicPath;
        bodyType = pawn.BodyType.defName;
        beard = pawn.Beard?.defName;
        faceTattoo = pawn.FaceTattoo?.defName;
        bodyTattoo = pawn.BodyTattoo?.defName;
        firstName = pawn.FirstName;
        nickName = pawn.NickName;
        lastName = pawn.LastName;
        favoriteColor = pawn.Pawn.story.favoriteColor;
        age = 0;
        biologicalAge = pawn.BiologicalAge;
        chronologicalAge = pawn.ChronologicalAge;
        foreach (var trait in pawn.Traits) {
            if (trait != null) {
                traits.Add(new SaveRecordTraitV5 { def = trait.def.defName, degree = trait.Degree });
            }
        }

        foreach (var skill in pawn.Pawn.skills.skills) {
            skills.Add(new SaveRecordSkillV4 {
                name = skill.def.defName,
                value = pawn.GetUnmodifiedSkillLevel(skill.def),
                passion = pawn.currentPassions[skill.def]
            });
        }

        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(pawn)) {
            if (layer.Apparel) {
                var apparelThingDef = pawn.GetAcceptedApparel(layer);
                var color = pawn.GetColor(layer);
                if (apparelThingDef != null) {
                    var apparelStuffDef = pawn.GetSelectedStuff(layer);
                    apparel.Add(new SaveRecordApparelV4 {
                        layer = layer.Name,
                        apparel = apparelThingDef.defName,
                        stuff = apparelStuffDef?.defName ?? "",
                        color = color
                    });
                }
            }
        }

        var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(pawn);
        foreach (var implant in pawn.Implants) {
            var saveRecord = new SaveRecordImplantV3(implant);
            if (implant.BodyPartRecord != null) {
                var part = healthOptions.FindBodyPartsForRecord(implant.BodyPartRecord);
                if (part != null && part.Index > 0) {
                    saveRecord.bodyPartIndex = part.Index;
                }
            }

            implants.Add(saveRecord);
        }

        foreach (var injury in pawn.Injuries) {
            var saveRecord = new SaveRecordInjuryV3(injury);
            if (injury.BodyPartRecord != null) {
                var part = healthOptions.FindBodyPartsForRecord(injury.BodyPartRecord);
                if (part != null && part.Index > 0) {
                    saveRecord.bodyPartIndex = part.Index;
                }
            }

            injuries.Add(saveRecord);
        }

        if (pawn.Pawn?.abilities != null) {
            abilities.AddRange(pawn.Pawn.abilities.abilities.Select(a => a.def.defName));
        }

        if (ModsConfig.IdeologyActive && pawn.Pawn.ideo != null) {
            var ideo = pawn.Pawn.ideo.Ideo;
            this.ideo = new SaveRecordIdeoV5 {
                certainty = pawn.Pawn.ideo.Certainty,
                name = ideo?.name,
                sameAsColony = ideo == Find.FactionManager.OfPlayer.ideos.PrimaryIdeo,
                culture = ideo?.culture.defName
            };
            if (ideo != null) {
                this.ideo.memes = new List<string>(ideo.memes.Select(m => m.defName));
            }
            //Logger.Debug(string.Join(", ", pawn.Pawn.ideo.Ideo?.memes.Select(m => m.defName)));
        }

        pawnCompsSaver = new PawnCompsSaver(pawn.Pawn, DefaultPawnCompRules.RulesForSaving);
    }

    public void ExposeData() {
        Scribe_Values.Look<string>(ref id, "id");
        Scribe_Values.Look<string>(ref type, "type");
        Scribe_Deep.Look(ref faction, "faction");
        Scribe_Values.Look<string>(ref pawnKindDef, "pawnKindDef");
        Scribe_Values.Look<string>(ref originalFactionDef, "originalFactionDef");
        Scribe_Values.Look(ref thingDef, "thingDef", ThingDefOf.Human.defName);
        Scribe_Values.Look(ref gender, "gender", Gender.Male);
        Scribe_Values.Look<string>(ref childhood, "childhood");
        Scribe_Values.Look<string>(ref adulthood, "adulthood");
        Scribe_Collections.Look(ref traits, "traits", LookMode.Deep, null);
        Scribe_Collections.Look(ref traitNames, "traitNames", LookMode.Value, null);
        Scribe_Collections.Look(ref traitDegrees, "traitDegrees", LookMode.Value, null);
        Scribe_Values.Look(ref skinColor, "skinColor", Color.white);
        Scribe_Values.Look(ref melanin, "melanin", -1.0f);
        Scribe_Values.Look<string>(ref bodyType, "bodyType");
        Scribe_Values.Look<string>(ref headGraphicPath, "headGraphicPath");
        Scribe_Values.Look<string>(ref hairDef, "hairDef");
        Scribe_Values.Look(ref hairColor, "hairColor", Color.white);
        Scribe_Values.Look<string>(ref beard, "beard");
        Scribe_Values.Look<string>(ref faceTattoo, "faceTattoo");
        Scribe_Values.Look<string>(ref bodyTattoo, "bodyTattoo");
        Scribe_Values.Look<string>(ref hairDef, "hairDef");
        Scribe_Values.Look<string>(ref firstName, "firstName");
        Scribe_Values.Look<string>(ref nickName, "nickName");
        Scribe_Values.Look<string>(ref lastName, "lastName");
        Scribe_Values.Look(ref favoriteColor, "favoriteColor");
        Scribe_Values.Look(ref biologicalAge, "biologicalAge");
        Scribe_Values.Look(ref chronologicalAge, "chronologicalAge");
        Scribe_Collections.Look(ref skills, "skills", LookMode.Deep, null);
        Scribe_Collections.Look(ref apparel, "apparel", LookMode.Deep, null);
        Scribe_Deep.Look(ref ideo, "ideo");
        Scribe_Collections.Look(ref abilities, "abilities", LookMode.Value, null);

        if (Scribe.mode == LoadSaveMode.Saving) {
            Scribe_Collections.Look(ref implants, "implants", LookMode.Deep, null);
        }
        else {
            if (Scribe.loader.curXmlParent["implants"] != null) {
                Scribe_Collections.Look(ref implants, "implants", LookMode.Deep, null);
            }
        }

        if (Scribe.mode == LoadSaveMode.Saving) {
            Scribe_Collections.Look(ref injuries, "injuries", LookMode.Deep, null);
        }
        else {
            if (Scribe.loader.curXmlParent["injuries"] != null) {
                Scribe_Collections.Look(ref injuries, "injuries", LookMode.Deep, null);
            }
        }

        if (Scribe.mode == LoadSaveMode.Saving) {
            Scribe_Deep.Look(ref pawnCompsSaver, "compFields");
            Scribe_Collections.Look(ref pawnCompsSaver.savedComps, "savedComps");
        }
        else {
            if (Scribe.loader.EnterNode("compFields")) {
                try {
                    compsXml = Scribe.loader.curXmlParent.InnerXml;
                }
                finally {
                    Scribe.loader.ExitNode();
                }
            }

            Scribe_Collections.Look(ref savedComps, "savedComps");
        }
    }
}
