using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLoaderV5 {
    private readonly Dictionary<string, List<string>> skillDefReplacementLookup = new();
    public Dictionary<string, ReplacementBodyPart> bodyPartReplacements = new();
    public Dictionary<string, string> recipeReplacements = new();

    // Maintain lists of definitions that were replaced in newer versions of the game.
    public Dictionary<string, string> thingDefReplacements = new();
    public Dictionary<string, string> traitReplacements = new();

    public PawnLoaderV5() {
        thingDefReplacements.Add("Gun_SurvivalRifle", "Gun_BoltActionRifle");
        thingDefReplacements.Add("Gun_Pistol", "Gun_Revolver");
        thingDefReplacements.Add("Medicine", "MedicineIndustrial");
        thingDefReplacements.Add("Component", "ComponentIndustrial");
        thingDefReplacements.Add("WolfTimber", "Wolf_Timber");

        traitReplacements.Add("Prosthophobe", "BodyPurist");
        traitReplacements.Add("Prosthophile", "Transhumanist");
        traitReplacements.Add("SuperImmune", "Immunity");

        InitializeRecipeReplacements();
        InitializeBodyPartReplacements();
        InitializeSkillDefReplacements();
    }

    public Dictionary<string, Ideo> IdeoMap { get; set; } = new();

    protected void InitializeRecipeReplacements() {
        recipeReplacements.Add("InstallSyntheticHeart", "InstallSimpleProstheticHeart");
        recipeReplacements.Add("InstallAdvancedBionicArm", "InstallArchtechBionicArm");
        recipeReplacements.Add("InstallAdvancedBionicLeg", "InstallArchtechBionicLeg");
        recipeReplacements.Add("InstallAdvancedBionicEye", "InstallArchtechBionicEye");
    }

    protected void InitializeBodyPartReplacements() {
        AddBodyPartReplacement("LeftFoot", "Foot", 0);
        AddBodyPartReplacement("LeftLeg", "Leg", 0);
        AddBodyPartReplacement("LeftEye", "Eye", 0);
        AddBodyPartReplacement("LeftEar", "Ear", 0);
        AddBodyPartReplacement("LeftLung", "Lung", 0);
        AddBodyPartReplacement("LeftArm", "Arm", 0);
        AddBodyPartReplacement("LeftShoulder", "Shoulder", 0);
        AddBodyPartReplacement("LeftKidney", "Kidney", 0);
        AddBodyPartReplacement("RightFoot", "Foot", 1);
        AddBodyPartReplacement("RightLeg", "Leg", 1);
        AddBodyPartReplacement("RightEye", "Eye", 1);
        AddBodyPartReplacement("RightEar", "Ear", 1);
        AddBodyPartReplacement("RightLung", "Lung", 1);
        AddBodyPartReplacement("RightArm", "Arm", 1);
        AddBodyPartReplacement("RightShoulder", "Shoulder", 1);
        AddBodyPartReplacement("RightKidney", "Kidney", 1);
    }

    protected void InitializeSkillDefReplacements() {
        AddSkillDefReplacement("Growing", "Plants");
        AddSkillDefReplacement("Research", "Intellectual");
    }

    public void AddBodyPartReplacement(string name, string newPart, int index) {
        var def = DefDatabase<BodyPartDef>.GetNamedSilentFail(newPart);
        if (def == null) {
            Logger.Warning("Could not find body part definition \"" + newPart + "\" to replace body part \"" + name +
                           "\"");
            return;
        }

        bodyPartReplacements.Add(name, new ReplacementBodyPart(def, index));
    }

    public CustomPawn Load(PrepareCarefully loadout, string name) {
        var pawnRecord = new SaveRecordPawnV5();
        var modString = "";
        var version = "";
        try {
            Scribe.loader.InitLoading(ColonistFiles.FilePathForSavedColonist(name));
            Scribe_Values.Look<string>(ref version, "version", "unknown");
            Scribe_Values.Look<string>(ref modString, "mods", "");

            try {
                Scribe_Deep.Look(ref pawnRecord, "pawn", null);
            }
            catch (Exception e) {
                Messages.Message(modString, MessageTypeDefOf.SilentInput);
                Messages.Message("EdB.PC.Dialog.PawnPreset.Error.Failed".Translate(), MessageTypeDefOf.RejectInput);
                Logger.Warning(e.ToString());
                Logger.Warning("Colonist was created with the following mods: " + modString);
                return null;
            }
        }
        catch (Exception e) {
            Logger.Error("Failed to load preset file");
            throw e;
        }
        finally {
            PresetLoader.ClearSaveablesAndCrossRefs();
        }

        if (pawnRecord == null) {
            Messages.Message(modString, MessageTypeDefOf.SilentInput);
            Messages.Message("EdB.PC.Dialog.PawnPreset.Error.Failed".Translate(), MessageTypeDefOf.RejectInput);
            Logger.Warning("Colonist was created with the following mods: " + modString);
            return null;
        }

        var pawn = ConvertSaveRecordToPawn(pawnRecord);

        return pawn;
    }

    public CustomPawn ConvertSaveRecordToPawn(SaveRecordPawnV5 record) {
        var partialFailure = false;

        PawnKindDef pawnKindDef = null;
        if (record.pawnKindDef != null) {
            pawnKindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(record.pawnKindDef);
            if (pawnKindDef == null) {
                Logger.Warning("Pawn kind definition for the saved character (" + record.pawnKindDef +
                               ") not found.  Picking the basic colony pawn kind definition.");
                pawnKindDef = FactionDefOf.PlayerColony.basicMemberKind;
                if (pawnKindDef == null) {
                    return null;
                }
            }
        }

        var pawnThingDef = ThingDefOf.Human;
        if (record.thingDef != null) {
            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(record.thingDef);
            if (thingDef != null) {
                pawnThingDef = thingDef;
            }
            else {
                Logger.Warning("Pawn's thing definition {" + record.thingDef + "} was not found.");
            }
        }
        else {
            Logger.Warning("Pawn's thing definition was null.");
        }

        // Create the pawn generation request.
        var generationRequest = new PawnGenerationRequestWrapper {
            FixedBiologicalAge = record.biologicalAge,
            FixedChronologicalAge = record.chronologicalAge,
            FixedGender = record.gender,
            Context = PawnGenerationContext.NonPlayer,
            WorldPawnFactionDoesntMatter = true
        };
        var playerFaction = Find.FactionManager.OfPlayer;
        var ideology = playerFaction?.ideos?.PrimaryIdeo;

        if (record.ideo != null) {
            if (!record.ideo.sameAsColony && IdeoMap != null) {
                if (IdeoMap.TryGetValue(record.ideo.name, out var ideo)) {
                    ideology = ideo;
                }
            }
        }

        if (ideology != null) {
            generationRequest.FixedIdeology = ideology;
        }

        // Add a pawn kind definition to the generation request, if possible.
        if (pawnKindDef != null) {
            generationRequest.KindDef = pawnKindDef;
        }

        // Create the pawn.
        Pawn source = null;
        try {
            source = PawnGenerator.GeneratePawn(generationRequest.Request);
        }
        catch (Exception e) {
            Logger.Warning(
                "Failed to generate a pawn from preset for pawn {" + record.nickName +
                "}. Will try to create it using fallback settings", e);
            generationRequest = new PawnGenerationRequestWrapper {
                FixedBiologicalAge = record.biologicalAge,
                FixedChronologicalAge = record.chronologicalAge,
                FixedGender = record.gender
            };
            try {
                source = PawnGenerator.GeneratePawn(generationRequest.Request);
            }
            catch (Exception) {
                Logger.Warning(
                    "Failed to generate a pawn using fallback settings from preset for pawn {" + record.nickName + "}",
                    e);
                return null;
            }
        }

        if (source.health != null) {
            source.health.Reset();
        }

        var pawn = new CustomPawn(source);
        if (record.id == null) {
            pawn.GenerateId();
        }
        else {
            pawn.Id = record.id;
        }

        if (record.type != null) {
            try {
                pawn.Type = (CustomPawnType)Enum.Parse(typeof(CustomPawnType), record.type);
            }
            catch (Exception) {
                pawn.Type = CustomPawnType.Colonist;
            }
        }
        else {
            pawn.Type = CustomPawnType.Colonist;
        }

        pawn.Gender = record.gender;
        if (record.age > 0) {
            pawn.ChronologicalAge = record.age;
            pawn.BiologicalAge = record.age;
        }

        if (record.chronologicalAge > 0) {
            pawn.ChronologicalAge = record.chronologicalAge;
        }

        if (record.biologicalAge > 0) {
            pawn.BiologicalAge = record.biologicalAge;
        }

        pawn.FirstName = record.firstName;
        pawn.NickName = record.nickName;
        pawn.LastName = record.lastName;

        if (ModsConfig.IdeologyActive && record.favoriteColor.HasValue) {
            pawn.Pawn.story.favoriteColor = record.favoriteColor;
        }

        if (record.originalFactionDef != null) {
            pawn.OriginalFactionDef = DefDatabase<FactionDef>.GetNamedSilentFail(record.originalFactionDef);
        }

        pawn.OriginalKindDef = pawnKindDef;

        if (pawn.Type == CustomPawnType.Colonist) {
            playerFaction = Faction.OfPlayerSilentFail;
            if (playerFaction != null) {
                pawn.Pawn.SetFactionDirect(playerFaction);
            }
        }
        else if (pawn.Type == CustomPawnType.World) {
            if (record.faction != null) {
                if (record.faction.def != null) {
                    var factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(record.faction.def);
                    if (factionDef != null) {
                        var randomFaction = false;
                        if (record.faction.index != null) {
                            CustomFaction customFaction = null;
                            if (!record.faction.leader) {
                                customFaction =
                                    PrepareCarefully.Instance.Providers.Factions.FindCustomFactionByIndex(factionDef,
                                        record.faction.index.Value);
                            }
                            else {
                                customFaction =
                                    PrepareCarefully.Instance.Providers.Factions
                                        .FindCustomFactionWithLeaderOptionByIndex(factionDef,
                                            record.faction.index.Value);
                            }

                            if (customFaction != null) {
                                pawn.Faction = customFaction;
                            }
                            else {
                                Logger.Warning(
                                    "Could not place at least one preset character into a saved faction because there were not enough available factions of that type in the world");
                                randomFaction = true;
                            }
                        }
                        else {
                            randomFaction = true;
                        }

                        if (randomFaction) {
                            var customFaction =
                                PrepareCarefully.Instance.Providers.Factions.FindRandomCustomFactionByDef(factionDef);
                            if (customFaction != null) {
                                pawn.Faction = customFaction;
                            }
                        }
                    }
                    else {
                        Logger.Warning(
                            "Could not place at least one preset character into a saved faction because that faction is not available in the world");
                    }
                }
            }
        }

        var h = DefDatabase<HairDef>.GetNamedSilentFail(record.hairDef);
        if (h != null) {
            pawn.HairDef = h;
        }
        else {
            Logger.Warning("Could not load hair definition \"" + record.hairDef + "\"");
            partialFailure = true;
        }

        pawn.HeadType = ToHeadTypeFromHeadGraphicPath(record.headGraphicPath);
        pawn.HairColor = record.hairColor;

        if (record.melanin >= 0.0f) {
            pawn.MelaninLevel = record.melanin;
        }
        else {
            pawn.MelaninLevel = PawnColorUtils.FindMelaninValueFromColor(record.skinColor);
        }

        var backstory = FindBackstory(record.childhood);
        if (backstory != null) {
            pawn.Childhood = backstory;
        }
        else {
            Logger.Warning("Could not load childhood backstory definition \"" + record.childhood + "\"");
            partialFailure = true;
        }

        if (record.adulthood != null) {
            backstory = FindBackstory(record.adulthood);
            if (backstory != null) {
                pawn.Adulthood = backstory;
            }
            else {
                Logger.Warning("Could not load adulthood backstory definition \"" + record.adulthood + "\"");
                partialFailure = true;
            }
        }

        BodyTypeDef bodyType = null;
        try {
            bodyType = DefDatabase<BodyTypeDef>.GetNamedSilentFail(record.bodyType);
        }
        catch (Exception) {
        }

        if (bodyType == null) {
            if (pawn.Adulthood != null) {
                bodyType = pawn.Adulthood.BodyTypeFor(pawn.Gender);
            }
            else {
                bodyType = pawn.Childhood.BodyTypeFor(pawn.Gender);
            }
        }

        if (bodyType != null) {
            pawn.BodyType = bodyType;
        }

        BeardDef beardDef = null;
        if (record.beard != null) {
            beardDef = DefDatabase<BeardDef>.GetNamedSilentFail(record.beard);
        }

        if (beardDef == null) {
            beardDef = BeardDefOf.NoBeard;
        }

        pawn.Beard = beardDef;

        TattooDef faceTattooDef = null;
        if (record.bodyTattoo != null) {
            faceTattooDef = DefDatabase<TattooDef>.GetNamedSilentFail(record.faceTattoo);
        }

        if (faceTattooDef == null) {
            faceTattooDef = TattooDefOf.NoTattoo_Face;
        }

        pawn.FaceTattoo = faceTattooDef;

        TattooDef bodyTattooDef = null;
        if (record.bodyTattoo != null) {
            bodyTattooDef = DefDatabase<TattooDef>.GetNamedSilentFail(record.bodyTattoo);
        }

        if (bodyTattooDef == null) {
            bodyTattooDef = TattooDefOf.NoTattoo_Body;
        }

        pawn.BodyTattoo = bodyTattooDef;

        // Load pawn comps
        //Logger.Debug("pre-copy comps xml: " + record.compsXml);
        var compsXml = "<saveable Class=\"" + typeof(PawnCompsLoader).FullName + "\">" + record.compsXml +
                       "</saveable>";
        var rules = new PawnCompInclusionRules();
        rules.IncludeComps(record.savedComps);
        UtilityCopy.DeserializeExposable<PawnCompsLoader>(compsXml, new object[] { pawn.Pawn, rules });
        var compLookup = new Dictionary<string, ThingComp>();
        foreach (var c in pawn.Pawn.AllComps) {
            if (!compLookup.ContainsKey(c.GetType().FullName)) {
                //Logger.Debug("Added comp to comp lookup with key: " + c.GetType().FullName);
                compLookup.Add(c.GetType().FullName, c);
            }
        }

        var savedComps = record.savedComps != null ? new HashSet<string>(record.savedComps) : new HashSet<string>();
        DefaultPawnCompRules.PostLoadModifiers.Apply(pawn, compLookup, savedComps);

        pawn.ClearTraits();
        if (record.traits != null) {
            for (var i = 0; i < record.traits.Count; i++) {
                var traitName = record.traits[i].def;
                var trait = FindTrait(traitName, record.traits[i].degree);
                if (trait != null) {
                    pawn.AddTrait(trait);
                }
                else {
                    Logger.Warning("Could not load trait definition \"" + traitName + "\"");
                    partialFailure = true;
                }
            }
        }
        else if (record.traitNames != null && record.traitDegrees != null &&
                 record.traitNames.Count == record.traitDegrees.Count) {
            for (var i = 0; i < record.traitNames.Count; i++) {
                var traitName = record.traitNames[i];
                var trait = FindTrait(traitName, record.traitDegrees[i]);
                if (trait != null) {
                    pawn.AddTrait(trait);
                }
                else {
                    Logger.Warning("Could not load trait definition \"" + traitName + "\"");
                    partialFailure = true;
                }
            }
        }

        foreach (var skill in record.skills) {
            var def = FindSkillDef(pawn.Pawn, skill.name);
            if (def == null) {
                Logger.Warning("Could not load skill definition \"" + skill.name + "\" from saved preset");
                partialFailure = true;
                continue;
            }

            pawn.currentPassions[def] = skill.passion;
            pawn.originalPassions[def] = skill.passion;
            pawn.SetOriginalSkillLevel(def, skill.value);
            pawn.SetUnmodifiedSkillLevel(def, skill.value);
        }

        pawn.ClearApparel();
        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(pawn)) {
            if (layer.Apparel) {
                pawn.SetSelectedApparel(layer, null);
                pawn.SetSelectedStuff(layer, null);
            }
        }

        var apparelLayers = PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(pawn)
            .FindAll(layer => { return layer.Apparel; });
        foreach (var apparelRecord in record.apparel) {
            // Find the pawn layer for the saved apparel record.
            var layer = apparelLayers.FirstOrDefault(apparelLayer => {
                return apparelLayer.Name == apparelRecord.layer;
            });
            if (layer == null) {
                Logger.Warning("Could not find a matching pawn layer for the saved apparel \"" + apparelRecord.layer +
                               "\"");
                partialFailure = true;
                continue;
            }

            if (apparelRecord.apparel.NullOrEmpty()) {
                Logger.Warning("Saved apparel entry for layer \"" + apparelRecord.layer +
                               "\" had an empty apparel def");
                partialFailure = true;
                continue;
            }

            // Set the defaults.
            pawn.SetSelectedApparel(layer, null);
            pawn.SetSelectedStuff(layer, null);
            pawn.SetColor(layer, Color.white);

            var def = DefDatabase<ThingDef>.GetNamedSilentFail(apparelRecord.apparel);
            if (def == null) {
                Logger.Warning("Could not load thing definition for apparel \"" + apparelRecord.apparel + "\"");
                partialFailure = true;
                continue;
            }

            ThingDef stuffDef = null;
            if (!string.IsNullOrEmpty(apparelRecord.stuff)) {
                stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(apparelRecord.stuff);
                if (stuffDef == null) {
                    Logger.Warning("Could not load stuff definition \"" + apparelRecord.stuff + "\" for apparel \"" +
                                   apparelRecord.apparel + "\"");
                    partialFailure = true;
                    continue;
                }
            }

            pawn.SetSelectedApparel(layer, def);
            pawn.SetSelectedStuff(layer, stuffDef);
            pawn.SetColor(layer, apparelRecord.color);
        }

        var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(pawn);
        for (var i = 0; i < record.implants.Count; i++) {
            var implantRecord = record.implants[i];
            var uniqueBodyPart = healthOptions.FindBodyPartByName(implantRecord.bodyPart,
                implantRecord.bodyPartIndex != null ? implantRecord.bodyPartIndex.Value : 0);
            if (uniqueBodyPart == null) {
                uniqueBodyPart = FindReplacementBodyPart(healthOptions, implantRecord.bodyPart);
            }

            if (uniqueBodyPart == null) {
                Logger.Warning("Could not add the implant because it could not find the needed body part \"" +
                               implantRecord.bodyPart + "\""
                               + (implantRecord.bodyPartIndex != null
                                   ? " with index " + implantRecord.bodyPartIndex
                                   : ""));
                partialFailure = true;
                continue;
            }

            var bodyPart = uniqueBodyPart.Record;
            if (implantRecord.recipe != null) {
                var recipeDef = FindRecipeDef(implantRecord.recipe);
                if (recipeDef == null) {
                    Logger.Warning("Could not add the implant because it could not find the recipe definition \"" +
                                   implantRecord.recipe + "\"");
                    partialFailure = true;
                    continue;
                }

                var found = false;
                foreach (var p in recipeDef.appliedOnFixedBodyParts) {
                    if (p.defName.Equals(bodyPart.def.defName)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    Logger.Warning("Could not apply the saved implant recipe \"" + implantRecord.recipe +
                                   "\" to the body part \"" + bodyPart.def.defName +
                                   "\".  Recipe does not support that part.");
                    partialFailure = true;
                    continue;
                }

                var implant = new Implant();
                implant.BodyPartRecord = bodyPart;
                implant.recipe = recipeDef;
                implant.label = implant.Label;
                pawn.AddImplant(implant);
            }
        }

        foreach (var injuryRecord in record.injuries) {
            var def = DefDatabase<HediffDef>.GetNamedSilentFail(injuryRecord.hediffDef);
            if (def == null) {
                Logger.Warning("Could not add the injury because it could not find the hediff definition \"" +
                               injuryRecord.hediffDef + "\"");
                partialFailure = true;
                continue;
            }

            var option = healthOptions.FindInjuryOptionByHediffDef(def);
            if (option == null) {
                Logger.Warning(
                    "Could not add the injury because it could not find a matching injury option for the saved hediff \"" +
                    injuryRecord.hediffDef + "\"");
                partialFailure = true;
                continue;
            }

            BodyPartRecord bodyPart = null;
            if (injuryRecord.bodyPart != null) {
                var uniquePart = healthOptions.FindBodyPartByName(injuryRecord.bodyPart,
                    injuryRecord.bodyPartIndex != null ? injuryRecord.bodyPartIndex.Value : 0);
                if (uniquePart == null) {
                    uniquePart = FindReplacementBodyPart(healthOptions, injuryRecord.bodyPart);
                }

                if (uniquePart == null) {
                    Logger.Warning("Could not add the injury because it could not find the needed body part \"" +
                                   injuryRecord.bodyPart + "\""
                                   + (injuryRecord.bodyPartIndex != null
                                       ? " with index " + injuryRecord.bodyPartIndex
                                       : ""));
                    partialFailure = true;
                    continue;
                }

                bodyPart = uniquePart.Record;
            }

            var injury = new Injury();
            injury.Option = option;
            injury.BodyPartRecord = bodyPart;
            if (injuryRecord.severity != null) {
                injury.Severity = injuryRecord.Severity;
            }

            if (injuryRecord.painFactor != null) {
                injury.PainFactor = injuryRecord.PainFactor;
            }

            pawn.AddInjury(injury);
        }

        // Ideoligion Certainty
        try {
            if (record.ideo != null && ModsConfig.IdeologyActive && pawn.Pawn?.ideo != null) {
                pawn.Certainty = record.ideo.certainty;
            }
        }
        catch (Exception e) {
            Logger.Error("Failed to load ideoligion certainty", e);
        }

        // Abilities
        try {
            if (record.abilities != null && pawn.Pawn?.abilities != null) {
                foreach (var a in record.abilities) {
                    var def = DefDatabase<AbilityDef>.GetNamedSilentFail(a);
                    if (def != null) {
                        pawn.Pawn.abilities.GainAbility(def);
                    }
                }
            }
        }
        catch (Exception e) {
            Logger.Error("Failed to load abilities", e);
        }

        pawn.CopySkillsAndPassionsToPawn();
        pawn.ClearPawnCaches();

        return pawn;
    }

    protected RecipeDef FindRecipeDef(string name) {
        var result = DefDatabase<RecipeDef>.GetNamedSilentFail(name);
        if (result == null) {
            return FindReplacementRecipe(name);
        }

        return result;
    }

    protected RecipeDef FindReplacementRecipe(string name) {
        String replacementName = null;
        if (recipeReplacements.TryGetValue(name, out replacementName)) {
            return DefDatabase<RecipeDef>.GetNamedSilentFail(replacementName);
        }

        return null;
    }

    protected UniqueBodyPart FindReplacementBodyPart(OptionsHealth healthOptions, string name) {
        ReplacementBodyPart replacement = null;
        if (bodyPartReplacements.TryGetValue(name, out replacement)) {
            return healthOptions.FindBodyPart(replacement.def, replacement.index);
        }

        return null;
    }

    private BackstoryDef FindBackstory(string name) {
        var matchingBackstory = DefDatabase<BackstoryDef>.AllDefsListForReading.Find(it => it.defName == name);
        // If we couldn't find a matching backstory, look for one with the same identifier, but a different version number at the end.
        if (matchingBackstory == null) {
            var expression = new Regex("\\d+$");
            var backstoryMinusVersioning = expression.Replace(name, "");
            matchingBackstory = DefDatabase<BackstoryDef>.AllDefsListForReading.Find(it =>
                it.defName.StartsWith(backstoryMinusVersioning)
            );
            if (matchingBackstory != null) {
                Logger.Message("Found replacement backstory.  Using " + matchingBackstory.identifier + " in place of " +
                               name);
            }
        }

        return matchingBackstory;
    }

    protected Trait FindTrait(string name, int degree) {
        var trait = LookupTrait(name, degree);
        if (trait != null) {
            return trait;
        }

        if (traitReplacements.ContainsKey(name)) {
            return LookupTrait(traitReplacements[name], degree);
        }

        return null;
    }

    protected Trait LookupTrait(string name, int degree) {
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

    protected void AddSkillDefReplacement(String skill, String replacement) {
        if (!skillDefReplacementLookup.TryGetValue(skill, out var replacements)) {
            replacements = new List<string>();
            skillDefReplacementLookup.Add(skill, replacements);
        }

        replacements.Add(replacement);
    }

    protected SkillDef FindSkillDef(Pawn pawn, string name) {
        List<string> replacements = null;
        if (skillDefReplacementLookup.ContainsKey(name)) {
            replacements = skillDefReplacementLookup[name];
        }

        foreach (var skill in pawn.skills.skills) {
            if (skill.def.defName.Equals(name)) {
                return skill.def;
            }

            if (replacements != null) {
                foreach (var r in replacements) {
                    if (skill.def.defName.Equals(r)) {
                        return skill.def;
                    }
                }
            }
        }

        return null;
    }

    private static HeadTypeDef ToHeadTypeFromHeadGraphicPath(string headGraphicPath) {
        var gender = headGraphicPath.Contains("Female_")
            ? 1
            : 0;
        var width = headGraphicPath.Contains("Narrow_")
            ? 1
            : 0;
        var jaw = headGraphicPath.Contains("_Wide")
            ? 2
            : headGraphicPath.Contains("_Pointy")
                ? 1
                : 0;

        return (gender, width, jaw) switch {
            (0, 0, 0) => DefDatabase<HeadTypeDef>.GetNamed("Male_AverageNormal"),
            (0, 0, 1) => DefDatabase<HeadTypeDef>.GetNamed("Male_AveragePointy"),
            (0, 0, 2) => DefDatabase<HeadTypeDef>.GetNamed("Male_AverageWide"),
            (0, 1, 0) => DefDatabase<HeadTypeDef>.GetNamed("Male_NarrowNormal"),
            (0, 1, 1) => DefDatabase<HeadTypeDef>.GetNamed("Male_NarrowPointy"),
            (0, 1, 2) => DefDatabase<HeadTypeDef>.GetNamed("Male_NarrowWide"),
            (1, 0, 0) => DefDatabase<HeadTypeDef>.GetNamed("Female_AverageNormal"),
            (1, 0, 1) => DefDatabase<HeadTypeDef>.GetNamed("Female_AveragePointy"),
            (1, 0, 2) => DefDatabase<HeadTypeDef>.GetNamed("Female_AverageWide"),
            (1, 1, 0) => DefDatabase<HeadTypeDef>.GetNamed("Female_NarrowNormal"),
            (1, 1, 1) => DefDatabase<HeadTypeDef>.GetNamed("Female_NarrowPointy"),
            (1, 1, 2) => DefDatabase<HeadTypeDef>.GetNamed("Female_NarrowWide"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public class ReplacementBodyPart {
        public BodyPartDef def;
        public int index;

        public ReplacementBodyPart(BodyPartDef def, int index = 0) {
            this.def = def;
            this.index = index;
        }
    }
}
