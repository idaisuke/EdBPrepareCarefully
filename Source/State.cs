using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class State {
    private readonly List<string> errors = new();
    private readonly List<string> messages = new();

    public PagePrepareCarefully? Page { get; set; }

    public CustomPawn? CurrentPawn {
        get => PawnListMode == PawnListMode.ColonyPawnsMaximized ? CurrentColonyPawn : CurrentWorldPawn;
        set {
            if (PawnListMode == PawnListMode.ColonyPawnsMaximized) {
                CurrentColonyPawn = value;
            }
            else {
                CurrentWorldPawn = value;
            }
        }
    }

    public CustomPawn? CurrentColonyPawn { get; set; }

    public CustomPawn? CurrentWorldPawn { get; set; }

    public PawnKindDef? LastSelectedPawnKindDef { get; set; }

    public ITabView? CurrentTab { get; set; }

    public PawnListMode PawnListMode { get; set; } = PawnListMode.ColonyPawnsMaximized;

    public IEnumerable<string> Errors => errors;

    public List<string> MissingWorkTypes { get; set; } = new();

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
