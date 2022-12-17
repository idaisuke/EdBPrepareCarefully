using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderApparel {
    protected Dictionary<ThingDef, OptionsApparel> apparelLookup = new();
    protected OptionsApparel humanlikeApparel;
    protected OptionsApparel noApparel = new();

    protected OptionsApparel HumanlikeApparel {
        get {
            if (humanlikeApparel == null) {
                humanlikeApparel = InitializeHumanlikeApparel();
            }

            return humanlikeApparel;
        }
    }

    public List<ThingDef> GetApparel(CustomPawn pawn, PawnLayer layer) {
        return GetApparel(pawn.Pawn.def, layer);
    }

    public List<ThingDef> GetApparel(ThingDef raceDef, PawnLayer layer) {
        var apparel = GetApparelForRace(raceDef);
        return apparel.GetApparel(layer);
    }

    public OptionsApparel GetApparelForRace(CustomPawn pawn) {
        return GetApparelForRace(pawn.Pawn.def);
    }

    public OptionsApparel GetApparelForRace(ThingDef raceDef) {
        OptionsApparel apparel;
        if (apparelLookup.TryGetValue(raceDef, out apparel)) {
            return apparel;
        }

        apparel = InitializeApparel(raceDef);
        if (apparel == null) {
            if (raceDef != ThingDefOf.Human) {
                return GetApparelForRace(ThingDefOf.Human);
            }

            return null;
        }

        apparelLookup.Add(raceDef, apparel);
        return apparel;
    }

    protected PawnLayer LayerForApparel(ThingDef def) {
        if (def.apparel == null) {
            return null;
        }

        return PrepareCarefully.Instance.Providers.PawnLayers.FindLayerForApparel(def);
    }

    protected void AddApparelToOptions(OptionsApparel options, string defName) {
        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (def != null) {
            AddApparelToOptions(options, def);
        }
    }

    protected void AddApparelToOptions(OptionsApparel options, ThingDef def) {
        var layer = LayerForApparel(def);
        if (layer != null) {
            options.Add(layer, def);
        }
    }

    protected OptionsApparel InitializeApparel(ThingDef raceDef) {
        return HumanlikeApparel;
    }

    protected OptionsApparel InitializeHumanlikeApparel() {
        var result = new OptionsApparel();
        foreach (var apparelDef in DefDatabase<ThingDef>.AllDefs) {
            if (apparelDef.apparel == null) {
                continue;
            }

            AddApparelToOptions(result, apparelDef);
        }

        result.Sort();
        return result;
    }
}
