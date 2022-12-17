using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public class State {
    protected CustomPawn currentColonyPawn;
    protected CustomPawn currentWorldPawn;

    protected List<string> errors = new();
    protected List<string> messages = new();

    public Page_PrepareCarefully Page {
        get;
        set;
    }

    public List<CustomPawn> Pawns => PrepareCarefully.Instance.Pawns;

    public CustomPawn CurrentPawn {
        get => PawnListMode == PawnListMode.ColonyPawnsMaximized ? currentColonyPawn : currentWorldPawn;
        set {
            if (PawnListMode == PawnListMode.ColonyPawnsMaximized) {
                currentColonyPawn = value;
            }
            else {
                currentWorldPawn = value;
            }
        }
    }

    public CustomPawn CurrentColonyPawn {
        get => currentColonyPawn;
        set => currentColonyPawn = value;
    }

    public CustomPawn CurrentWorldPawn {
        get => currentWorldPawn;
        set => currentWorldPawn = value;
    }

    public List<CustomPawn> ColonyPawns => PrepareCarefully.Instance.ColonyPawns;

    public FactionDef LastSelectedFactionDef {
        get;
        set;
    }

    public PawnKindDef LastSelectedPawnKindDef {
        get;
        set;
    }

    public List<CustomPawn> WorldPawns => PrepareCarefully.Instance.WorldPawns;

    public ITabView CurrentTab {
        get;
        set;
    }

    public PawnListMode PawnListMode { get; set; } = PawnListMode.ColonyPawnsMaximized;

    public IEnumerable<string> Errors => errors;

    public List<string> MissingWorkTypes { get; set; }

    public IEnumerable<string> Messages => messages;

    public void AddError(string error) {
        errors.Add(error);
    }

    public void ClearErrors() {
        errors.Clear();
    }

    public void AddMessage(string message) {
        messages.Add(message);
    }

    public void ClearMessages() {
        messages.Clear();
    }
}
