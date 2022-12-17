using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

public static class ExtensionsBackstory {
    private static readonly HashSet<string> ProblemBackstories = new() { "pirate king" };

    public static string CheckedDescriptionFor(this Backstory backstory, Pawn pawn) {
        if (ProblemBackstories.Contains(backstory.untranslatedTitle)) {
            return PartialDescriptionFor(backstory);
        }

        var description = backstory.FullDescriptionFor(pawn).Resolve();
        if (description.StartsWith("Could not resolve")) {
            return PartialDescriptionFor(backstory);
            //Logger.Debug("Failed to resolve description for backstory with this pawn: " + backstory.title + ", " + backstory.identifier);
        }

        return description;
    }

    // EVERY RELEASE:
    // This is a copy of Backstory.FullDescriptionFor() that only includes the disabled work types and the skill adjustments.
    // Every release, we should evaluate that method to make sure that the logic has not changed.
    public static string PartialDescriptionFor(this Backstory backstory) {
        var stringBuilder = new StringBuilder();
        var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
        for (var i = 0; i < allDefsListForReading.Count; i++) {
            var skillDef = allDefsListForReading[i];
            if (backstory.skillGainsResolved.ContainsKey(skillDef)) {
                stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " +
                                         backstory.skillGainsResolved[skillDef].ToString("+##;-##"));
            }
        }

        stringBuilder.AppendLine();
        foreach (var current in backstory.DisabledWorkTypes) {
            stringBuilder.AppendLine(current.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
        }

        foreach (var current2 in backstory.DisabledWorkGivers) {
            stringBuilder.AppendLine(current2.workType.gerundLabel.CapitalizeFirst() + ": " + current2.LabelCap + " " +
                                     "DisabledLower".Translate());
        }

        var str = stringBuilder.ToString().TrimEndNewlines();
        return str;
    }
}
