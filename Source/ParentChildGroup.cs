using System.Collections.Generic;
using System.Linq;

namespace EdB.PrepareCarefully;

public class ParentChildGroup {
    public List<CustomPawn> Parents { get; set; } = new();

    public List<CustomPawn> Children { get; set; } = new();

    public override string ToString() {
        var result = " CustomParentChildGroup { parents = ";
        if (Parents == null) {
            result += "null";
        }
        else {
            result += "[" + string.Join(", ",
                Parents.Select(pawn => { return pawn == null ? "null" : pawn.ToString(); }).ToArray()) + "]";
        }

        result += ", " + (Children != null ? Children.Count.ToString() : "0") + " children = ";
        if (Children == null) {
            result += "null";
        }
        else {
            result += "[" + string.Join(", ",
                Children.Select(pawn => { return pawn == null ? "null" : pawn.ToString(); }).ToArray()) + "]";
        }

        result += " }";
        return result;
    }
}
