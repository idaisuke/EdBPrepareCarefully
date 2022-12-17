using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully; 

public class SaveRecordPawnV4 : IExposable {
    public string adulthood;
    public int age;
    public SaveRecordAlienV4 alien;
    public List<SaveRecordApparelV4> apparel = new();
    public List<Color> apparelColors = new();
    public List<int> apparelLayers = new();
    public List<string> apparelStuff = new();
    public int biologicalAge;
    public string bodyType;
    public string childhood;
    public int chronologicalAge;
    public SaveRecordFactionV4 faction;
    public string firstName;
    public Gender gender;
    public Color hairColor;
    public string hairDef;
    public string headGraphicPath;
    public string id;
    public List<SaveRecordImplantV3> implants = new();
    public List<SaveRecordInjuryV3> injuries = new();
    public string lastName;
    public float melanin;
    public string nickName;
    public string originalFactionDef;
    public string pawnKindDef;
    public bool randomInjuries = true;
    public bool randomRelations = false;
    public List<SaveRecordSkillV4> skills = new();
    public Color skinColor;
    public string thingDef;
    public List<int> traitDegrees = new();
    public List<string> traitNames = new();
    public string type;

    public SaveRecordPawnV4() {
    }

    public SaveRecordPawnV4(CustomPawn pawn) {
        id = pawn.Id;
        thingDef = pawn.Pawn.def.defName;
        type = pawn.Type.ToString();
        if (pawn.Type == CustomPawnType.World && pawn.Faction != null) {
            faction = new SaveRecordFactionV4();
            faction.def = pawn.Faction.Def != null ? pawn.Faction.Def.defName : null;
            faction.index = pawn.Faction.Index;
            faction.leader = pawn.Faction.Leader;
        }

        pawnKindDef = pawn.OriginalKindDef != null ? pawn.OriginalKindDef.defName : pawn.Pawn.kindDef.defName;
        originalFactionDef = pawn.OriginalFactionDef != null ? pawn.OriginalFactionDef.defName : null;
        gender = pawn.Gender;
        if (pawn.Adulthood != null) {
            adulthood = pawn.Adulthood.identifier;
        }
        else {
            adulthood = pawn.LastSelectedAdulthoodBackstory?.identifier;
        }

        childhood = pawn.Childhood.identifier;
        skinColor = pawn.Pawn.story.SkinColor;
        melanin = pawn.Pawn.story.melanin;
        hairDef = pawn.HairDef.defName;
        hairColor = pawn.Pawn.story.hairColor;
        headGraphicPath = pawn.HeadGraphicPath;
        bodyType = pawn.BodyType.defName;
        firstName = pawn.FirstName;
        nickName = pawn.NickName;
        lastName = pawn.LastName;
        age = 0;
        biologicalAge = pawn.BiologicalAge;
        chronologicalAge = pawn.ChronologicalAge;
        foreach (var trait in pawn.Traits) {
            if (trait != null) {
                traitNames.Add(trait.def.defName);
                traitDegrees.Add(trait.Degree);
            }
        }

        foreach (var skill in pawn.Pawn.skills.skills) {
            var skillRecord = new SaveRecordSkillV4();
            skillRecord.name = skill.def.defName;
            skillRecord.value = pawn.GetUnmodifiedSkillLevel(skill.def);
            skillRecord.passion = pawn.currentPassions[skill.def];
            skills.Add(skillRecord);
        }

        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(pawn)) {
            if (layer.Apparel) {
                var apparelThingDef = pawn.GetAcceptedApparel(layer);
                var color = pawn.GetColor(layer);
                if (apparelThingDef != null) {
                    var apparelStuffDef = pawn.GetSelectedStuff(layer);
                    var apparelRecord = new SaveRecordApparelV4();
                    apparelRecord.layer = layer.Name;
                    apparelRecord.apparel = apparelThingDef.defName;
                    apparelRecord.stuff = apparelStuffDef != null ? apparelStuffDef.defName : "";
                    apparelRecord.color = color;
                    apparel.Add(apparelRecord);
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

        var alienComp = ProviderAlienRaces.FindAlienCompForPawn(pawn.Pawn);
        if (alienComp != null) {
            alien = new SaveRecordAlienV4();
            alien.crownType = ProviderAlienRaces.GetCrownTypeFromComp(alienComp);
            alien.skinColor = ProviderAlienRaces.GetSkinColorFromComp(alienComp);
            alien.skinColorSecond = ProviderAlienRaces.GetSkinColorSecondFromComp(alienComp);
            alien.hairColorSecond = ProviderAlienRaces.GetHairColorSecondFromComp(alienComp);
        }
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
        Scribe_Collections.Look(ref traitNames, "traitNames", LookMode.Value, null);
        Scribe_Collections.Look(ref traitDegrees, "traitDegrees", LookMode.Value, null);
        Scribe_Values.Look(ref skinColor, "skinColor", Color.white);
        Scribe_Values.Look(ref melanin, "melanin", -1.0f);
        Scribe_Values.Look<string>(ref bodyType, "bodyType");
        Scribe_Values.Look<string>(ref hairDef, "hairDef");
        Scribe_Values.Look(ref hairColor, "hairColor", Color.white);
        Scribe_Values.Look<string>(ref headGraphicPath, "headGraphicPath");
        Scribe_Values.Look<string>(ref firstName, "firstName");
        Scribe_Values.Look<string>(ref nickName, "nickName");
        Scribe_Values.Look<string>(ref lastName, "lastName");
        if (Scribe.mode == LoadSaveMode.LoadingVars) {
            Scribe_Values.Look(ref age, "age");
        }

        Scribe_Values.Look(ref biologicalAge, "biologicalAge");
        Scribe_Values.Look(ref chronologicalAge, "chronologicalAge");
        Scribe_Collections.Look(ref skills, "skills", LookMode.Deep, null);
        Scribe_Collections.Look(ref apparel, "apparel", LookMode.Deep, null);
        Scribe_Deep.Look(ref alien, "alien");

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
    }

    public HairDef FindHairDef(string name) {
        return DefDatabase<HairDef>.GetNamedSilentFail(name);
    }

    public Backstory FindBackstory(string name) {
        return BackstoryDatabase.allBackstories.Values.ToList().Find((Backstory b) => {
            return b.identifier.Equals(name);
        });
    }

    public Trait FindTrait(string name, int degree) {
        foreach (var def in DefDatabase<TraitDef>.AllDefs) {
            if (!def.defName.Equals(name)) {
                continue;
            }

            List<TraitDegreeData> degreeData = def.degreeDatas;
            var count = degreeData.Count;
            if (count > 0) {
                for (var i = 0; i < count; i++) {
                    if (degree == degreeData[i].degree) {
                        var trait = new Trait(def, degreeData[i].degree, true);
                        return trait;
                    }
                }
            }
            else {
                return new Trait(def, 0, true);
            }
        }

        return null;
    }

    public SkillDef FindSkillDef(Pawn pawn, string name) {
        foreach (var skill in pawn.skills.skills) {
            if (skill.def.defName.Equals(name)) {
                return skill.def;
            }
        }

        return null;
    }
}
