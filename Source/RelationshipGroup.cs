using System.Collections.Generic;

namespace EdB.PrepareCarefully;

internal class RelationshipGroup {
    public List<RelatedPawn> Children = new();
    public List<RelatedPawn> Parents = new();
}
