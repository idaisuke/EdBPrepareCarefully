using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordPresetV5 : IExposable {
    public static readonly int VERSION = 5;
    public List<SaveRecordEquipmentV3> equipment = new();
    public string mods;
    public List<SaveRecordParentChildGroupV3> parentChildGroups = new();
    public List<SaveRecordPawnV5> pawns = new();
    public List<SaveRecordRelationshipV3> relationships = new();
    public int version = VERSION;

    public void ExposeData() {
        Scribe_Values.Look(ref version, "version", VERSION, true);
        Scribe_Values.Look<string>(ref mods, "mods", "", true);
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep, null);
        Scribe_Collections.Look(ref parentChildGroups, "parentChildGroups", LookMode.Deep, null);
        Scribe_Collections.Look(ref relationships, "relationships", LookMode.Deep, null);
        Scribe_Collections.Look(ref equipment, "equipment", LookMode.Deep, null);
    }
}
