using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully;

public class AnimalDatabase {
    private readonly Dictionary<AnimalRecordKey, AnimalRecord> animalDictionary = new();
    private readonly List<AnimalRecord> animals = new();
    private readonly CostCalculator costCalculator = new();

    public AnimalDatabase() {
        Initialize();
    }

    public IEnumerable<AnimalRecord> AllAnimals => animals;

    public AnimalRecord FindAnimal(AnimalRecordKey key) {
        AnimalRecord result;
        if (animalDictionary.TryGetValue(key, out result)) {
            return result;
        }

        return null;
    }

    protected void Initialize() {
        foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(def => {
                     if (def.race != null && def.race.Animal) {
                         return true;
                     }

                     return false;
                 })) {
            if (def.race.hasGenders) {
                var femaleRecord = CreateAnimalRecord(def, Gender.Female);
                if (femaleRecord != null) {
                    AddAnimalRecord(femaleRecord);
                }

                var maleRecord = CreateAnimalRecord(def, Gender.Male);
                if (maleRecord != null) {
                    AddAnimalRecord(maleRecord);
                }
            }
            else {
                var record = CreateAnimalRecord(def, Gender.None);
                if (record != null) {
                    AddAnimalRecord(record);
                }
            }
        }
    }

    protected void AddAnimalRecord(AnimalRecord animal) {
        if (!animalDictionary.ContainsKey(animal.Key)) {
            animals.Add(animal);
            animalDictionary.Add(animal.Key, animal);
        }
    }

    protected AnimalRecord CreateAnimalRecord(ThingDef def, Gender gender) {
        var baseCost = costCalculator.GetBaseThingCost(def, null);
        if (baseCost == 0) {
            return null;
        }

        var result = new AnimalRecord();
        result.ThingDef = def;
        result.Gender = gender;
        result.Cost = baseCost;
        try {
            var pawn = CreatePawn(def, gender);
            if (pawn == null) {
                return null;
            }

            result.Thing = pawn;
        }
        catch (Exception e) {
            Logger.Warning("Failed to create a pawn for animal database record: " + def.defName, e);
            return null;
        }

        return result;
    }

    protected Pawn CreatePawn(ThingDef def, Gender gender) {
        var kindDef = (from td in DefDatabase<PawnKindDef>.AllDefs
            where td.race == def
            select td).FirstOrDefault();
        if (kindDef != null) {
            var pawn = PawnGenerator.GeneratePawn(kindDef);
            pawn.gender = gender;
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            return pawn;
        }

        return null;
    }
}
