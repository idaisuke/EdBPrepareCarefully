using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderAgeLimits {
    private const int DefaultMinAge = 15;
    private const int DefaultMaxAge = 100;
    private readonly Dictionary<ThingDef, int> maxAgeLookup = new();
    private readonly Dictionary<ThingDef, int> minAgeLookup = new();

    private static SimpleCurve? DefaultAgeGenerationCurve {
        get {
            var field = ReflectionUtil.GetNonPublicStaticField(typeof(PawnGenerator), "DefaultAgeGenerationCurve");
            return field.GetValue(null) as SimpleCurve;
        }
    }

    public int MinAgeForPawn(Pawn pawn) {
        if (minAgeLookup.TryGetValue(pawn.def, out var age)) {
            return age;
        }

        var simpleCurve = pawn.def.race.ageGenerationCurve;
        if (simpleCurve == null) {
            Logger.Warning("No age generation curve defined for " + pawn.def.defName +
                           ". Using default age generation curve to determine minimum age.");
            simpleCurve = DefaultAgeGenerationCurve;
            if (simpleCurve == null) {
                Logger.Warning("Failed to get default age generation curve. Using default minimum age of " +
                               DefaultMinAge);
                age = DefaultMinAge;
            }
            else {
                age = Mathf.CeilToInt(pawn.def.race.lifeExpectancy * simpleCurve.First().x);
            }
        }
        else {
            var point = simpleCurve.First();
            age = (int)point.x;
        }

        minAgeLookup.Add(pawn.def, age);

        return age;
    }

    public int MaxAgeForPawn(Pawn pawn) {
        if (maxAgeLookup.TryGetValue(pawn.def, out var age)) {
            return age;
        }

        var simpleCurve = pawn.def.race.ageGenerationCurve;
        if (simpleCurve == null) {
            Logger.Warning("No age generation curve defined for " + pawn.def.defName +
                           ". Using default age generation curve to determine maximum age.");
            simpleCurve = DefaultAgeGenerationCurve;
            if (simpleCurve == null) {
                Logger.Warning("Failed to get default age generation curve. Using default maximum age of " +
                               DefaultMaxAge);
                age = DefaultMaxAge;
            }
            else {
                age = Mathf.CeilToInt(pawn.def.race.lifeExpectancy * simpleCurve.Last().x);
            }
        }
        else {
            var point = simpleCurve.Last();
            age = (int)(point.x * 1.2f);
        }

        maxAgeLookup.Add(pawn.def, age);

        return age;
    }
}
