using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public enum SortField {
    Name,
    Cost
}

public enum SortOrder {
    Ascending,
    Descending
}

public class PrepareCarefully {
    public static readonly Version MinimumGameVersion = new(1, 3, 3102);
    protected static PrepareCarefully instance;

    private readonly Dictionary<CustomPawn, Pawn> copiedPawnToOriginalPawnLookup = new();

    private readonly CostDetails cost = new();
    private readonly Dictionary<Pawn, CustomPawn> originalPawnToCopiedPawnLookup = new();
    private readonly Dictionary<CustomPawn, Pawn> pawnLookup = new();
    private readonly Dictionary<Pawn, CustomPawn> reversePawnLookup = new();
    protected bool active;
    protected AnimalDatabase animalDatabase = null;
    protected List<SelectedAnimal> animals = new();
    protected List<SelectedAnimal> animalsToRemove = new();

    protected List<Pawn> colonists = new();
    protected Configuration config = new();
    protected CostCalculator costCalculator;

    protected Dictionary<CustomPawn, Pawn> customPawnToOriginalPawnMap = new();

    protected List<EquipmentSelection> equipment = new();

    protected EquipmentDatabase equipmentDatabase;
    protected List<EquipmentSelection> equipmentToRemove = new();
    protected string filename = "";
    protected Dictionary<Pawn, CustomPawn> originalPawnToCustomPawnMap = new();

    protected List<CustomPawn> pawns = new();
    protected List<SelectedPet> pets = new();
    protected List<SelectedPet> petsToRemove = new();
    protected Randomizer randomizer = new();
    protected RelationshipManager relationshipManager;

    // Use this set to keep track of which scenario parts we're replacing with our custom ones
    public HashSet<ScenPart> ReplacedScenarioParts = new();
    protected State state = new();

    public PrepareCarefully() {
        NameSortOrder = SortOrder.Ascending;
        CostSortOrder = SortOrder.Ascending;
        SortField = SortField.Name;
    }

    public static PrepareCarefully Instance {
        get {
            if (instance == null) {
                instance = new PrepareCarefully();
            }

            return instance;
        }
    }

    public Configuration Config {
        get => config;
        set => config = value;
    }

    public State State => state;

    public static Scenario VanillaFriendlyScenario {
        get;
        set;
    }

    public Providers Providers {
        get;
        set;
    }

    public RelationshipManager RelationshipManager => relationshipManager;

    public SortField SortField { get; set; }
    public SortOrder NameSortOrder { get; set; }
    public SortOrder CostSortOrder { get; set; }
    public int StartingPoints { get; set; }
    public Page_ConfigureStartingPawns OriginalPage { get; set; }

    public int PointsRemaining => StartingPoints - (int)Cost.total;

    public bool Active {
        get => active;
        set => active = value;
    }

    public string Filename {
        get => filename;
        set => filename = value;
    }

    public List<CustomPawn> Pawns => pawns;

    public List<CustomPawn> ColonyPawns => pawns.FindAll(pawn => { return pawn.Type == CustomPawnType.Colonist; });

    public List<CustomPawn> WorldPawns => pawns.FindAll(pawn => { return pawn.Type == CustomPawnType.World; });

    public List<CustomPawn> HiddenPawns => pawns.FindAll(pawn => { return pawn.Type == CustomPawnType.Hidden; });

    public List<CustomPawn> TemporaryPawns => pawns.FindAll(pawn => { return pawn.Type == CustomPawnType.Temporary; });

    public EquipmentDatabase EquipmentDatabase {
        get {
            if (equipmentDatabase == null) {
                equipmentDatabase = new EquipmentDatabase();
            }

            return equipmentDatabase;
        }
    }

    public List<EquipmentSelection> Equipment {
        get {
            SyncEquipmentRemovals();
            return equipment;
        }
    }

    public List<SelectedAnimal> Animals {
        get {
            SyncAnimalRemovals();
            return animals;
        }
    }

    public List<SelectedPet> Pets {
        get {
            SyncPetRemovals();
            return pets;
        }
    }

    public CostDetails Cost {
        get {
            if (costCalculator == null) {
                costCalculator = new CostCalculator();
            }

            costCalculator.Calculate(cost, pawns, equipment, animals);
            return cost;
        }
    }

    public static void RemoveInstance() {
        instance = null;
    }

    public static void ClearVanillaFriendlyScenario() {
        VanillaFriendlyScenario = null;
    }

    // Performs the logic from the Page.DoNext() method in the base Page class instead of calling the override
    // in Page_ConfigureStartingPawns.  We want to prevent the missing required work type dialog from appearing
    // in the context of the configure pawns page.  We're adding our own version here.
    public void DoNextInBasePage() {
        if (OriginalPage != null) {
            var next = OriginalPage.next;
            var nextAction = OriginalPage.nextAct;
            if (next != null) {
                Verse.Find.WindowStack.Add(next);
            }

            if (nextAction != null) {
                nextAction();
            }

            TutorSystem.Notify_Event("PageClosed");
            TutorSystem.Notify_Event("GoToNextPage");
            OriginalPage.Close();
        }
    }

    public void Clear() {
        ClearVanillaFriendlyScenario();
        Active = false;
        Providers = new Providers();
        equipmentDatabase = new EquipmentDatabase();
        costCalculator = new CostCalculator();
        pawns.Clear();
        equipment.Clear();
        animals.Clear();
        pets.Clear();
    }

    public void Initialize() {
        Textures.Reset();
        Clear();
        InitializeProviders();
        PawnColorUtils.InitializeColors();
        InitializePawns();
        InitializeRelationshipManager(pawns);
        InitializeDefaultEquipment();
        StartingPoints = (int)Cost.total;
        state = new State();
    }

    protected void InitializeProviders() {
        // Initialize providers.  Note that the initialization order may matter as some providers depend on others.
        // TODO: For providers that do depend on other providers, consider adding constructor arguments for those
        // required providers so that they don't need to go back to this singleton to get the references.
        // If those dependencies get complicated, we might want to separate out the provider construction from
        // initialization.
        Providers.BodyTypes = new ProviderBodyTypes();
        Providers.HeadTypes = new ProviderHeadTypes();
        Providers.Hair = new ProviderHair();
        Providers.Apparel = new ProviderApparel();
        Providers.Health = new ProviderHealthOptions();
        Providers.Factions = new ProviderFactions();
        Providers.PawnLayers = new ProviderPawnLayers();
        Providers.AgeLimits = new ProviderAgeLimits();
        Providers.Backstories = new ProviderBackstories();
        Providers.Beards = new ProviderBeards();
        Providers.FaceTattoos = new ProviderFaceTattoos();
        Providers.BodyTattoos = new ProviderBodyTattoos();
        Providers.PawnKinds = new ProviderPawnKinds();
        Providers.Titles = new ProviderTitles();
    }

    // TODO:
    // Think about whether or not this is the best approach.  Might need to do a bug report for the vanilla game?
    // The tribal scenario adds a weapon with an invalid thing/stuff combination (jade knife).  The 
    // knife ThingDef should allow the jade material, but it does not.  We need this workaround to
    // add the normally disallowed equipment to our equipment database.
    protected EquipmentRecord AddNonStandardScenarioEquipmentEntry(EquipmentKey key) {
        var type = equipmentDatabase.ClassifyThingDef(key.ThingDef);
        return equipmentDatabase.AddThingDefWithStuff(key.ThingDef, key.StuffDef, type);
    }

    protected void InitializeDefaultEquipment() {
        var index = -1;
        ReplacedScenarioParts.Clear();

        // Go through all of the scenario steps that scatter resources near the player starting location and add
        // them to the resource/equipment list.
        foreach (var part in Verse.Find.Scenario.AllParts) {
            index++;

            if (part is ScenPart_ConfigPage_ConfigureStartingPawns) {
                ReplacedScenarioParts.Add(part);
            }

            if (part is ScenPart_ScatterThingsNearPlayerStart nearPlayerStart) {
                var thingDefField =
                    typeof(ScenPart_ScatterThingsNearPlayerStart).GetField("thingDef",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var stuffDefField =
                    typeof(ScenPart_ScatterThingsNearPlayerStart).GetField("stuff",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var countField =
                    typeof(ScenPart_ScatterThingsNearPlayerStart).GetField("count",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var thingDef = (ThingDef)thingDefField.GetValue(nearPlayerStart);
                var stuffDef = (ThingDef)stuffDefField.GetValue(nearPlayerStart);
                equipmentDatabase.PreloadDefinition(stuffDef);
                equipmentDatabase.PreloadDefinition(thingDef);
                var count = (int)countField.GetValue(nearPlayerStart);
                var key = new EquipmentKey(thingDef, stuffDef);
                var record = equipmentDatabase.LookupEquipmentRecord(key);
                if (record == null) {
                    Logger.Warning("Couldn't initialize all scenario equipment.  Didn't find an equipment entry for " +
                                   thingDef.defName);
                    record = AddNonStandardScenarioEquipmentEntry(key);
                }

                if (record != null) {
                    AddEquipment(record, count);
                    ReplacedScenarioParts.Add(part);
                }
            }

            // Go through all of the scenario steps that place starting equipment with the colonists and
            // add them to the resource/equipment list.
            if (part is ScenPart_StartingThing_Defined startingThing) {
                var thingDefField =
                    typeof(ScenPart_StartingThing_Defined).GetField("thingDef",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var stuffDefField =
                    typeof(ScenPart_StartingThing_Defined).GetField("stuff",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var countField =
                    typeof(ScenPart_StartingThing_Defined).GetField("count",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                var thingDef = (ThingDef)thingDefField.GetValue(startingThing);
                var stuffDef = (ThingDef)stuffDefField.GetValue(startingThing);
                equipmentDatabase.PreloadDefinition(stuffDef);
                equipmentDatabase.PreloadDefinition(thingDef);
                var count = (int)countField.GetValue(startingThing);
                var key = new EquipmentKey(thingDef, stuffDef);
                var entry = equipmentDatabase.LookupEquipmentRecord(key);
                if (entry == null) {
                    entry = AddNonStandardScenarioEquipmentEntry(key);
                }

                if (entry != null) {
                    AddEquipment(entry, count);
                    ReplacedScenarioParts.Add(part);
                }
                else {
                    Logger.Warning(String.Format(
                        "Couldn't initialize all scenario equipment.  Didn't find an equipment entry for {0} ({1})",
                        thingDef.defName, stuffDef != null ? stuffDef.defName : "no material"));
                }
            }

            // Go through all of the scenario steps that spawn a pet and add the pet to the equipment/resource
            // list.
            if (part is ScenPart_StartingAnimal animal) {
                var animalCountField =
                    typeof(ScenPart_StartingAnimal).GetField("count", BindingFlags.Instance | BindingFlags.NonPublic);
                var count = (int)animalCountField.GetValue(animal);
                for (var i = 0; i < count; i++) {
                    var animalKindDef = RandomPet(animal);
                    if (animalKindDef == null) {
                        Logger.Warning("Could not add random pet");
                        continue;
                    }

                    equipmentDatabase.PreloadDefinition(animalKindDef.race);

                    var entries = Instance.EquipmentDatabase.Animals.FindAll(e => {
                        return e.def == animalKindDef.race;
                    });
                    EquipmentRecord entry = null;
                    if (entries.Count > 0) {
                        entry = entries.RandomElement();
                    }

                    if (entry != null) {
                        AddEquipment(entry);
                        ReplacedScenarioParts.Add(part);
                    }
                    else {
                        Logger.Warning("Failed to add the expected scenario animal to list of selected equipment");
                    }
                }
            }
        }

        //index = 0;
        //foreach (ScenPart part in Verse.Find.Scenario.AllParts) {
        //    Logger.Debug(String.Format("[{0}] Replaced? {1}: {2} {3}", index, ReplacedScenarioParts.Contains(part), part.Label, String.Join(", ", part.GetSummaryListEntries("PlayerStartsWith"))));
        //    index++;
        //}
    }

    private static PawnKindDef RandomPet(ScenPart_StartingAnimal startingAnimal) {
        var animalKindDef = QuietReflectionUtil.GetFieldValue<PawnKindDef>(startingAnimal, "animalKind");
        if (animalKindDef != null) {
            return animalKindDef;
        }

        if (animalKindDef == null) {
            var animalKindDefs = Reflection.ScenPart_StartingAnimal.RandomPets(startingAnimal);
            if (animalKindDefs != null) {
                var enumerator = animalKindDefs.GetEnumerator();
                if (enumerator != null) {
                    var validAnimalKindDefs = new List<PawnKindDef>();
                    try {
                        while (enumerator.MoveNext()) {
                            var kindDef = enumerator.Current;
                            if (kindDef != null) {
                                validAnimalKindDefs.Add(kindDef);
                            }
                        }
                    }
                    catch (Exception) {
                        Logger.Error(
                            "There was an error while selecting a random pet.  We could not select from the full range of available animals.  " +
                            "This may be caused by a bad definition in another mod, or you may be missing a mod that's required by another mod.");
                    }

                    if (validAnimalKindDefs.Count > 0) {
                        animalKindDef = validAnimalKindDefs.RandomElementByWeight(td => td.RaceProps.petness);
                    }
                }
            }
        }

        return animalKindDef;
    }

    public void ClearPawns() {
        pawns.Clear();
    }

    public void AddPawn(CustomPawn customPawn) {
        PreloadPawnEquipment(customPawn.Pawn);
        pawns.Add(customPawn);
    }

    protected void PreloadPawnEquipment(Pawn pawn) {
        if (pawn.equipment != null) {
            foreach (var e in pawn.equipment.AllEquipmentListForReading) {
                if (e.Stuff != null) {
                    equipmentDatabase.PreloadDefinition(e.Stuff);
                }

                equipmentDatabase.PreloadDefinition(e.def);
            }
        }

        if (pawn.apparel != null) {
            foreach (var e in pawn.apparel.WornApparel) {
                if (e.Stuff != null) {
                    equipmentDatabase.PreloadDefinition(e.Stuff);
                }

                equipmentDatabase.PreloadDefinition(e.def);
            }
        }
    }

    public void RemovePawn(CustomPawn customPawn) {
        pawns.Remove(customPawn);
    }

    public Pawn FindPawn(CustomPawn pawn) {
        Pawn result;
        if (pawnLookup.TryGetValue(pawn, out result)) {
            return result;
        }

        return null;
    }

    public CustomPawn FindCustomPawn(Pawn pawn) {
        CustomPawn result;
        if (reversePawnLookup.TryGetValue(pawn, out result)) {
            return result;
        }

        return null;
    }

    public bool AddEquipment(EquipmentRecord entry) {
        if (entry == null) {
            return false;
        }

        SyncEquipmentRemovals();
        var e = Find(entry);
        if (e == null) {
            equipment.Add(new EquipmentSelection(entry));
            return true;
        }

        e.Count += entry.stackSize;
        return false;
    }

    public bool AddEquipment(EquipmentRecord record, int count) {
        if (record == null) {
            return false;
        }

        SyncEquipmentRemovals();
        var e = Find(record);
        if (e == null) {
            equipment.Add(new EquipmentSelection(record, count));
            return true;
        }

        e.Count += count;
        return false;
    }

    public void RemoveEquipment(EquipmentSelection equipment) {
        equipmentToRemove.Add(equipment);
    }

    public void RemoveEquipment(EquipmentRecord entry) {
        var e = Find(entry);
        if (e != null) {
            equipmentToRemove.Add(e);
        }
    }

    protected void SyncEquipmentRemovals() {
        if (equipmentToRemove.Count > 0) {
            foreach (var e in equipmentToRemove) {
                equipment.Remove(e);
            }

            equipmentToRemove.Clear();
        }
    }

    public EquipmentSelection Find(EquipmentRecord entry) {
        return equipment.Find(e => {
            return e.Record == entry;
        });
    }

    public SelectedAnimal FindSelectedAnimal(AnimalRecordKey key) {
        return Animals.FirstOrDefault(animal => {
            return Equals(animal.Key, key);
        });
    }

    public void AddAnimal(AnimalRecord record) {
        var existingAnimal = FindSelectedAnimal(record.Key);
        if (existingAnimal != null) {
            existingAnimal.Count++;
        }
        else {
            var animal = new SelectedAnimal();
            animal.Record = record;
            animal.Count = 1;
            animals.Add(animal);
        }
    }

    public void RemoveAnimal(SelectedAnimal animal) {
        animalsToRemove.Add(animal);
    }

    protected void SyncAnimalRemovals() {
        if (animalsToRemove.Count > 0) {
            foreach (var a in animalsToRemove) {
                animals.Remove(a);
            }

            animalsToRemove.Clear();
        }
    }

    public SelectedPet FindSelectedPet(string id) {
        return Pets.FirstOrDefault(pet => {
            return Equals(pet.Id, id);
        });
    }

    public void AddPet(SelectedPet pet) {
        Pets.Add(pet);
    }

    public void RemovePet(SelectedPet pet) {
        petsToRemove.Add(pet);
    }

    protected void SyncPetRemovals() {
        if (petsToRemove.Count > 0) {
            foreach (var p in petsToRemove) {
                pets.Remove(p);
            }

            petsToRemove.Clear();
        }
    }

    public Dictionary<string, IExposable> PopulateCrossReferencesForInitialPawnCopying() {
        var crossRefs = new Dictionary<string, IExposable>();
        foreach (var i in Verse.Find.World.ideoManager.IdeosListForReading) {
            crossRefs.Add(i.GetUniqueLoadID(), i);
        }

        foreach (var i in Verse.Find.World.factionManager.AllFactions) {
            crossRefs.Add(i.GetUniqueLoadID(), i);
        }

        return crossRefs;
    }

    public void InitializePawns() {
        //Verse.Find.World.worldPawns.LogWorldPawns();
        var payload = new PawnPayload {
            pawns = Verse.Find.GameInitData.startingAndOptionalPawns.ConvertAll(o => o),
            worldPawns = Verse.Find.World.worldPawns.AllPawnsAliveOrDead.FindAll(o => !o.DestroyedOrNull())
        };
        var copiedPayload = UtilityCopy.CopyExposable(payload, PopulateCrossReferencesForInitialPawnCopying());

        var startingPawnCount = Verse.Find.GameInitData.startingPawnCount;
        var pawnCount = Verse.Find.GameInitData.startingAndOptionalPawns.Count;
        for (var i = 0; i < pawnCount; i++) {
            var copiedPawn = copiedPayload.pawns[i];
            var customPawn = new CustomPawn(copiedPawn) {
                Type = i < startingPawnCount ? CustomPawnType.Colonist : CustomPawnType.World
            };
            AddPawn(customPawn);
        }

        copiedPawnToOriginalPawnLookup.Clear();
        originalPawnToCopiedPawnLookup.Clear();
        var index = -1;
        foreach (var hiddenPawn in copiedPayload.worldPawns) {
            index++;
            var customPawn = new CustomPawn(hiddenPawn) { Type = CustomPawnType.Hidden };
            AddPawn(customPawn);
            copiedPawnToOriginalPawnLookup.Add(customPawn, payload.worldPawns[index]);
            originalPawnToCopiedPawnLookup.Add(payload.worldPawns[index], customPawn);
        }
    }

    public Pawn FindOriginalPawnFromCopy(CustomPawn customPawn) {
        if (copiedPawnToOriginalPawnLookup.ContainsKey(customPawn)) {
            return copiedPawnToOriginalPawnLookup[customPawn];
        }

        return null;
    }

    public CustomPawn FindCopiedPawnFromOriginal(Pawn pawn) {
        if (originalPawnToCopiedPawnLookup.ContainsKey(pawn)) {
            return originalPawnToCopiedPawnLookup[pawn];
        }

        return null;
    }

    public void InitializeRelationshipManager(List<CustomPawn> pawns) {
        relationshipManager = new RelationshipManager();
        relationshipManager.InitializeWithPawns(pawns);
    }

    public class PawnPayload : IExposable {
        public List<Pawn> pawns = new();
        public List<Pawn> worldPawns = new();

        void IExposable.ExposeData() {
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep, null);
            Scribe_Collections.Look(ref worldPawns, "worldPawns", LookMode.Deep, null);
        }
    }
}
