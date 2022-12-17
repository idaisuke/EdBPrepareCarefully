using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully; 

public class ProviderAlienRaces {
    protected float defaultMinAgeForAdulthood = 20f;
    protected Dictionary<ThingDef, AlienRace> lookup = new();

    public ProviderAlienRaces() {
        defaultMinAgeForAdulthood =
            ReflectionUtil.GetNonPublicStatic<float>(typeof(PawnBioAndNameGenerator), "MinAgeForAdulthood");
        if (defaultMinAgeForAdulthood <= 0f) {
            defaultMinAgeForAdulthood = 20.0f;
        }
    }

    public float DefaultMinAgeForAdulthood => defaultMinAgeForAdulthood;

    public AlienRace GetAlienRace(ThingDef def) {
        AlienRace result;
        if (lookup.TryGetValue(def, out result)) {
            return result;
        }

        if (IsAlienRace(def)) {
            result = InitializeAlienRace(def);
            if (result != null) {
                lookup.Add(def, result);
            }

            return result;
        }

        return null;
    }

    public static ThingComp FindAlienCompForPawn(Pawn pawn) {
        return pawn.AllComps.FirstOrDefault(comp => {
            return comp.GetType().Name == "AlienComp";
        });
    }

    public static string GetCrownTypeFromComp(ThingComp alienComp) {
        return ReflectionUtil.GetPublicField(alienComp, "crownType").GetValue(alienComp) as string;
    }

    public static void SetCrownTypeOnComp(ThingComp alienComp, string value) {
        ReflectionUtil.GetPublicField(alienComp, "crownType").SetValue(alienComp, value);
    }

    public static Color GetSkinColorFromComp(ThingComp alienComp) {
        return (Color)ReflectionUtil.GetPublicField(alienComp, "skinColor").GetValue(alienComp);
    }

    public static void SetSkinColorOnComp(ThingComp alienComp, Color value) {
        ReflectionUtil.GetPublicField(alienComp, "skinColor").SetValue(alienComp, value);
    }

    public static Color GetSkinColorSecondFromComp(ThingComp alienComp) {
        return (Color)ReflectionUtil.GetPublicField(alienComp, "skinColorSecond").GetValue(alienComp);
    }

    public static void SetSkinColorSecondOnComp(ThingComp alienComp, Color value) {
        ReflectionUtil.GetPublicField(alienComp, "skinColorSecond").SetValue(alienComp, value);
    }

    public static Color GetHairColorSecondFromComp(ThingComp alienComp) {
        return (Color)ReflectionUtil.GetPublicField(alienComp, "hairColorSecond").GetValue(alienComp);
    }

    public static void SetHairColorSecondOnComp(ThingComp alienComp, Color value) {
        ReflectionUtil.GetPublicField(alienComp, "hairColorSecond").SetValue(alienComp, value);
    }

    public static string GetCrownTypeFromPawn(Pawn pawn) {
        var alienComp = FindAlienCompForPawn(pawn);
        if (alienComp == null) {
            return null;
        }

        return GetCrownTypeFromComp(alienComp);
    }

    public static bool IsAlienRace(ThingDef raceDef) {
        var alienRaceField = raceDef.GetType().GetField("alienRace", BindingFlags.Public | BindingFlags.Instance);
        return alienRaceField != null;
    }

    protected ColorGenerator FindPrimarySkinColorGenerator(ThingDef raceDef, object alienPartGeneratorObject) {
        var generator = FindPrimarySkinColorGeneratorPre12(raceDef, alienPartGeneratorObject);
        if (generator != null) {
            return generator;
        }

        return FindPrimaryColorGenerator(raceDef, alienPartGeneratorObject, "skin");
    }

    protected ColorGenerator FindSecondarySkinColorGenerator(ThingDef raceDef, object alienPartGeneratorObject) {
        var generator = FindSecondarySkinColorGeneratorPre12(raceDef, alienPartGeneratorObject);
        if (generator != null) {
            return generator;
        }

        return FindSecondaryColorGenerator(raceDef, alienPartGeneratorObject, "skin");
    }

    protected ColorGenerator FindPrimarySkinColorGeneratorPre12(ThingDef raceDef, object alienPartGeneratorObject) {
        return QuietReflectionUtil.GetFieldValue<ColorGenerator>(alienPartGeneratorObject, "alienskincolorgen");
    }

    protected ColorGenerator FindSecondarySkinColorGeneratorPre12(ThingDef raceDef, object alienPartGeneratorObject) {
        return QuietReflectionUtil.GetFieldValue<ColorGenerator>(alienPartGeneratorObject, "alienskinsecondcolorgen");
    }

    protected ColorGenerator FindPrimaryColorGenerator(ThingDef raceDef, object alienPartGeneratorObject,
        string channelName) {
        return FindColorGenerator(raceDef, alienPartGeneratorObject, channelName, "first");
    }

    protected ColorGenerator FindSecondaryColorGenerator(ThingDef raceDef, object alienPartGeneratorObject,
        string channelName) {
        return FindColorGenerator(raceDef, alienPartGeneratorObject, channelName, "second");
    }

    protected ColorGenerator FindColorGenerator(ThingDef raceDef, object alienPartGeneratorObject, string channelName,
        string generatorFieldName) {
        var colorChannelsObject = GetFieldValue(raceDef, alienPartGeneratorObject, "colorChannels", true);
        if (colorChannelsObject == null) {
            Logger.Warning("didn't find colorChannels field");
            return null;
        }

        var colorChannelList = colorChannelsObject as IList;
        if (colorChannelList == null) {
            return null;
        }

        object foundGenerator = null;
        foreach (var generator in colorChannelList) {
            var name = GetFieldValue(raceDef, generator, "name", true) as string;
            if (channelName == name) {
                foundGenerator = generator;
                break;
            }
        }

        if (foundGenerator == null) {
            return null;
        }

        return GetFieldValue(raceDef, foundGenerator, generatorFieldName, true) as ColorGenerator;
    }

    protected AlienRace InitializeAlienRace(ThingDef raceDef) {
        try {
            var alienRaceObject = GetFieldValue(raceDef, raceDef, "alienRace");
            if (alienRaceObject == null) {
                return null;
            }

            var generalSettingsObject = GetFieldValue(raceDef, alienRaceObject, "generalSettings");
            if (generalSettingsObject == null) {
                return null;
            }

            var alienPartGeneratorObject = GetFieldValue(raceDef, generalSettingsObject, "alienPartGenerator");
            if (alienPartGeneratorObject == null) {
                return null;
            }

            var graphicPathsCollection = GetFieldValueAsCollection(raceDef, alienRaceObject, "graphicPaths");
            if (graphicPathsCollection == null) {
                return null;
            }

            /*
            Logger.Debug("GraphicsPaths for " + raceDef.defName + ":");
            if (graphicPathsCollection.Count > 0) {
                foreach (object o in graphicPathsCollection) {
                    Logger.Debug("  GraphicsPath");
                    Logger.Debug("    .body = " + GetFieldValueAsString(raceDef, o, "body"));
                    Logger.Debug("    .head = " + GetFieldValueAsString(raceDef, o, "head"));
                    System.Collections.ICollection lifeStagesCollections = GetFieldValueAsCollection(raceDef, o, "lifeStageDefs");
                }
            }
            */

            // We have enough to start putting together the result object, so we instantiate it now.
            var result = new AlienRace();
            result.ThingDef = raceDef;

            //Logger.Debug("InitializeAlienRace: " + raceDef.defName);

            var minAgeForAdulthood = ReflectionUtil.GetFieldValue<float>(generalSettingsObject, "minAgeForAdulthood");
            if (minAgeForAdulthood <= 0) {
                minAgeForAdulthood = DefaultMinAgeForAdulthood;
            }

            result.MinAgeForAdulthood = minAgeForAdulthood;

            // Get the list of body types.
            var alienBodyTypesCollection =
                GetFieldValueAsCollection(raceDef, alienPartGeneratorObject, "alienbodytypes");
            if (alienBodyTypesCollection == null) {
                return null;
            }

            var bodyTypes = new List<BodyTypeDef>();
            if (alienBodyTypesCollection.Count > 0) {
                foreach (var o in alienBodyTypesCollection) {
                    if (o.GetType() == typeof(BodyTypeDef)) {
                        var def = o as BodyTypeDef;
                        bodyTypes.Add((BodyTypeDef)o);
                    }
                }
            }

            //Logger.Debug("  none");
            //Logger.Debug($"Body types for alien race {raceDef.defName}: {string.Join(", ", bodyTypes.Select(b => b.defName + ", " + b.LabelCap))}");
            result.BodyTypes = bodyTypes;

            // Determine if the alien races uses gender-specific heads.
            var useGenderedHeads = GetFieldValueAsBool(raceDef, alienPartGeneratorObject, "useGenderedHeads");
            if (useGenderedHeads == null) {
                return null;
            }

            result.GenderSpecificHeads = useGenderedHeads.Value;

            // Get the list of crown types.
            var alienCrownTypesCollection =
                GetFieldValueAsCollection(raceDef, alienPartGeneratorObject, "aliencrowntypes");
            if (alienCrownTypesCollection == null) {
                return null;
            }

            var crownTypes = new List<string>();
            //Logger.Debug("Crown Types for " + raceDef.defName + ":");
            if (alienCrownTypesCollection.Count > 0) {
                foreach (var o in alienCrownTypesCollection) {
                    var crownTypeString = o as string;
                    if (crownTypeString != null) {
                        crownTypes.Add(crownTypeString);
                        //Logger.Debug("  " + crownTypeString);
                    }
                }
            }

            result.CrownTypes = crownTypes;

            // Go through the graphics paths and find the heads path.
            // TODO: What is this?  
            string graphicsPathForHeads = null;
            string graphicsPathForBodyTypes = null;
            foreach (var graphicsPath in graphicPathsCollection) {
                var lifeStageCollection = GetFieldValueAsCollection(raceDef, graphicsPath, "lifeStageDefs");
                if (lifeStageCollection == null || lifeStageCollection.Count == 0) {
                    var headsPath = GetFieldValueAsString(raceDef, graphicsPath, "head");
                    var bodyTypesPath = GetFieldValueAsString(raceDef, graphicsPath, "body");
                    if (headsPath != null) {
                        graphicsPathForHeads = headsPath;
                    }

                    if (bodyTypesPath != null) {
                        graphicsPathForBodyTypes = bodyTypesPath;
                    }
                }
            }

            result.GraphicsPathForHeads = graphicsPathForHeads;
            result.GraphicsPathForBodyTypes = graphicsPathForBodyTypes;

            // Figure out colors.
            var primaryGenerator = FindPrimarySkinColorGenerator(raceDef, alienPartGeneratorObject);
            var secondaryGenerator = FindSecondarySkinColorGenerator(raceDef, alienPartGeneratorObject);
            result.UseMelaninLevels = true;
            result.ChangeableColor = true;
            result.HasSecondaryColor = false;

            if (primaryGenerator != null) {
                if (primaryGenerator.GetType().Name != "ColorGenerator_SkinColorMelanin") {
                    if (primaryGenerator != null) {
                        result.UseMelaninLevels = false;
                        result.PrimaryColors = primaryGenerator.GetColorList();
                    }
                    else {
                        result.PrimaryColors = new List<Color>();
                    }

                    if (secondaryGenerator != null) {
                        result.HasSecondaryColor = true;
                        result.SecondaryColors = secondaryGenerator.GetColorList();
                    }
                    else {
                        result.SecondaryColors = new List<Color>();
                    }
                }
            }

            // Style settings
            var styleSettingsValue = GetFieldValue(raceDef, alienRaceObject, "styleSettings", true);

            result.HasHair = true;
            result.HasBeards = true;
            result.HasTattoos = true;
            if (styleSettingsValue is IDictionary styleSettings) {
                // Hair properties.
                if (styleSettings.Contains(typeof(HairDef))) {
                    var hairSettings = styleSettings[typeof(HairDef)];
                    var hasStyle = GetFieldValueAsBool(raceDef, hairSettings, "hasStyle");
                    if (hasStyle.HasValue && !hasStyle.Value) {
                        result.HasHair = false;
                    }

                    var hairTagCollection = GetFieldValueAsCollection(raceDef, hairSettings, "styleTagsOverride");
                    if (hairTagCollection != null) {
                        var hairTags = new HashSet<string>();
                        foreach (var o in hairTagCollection) {
                            var tag = o as string;
                            if (tag != null) {
                                hairTags.Add(tag);
                            }
                        }

                        if (hairTags.Count > 0) {
                            result.HairTags = hairTags;
                        }
                    }
                }

                // Beard properties.
                if (styleSettings.Contains(typeof(BeardDef))) {
                    var settings = styleSettings[typeof(BeardDef)];
                    var hasBeards = GetFieldValueAsBool(raceDef, settings, "hasStyle");
                    if (hasBeards.HasValue && !hasBeards.Value) {
                        result.HasBeards = false;
                    }
                }

                // Tattoo properties.
                if (styleSettings.Contains(typeof(TattooDef))) {
                    var settings = styleSettings[typeof(TattooDef)];
                    var hasTattoos = GetFieldValueAsBool(raceDef, settings, "hasStyle");
                    if (hasTattoos.HasValue && !hasTattoos.Value) {
                        result.HasTattoos = false;
                    }
                }
            }

            var hairColorGenerator = FindPrimaryColorGenerator(raceDef, alienPartGeneratorObject, "hair");
            if (hairColorGenerator != null) {
                result.HairColors = hairColorGenerator.GetColorList();
            }
            else {
                result.HairColors = null;
            }

            // Apparel properties.
            var restrictionSettingsValue = GetFieldValue(raceDef, alienRaceObject, "raceRestriction", true);
            result.RaceSpecificApparelOnly = false;
            result.RaceSpecificApparel = new HashSet<string>();
            result.AllowedApparel = new HashSet<string>();
            result.DisallowedApparel = new HashSet<string>();
            if (restrictionSettingsValue != null) {
                var restrictedApparelOnly =
                    GetFieldValueAsBool(raceDef, restrictionSettingsValue, "onlyUseRaceRestrictedApparel");
                if (restrictedApparelOnly != null) {
                    result.RaceSpecificApparelOnly = restrictedApparelOnly.Value;
                }

                var restrictedApparelCollection =
                    GetFieldValueAsCollection(raceDef, restrictionSettingsValue, "apparelList");
                if (restrictedApparelCollection != null) {
                    foreach (var o in restrictedApparelCollection) {
                        if (o is ThingDef def) {
                            result.RaceSpecificApparel.Add(def.defName);
                        }
                    }
                }

                var allowedApparelCollection =
                    GetFieldValueAsCollection(raceDef, restrictionSettingsValue, "whiteApparelList");
                if (allowedApparelCollection != null) {
                    foreach (var o in allowedApparelCollection) {
                        if (o is ThingDef def) {
                            result.AllowedApparel.Add(def.defName);
                        }
                    }
                }

                var disallowedApparelCollection =
                    GetFieldValueAsCollection(raceDef, restrictionSettingsValue, "blackApparelList");
                if (disallowedApparelCollection != null) {
                    foreach (var o in disallowedApparelCollection) {
                        if (o is ThingDef def) {
                            result.DisallowedApparel.Add(def.defName);
                        }
                    }
                }
            }

            var bodyAddonsCollection = GetFieldValueAsCollection(raceDef, alienPartGeneratorObject, "bodyAddons");
            if (bodyAddonsCollection != null) {
                var addons = new List<AlienRaceBodyAddon>();
                var index = -1;
                foreach (var o in bodyAddonsCollection) {
                    index++;
                    var addon = new AlienRaceBodyAddon();
                    var path = GetFieldValueAsString(raceDef, o, "path");
                    if (path == null) {
                        Logger.Warning("Failed to get path for body add-on for alien race: " + raceDef.defName);
                        continue;
                    }

                    addon.Path = path;
                    var variantCount = GetFieldValueAsInt(raceDef, o, "variantCount");
                    if (variantCount == null) {
                        Logger.Warning("Failed to get variant count for body add-on for alien race: " +
                                       raceDef.defName);
                        continue;
                    }

                    addon.OptionCount = variantCount.Value;
                    var name = ParseAddonName(path);
                    if (name == null) {
                        Logger.Warning("Failed to parse a name from its path for body add-on for alien race: " +
                                       raceDef.defName);
                        continue;
                    }

                    addon.Name = name;
                    addon.VariantIndex = index;
                    addons.Add(addon);
                }

                result.addons = addons;
            }

            return result;
        }
        catch (Exception e) {
            throw new InitializationException("Failed to initialize an alien race: " + raceDef.defName, e);
        }
    }

    protected string ParseAddonName(string path) {
        var trimmedPath = path.TrimEnd('/').TrimStart('/');
        var items = trimmedPath.Split('/');
        if (items.Length > 0) {
            return items[items.Length - 1].Replace("_", " ");
        }

        return null;
    }

    protected object GetFieldValue(ThingDef raceDef, object source, string name, bool allowNull = false) {
        try {
            var field = source.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) {
                Logger.Warning("Could not find " + name + " field for " + raceDef.defName);
                return null;
            }

            var result = field.GetValue(source);
            if (result == null) {
                if (!allowNull) {
                    Logger.Warning("Could not find " + name + " field value for " + raceDef.defName);
                }

                return null;
            }

            return result;
        }
        catch (Exception) {
            Logger.Warning("Could resolve value of the " + name + " field for " + raceDef.defName);
            return null;
        }
    }

    protected ICollection GetFieldValueAsCollection(ThingDef raceDef, object source, string name) {
        var result = GetFieldValue(raceDef, source, name, true);
        if (result == null) {
            return null;
        }

        var collection = result as ICollection;
        if (collection == null) {
            Logger.Warning("Could not convert " + name + " field value into a collection for " + raceDef.defName + ".");
            return null;
        }

        return collection;
    }

    protected bool? GetFieldValueAsBool(ThingDef raceDef, object source, string name) {
        var result = GetFieldValue(raceDef, source, name, true);
        if (result == null) {
            return null;
        }

        if (result.GetType() == typeof(bool)) {
            return (bool)result;
        }

        Logger.Warning("Could not convert " + name + " field value into a bool for " + raceDef.defName + ".");
        return null;
    }

    protected string GetFieldValueAsString(ThingDef raceDef, object source, string name) {
        var value = GetFieldValue(raceDef, source, name, true);
        if (value == null) {
            return null;
        }

        var result = value as string;
        if (result != null) {
            return result;
        }

        Logger.Warning("Could not convert " + name + " field value into a string for " + raceDef.defName + ".");
        return null;
    }

    protected int? GetFieldValueAsInt(ThingDef raceDef, object source, string name) {
        var value = GetFieldValue(raceDef, source, name, true);
        if (value == null) {
            return null;
        }

        try {
            return (int)value;
        }
        catch (Exception) {
            Logger.Warning("Could not convert " + name + " field value into an int for " + raceDef.defName + ".");
            return null;
        }
    }
}
