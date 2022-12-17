using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully; 

public class SaveRecordPresetV4 : IExposable {
    public List<SaveRecordEquipmentV3> equipment = new();
    public string mods;
    public List<SaveRecordParentChildGroupV3> parentChildGroups = new();
    public List<SaveRecordPawnV4> pawns = new();
    public List<SaveRecordRelationshipV3> relationships = new();
    public int version = 4;

    public void ExposeData() {
        Scribe_Values.Look(ref version, "version", 4, true);
        Scribe_Values.Look<string>(ref mods, "mods", "", true);
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Deep, null);
        Scribe_Collections.Look(ref parentChildGroups, "parentChildGroups", LookMode.Deep, null);
        Scribe_Collections.Look(ref relationships, "relationships", LookMode.Deep, null);
        Scribe_Collections.Look(ref equipment, "equipment", LookMode.Deep, null);
    }
}
