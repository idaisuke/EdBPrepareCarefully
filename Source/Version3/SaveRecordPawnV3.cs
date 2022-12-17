using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully; 

public class SaveRecordPawnV3 : IExposable {
    public string adulthood;
    public int age;
    public List<string> apparel = new();
    public List<Color> apparelColors = new();
    public List<int> apparelLayers = new();
    public List<string> apparelStuff = new();
    public int biologicalAge;
    public string bodyType;
    public string childhood;
    public int chronologicalAge;
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
    public List<Passion> originalPassions = new();
    public List<Passion> passions = new();
    public string pawnKindDef;
    public bool randomInjuries = true;
    public bool randomRelations = false;
    public List<string> skillNames = new();
    public List<int> skillValues = new();
    public Color skinColor;
    public string thingDef;
    public List<int> traitDegrees = new();
    public List<string> traitNames = new();

    public void ExposeData() {
        Scribe_Values.Look<string>(ref id, "id");
        Scribe_Values.Look<string>(ref pawnKindDef, "pawnKindDef");
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
        Scribe_Collections.Look(ref skillNames, "skillNames", LookMode.Value, null);
        Scribe_Collections.Look(ref skillValues, "skillValues", LookMode.Value, null);
        Scribe_Collections.Look(ref passions, "passions", LookMode.Value, null);
        Scribe_Collections.Look(ref apparel, "apparel", LookMode.Value, null);
        Scribe_Collections.Look(ref apparelLayers, "apparelLayers", LookMode.Value, null);
        Scribe_Collections.Look(ref apparelStuff, "apparelStuff", LookMode.Value, null);
        Scribe_Collections.Look(ref apparelColors, "apparelColors", LookMode.Value, null);

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
