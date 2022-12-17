using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public static class ExtensionsThing {
    public static Color GetColor(this Thing thing) {
        var thingWithComps = thing as ThingWithComps;
        if (thingWithComps == null) {
            return thing.DrawColor;
        }

        var comp = thingWithComps.GetComp<CompColorable>();
        if (comp == null) {
            return thing.DrawColor;
        }

        return comp.Color;
    }

    public static QualityCategory GetQuality(this Thing thing) {
        var minifiedThing = thing as MinifiedThing;
        var compQuality = minifiedThing == null
            ? thing.TryGetComp<CompQuality>()
            : minifiedThing.InnerThing.TryGetComp<CompQuality>();
        if (compQuality == null) {
            return QualityCategory.Normal;
        }

        return compQuality.Quality;
    }

    public static void SetQuality(this Thing thing, QualityCategory quality) {
        var minifiedThing = thing as MinifiedThing;
        var compQuality = minifiedThing == null
            ? thing.TryGetComp<CompQuality>()
            : minifiedThing.InnerThing.TryGetComp<CompQuality>();
        if (compQuality != null) {
            typeof(CompQuality).GetField("qualityInt", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(compQuality, quality);
        }
    }
}
