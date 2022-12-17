using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderBodyTypes {
    protected HashSet<string> defaultBodyTypesPaths = new() { "Things/Pawn/Humanlike/Bodies/" };

    protected List<BodyTypeDef> defaultNonModdedBodyTypes = new() {
        BodyTypeDefOf.Male,
        BodyTypeDefOf.Female,
        BodyTypeDefOf.Thin,
        BodyTypeDefOf.Fat,
        BodyTypeDefOf.Hulk
    };

    protected Dictionary<string, string> labels = new();
    protected Dictionary<ThingDef, OptionsBodyType> raceBodyTypeLookup = new();

    public ProviderBodyTypes() {
        labels.Add("Female", "EdB.PC.Pawn.BodyType.Average".Translate());
        labels.Add("Male", "EdB.PC.Pawn.BodyType.Average".Translate());
        labels.Add("Hulk", "EdB.PC.Pawn.BodyType.Hulking".Translate());
        labels.Add("Thin", "EdB.PC.Pawn.BodyType.Thin".Translate());
        labels.Add("Fat", "EdB.PC.Pawn.BodyType.Heavyset".Translate());
        labels.Add("Oval", "EdB.PC.Pawn.BodyType.Oval".Translate());
    }

    public List<BodyTypeDef> GetBodyTypesForPawn(CustomPawn pawn) {
        return GetBodyTypesForPawn(pawn.Pawn);
    }

    public List<BodyTypeDef> GetBodyTypesForPawn(Pawn pawn) {
        return GetBodyTypesForPawn(pawn.def, pawn.gender);
    }

    public List<BodyTypeDef> GetBodyTypesForPawn(ThingDef race, Gender gender) {
        OptionsBodyType bodyTypes;
        if (!raceBodyTypeLookup.TryGetValue(race, out bodyTypes)) {
            bodyTypes = InitializeBodyTypes(race);
            raceBodyTypeLookup.Add(race, bodyTypes);
        }

        return bodyTypes.GetBodyTypes(gender);
    }

    public string GetBodyTypeLabel(BodyTypeDef bodyType) {
        if (bodyType.label.NullOrEmpty()) {
            string label = null;
            if (labels.TryGetValue(bodyType.defName, out label)) {
                return label;
            }

            return "EdB.PC.Pawn.BodyType.Unnamed".Translate();
        }

        return bodyType.LabelCap;
    }

    protected OptionsBodyType InitializeBodyTypes(ThingDef def) {
        return InitializeHumanlikeBodyTypes();
    }

    protected OptionsBodyType InitializeHumanlikeBodyTypes() {
        var result = new OptionsBodyType();
        foreach (var d in DefDatabase<BodyTypeDef>.AllDefs) {
            if (d != BodyTypeDefOf.Female) {
                result.MaleBodyTypes.Add(d);
            }

            if (d != BodyTypeDefOf.Male) {
                result.FemaleBodyTypes.Add(d);
            }

            result.NoGenderBodyTypes.Add(d);
        }

        return result;
    }

    protected OptionsBodyType InitializeNonModdedDefaultHumanlikeBodyTypes() {
        var result = new OptionsBodyType();
        foreach (var d in defaultNonModdedBodyTypes) {
            if (d != BodyTypeDefOf.Female) {
                result.MaleBodyTypes.Add(d);
            }

            if (d != BodyTypeDefOf.Male) {
                result.FemaleBodyTypes.Add(d);
            }

            result.NoGenderBodyTypes.Add(d);
        }

        return result;
    }

    // TODO: Evaluate this.  Disabled for now, but this method looks in the alien body graphics path for the specified
    // body def.  This was to try to guard against the addition of modded BodyTypeDefs that work for vanilla, but that
    // the alien race mod maker has not provided a texture for.
    /// Instead of calling this, we're just assigning only vanilla body type defs for alien races without explicit body types
    /// and with a custom graphic path for body textures.
    //protected bool ValidateBodyTypeForAlienRace(AlienRace race, BodyTypeDef def) {
    //    string path = race.GraphicsPathForBodyTypes + "/" + def.bodyNakedGraphicPath.Split('/').Last();
    //    path = path.Replace("//", "/");
    //    path += "_south";
    //    Logger.Debug("ValidateBodyTypeForRace(" + race + ", " + def.defName + "), path = " + path);
    //    try {
    //        // TODO: Figure out which mod we're dealing with and only go through that content pack.
    //        List<ModContentPack> modsListForReading = LoadedModManager.RunningModsListForReading;
    //        for (int index = modsListForReading.Count - 1; index >= 0; --index) {
    //            ModContentPack pack = modsListForReading[index];
    //            Logger.Debug("Looking for path in " + pack.Identifier);
    //            var contentHolder = pack.GetContentHolder<Texture2D>();
    //            if (contentHolder.contentList.ContainsKey(path)) {
    //                Logger.Warning("Found it");
    //                return true;
    //            }
    //        }
    //        Logger.Debug("Didn't find it");
    //        return false;
    //    } 
    //    catch (Exception) {
    //        return false;
    //    }
    //}
}
