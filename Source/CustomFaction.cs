using RimWorld;

namespace EdB.PrepareCarefully;

public class CustomFaction {
    public FactionDef Def {
        get;
        set;
    }

    public string Name { get; set; }

    public int? Index {
        get;
        set;
    }

    public Faction Faction {
        get;
        set;
    }

    public int SimilarFactionCount {
        get;
        set;
    }

    public bool Leader {
        get;
        set;
    }
}
