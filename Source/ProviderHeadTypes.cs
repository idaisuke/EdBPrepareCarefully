using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EdB.PrepareCarefully.Reflection;
using RimWorld;
using Verse;
using Pawn = Verse.Pawn;

namespace EdB.PrepareCarefully;

public class ProviderHeadTypes {
    protected List<string> headPaths = new();
    protected List<Graphic> heads = new();
    protected Dictionary<ThingDef, OptionsHeadType> headTypeLookup = new();
    public Dictionary<string, CustomHeadType> pathDictionary = new();

    public ProviderAlienRaces AlienRaceProvider {
        get;
        set;
    }

    public IEnumerable<CustomHeadType> GetHeadTypes(CustomPawn pawn) {
        return GetHeadTypes(pawn.Pawn.def, pawn.Gender);
    }

    public IEnumerable<CustomHeadType> GetHeadTypes(ThingDef race, Gender gender) {
        var headTypes = GetHeadTypesForRace(race);
        return headTypes.GetHeadTypesForGender(gender);
    }

    public CustomHeadType FindHeadTypeForPawn(Pawn pawn) {
        var headTypes = GetHeadTypesForRace(pawn.def);
        var result = headTypes.FindHeadTypeForPawn(pawn);
        if (result == null) {
            Logger.Warning("Could not find a head type for the pawn: " + pawn.def.defName +
                           ". Head type selection disabled for this pawn");
        }

        return result;
    }

    public CustomHeadType FindHeadType(ThingDef race, string graphicsPath) {
        var headTypes = GetHeadTypesForRace(race);
        //Logger.Debug("headTypes: \n" + String.Join("\n", headTypes.headTypes.ToList().ConvertAll(t => t.GraphicPath)));
        return headTypes.FindHeadTypeByGraphicsPath(graphicsPath);
    }

    public CustomHeadType FindHeadTypeForGender(ThingDef race, CustomHeadType headType, Gender gender) {
        var headTypes = GetHeadTypesForRace(race);
        return headTypes.FindHeadTypeForGender(headType, gender);
    }

    protected OptionsHeadType GetHeadTypesForRace(ThingDef race) {
        if (!headTypeLookup.TryGetValue(race, out var headTypes)) {
            headTypes = InitializeHeadTypes(race);
            headTypeLookup.Add(race, headTypes);
        }

        if (headTypes == null && race != ThingDefOf.Human) {
            return GetHeadTypesForRace(ThingDefOf.Human);
        }

        return headTypes;
    }

    protected OptionsHeadType InitializeHeadTypes(ThingDef race) {
        OptionsHeadType result;
        // If the race definition has an alien comp, then look for the head types in it.  If not, then use the default human head types.
        CompProperties alienCompProperties = null;
        if (race != null && race.comps != null) {
            alienCompProperties = race.comps.FirstOrDefault(comp => {
                return comp.compClass.Name == "AlienComp";
            });
        }

        if (alienCompProperties == null) {
            result = InitializeHumanHeadTypes();
        }
        else {
            result = InitializeAlienHeadTypes(race);
        }

        //Logger.Debug("Head Types for " + race.defName + ":");
        //Logger.Debug("  Male: ");
        //foreach (var h in result.GetHeadTypesForGender(Gender.Male)) {
        //    Logger.Debug("    " + h.ToString());
        //}
        //Logger.Debug("  Female: ");
        //foreach (var h in result.GetHeadTypesForGender(Gender.Female)) {
        //    Logger.Debug("    " + h.ToString());
        //}
        return result;
    }

    protected OptionsHeadType InitializeHumanHeadTypes() {
        //Logger.Debug("InitializeHumanHeadTypes()");
        GraphicDatabaseHeadRecords.BuildDatabaseIfNecessary();
        string[] headsFolderPaths = { "Things/Pawn/Humanlike/Heads/Male", "Things/Pawn/Humanlike/Heads/Female" };
        var result = new OptionsHeadType();
        for (var i = 0; i < headsFolderPaths.Length; i++) {
            var text = headsFolderPaths[i];
            IEnumerable<string> graphicsInFolder = GraphicDatabaseUtility.GraphicNamesInFolder(text);
            foreach (var current in GraphicDatabaseUtility.GraphicNamesInFolder(text)) {
                var fullPath = text + "/" + current;
                var headType = CreateHumanHeadTypeFromGenderedGraphicPath(fullPath);
                result.AddHeadType(headType);
                if (!pathDictionary.ContainsKey(fullPath)) {
                    pathDictionary.Add(fullPath, headType);
                }
            }
        }

        return result;
    }

    protected OptionsHeadType InitializeAlienHeadTypes(ThingDef raceDef) {
        //Logger.Debug("InitializeAlienHeadTypes(" + raceDef.defName + ")");
        var alienRace = AlienRaceProvider.GetAlienRace(raceDef);
        var result = new OptionsHeadType();
        if (alienRace == null) {
            Logger.Warning("Could not initialize head types for alien race, " + raceDef +
                           ", because the race's thing definition was missing");
            return result;
        }

        //Logger.Debug("alienRace.GraphicsPathForHeads = " + alienRace.GraphicsPathForHeads);
        if (alienRace.GraphicsPathForHeads == null) {
            Logger.Warning("Could not initialize head types for alien race, " + raceDef +
                           ", because no path for head graphics was found.");
            return result;
        }

        foreach (var crownType in alienRace.CrownTypes) {
            //Logger.Debug(" - " + crownType);
            if (alienRace.GenderSpecificHeads) {
                var maleHead =
                    CreateGenderedAlienHeadTypeFromCrownType(alienRace.GraphicsPathForHeads, crownType, Gender.Male);
                var femaleHead =
                    CreateGenderedAlienHeadTypeFromCrownType(alienRace.GraphicsPathForHeads, crownType, Gender.Female);
                if (maleHead != null) {
                    //Logger.Debug("   - MALE: " + maleHead.GraphicPath);
                    result.AddHeadType(maleHead);
                }

                if (femaleHead != null) {
                    //Logger.Debug("   - FEMALE: " + femaleHead.GraphicPath);
                    result.AddHeadType(femaleHead);
                }
            }
            else {
                var head = CreateMultiGenderAlienHeadTypeFromCrownType(alienRace.GraphicsPathForHeads, crownType);
                if (head != null) {
                    //Logger.Debug("   - MULTIGENDER: " + head.GraphicPath);
                    result.AddHeadType(head);
                }
            }
        }

        return result;
    }

    protected CrownType FindCrownTypeEnumValue(string crownType) {
        if (crownType.Contains(CrownType.Average.ToString() + "_")) {
            return CrownType.Average;
        }

        if (crownType.Contains(CrownType.Narrow.ToString() + "_")) {
            return CrownType.Narrow;
        }

        return CrownType.Undefined;
    }

    protected CustomHeadType CreateGenderedAlienHeadTypeFromCrownType(string graphicsPath, string crownType,
        Gender gender) {
        var result = new CustomHeadType();
        result.Gender = gender;
        result.Label = LabelFromCrownType(crownType);

        // Build the full graphics path for this head type
        var pathValue = string.Copy(graphicsPath);
        if (!pathValue.EndsWith("/")) {
            pathValue += "/";
        }

        string genderPrefix;
        ;
        if (gender == Gender.Female) {
            genderPrefix = "Female_";
        }
        else if (gender == Gender.Male) {
            genderPrefix = "Male_";
        }
        else {
            genderPrefix = "None_";
        }

        string altGenderPrefix;
        if (gender == Gender.Female) {
            altGenderPrefix = "Female/";
        }
        else if (gender == Gender.Male) {
            altGenderPrefix = "Male/";
        }
        else {
            altGenderPrefix = "None/";
        }

        result.GraphicPath = pathValue + genderPrefix + crownType;
        result.AlternateGraphicPath = pathValue + altGenderPrefix + genderPrefix + crownType;
        result.CrownType = FindCrownTypeEnumValue(crownType);
        result.AlienCrownType = crownType;
        return result;
    }

    protected CustomHeadType CreateMultiGenderAlienHeadTypeFromCrownType(string graphicsPath, string crownType) {
        var result = new CustomHeadType();
        var pathValue = string.Copy(graphicsPath);
        if (!pathValue.EndsWith("/")) {
            pathValue += "/";
        }

        pathValue += crownType;
        result.GraphicPath = pathValue;
        result.AlternateGraphicPath = null;
        result.Label = LabelFromCrownType(crownType);
        result.Gender = null;
        result.CrownType = FindCrownTypeEnumValue(crownType);
        result.AlienCrownType = crownType;
        return result;
    }

    protected CustomHeadType CreateHumanHeadTypeFromGenderedGraphicPath(string graphicPath) {
        var result = new CustomHeadType();
        result.GraphicPath = graphicPath;
        result.AlternateGraphicPath = null;
        result.Label = LabelFromGraphicsPath(graphicPath);
        var strArray = Path.GetFileNameWithoutExtension(graphicPath).Split('_');
        try {
            result.CrownType = (CrownType)ParseHelper.FromString(strArray[strArray.Length - 2], typeof(CrownType));
            result.Gender = (Gender)ParseHelper.FromString(strArray[strArray.Length - 3], typeof(Gender));
        }
        catch (Exception ex) {
            Logger.Warning("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
            result.CrownType = CrownType.Undefined;
            result.Gender = Gender.None;
        }

        return result;
    }

    protected string LabelFromGraphicsPath(string path) {
        try {
            var pathValues = path.Split('/');
            var crownType = pathValues[pathValues.Length - 1];
            var values = crownType.Split('_');
            return values[values.Count() - 2] + ", " + values[values.Count() - 1];
        }
        catch (Exception) {
            Logger.Warning("Could not determine head type label from graphics path: " + path);
            return "EdB.PC.Common.Default".Translate();
        }
    }

    protected string LabelFromCrownType(string crownType) {
        string value;
        value = Regex.Replace(crownType, "(\\B[A-Z]+?(?=[A-Z][^A-Z])|\\B[A-Z]+?(?=[^A-Z]))", " $1");
        value = value.Replace("_", " ");
        value = Regex.Replace(value, "\\s+", " ");
        return value;
    }
}
