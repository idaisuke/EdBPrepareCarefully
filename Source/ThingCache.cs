using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class ThingCache {
    protected Dictionary<ThingDef, LinkedList<Thing>> cache = new();

    public Thing Get(ThingDef thingDef) {
        return Get(thingDef, null);
    }

    public Thing Get(ThingDef thingDef, ThingDef stuffDef) {
        if (thingDef.MadeFromStuff && stuffDef == null) {
            stuffDef = GenStuff.DefaultStuffFor(thingDef);
        }

        LinkedList<Thing> cachedThings;
        if (cache.TryGetValue(thingDef, out cachedThings)) {
            var thingNode = cachedThings.Last;
            if (thingNode != null) {
                cachedThings.Remove(thingNode);
                var result = thingNode.Value;
                result.SetStuffDirect(stuffDef);
                return result;
            }
        }

        return ThingMaker.MakeThing(thingDef, stuffDef);
    }

    public void Put(Thing thing) {
        var def = thing.def;
        LinkedList<Thing> cachedThings = null;
        if (!cache.TryGetValue(def, out cachedThings)) {
            cachedThings = new LinkedList<Thing>();
            cache.Add(def, cachedThings);
        }

        thing.SetQuality(QualityCategory.Normal);
        thing.HitPoints = thing.MaxHitPoints;
        cachedThings.AddLast(thing);
    }
}
