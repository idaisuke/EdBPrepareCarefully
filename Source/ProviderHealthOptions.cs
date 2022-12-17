using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderHealthOptions {
    protected Dictionary<ThingDef, OptionsHealth> optionsLookup = new();

    public OptionsHealth GetOptions(CustomPawn pawn) {
        OptionsHealth result = null;
        if (!optionsLookup.TryGetValue(pawn.Pawn.def, out result)) {
            result = InitializeHealthOptions(pawn.Pawn.def);
            optionsLookup.Add(pawn.Pawn.def, result);
        }

        return result;
    }

    protected OptionsHealth InitializeHealthOptions(ThingDef pawnThingDef) {
        var result = new OptionsHealth();
        var bodyDef = pawnThingDef.race.body;
        result.BodyDef = bodyDef;

        var ancestors = new HashSet<UniqueBodyPart>();
        ProcessBodyPart(result, bodyDef.corePart, 1, ancestors);

        InitializeImplantRecipes(result, pawnThingDef);
        InitializeInjuryOptions(result, pawnThingDef);

        result.Sort();
        return result;
    }

    protected int ProcessBodyPart(OptionsHealth options, BodyPartRecord record, int index,
        HashSet<UniqueBodyPart> ancestors) {
        var partIndex = options.CountOfMatchingBodyParts(record.def);
        var skinCoveredField =
            typeof(BodyPartDef).GetField("skinCovered", BindingFlags.Instance | BindingFlags.NonPublic);
        var skinCoveredValue = (bool)skinCoveredField.GetValue(record.def);
        var solidField = typeof(BodyPartDef).GetField("solid", BindingFlags.Instance | BindingFlags.NonPublic);
        var isSolidValue = (bool)solidField.GetValue(record.def);
        var part = new UniqueBodyPart {
            Index = partIndex,
            Record = record,
            SkinCovered = skinCoveredValue,
            Solid = isSolidValue,
            Ancestors = ancestors.ToList()
        };
        options.AddBodyPart(part);
        ancestors.Add(part);
        foreach (var c in record.parts) {
            index = ProcessBodyPart(options, c, index + 1, ancestors);
        }

        ancestors.Remove(part);
        return index;
    }

    protected void InitializeImplantRecipes(OptionsHealth options, ThingDef pawnThingDef) {
        // Find all recipes that replace a body part.
        var recipes = new List<RecipeDef>();
        recipes.AddRange(DefDatabase<RecipeDef>.AllDefs.Where(def => {
            if (def.addsHediff != null
                && ((def.appliedOnFixedBodyParts != null && def.appliedOnFixedBodyParts.Count > 0) ||
                    (def.appliedOnFixedBodyPartGroups != null && def.appliedOnFixedBodyPartGroups.Count > 0))
                && (def.recipeUsers.NullOrEmpty() || def.recipeUsers.Contains(pawnThingDef))) {
                return true;
            }

            return false;
        }));

        // Remove duplicates: recipes that apply the same hediff on the same body parts.
        var recipeHashes = new HashSet<int>();
        var dedupedRecipes = new List<RecipeDef>();
        foreach (var recipe in recipes) {
            var hash = recipe.addsHediff.GetHashCode();
            foreach (var part in recipe.appliedOnFixedBodyParts) {
                hash = (hash * 31) + part.GetHashCode();
            }

            if (!recipeHashes.Contains(hash)) {
                dedupedRecipes.Add(recipe);
                recipeHashes.Add(hash);
            }
        }

        recipes = new List<RecipeDef>(dedupedRecipes);

        // Iterate the recipes. Populate a list of all of the body parts that apply to a given recipe.
        foreach (var r in recipes) {
            // Add all of the body parts for that recipe to the list.
            foreach (var bodyPartDef in r.appliedOnFixedBodyParts) {
                var fixedParts = options.FindBodyPartsForDef(bodyPartDef);
                if (fixedParts != null && fixedParts.Count > 0) {
                    //Logger.Debug("Adding recipe for " + r.defName + " for fixed parts " + String.Join(", ", fixedParts.ConvertAll(p => p.Record.LabelCap)));
                    options.AddImplantRecipe(r, fixedParts);
                    foreach (var part in fixedParts) {
                        part.Replaceable = true;
                    }
                }
            }

            foreach (var group in r.appliedOnFixedBodyPartGroups) {
                var partsFromGroup = options.PartsForBodyPartGroup(group.defName);
                if (partsFromGroup != null && partsFromGroup.Count > 0) {
                    //Logger.Debug("Adding recipe for " + r.defName + " for group " + group.defName + " for parts " + String.Join(", ", partsFromGroup.ConvertAll(p => p.Record.LabelCap)));
                    options.AddImplantRecipe(r, partsFromGroup);
                    foreach (var part in partsFromGroup) {
                        part.Replaceable = true;
                    }
                }
            }
        }
    }

    protected bool InitializeHediffGivenByUseEffect(OptionsHealth options,
        CompProperties_UseEffectInstallImplant useEffect) {
        var option = new InjuryOption();
        var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail("PsychicAmplifier");
        if (hediffDef == null) {
            return false;
        }

        option.HediffDef = hediffDef;
        option.Label = useEffect.hediffDef.LabelCap;
        if (useEffect.bodyPart == null) {
            //Logger.Debug("Body part was null for hediff use effect: " + hediffDef.defName);
            return false;
        }

        if (useEffect.bodyPart != null) {
            var validParts = new List<BodyPartDef> { useEffect.bodyPart };
            var parts = options.FindBodyPartsForDef(useEffect.bodyPart);
            if (parts == null || parts.Count == 0) {
                //Logger.Debug("Found no valid body parts for hediff use effect: " + hediffDef.defName + ", " + useEffect.bodyPart.defName);
                return false;
            }

            option.ValidParts = validParts;
        }

        //Logger.Debug($"Add hediff option given by use effect. Hediff = {option.HediffDef.defName}, Label = {option.Label}, BodyPart = {string.Join(", ", option.ValidParts)}");
        options.AddInjury(option);
        return true;
    }

    protected void InitializeHediffGiverInjuries(OptionsHealth options, HediffGiver giver) {
        if (giver == null) {
            Logger.Warning("Could not add injury/health condition because a HediffGiver was null");
            return;
        }

        if (giver.hediff == null) {
            Logger.Warning("Could not add injury/health condition because the hediff for " + giver.GetType().FullName +
                           " was null");
            return;
        }

        var option = new InjuryOption();
        option.HediffDef = giver.hediff;
        option.Label = giver.hediff.LabelCap;
        option.Giver = giver;
        if (giver.partsToAffect == null) {
            option.WholeBody = true;
        }

        if (giver.canAffectAnyLivePart) {
            option.WholeBody = false;
        }

        if (giver.partsToAffect != null && !giver.canAffectAnyLivePart) {
            var validParts = new List<BodyPartDef>();
            foreach (var def in giver.partsToAffect) {
                var parts = options.FindBodyPartsForDef(def);
                if (parts != null) {
                    validParts.Add(def);
                }
            }

            if (validParts.Count == 0) {
                return;
            }

            option.ValidParts = validParts;
        }

        options.AddInjury(option);
    }

    protected InjuryOption CreateMissingPartInjuryOption(OptionsHealth options, HediffDef hd, ThingDef pawnThingDef) {
        var option = new InjuryOption();
        option.HediffDef = hd;
        option.Label = hd.LabelCap;

        var uniquenessLookup = new HashSet<BodyPartDef>();
        var validParts = new List<BodyPartDef>();
        foreach (var p in pawnThingDef.race.body.AllParts.Where(p => p.def.canSuggestAmputation)) {
            if (!uniquenessLookup.Contains(p.def)) {
                validParts.Add(p.def);
                uniquenessLookup.Add(p.def);
            }
        }

        option.ValidParts = validParts;
        //Logger.Debug("For pawn of {" + pawnThingDef.defName + "} missing parts allowed are {" + String.Join(", ", option.ValidParts) + "}");
        return option;
    }

    protected void InitializeInjuryOptions(OptionsHealth options, ThingDef pawnThingDef) {
        var addedDefs = new HashSet<HediffDef>();
        // Go through all of the hediff giver sets for the pawn's race and intialize injuries from
        // each giver.
        if (pawnThingDef.race.hediffGiverSets != null) {
            foreach (var giverSetDef in pawnThingDef.race.hediffGiverSets) {
                foreach (var giver in giverSetDef.hediffGivers) {
                    InitializeHediffGiverInjuries(options, giver);
                }
            }
        }

        // Go through all hediff stages, looking for hediff givers.
        foreach (var hd in DefDatabase<HediffDef>.AllDefs) {
            if (hd.stages != null) {
                foreach (var stage in hd.stages) {
                    if (stage.hediffGivers != null) {
                        foreach (var giver in stage.hediffGivers) {
                            InitializeHediffGiverInjuries(options, giver);
                        }
                    }
                }
            }
        }

        // Go through all of the chemical defs, looking for hediff givers.
        foreach (var chemicalDef in DefDatabase<ChemicalDef>.AllDefs) {
            if (chemicalDef.onGeneratedAddictedEvents != null) {
                foreach (var giver in chemicalDef.onGeneratedAddictedEvents) {
                    InitializeHediffGiverInjuries(options, giver);
                }
            }
        }

        // Go through all thing defs with a CompProperties_UseEffectInstallImplant
        foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.HasComp(typeof(CompUsableImplant)))) {
            var props = def.GetCompProperties<CompProperties_UseEffectInstallImplant>();
            if (props != null) {
                if (InitializeHediffGivenByUseEffect(options, props)) {
                    addedDefs.Add(props.hediffDef);
                }
            }
        }

        // Get all of the hediffs that can be added via the "forced hediff" scenario part and
        // add them to a hash set so that we can quickly look them up.
        var scenPart = new ScenPart_ForcedHediff();
        var scenPartDefs = Reflection.ScenPart_ForcedHediff.PossibleHediffs(scenPart);
        var scenPartDefSet = new HashSet<HediffDef>(scenPartDefs);

        // Add injury options.
        foreach (var hd in DefDatabase<HediffDef>.AllDefs) {
            //Logger.Debug("{0} ({1}), comps = {2}, givers = {3} tags = {4}",
            //    hd.LabelCap,
            //    hd.defName,
            //    string.Join(", ", hd?.comps?.Select(c => c.compClass.FullName) ?? new string[] { "none" }),
            //    string.Join(", ", hd?.hediffGivers?.Select(g => g.GetType().FullName) ?? new string[] { "none" }),
            //    string.Join(", ", hd?.tags ?? new List<string>(new string[] { "none" }))
            //);
            try {
                // Exclude hediffs that are added by wearing apparel
                if (hd.comps != null && hd.comps.Where(c => c is HediffCompProperties_RemoveIfApparelDropped).Any()) {
                    Logger.Debug($"Skipping hediff because it is removed when the pawn drops apparel: {hd.defName}");
                    continue;
                }

                if (hd.hediffClass == typeof(Hediff_MissingPart)) {
                    options.AddInjury(CreateMissingPartInjuryOption(options, hd, pawnThingDef));
                    continue;
                }

                // Filter out defs that were already added via the hediff giver sets.
                if (addedDefs.Contains(hd)) {
                    //Logger.Debug($"Skipping hediff because it was already added: {hd.defName}");
                    continue;
                }

                // Filter out implants.
                if (hd.hediffClass != null && typeof(Hediff_Implant).IsAssignableFrom(hd.hediffClass)) {
                    continue;
                }

                // If it's an old injury, use the old injury properties to get the label.
                var p = hd.CompPropsFor(typeof(HediffComp_GetsPermanent));
                var getsPermanentProperties = p as HediffCompProperties_GetsPermanent;

                var warning = false;
                if (getsPermanentProperties == null) {
                    if (!hd.scenarioCanAdd) {
                        if (hd.comps != null && hd.comps.Count > 0) {
                            warning = true;
                        }
                    }
                }

                String label;
                if (getsPermanentProperties != null) {
                    if (getsPermanentProperties.permanentLabel != null) {
                        label = getsPermanentProperties.permanentLabel.CapitalizeFirst();
                    }
                    else {
                        Logger.Warning("Could not find label for old injury: " + hd.defName);
                        continue;
                    }
                }
                else {
                    label = hd.LabelCap;
                }

                // Add the injury option..
                var option = new InjuryOption();
                option.HediffDef = hd;
                option.Label = label;
                option.Warning = warning;
                if (getsPermanentProperties != null) {
                    option.IsOldInjury = true;
                }
                else if (hd.hediffClass == typeof(Hediff_Injury)) {
                    continue;
                }
                else {
                    option.ValidParts = new List<BodyPartDef>();
                }

                options.AddInjury(option);
            }
            catch (Exception e) {
                Logger.Warning(
                    "There was en error while processing hediff {" + hd.defName +
                    "} when trying to add it to the list of available injury options", e);
            }
        }

        // Disambiguate duplicate injury labels.
        var labels = new HashSet<string>();
        var duplicateLabels = new HashSet<string>();
        foreach (var option in options.InjuryOptions) {
            if (labels.Contains(option.Label)) {
                duplicateLabels.Add(option.Label);
            }
            else {
                labels.Add(option.Label);
            }
        }

        foreach (var option in options.InjuryOptions) {
            var p = option.HediffDef.CompPropsFor(typeof(HediffComp_GetsPermanent));
            var props = p as HediffCompProperties_GetsPermanent;
            if (props != null) {
                if (duplicateLabels.Contains(option.Label)) {
                    string label =
                        "EdB.PC.Dialog.Injury.OldInjury.Label".Translate(props.permanentLabel.CapitalizeFirst(),
                            option.HediffDef.LabelCap);
                    option.Label = label;
                }
            }
        }

        foreach (var option in options.InjuryOptions) {
            var uniqueParts = new List<UniqueBodyPart>();
            if (option.ValidParts != null && option.ValidParts.Count > 0) {
                foreach (var part in option.ValidParts) {
                    var uniquenessLookup = new HashSet<BodyPartDef>();
                    if (!uniquenessLookup.Contains(part)) {
                        uniquenessLookup.Add(part);
                        uniqueParts.AddRange(options.FindBodyPartsForDef(part));
                    }
                }
            }

            option.UniqueParts = uniqueParts;
        }
    }
}
