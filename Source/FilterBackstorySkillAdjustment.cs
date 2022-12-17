using RimWorld;
using Verse;

namespace EdB.PrepareCarefully;

internal class FilterBackstorySkillAdjustment : Filter<BackstoryDef> {
    public FilterBackstorySkillAdjustment(SkillDef skillDef, int bonusOrPenalty) {
        BonusOrPenalty = bonusOrPenalty;
        SkillDef = skillDef;
        if (BonusOrPenalty < 0) {
            LabelShort = "EdB.PC.Dialog.Backstory.Filter.SkillPenalty".Translate(SkillDef.LabelCap, bonusOrPenalty);
            LabelFull = "EdB.PC.Dialog.Backstory.Filter.SkillPenaltyFull".Translate(SkillDef.LabelCap, bonusOrPenalty);
        }
        else {
            LabelShort = "EdB.PC.Dialog.Backstory.Filter.SkillBonus".Translate(SkillDef.LabelCap, bonusOrPenalty);
            LabelFull = "EdB.PC.Dialog.Backstory.Filter.SkillBonusFull".Translate(SkillDef.LabelCap, bonusOrPenalty);
        }

        FilterFunction = backstory => {
            if (SkillDef != null && backstory.skillGains.ContainsKey(SkillDef)) {
                var value = backstory.skillGains[skillDef];
                if (bonusOrPenalty > 0) {
                    return value >= bonusOrPenalty;
                }

                return value <= bonusOrPenalty;
            }

            return false;
        };
    }

    private int BonusOrPenalty {
        get;
    }

    public SkillDef SkillDef {
        get;
        set;
    }

    public override bool ConflictsWith(Filter<BackstoryDef> filter) {
        if (filter as FilterBackstorySkillAdjustment == null) {
            return false;
        }

        var f = (FilterBackstorySkillAdjustment)filter;
        if (f.SkillDef == SkillDef) {
            if (f.BonusOrPenalty > 0 && BonusOrPenalty > 0) {
                return true;
            }

            if (f.BonusOrPenalty < 0 && f.BonusOrPenalty < 0) {
                return true;
            }
        }

        return false;
    }
}
