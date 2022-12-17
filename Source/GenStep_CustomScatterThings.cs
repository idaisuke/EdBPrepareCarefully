using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

// Duplicate of GenStep_ScatterThings with a radius field to allow for a large spawn area.
public class GenStep_CustomScatterThings : GenStep_Scatterer {
    private const int ClusterRadius = 4;

    //
    // Static Fields
    //
    private static readonly List<Rot4> tmpRotations = new();

    public int clearSpaceSize;

    [Unsaved] private IntVec3 clusterCenter;

    public int clusterSize = 1;

    [Unsaved] private int leftInCluster;

    private List<Rot4> possibleRotationsInt;

    // EdB: New radius field
    public int radius = 4;

    public ThingDef stuff;

    [NoTranslate] private List<string> terrainValidationDisallowed;

    public float terrainValidationRadius;

    //
    // Fields
    //
    public ThingDef thingDef;

    //
    // Properties
    //
    private List<Rot4> PossibleRotations {
        get {
            if (possibleRotationsInt == null) {
                possibleRotationsInt = new List<Rot4>();
                if (thingDef.rotatable) {
                    possibleRotationsInt.Add(Rot4.North);
                    possibleRotationsInt.Add(Rot4.East);
                    possibleRotationsInt.Add(Rot4.South);
                    possibleRotationsInt.Add(Rot4.West);
                }
                else {
                    possibleRotationsInt.Add(Rot4.North);
                }
            }

            return possibleRotationsInt;
        }
    }

    public override int SeedPart => 1158116095;

    //
    // Static Methods
    //
    public static List<int> CountDividedIntoStacks(int count, IntRange stackSizeRange) {
        var list = new List<int>();
        while (count > 0) {
            var num = Mathf.Min(count, stackSizeRange.RandomInRange);
            count -= num;
            list.Add(num);
        }

        if (stackSizeRange.max > 2) {
            for (var i = 0; i < list.Count * 4; i++) {
                var num2 = Rand.RangeInclusive(0, list.Count - 1);
                var num3 = Rand.RangeInclusive(0, list.Count - 1);
                if (num2 != num3 && list[num2] > list[num3]) {
                    var num4 = (int)((list[num2] - list[num3]) * Rand.Value);
                    var list2 = list;
                    var index = num2;
                    list2[index] -= num4;
                    list2 = list;
                    index = num3;
                    list2[index] += num4;
                }
            }
        }

        return list;
    }

    //
    // Methods
    //
    protected override bool CanScatterAt(IntVec3 loc, Map map) {
        if (!base.CanScatterAt(loc, map)) {
            return false;
        }

        Rot4 rot;
        if (!TryGetRandomValidRotation(loc, map, out rot)) {
            return false;
        }

        if (terrainValidationRadius > 0f) {
            foreach (var current in GenRadial.RadialCellsAround(loc, terrainValidationRadius, true)) {
                if (current.InBounds(map)) {
                    var terrain = current.GetTerrain(map);
                    for (var i = 0; i < terrainValidationDisallowed.Count; i++) {
                        if (terrain.HasTag(terrainValidationDisallowed[i])) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        return true;
    }

    public override void Generate(Map map, GenStepParams parms) {
        if (!allowInWaterBiome && map.TileInfo.WaterCovered) {
            return;
        }

        var arg_AA_0 = CalculateFinalCount(map);
        IntRange one;
        if (thingDef.ingestible != null && thingDef.ingestible.IsMeal && thingDef.stackLimit <= 10) {
            one = IntRange.one;
        }
        else if (thingDef.stackLimit > 5) {
            one = new IntRange(Mathf.RoundToInt(thingDef.stackLimit * 0.5f), thingDef.stackLimit);
        }
        else {
            one = new IntRange(thingDef.stackLimit, thingDef.stackLimit);
        }

        var list = GenStep_ScatterThings.CountDividedIntoStacks(arg_AA_0, one);
        for (var i = 0; i < list.Count; i++) {
            IntVec3 intVec;
            if (!TryFindScatterCell(map, out intVec)) {
                return;
            }

            ScatterAt(intVec, map, parms, list[i]);
            usedSpots.Add(intVec);
        }

        usedSpots.Clear();
        clusterCenter = IntVec3.Invalid;
        leftInCluster = 0;
    }

    private bool IsRotationValid(IntVec3 loc, Rot4 rot, Map map) {
        return GenAdj.OccupiedRect(loc, rot, thingDef.size).InBounds(map) && !GenSpawn.WouldWipeAnythingWith(loc, rot,
            thingDef, map,
            x => x.def == thingDef || (x.def.category != ThingCategory.Plant && x.def.category != ThingCategory.Filth));
    }

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1) {
        Rot4 rot;
        if (!TryGetRandomValidRotation(loc, map, out rot)) {
            Logger.Warning("Could not find any valid rotation for " + thingDef);
            return;
        }

        if (clearSpaceSize > 0) {
            using (var enumerator = GridShapeMaker.IrregularLump(loc, map, clearSpaceSize).GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var edifice = enumerator.Current.GetEdifice(map);
                    if (edifice != null) {
                        edifice.Destroy();
                    }
                }
            }
        }

        var thing = ThingMaker.MakeThing(thingDef, stuff);
        if (thingDef.Minifiable) {
            thing = thing.MakeMinified();
        }

        if (thing.def.category == ThingCategory.Item) {
            thing.stackCount = stackCount;
            thing.SetForbidden(true, false);
            Thing thing2;
            GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near, out thing2);
            if (nearPlayerStart && thing2 != null && thing2.def.category == ThingCategory.Item &&
                TutorSystem.TutorialMode) {
                Find.TutorialState.AddStartingItem(thing2);
            }
        }
        else {
            GenSpawn.Spawn(thing, loc, map, rot);
        }
    }

    protected override bool TryFindScatterCell(Map map, out IntVec3 result) {
        if (clusterSize > 1) {
            if (leftInCluster <= 0) {
                if (!base.TryFindScatterCell(map, out clusterCenter)) {
                    Log.Error("Could not find cluster center to scatter " + thingDef);
                }

                leftInCluster = clusterSize;
            }

            leftInCluster--;
            // EdB: Replaced the hard-coded value of 4 with the new radius field
            //result = CellFinder.RandomClosewalkCellNear(this.clusterCenter, map, 4, delegate (IntVec3 x) {
            result = CellFinder.RandomClosewalkCellNear(clusterCenter, map, radius, delegate(IntVec3 x) {
                Rot4 rot;
                return TryGetRandomValidRotation(x, map, out rot);
            });
            return result.IsValid;
        }

        return base.TryFindScatterCell(map, out result);
    }

    private bool TryGetRandomValidRotation(IntVec3 loc, Map map, out Rot4 rot) {
        var possibleRotations = PossibleRotations;
        for (var i = 0; i < possibleRotations.Count; i++) {
            if (IsRotationValid(loc, possibleRotations[i], map)) {
                // EdB: Changed class name to match
                //GenStep_ScatterThings.tmpRotations.Add(possibleRotations[i]);
                tmpRotations.Add(possibleRotations[i]);
            }
        }

        // EdB: Changed class name to match
        //if (GenStep_ScatterThings.tmpRotations.TryRandomElement(out rot)) {
        if (tmpRotations.TryRandomElement(out rot)) {
            // EdB: Changed class name to match
            //GenStep_ScatterThings.tmpRotations.Clear();
            tmpRotations.Clear();
            return true;
        }

        rot = Rot4.Invalid;
        return false;
    }
}
