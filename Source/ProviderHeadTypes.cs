using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully;

public class ProviderHeadTypes {
    public static IEnumerable<HeadTypeDef> GetHeadTypes(Gender gender) {
        return DefDatabase<HeadTypeDef>.AllDefsListForReading.Where(it => it.gender == gender);
    }
}
