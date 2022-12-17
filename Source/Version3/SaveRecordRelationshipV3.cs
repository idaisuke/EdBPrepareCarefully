using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordRelationshipV3 : IExposable {
    public string relation;
    public string source;
    public string target;

    public SaveRecordRelationshipV3() {
    }

    public SaveRecordRelationshipV3(CustomRelationship relationship) {
        source = relationship.source.Id;
        target = relationship.target.Id;
        relation = relationship.def.defName;
    }

    public void ExposeData() {
        Scribe_Values.Look<string>(ref source, "source", null, true);
        Scribe_Values.Look<string>(ref target, "target", null, true);
        Scribe_Values.Look<string>(ref relation, "relation", null, true);
    }

    public override string ToString() {
        return "SaveRecordRelationshipV3: { source = " + source + ", target = " + target + ", relationship = " +
               relation + "}";
    }
}
