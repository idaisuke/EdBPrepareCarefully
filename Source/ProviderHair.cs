using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderHair {
    protected Dictionary<ThingDef, OptionsHair> hairLookup = new();
    protected OptionsHair humanlikeHairs;
    protected OptionsHair noHair = new();

    protected OptionsHair HumanlikeHairs {
        get {
            if (humanlikeHairs == null) {
                humanlikeHairs = InitializeHumanlikeHairs();
            }

            return humanlikeHairs;
        }
    }

    public List<HairDef> GetHairs(CustomPawn pawn) {
        return GetHairs(pawn.Pawn.def, pawn.Gender);
    }

    public List<HairDef> GetHairs(ThingDef raceDef, Gender gender) {
        var hairs = GetHairsForRace(raceDef);
        return hairs.GetHairs(gender);
    }

    public OptionsHair GetHairsForRace(CustomPawn pawn) {
        return GetHairsForRace(pawn.Pawn.def);
    }

    public OptionsHair GetHairsForRace(ThingDef raceDef) {
        OptionsHair hairs;
        if (hairLookup.TryGetValue(raceDef, out hairs)) {
            return hairs;
        }

        hairs = InitializeHairs(raceDef);
        if (hairs == null) {
            if (raceDef != ThingDefOf.Human) {
                return GetHairsForRace(ThingDefOf.Human);
            }

            return null;
        }

        hairLookup.Add(raceDef, hairs);
        return hairs;
    }

    protected OptionsHair InitializeHairs(ThingDef raceDef) {
        return HumanlikeHairs;
    }

    protected OptionsHair InitializeHumanlikeHairs() {
        var nonHumanHairTags = new HashSet<string>();
        // This was meant to remove alien race-specific hair defs from those available when customizing non-aliens.
        // However, there's no way to distinguish between hair tags that are ONLY for aliens vs. the non-alien
        // hair defs that are also allowed for aliens.  This makes the logic below fail.  Instead, we'll include
        // all hair def (both alien and non-alien) in the list of available hairs for non-aliens.
        // TODO: Implement filtering in the hair selection to make it easier to find appropriate hairs when there
        // are a lot of mods that add hairs.
        /*
        IEnumerable<ThingDef> alienRaces = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => {
            return def.race != null && ProviderAlienRaces.IsAlienRace(def);
        });
        foreach (var alienRaceDef in alienRaces) {
            AlienRace alienRace = AlienRaceProvider.GetAlienRace(alienRaceDef);
            if (alienRace == null) {
                continue;
            }
            if (alienRace.HairTags != null) {
                foreach (var tag in alienRace.HairTags) {
                    nonHumanHairTags.Add(tag);
                }
            }
        }
        */
        var result = new OptionsHair();
        foreach (var hairDef in DefDatabase<HairDef>.AllDefs.Where(def => {
                     foreach (var tag in def.styleTags) {
                         if (nonHumanHairTags.Contains(tag)) {
                             return false;
                         }
                     }

                     return true;
                 })) {
            result.AddHair(hairDef);
        }

        result.Sort();

        // Set up default hair colors
        result.Colors.Add(new Color(0.2f, 0.2f, 0.2f));
        result.Colors.Add(new Color(0.31f, 0.28f, 0.26f));
        result.Colors.Add(new Color(0.25f, 0.2f, 0.15f));
        result.Colors.Add(new Color(0.3f, 0.2f, 0.1f));
        result.Colors.Add(new Color(0.3529412f, 0.227451f, 0.1254902f));
        result.Colors.Add(new Color(0.5176471f, 0.3254902f, 0.1843137f));
        result.Colors.Add(new Color(0.7568628f, 0.572549f, 0.3333333f));
        result.Colors.Add(new Color(0.9294118f, 0.7921569f, 0.6117647f));

        return result;
    }
}
