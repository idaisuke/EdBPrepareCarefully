using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using CharacterCardUtility = EdB.PrepareCarefully.Reflection.CharacterCardUtility;

namespace EdB.PrepareCarefully;

public class ApparelConflict {
    public ThingDef conflict;
    public ThingDef def;
}

public class CustomPawn {
    protected Dictionary<PawnLayer, ThingDef> acceptedApparel = new();
    protected List<ApparelConflict> apparelConflicts = new();
    protected string apparelConflictText;
    public List<CustomBodyPart> bodyParts = new();
    protected Dictionary<EquipmentKey, Color> colorCache = new();

    protected Dictionary<PawnLayer, Color> colors = new();
    public Dictionary<SkillDef, Passion> currentPassions = new();

    // The pawn's current skill levels, without modifiers for backstories and traits.
    protected Dictionary<SkillDef, int> currentSkillLevels = new();
    protected CustomFaction faction;

    // A GUID provides a unique identifier for the CustomPawn.
    protected string id;

    protected List<Implant> implants = new();

    protected string incapable;
    protected List<Injury> injuries = new();

    // Keep track of the most recently selected adulthood option so that if the user updates the pawn's
    // age in a way that switches them back and forth from adult to child (which nulls out the adulthood
    // value in the Pawn), we can remember what the value was and restore it.
    protected BackstoryDef lastSelectedAdulthoodBackstory;
    protected FactionDef originalFactionDef;
    protected PawnKindDef originalKindDef;

    public Dictionary<SkillDef, Passion> originalPassions = new();

    // The pawn's skill values before customization, without modifiers for backstories and traits.
    // These values are saved so that the user can click the "Reset" button to restore them.
    protected Dictionary<SkillDef, int> originalSkillLevels = new();
    protected Pawn pawn;
    protected bool portraitDirty = true;

    protected Dictionary<PawnLayer, ThingDef> selectedApparel = new();
    protected Dictionary<PawnLayer, ThingDef> selectedStuff = new();

    // The pawn's skill value modifiers from selected backstories and traits.
    protected Dictionary<SkillDef, int> skillLevelModifiers = new();
    protected ThingCache thingCache = new();

    public CustomPawn(Pawn pawn) {
        GenerateId();
        InitializeWithPawn(pawn);
    }

    public string Id {
        get => id;
        set => id = value;
    }

    public CustomPawnType Type {
        get;
        set;
    }

    // For hidden or temporary pawns, keep track of an index number.
    public int? Index {
        get;
        set;
    }

    public bool Hidden => Type == CustomPawnType.Hidden || Type == CustomPawnType.Temporary;

    public CustomFaction Faction {
        get => faction;
        set => faction = value;
    }

    // Stores the original FactionDef of a pawn that was created from a faction template.
    public FactionDef OriginalFactionDef {
        get => originalFactionDef;
        set => originalFactionDef = value;
    }

    public BodyTypeDef BodyType {
        get => pawn.story.bodyType;
        set {
            pawn.story.bodyType = value;
            MarkPortraitAsDirty();
        }
    }

    public bool HasCustomBodyParts => bodyParts.Count > 0;

    public List<Injury> Injuries {
        get => injuries;
        set => injuries = value;
    }

    public List<CustomBodyPart> BodyParts => bodyParts;

    public float Certainty {
        get => pawn.ideo?.Certainty ?? 0.75f;
        set {
            if (pawn.ideo != null) {
                var current = pawn.ideo.Certainty;
                if (current != value) {
                    pawn.ideo.Debug_ReduceCertainty(current - value);
                }
            }
        }
    }

    public Color HairColor {
        get => pawn.story.HairColor;
        set {
            pawn.story.HairColor = value;
            MarkPortraitAsDirty();
        }
    }

    public BeardDef Beard {
        get => pawn.style.beardDef;
        set {
            pawn.style.beardDef = value;
            MarkPortraitAsDirty();
        }
    }

    public TattooDef FaceTattoo {
        get => pawn.style.FaceTattoo;
        set {
            if (ModLister.IdeologyInstalled) {
                pawn.style.FaceTattoo = value;
                MarkPortraitAsDirty();
            }
        }
    }

    public TattooDef BodyTattoo {
        get => pawn.style.BodyTattoo;
        set {
            if (ModLister.IdeologyInstalled) {
                pawn.style.BodyTattoo = value;
                MarkPortraitAsDirty();
            }
        }
    }

    public BackstoryDef LastSelectedAdulthoodBackstory {
        get {
            if (lastSelectedAdulthoodBackstory != null) {
                return lastSelectedAdulthoodBackstory;
            }

            lastSelectedAdulthoodBackstory = Randomizer.RandomAdulthood(this);
            return lastSelectedAdulthoodBackstory;
        }
        set => lastSelectedAdulthoodBackstory = value;
    }

    public NameTriple? Name {
        get => pawn.Name as NameTriple;
        set => pawn.Name = value;
    }

    public string FirstName {
        get {
            if (pawn.Name is NameTriple nameTriple) {
                return nameTriple.First;
            }

            return null;
        }
        set => pawn.Name = new NameTriple(value, NickName, LastName);
    }

    public string NickName {
        get {
            var nameTriple = pawn.Name as NameTriple;
            if (nameTriple != null) {
                return nameTriple.Nick;
            }

            return null;
        }
        set => pawn.Name = new NameTriple(FirstName, value, LastName);
    }

    public string LastName {
        get {
            var nameTriple = pawn.Name as NameTriple;
            if (nameTriple != null) {
                return nameTriple.Last;
            }

            return null;
        }
        set => pawn.Name = new NameTriple(FirstName, NickName, value);
    }

    public string ShortName {
        get {
            if (Type == CustomPawnType.Hidden) {
                return "EdB.PC.Pawn.HiddenPawnNameShort".Translate(Index.Value);
            }

            if (Type == CustomPawnType.Temporary) {
                return "EdB.PC.Pawn.TemporaryPawnNameShort".Translate(Index.Value);
            }

            if (pawn == null) {
                Logger.Warning("Pawn was null");
                return "";
            }

            return pawn.LabelShortCap;
        }
    }

    public string FullName {
        get {
            if (Type == CustomPawnType.Hidden) {
                if (Index.HasValue) {
                    return "EdB.PC.Pawn.HiddenPawnNameFull".Translate(Index.Value);
                }

                return "EdB.PC.Pawn.HiddenPawnNameFull".Translate();
            }

            if (Type == CustomPawnType.Temporary) {
                if (Index.HasValue) {
                    return "EdB.PC.Pawn.TemporaryPawnNameFull".Translate(Index.Value);
                }

                return "EdB.PC.Pawn.TemporaryPawnNameFull".Translate();
            }

            return pawn.Name.ToStringFull;
        }
    }

    public Pawn Pawn => pawn;

    public string Label {
        get {
            var name = pawn.Name as NameTriple;
            if (pawn.story.Adulthood == null) {
                return name.Nick;
            }

            return name.Nick + ", " + pawn.story.Adulthood.TitleShortFor(Gender);
        }
    }

    public string LabelShort => pawn.LabelShort;

    public IEnumerable<Implant> Implants => implants;

    public bool IsAdult => pawn.DevelopmentalStage.Adult();

    // Stores the original PawnKindDef of the pawn.  This value automatically changes when you assign
    // a pawn to the FactionOf.Colony, so we want to preserve it for faction pawns that are created from
    // a different PawnKindDef.
    public PawnKindDef OriginalKindDef {
        get => originalKindDef;
        set => originalKindDef = value;
    }

    public List<ThingDef> AllAcceptedApparel {
        get {
            var result = new List<ThingDef>();
            foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this)) {
                var def = acceptedApparel[layer];
                if (def != null) {
                    result.Add(def);
                }
            }

            return result;
        }
    }

    public string ProfessionLabel {
        get {
            if (IsAdult) {
                return Adulthood.TitleCapFor(Gender);
            }

            return Childhood.TitleCapFor(Gender);
        }
    }

    public string ProfessionLabelShort {
        get {
            if (IsAdult) {
                return Adulthood.TitleShortFor(Gender).CapitalizeFirst();
            }

            return Childhood.TitleShortFor(Gender).CapitalizeFirst();
        }
    }

    public string ApparelConflict => apparelConflictText;

    public BackstoryDef Childhood {
        get => pawn.story.Childhood;
        set {
            pawn.story.Childhood = value;
            ResetBackstories();
        }
    }

    public BackstoryDef Adulthood {
        get => pawn.story.Adulthood;
        set {
            if (value != null) {
                LastSelectedAdulthoodBackstory = value;
            }

            if (IsAdult) {
                pawn.story.Adulthood = value;
            }
            else {
                pawn.story.Adulthood = null;
            }

            ResetBackstories();
        }
    }

    public bool HasAdulthoodBackstory => Adulthood != null;

    public HeadTypeDef HeadType {
        get => pawn.story.headType;
        set {
            pawn.story.headType = value;

            MarkPortraitAsDirty();
        }
    }

    public IEnumerable<Trait> Traits => Pawn.story.traits.allTraits;

    public int TraitCount => Pawn.story.traits.allTraits.Count;

    public string IncapableOf => incapable;

    public Gender Gender {
        get => pawn.gender;
        set {
            if (pawn.gender != value) {
                pawn.gender = value;
                ResetGender();
                MarkPortraitAsDirty();
            }
        }
    }

    public Color? FavoriteColor {
        get {
            if (ModsConfig.IdeologyActive) {
                return Pawn.story.favoriteColor;
            }

            return null;
        }
        set {
            if (ModsConfig.IdeologyActive) {
                Pawn.story.favoriteColor = value;
            }
        }
    }

    public Color SkinColor {
        get => pawn.story.SkinColor;
        set {
            MelaninLevel = PawnColorUtils.FindMelaninValueFromColor(value);
            pawn.story.SkinColorBase = value;
        }
    }

    public float MelaninLevel {
        get => pawn.genes.GetMelaninGene().minMelanin;
        set {
            var melaninGene =
                pawn.genes.GenesListForReading.Find(it => it.def.endogeneCategory == EndogeneCategory.Melanin);

            if (melaninGene != null) {
                pawn.genes.RemoveGene(melaninGene);
            }

            pawn.genes.AddGene(PawnSkinColors.GetSkinColorGene(value), false);

            MarkPortraitAsDirty();
        }
    }

    public HairDef HairDef {
        get => pawn.story.hairDef;
        set {
            pawn.story.hairDef = value;
            MarkPortraitAsDirty();
        }
    }

    public int ChronologicalAge {
        get => pawn.ageTracker.AgeChronologicalYears;
        set {
            long years = pawn.ageTracker.AgeChronologicalYears;
            var diff = value - years;
            pawn.ageTracker.BirthAbsTicks -= diff * 3600000L;
            pawn.ClearCachedLifeStage();
            pawn.ClearCachedHealth();
        }
    }

    public int BiologicalAge {
        get => pawn.ageTracker.AgeBiologicalYears;
        set {
            long years = pawn.ageTracker.AgeBiologicalYears;
            var diff = value - years;
            pawn.ageTracker.AgeBiologicalTicks += diff * 3600000L;
            if (IsAdult && pawn.story.Adulthood == null) {
                pawn.story.Adulthood = LastSelectedAdulthoodBackstory;
                ResetBackstories();
            }
            else if (!IsAdult && pawn.story.Adulthood != null) {
                pawn.story.Adulthood = null;
                ResetBackstories();
            }

            pawn.ClearCachedLifeStage();
            pawn.ClearCachedHealth();
            MarkPortraitAsDirty();
        }
    }

    public void GenerateId() {
        id = Guid.NewGuid().ToStringSafe();
    }

    // We use a dirty flag for the portrait to avoid calling ClearCachedPortrait() every frame.
    protected void CheckPortraitCache() {
        if (portraitDirty) {
            portraitDirty = false;
            pawn.ClearCachedPortraits();
        }
    }

    public void MarkPortraitAsDirty() {
        portraitDirty = true;
    }

    public void UpdatePortrait() {
        CheckPortraitCache();
    }

    public RenderTexture GetPortrait(Vector2 size) {
        return PortraitsCache.Get(Pawn, size, Rot4.South, new Vector3(0, 0, 0));
    }

    public void InitializeWithPawn(Pawn pawn) {
        this.pawn = pawn;
        this.pawn.ClearCaches();

        originalKindDef = pawn.kindDef;
        originalFactionDef = pawn.Faction != null ? pawn.Faction.def : null;

        PrepareCarefully.Instance.Providers.Health.GetOptions(this);

        // Set the skills.
        InitializeSkillLevelsAndPassions();
        ComputeSkillLevelModifiers();

        // Clear all of the pawn layer colors.  The apparel colors will be set a little later
        // when we initialize the apparel layer.
        colors.Clear();
        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this)) {
            colors.Add(layer, Color.white);
        }

        // Clear all of the apparel and alien addon layers that we're tracking in the CustomPawn.
        selectedApparel.Clear();
        acceptedApparel.Clear();
        selectedStuff.Clear();
        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this)) {
            if (layer.Apparel) {
                selectedApparel.Add(layer, null);
                acceptedApparel.Add(layer, null);
                selectedStuff.Add(layer, null);
            }
        }

        // Store the current value of each apparel layer based on the apparel worn by the Pawn.
        foreach (var current in this.pawn.apparel.WornApparel) {
            var color = current.DrawColor;
            var colorable = current.TryGetComp<CompColorable>();
            if (colorable != null) {
                //Logger.Debug(String.Format("{0} {1}, CompColorable: color={2}, desiredColor={3}, active={4}", current.def.defName, current.Stuff?.defName, colorable?.Color, colorable?.DesiredColor, colorable?.Active));
                if (colorable.Active) {
                    color = colorable.Color;
                }
            }

            var layer = PrepareCarefully.Instance.Providers.PawnLayers.FindLayerForApparel(current.def);
            if (layer != null) {
                SetSelectedApparelInternal(layer, current.def);
                acceptedApparel[layer] = current.def;
                SetSelectedStuffInternal(layer, current.Stuff);
                SetColorInternal(layer, color);
            }
        }

        // Reset CustomPawn cached values.
        ResetApparel();
        ResetCachedIncapableOf();

        // Copy the adulthood backstory or set a random one if it's null.
        LastSelectedAdulthoodBackstory = pawn.story.Adulthood;

        // Evaluate all hediffs.
        InitializeInjuriesAndImplantsFromPawn(pawn);

        // Clear all of the pawn caches.
        ClearPawnCaches();
    }

    public void InitializeInjuriesAndImplantsFromPawn(Pawn pawn) {
        var healthOptions = PrepareCarefully.Instance.Providers.Health.GetOptions(this);
        var injuries = new List<Injury>();
        var implants = new List<Implant>();

        // Create a lookup of all of the body parts that are missing
        var missingParts = new HashSet<BodyPartRecord>();
        foreach (var hediff in pawn.health.hediffSet.hediffs) {
            if (hediff is Hediff_MissingPart || hediff is Hediff_AddedPart) {
                missingParts.Add(hediff.Part);
            }
        }

        foreach (var hediff in pawn.health.hediffSet.hediffs) {
            var option = healthOptions.FindInjuryOptionByHediffDef(hediff.def);
            if (option != null) {
                //Logger.Debug("Found injury option for {" + hediff.def.defName + "} for part {" + hediff.Part?.LabelCap + "}");

                // If the hediff is a missing part and the part's parent is also missing, we don't add a missing part hediff for the child part.
                if (hediff is Hediff_MissingPart) {
                    if (hediff.Part.parent != null && missingParts.Contains(hediff.Part.parent)) {
                        continue;
                    }
                }

                var injury = new Injury();
                injury.BodyPartRecord = hediff.Part;
                injury.Option = option;
                injury.Severity = hediff.Severity;
                injury.Hediff = hediff;
                var getsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
                if (getsPermanent != null) {
                    injury.PainFactor = getsPermanent.PainFactor;
                }

                injuries.Add(injury);
            }
            else {
                //Logger.Debug("Did not find injury option for {" + hediff.def.defName + "} for part {" + hediff.Part?.LabelCap + "}");
                var implantRecipe = healthOptions.FindImplantRecipesThatAddHediff(hediff).RandomElementWithFallback();
                if (implantRecipe != null) {
                    var implant = new Implant();
                    implant.recipe = implantRecipe;
                    implant.BodyPartRecord = hediff.Part;
                    implant.Hediff = hediff;
                    implants.Add(implant);
                    //Logger.Debug("Found implant recipes for {" + hediff.def.defName + "} for part {" + hediff.Part?.LabelCap + "}");
                }
                else if (hediff.def.defName != "MissingBodyPart") {
                    Logger.Warning("Could not add hediff {" + hediff.def.defName +
                                   "} to the pawn because no recipe adds it to the body part {" +
                                   (hediff.Part?.def?.defName ?? "WholeBody") + "}");
                }
                else {
                    Logger.Warning("Could not add hediff {" + hediff.def.defName +
                                   "} to the pawn.  It is not currently supported");
                }
            }
        }

        this.injuries.Clear();
        this.implants.Clear();
        bodyParts.Clear();
        foreach (var injury in injuries) {
            this.injuries.Add(injury);
            bodyParts.Add(injury);
        }

        foreach (var implant in implants) {
            this.implants.Add(implant);
            bodyParts.Add(implant);
        }
    }

    protected void InitializeSkillLevelsAndPassions() {
        if (pawn.skills == null) {
            Logger.Warning("Could not initialize skills for the pawn.  No pawn skill tracker for " + pawn.def.defName +
                           ", " + pawn.kindDef.defName);
        }

        // Save the original passions and set the current values to the same.
        foreach (var record in pawn.skills.skills) {
            originalPassions[record.def] = record.passion;
            currentPassions[record.def] = record.passion;
        }

        // Compute and save the original unmodified skill levels.
        // If the user's original, modified skill level was zero, we dont actually know what
        // their original unadjusted value was.  For example, if they have the brawler trait
        // (-6 shooting) and their shooting level is zero, what was the original skill level?
        // We don't know.  It could have been anywhere from 0 to 6.
        // We could maybe borrow some code from Pawn_StoryTracker.FinalLevelOfSkill() to be
        // smarter about computing the value (i.e. factoring in the pawn's age, etc.), but
        // instead we'll just pick a random number from the correct range if this happens.
        foreach (var record in pawn.skills.skills) {
            var negativeAdjustment = 0;
            var positiveAdjustment = 0;
            var modifier = ComputeSkillModifier(record.def);
            if (modifier < 0) {
                negativeAdjustment = -modifier;
            }
            else if (modifier > 0) {
                positiveAdjustment = modifier;
            }

            // When figuring out the unadjusted value, take into account the special
            // case where the adjusted value is 0 or 20.
            var value = record.Level;
            if (value == 0 && negativeAdjustment > 0) {
                value = Rand.RangeInclusive(1, negativeAdjustment);
            }
            else if (value == 20 && positiveAdjustment > 0) {
                value = Rand.RangeInclusive(20 - positiveAdjustment, 20);
            }
            else {
                value -= positiveAdjustment;
                value += negativeAdjustment;
            }

            originalSkillLevels[record.def] = value;
        }

        // Set the current values to the original values.
        foreach (var record in pawn.skills.skills) {
            currentSkillLevels[record.def] = originalSkillLevels[record.def];
        }
    }

    public void ClearApparel() {
        pawn.apparel.DestroyAll();
    }

    public void CopyAppearance(Pawn pawn) {
        HairDef = pawn.story.hairDef;
        this.pawn.story.HairColor = pawn.story.HairColor;
        this.pawn.story.bodyType = pawn.story.bodyType;
        if (pawn.style != null && Pawn.style != null) {
            Beard = pawn.style.beardDef;
            FaceTattoo = pawn.style.FaceTattoo;
            BodyTattoo = pawn.style.BodyTattoo;
        }

        foreach (var gene in this.pawn.genes.GenesListForReading) {
            this.pawn.genes.RemoveGene(gene);
        }

        foreach (var gene in pawn.genes.Xenogenes) {
            this.pawn.genes.AddGene(gene.def, true);
        }

        foreach (var gene in pawn.genes.Endogenes) {
            this.pawn.genes.AddGene(gene.def, false);
        }

        Pawn.apparel.DestroyAll();
        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this)) {
            if (layer.Apparel) {
                SetSelectedStuff(layer, null);
                SetSelectedApparel(layer, null);
            }
        }

        foreach (var current in pawn.apparel.WornApparel) {
            var layer = PrepareCarefully.Instance.Providers.PawnLayers.FindLayerForApparel(current.def);
            if (layer != null) {
                SetSelectedStuff(layer, current.Stuff);
                SetSelectedApparel(layer, current.def);
            }
        }

        MarkPortraitAsDirty();
    }

    public void RestoreSkillLevelsAndPassions() {
        // Restore the original passions.
        foreach (var record in pawn.skills.skills) {
            currentPassions[record.def] = originalPassions[record.def];
        }

        // Restore the original skill levels.
        ApplyOriginalSkillLevels();
    }

    // Restores the current skill level values to the saved, original values.
    public void ApplyOriginalSkillLevels() {
        foreach (var record in pawn.skills.skills) {
            currentSkillLevels[record.def] = originalSkillLevels[record.def];
        }

        CopySkillsAndPassionsToPawn();
    }

    public void UpdateSkillLevelsForNewBackstoryOrTrait() {
        ComputeSkillLevelModifiers();
        ResetCachedIncapableOf();
        ClearPawnCaches();
        CopySkillsAndPassionsToPawn();
    }

    // Computes the skill level modifiers that the pawn gets from the selected backstories and traits.
    public void ComputeSkillLevelModifiers() {
        foreach (var record in pawn.skills.skills) {
            skillLevelModifiers[record.def] = ComputeSkillModifier(record.def);
        }

        CopySkillsAndPassionsToPawn();
    }

    protected int ComputeSkillModifier(SkillDef def) {
        var value = 0;
        if (pawn.story != null && pawn.story.Childhood != null && pawn.story.Childhood.skillGains != null) {
            if (pawn.story.Childhood.skillGains.ContainsKey(def)) {
                value += pawn.story.Childhood.skillGains[def];
            }
        }

        if (pawn.story != null && pawn.story.Adulthood != null && pawn.story.Adulthood.skillGains != null) {
            if (pawn.story.Adulthood.skillGains.ContainsKey(def)) {
                value += pawn.story.Adulthood.skillGains[def];
            }
        }

        foreach (var trait in Pawn.story.traits.allTraits) {
            if (trait != null && trait.def != null && trait.def.degreeDatas != null) {
                foreach (var data in trait.def.degreeDatas) {
                    if (data.degree == trait.Degree) {
                        if (data.skillGains != null) {
                            foreach (var pair in data.skillGains) {
                                if (pair.Key != null) {
                                    var skillDef = pair.Key;
                                    if (skillDef == def) {
                                        value += pair.Value;
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        return value;
    }

    public void DecrementSkillLevel(SkillDef def) {
        SetSkillLevel(def, GetSkillLevel(def) - 1);
    }

    public void IncrementSkillLevel(SkillDef def) {
        SetSkillLevel(def, GetSkillLevel(def) + 1);
    }

    public int GetSkillLevel(SkillDef def) {
        if (IsSkillDisabled(def)) {
            return 0;
        }

        var value = 0;
        if (currentSkillLevels.ContainsKey(def)) {
            value = currentSkillLevels[def];
            if (skillLevelModifiers.ContainsKey(def)) {
                value += skillLevelModifiers[def];
            }
        }

        if (value < SkillRecord.MinLevel) {
            return SkillRecord.MinLevel;
        }

        if (value > SkillRecord.MaxLevel) {
            value = SkillRecord.MaxLevel;
        }

        return value;
    }

    public void SetSkillLevel(SkillDef def, int value) {
        if (value > 20) {
            value = 20;
        }
        else if (value < 0) {
            value = 0;
        }

        var modifier = skillLevelModifiers[def];
        if (value < modifier) {
            currentSkillLevels[def] = 0;
        }
        else {
            currentSkillLevels[def] = value - modifier;
        }

        CopySkillsAndPassionsToPawn();
    }

    // Any time a skill changes, update the underlying pawn with the new values.
    public void CopySkillsAndPassionsToPawn() {
        foreach (var record in pawn.skills.skills) {
            record.Level = GetSkillLevel(record.def);
            // Reset the XP level based on the current value of the skill.
            record.xpSinceLastLevel =
                Rand.Range(record.XpRequiredForLevelUp * 0.1f, record.XpRequiredForLevelUp * 0.9f);
            if (!record.TotallyDisabled) {
                record.passion = currentPassions[record.def];
            }
            else {
                record.passion = Passion.None;
            }
        }
    }

    // Set all unmodified skill levels to zero.
    public void ClearSkills() {
        foreach (var record in pawn.skills.skills) {
            currentSkillLevels[record.def] = 0;
        }

        CopySkillsAndPassionsToPawn();
    }

    public void ClearPassions() {
        foreach (var record in pawn.skills.skills) {
            currentPassions[record.def] = Passion.None;
            ;
        }

        CopySkillsAndPassionsToPawn();
    }

    public bool IsSkillDisabled(SkillDef def) {
        return pawn.skills.GetSkill(def).TotallyDisabled;
    }

    public int GetSkillModifier(SkillDef def) {
        return skillLevelModifiers[def];
    }

    public int GetUnmodifiedSkillLevel(SkillDef def) {
        return currentSkillLevels[def];
    }

    public void SetUnmodifiedSkillLevel(SkillDef def, int value) {
        currentSkillLevels[def] = value;
        CopySkillsAndPassionsToPawn();
    }

    public int GetOriginalSkillLevel(SkillDef def) {
        return originalSkillLevels[def];
    }

    public void SetOriginalSkillLevel(SkillDef def, int value) {
        originalSkillLevels[def] = value;
    }

    public bool IsBodyPartReplaced(BodyPartRecord record) {
        var implant = implants.FirstOrDefault(i => {
            return i.BodyPartRecord == record;
        });
        return implant != null;
    }

    public void SetPassion(SkillDef def, Passion level) {
        if (IsSkillDisabled(def)) {
            return;
        }

        currentPassions[def] = level;
        var record = pawn.skills.GetSkill(def);
        if (record != null) {
            record.passion = level;
        }
    }

    public void IncreasePassion(SkillDef def) {
        if (IsSkillDisabled(def)) {
            return;
        }

        if (currentPassions[def] == Passion.None) {
            currentPassions[def] = Passion.Minor;
        }
        else if (currentPassions[def] == Passion.Minor) {
            currentPassions[def] = Passion.Major;
        }
        else if (currentPassions[def] == Passion.Major) {
            currentPassions[def] = Passion.None;
        }

        pawn.skills.GetSkill(def).passion = currentPassions[def];
        CopySkillsAndPassionsToPawn();
    }

    public void DecreasePassion(SkillDef def) {
        if (IsSkillDisabled(def)) {
            return;
        }

        if (currentPassions[def] == Passion.None) {
            currentPassions[def] = Passion.Major;
        }
        else if (currentPassions[def] == Passion.Minor) {
            currentPassions[def] = Passion.None;
        }
        else if (currentPassions[def] == Passion.Major) {
            currentPassions[def] = Passion.Minor;
        }

        pawn.skills.GetSkill(def).passion = currentPassions[def];
        CopySkillsAndPassionsToPawn();
    }

    public ThingDef GetAcceptedApparel(PawnLayer layer) {
        return acceptedApparel[layer];
    }

    public Color GetColor(PawnLayer layer) {
        if (colors.TryGetValue(layer, out var color)) {
            return color;
        }

        return Color.white;
    }

    public void ClearColorCache() {
        colorCache.Clear();
    }

    public Color GetStuffColor(PawnLayer layer) {
        var apparelDef = selectedApparel[layer];
        if (apparelDef != null) {
            var color = GetColor(layer);
            if (apparelDef.MadeFromStuff) {
                var stuffDef = selectedStuff[layer];
                if (stuffDef != null && stuffDef.stuffProps != null) {
                    if (!stuffDef.stuffProps.allowColorGenerators) {
                        return stuffDef.stuffProps.color;
                    }
                }
            }
        }

        return Color.white;
    }

    public void SetColor(PawnLayer layer, Color color) {
        SetColorInternal(layer, color);
        ResetApparel();
    }

    // Separate method that can be called internally without clearing the graphics caches or copying
    // to the target pawn.
    public void SetColorInternal(PawnLayer layer, Color color) {
        colors[layer] = color;
        if (layer.Apparel) {
            colorCache[new EquipmentKey(selectedApparel[layer], selectedStuff[layer])] = color;
        }
    }

    public bool ColorMatches(Color a, Color b) {
        if (a.r > b.r - 0.001f && a.r < b.r + 0.001f
                               && a.r > b.r - 0.001f && a.r < b.r + 0.001f
                               && a.r > b.r - 0.001f && a.r < b.r + 0.001f) {
            return true;
        }

        return false;
    }

    private void ResetApparel() {
        CopyApparelToPawn(pawn);
        MarkPortraitAsDirty();
    }

    public ThingDef GetSelectedApparel(PawnLayer layer) {
        return selectedApparel[layer];
    }

    public void SetSelectedApparel(PawnLayer layer, ThingDef def) {
        SetSelectedApparelInternal(layer, def);
        ResetApparel();
    }

    // Separate method that can be called internally without clearing the graphics caches or copying
    // to the target pawn.
    private void SetSelectedApparelInternal(PawnLayer layer, ThingDef def) {
        if (layer == null) {
            return;
        }

        selectedApparel[layer] = def;
        if (def != null) {
            var stuffDef = GetSelectedStuff(layer);
            var pair = new EquipmentKey(def, stuffDef);
            if (colorCache.ContainsKey(pair)) {
                colors[layer] = colorCache[pair];
            }
            else {
                if (stuffDef == null) {
                    if (def.colorGenerator != null) {
                        if (!ColorValidator.Validate(def.colorGenerator, colors[layer])) {
                            colors[layer] = def.colorGenerator.NewRandomizedColor();
                        }
                    }
                    else {
                        colors[layer] = Color.white;
                    }
                }
                else {
                    colors[layer] = stuffDef.stuffProps.color;
                }
            }
        }

        acceptedApparel[layer] = def;
        ApparelAcceptanceTest();
    }

    public ThingDef GetSelectedStuff(PawnLayer layer) {
        return selectedStuff[layer];
    }

    public void SetSelectedStuff(PawnLayer layer, ThingDef stuffDef) {
        SetSelectedStuffInternal(layer, stuffDef);
        ResetApparel();
    }

    public void SetSelectedStuffInternal(PawnLayer layer, ThingDef stuffDef) {
        if (layer == null) {
            return;
        }

        if (selectedStuff[layer] == stuffDef) {
            return;
        }

        selectedStuff[layer] = stuffDef;
        if (stuffDef != null) {
            var apparelDef = GetSelectedApparel(layer);
            if (apparelDef != null) {
                var pair = new EquipmentKey(apparelDef, stuffDef);
                Color color;
                if (colorCache.TryGetValue(pair, out color)) {
                    colors[layer] = color;
                }
                else {
                    colors[layer] = stuffDef.stuffProps.color;
                }
            }
        }
    }

    protected void ApparelAcceptanceTest() {
        // Clear out any conflicts from a previous check.
        apparelConflicts.Clear();

        // Assume that each peice of apparel will be accepted.
        foreach (var layer in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this).AsEnumerable()
                     .Where(layer => { return layer.Apparel; }).Reverse()) {
            acceptedApparel[layer] = selectedApparel[layer];
        }

        foreach (var i in PrepareCarefully.Instance.Providers.PawnLayers.GetLayersForPawn(this).AsEnumerable()
                     .Where(layer => { return layer.Apparel; }).Reverse()) {
            // If no apparel was selected for this layer, go to the next layer.
            if (selectedApparel[i] == null) {
                continue;
            }

            var apparel = selectedApparel[i];
            if (apparel.apparel != null && apparel.apparel.layers != null && apparel.apparel.layers.Count > 1) {
                foreach (var apparelLayer in apparel.apparel.layers) {
                    // If the apparel's layer matches the current layer, go to the apparel's next layer. 
                    if (apparelLayer == i.ApparelLayer) {
                        continue;
                    }

                    // If the apparel covers another layer as well as the current one, check to see
                    // if the user has selected another piece of apparel for that layer.  If so, check
                    // to see if it covers any of the same body parts.  If it does, it's a conflict.
                    var disallowedLayer =
                        PrepareCarefully.Instance.Providers.PawnLayers.FindLayerForApparelLayer(apparelLayer);
                    if (disallowedLayer != null && selectedApparel[disallowedLayer] != null) {
                        foreach (var group in selectedApparel[disallowedLayer].apparel.bodyPartGroups) {
                            if (apparel.apparel.bodyPartGroups.Contains(group)) {
                                var conflict = new ApparelConflict();
                                conflict.def = selectedApparel[i];
                                conflict.conflict = selectedApparel[disallowedLayer];
                                apparelConflicts.Add(conflict);
                                acceptedApparel[disallowedLayer] = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (apparelConflicts.Count > 0) {
            var defs = new HashSet<ThingDef>();
            foreach (var conflict in apparelConflicts) {
                defs.Add(conflict.def);
            }

            var sortedDefs = new List<ThingDef>(defs);
            /*
            sortedDefs.Sort((ThingDef a, ThingDef b) => {
                int c = PawnLayers.ToPawnLayerIndex(a.apparel);
                int d = PawnLayers.ToPawnLayerIndex(b.apparel);
                if (c > d) {
                    return -1;
                }
                else if (c < d) {
                    return 1;
                }
                else {
                    return 0;
                }
            });
            */

            var builder = new StringBuilder();
            var index = 0;
            foreach (var def in sortedDefs) {
                var label = def.label;
                string message = "EdB.PC.Panel.Appearance.ApparelConflict.Description".Translate();
                message = message.Replace("{0}", label);
                builder.Append(message);
                builder.AppendLine();
                foreach (var conflict in apparelConflicts.FindAll(c => { return c.def == def; })) {
                    builder.Append("EdB.PC.Panel.Appearance.ApparelConflict.LineItem".Translate()
                        .Replace("{0}", conflict.conflict.label));
                    builder.AppendLine();
                }

                if (++index < sortedDefs.Count) {
                    builder.AppendLine();
                }
            }

            apparelConflictText = builder.ToString();
        }
        else {
            apparelConflictText = null;
        }
    }

    public void ResetBackstories() {
        UpdateSkillLevelsForNewBackstoryOrTrait();
    }

    protected void SetHeadGraphicPathOnPawn(Pawn pawn, string value) {
        // Need to use reflection to set the private field.
        typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(pawn.story, value);
    }

    protected string FilterHeadPathForGender(string path) {
        if (pawn.gender == Gender.Male) {
            return path.Replace("Female", "Male");
        }

        return path.Replace("Male", "Female");
    }

    public void ClearTraits() {
        Pawn.story.traits.allTraits.Clear();
        ResetTraits();
    }

    public void AddTrait(Trait trait) {
        Pawn.story.traits.allTraits.Add(trait);
        ResetTraits();
    }

    public Trait GetTrait(int index) {
        return Pawn.story.traits.allTraits[index];
    }

    public void SetTrait(int index, Trait trait) {
        Pawn.story.traits.allTraits[index] = trait;
        ResetTraits();
    }

    public void RemoveTrait(Trait trait) {
        Pawn.story.traits.allTraits.Remove(trait);
        ResetTraits();
    }

    protected void ResetTraits() {
        ApplyInjuriesAndImplantsToPawn();
        UpdateSkillLevelsForNewBackstoryOrTrait();
    }

    public bool HasTrait(Trait trait) {
        return Pawn.story.traits.allTraits.Find(t => {
            if (t == null && trait == null) {
                return true;
            }

            if (trait == null || t == null) {
                return false;
            }

            if (trait.Label.Equals(t.Label)) {
                return true;
            }

            return false;
        }) != null;
    }

    protected void ResetGender() {
        var bodyTypes = PrepareCarefully.Instance.Providers.BodyTypes.GetBodyTypesForPawn(this);
        if (pawn.gender == Gender.Female) {
            if (HairDef.styleGender == StyleGender.Male) {
                HairDef = DefDatabase<HairDef>.AllDefsListForReading.Find(def => {
                    return def.styleGender != StyleGender.Male;
                });
            }

            if (BodyType == BodyTypeDefOf.Male) {
                if (bodyTypes.Contains(BodyTypeDefOf.Female)) {
                    BodyType = BodyTypeDefOf.Female;
                }
            }
        }
        else {
            if (HairDef.styleGender == StyleGender.Female) {
                HairDef = DefDatabase<HairDef>.AllDefsListForReading.Find(def => {
                    return def.styleGender != StyleGender.Female;
                });
            }

            if (BodyType == BodyTypeDefOf.Female) {
                if (bodyTypes.Contains(BodyTypeDefOf.Male)) {
                    BodyType = BodyTypeDefOf.Male;
                }
            }
        }
    }

    public string ResetCachedIncapableOf() {
        pawn.ClearCachedDisabledSkillRecords();
        var incapableList = new List<string>();
        var combinedDisabledWorkTags = pawn.story.DisabledWorkTagsBackstoryAndTraits;
        if (combinedDisabledWorkTags != WorkTags.None) {
            var list = CharacterCardUtility.WorkTagsFrom(combinedDisabledWorkTags);
            foreach (var tag in list) {
                incapableList.Add(tag.LabelTranslated().CapitalizeFirst());
            }

            if (incapableList.Count > 0) {
                incapable = string.Join(", ", incapableList.ToArray());
            }
        }
        else {
            incapable = null;
        }

        return incapable;
    }

    public bool IsApparelConflict() {
        return false;
    }

    protected void CopyApparelToPawn(Pawn pawn) {
        // Removes all apparel on the pawn and puts it in the thing cache for potential later re-use.
        List<Apparel> apparel = pawn.apparel.WornApparel;
        foreach (var a in apparel) {
            a.holdingOwner = null;
            thingCache.Put(a);
        }

        apparel.Clear();

        // Set each piece of apparel on the underlying Pawn from the CustomPawn.
        foreach (var layer in selectedApparel.Keys) {
            if (layer.Apparel) {
                AddApparelToPawn(pawn, layer);
            }
        }
    }

    public void AddApparelToPawn(Pawn targetPawn, PawnLayer layer) {
        if (acceptedApparel[layer] != null) {
            Apparel a;
            Color color;
            var madeFromStuff = acceptedApparel[layer].MadeFromStuff;

            if (madeFromStuff) {
                a = (Apparel)thingCache.Get(selectedApparel[layer], selectedStuff[layer]);
                color = colors[layer] * GetStuffColor(layer);
            }
            else {
                a = (Apparel)thingCache.Get(selectedApparel[layer]);
                color = colors[layer];
            }

            if (acceptedApparel[layer].HasComp(typeof(CompColorable))) {
                var colorable = a.TryGetComp<CompColorable>();
                if (colorable != null) {
                    var originalColor = Color.white;
                    if (madeFromStuff) {
                        originalColor = GetStuffColor(layer);
                    }

                    if (originalColor != colors[layer]) {
                        colorable.SetColor(colors[layer]);
                    }
                    else {
                        colorable.Disable();
                    }
                }
            }
            else {
                a.DrawColor = Color.white;
            }

            // This post-process will set the quality and damage on the apparel based on the 
            // pawn kind definition, so after we call it, we need to reset the quality and damage.
            PawnGenerator.PostProcessGeneratedGear(a, targetPawn);
            a.SetQuality(QualityCategory.Normal);
            a.HitPoints = a.MaxHitPoints;
            if (ApparelUtility.HasPartsToWear(targetPawn, a.def)) {
                targetPawn.apparel.Wear(a, false);
            }
        }
    }

    public void AddInjury(Injury injury) {
        injuries.Add(injury);
        bodyParts.Add(injury);
        ApplyInjuriesAndImplantsToPawn();
        InitializeInjuriesAndImplantsFromPawn(pawn);
    }

    public void UpdateImplants(List<Implant> implants) {
        var implantsToRemove = new List<Implant>();
        foreach (var bodyPart in bodyParts) {
            var asImplant = bodyPart as Implant;
            implantsToRemove.Add(asImplant);
        }

        foreach (var implant in implantsToRemove) {
            bodyParts.Remove(implant);
        }

        this.implants.Clear();
        foreach (var implant in implants) {
            bodyParts.Add(implant);
            this.implants.Add(implant);
        }

        ApplyInjuriesAndImplantsToPawn();
        InitializeInjuriesAndImplantsFromPawn(pawn);
    }

    protected void ApplyInjuriesAndImplantsToPawn() {
        pawn.health.Reset();
        var injuriesToRemove = new List<Injury>();
        foreach (var injury in injuries) {
            try {
                injury.AddToPawn(this, pawn);
            }
            catch (Exception e) {
                Logger.Warning(
                    "Failed to add injury {" + injury.Option?.HediffDef?.defName + "} to part {" +
                    injury.BodyPartRecord?.def?.defName + "}", e);
                injuriesToRemove.Add(injury);
            }
        }

        foreach (var injury in injuriesToRemove) {
            injuries.Remove(injury);
        }

        var implantsToRemove = new List<Implant>();
        foreach (var implant in implants) {
            try {
                implant.AddToPawn(this, pawn);
            }
            catch (Exception e) {
                Logger.Warning(
                    "Failed to add implant {" + implant.label + "} to part {" + implant.BodyPartRecord?.def?.defName +
                    "}", e);
                implantsToRemove.Add(implant);
            }
        }

        foreach (var implant in implantsToRemove) {
            implants.Remove(implant);
        }

        ClearPawnCaches();
        MarkPortraitAsDirty();
    }

    public void RemoveCustomBodyParts(CustomBodyPart part) {
        var implant = part as Implant;
        var injury = part as Injury;
        if (implant != null) {
            implants.Remove(implant);
        }

        if (injury != null) {
            injuries.Remove(injury);
        }

        bodyParts.Remove(part);
        ApplyInjuriesAndImplantsToPawn();
    }

    public void RemoveCustomBodyParts(BodyPartRecord part) {
        bodyParts.RemoveAll(p => {
            return part == p.BodyPartRecord;
        });
        implants.RemoveAll(i => {
            return part == i.BodyPartRecord;
        });
        ApplyInjuriesAndImplantsToPawn();
    }

    public void AddImplant(Implant implant) {
        if (implant != null && implant.BodyPartRecord != null) {
            implants.Add(implant);
            bodyParts.Add(implant);
            ApplyInjuriesAndImplantsToPawn();
            InitializeInjuriesAndImplantsFromPawn(pawn);
        }
        else {
            Logger.Warning("Discarding implant because of missing body part: " + implant.BodyPartRecord.def.defName);
        }
    }

    public void RemoveImplant(Implant implant) {
        implants.Remove(implant);
        bodyParts.Remove(implant);
        ApplyInjuriesAndImplantsToPawn();
    }

    public void RemoveImplants(IEnumerable<Implant> implants) {
        foreach (var implant in implants) {
            this.implants.Remove(implant);
            bodyParts.Remove(implant);
        }

        ApplyInjuriesAndImplantsToPawn();
    }

    public bool AtLeastOneImplantedPart(IEnumerable<BodyPartRecord> records) {
        foreach (var record in records) {
            if (IsImplantedPart(record)) {
                return true;
            }
        }

        return false;
    }

    public bool HasSameImplant(Implant implant) {
        return implants.FirstOrDefault(i => {
            return i.BodyPartRecord == implant.BodyPartRecord && i.Recipe == implant.Recipe;
        }) != null;
    }

    public bool HasSameImplant(BodyPartRecord part, RecipeDef def) {
        return implants.FirstOrDefault(i => {
            return i.BodyPartRecord == part && i.Recipe == def;
        }) != null;
    }

    public bool IsImplantedPart(BodyPartRecord record) {
        return FindImplant(record) != null;
    }

    public bool HasAtLeastOnePartBeenReplaced(IEnumerable<BodyPartRecord> records) {
        foreach (var record in records) {
            if (HasPartBeenReplaced(record)) {
                return true;
            }
        }

        return false;
    }

    public bool HasPartBeenReplaced(BodyPartRecord record) {
        var implant = FindImplant(record);
        if (implant == null) {
            return false;
        }

        return implant.ReplacesPart;
    }

    public Implant FindImplant(BodyPartRecord record) {
        if (implants.Count == 0) {
            return null;
        }

        return implants.FirstOrDefault(i => {
            return i.BodyPartRecord == record;
        });
    }

    public void ClearPawnCaches() {
        pawn.ClearCaches();
    }
}
