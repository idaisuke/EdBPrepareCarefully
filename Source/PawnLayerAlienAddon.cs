using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully; 

public class PawnLayerAlienAddon : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public bool Hair {
        get;
        set;
    }

    public bool Skin {
        get;
        set;
    }

    public override ColorSelectorType ColorSelectorType { get; set; } = ColorSelectorType.None;

    public override List<Color> ColorSwatches { get; set; }

    public AlienRaceBodyAddon AlienAddon {
        get;
        set;
    }

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        var addonOption = option as PawnLayerOptionAlienAddon;
        if (addonOption == null) {
            return false;
        }

        if (pawn.AlienRace != null) {
            var alienComp = pawn.Pawn.AllComps.FirstOrDefault(comp => {
                return comp.GetType().Name == "AlienComp";
            });
            if (alienComp == null) {
                return false;
            }

            var variantsField = ReflectionUtil.GetPublicField(alienComp, "addonVariants");
            if (variantsField == null) {
                return false;
            }

            List<int> variants = null;
            try {
                variants = (List<int>)variantsField.GetValue(alienComp);
            }
            catch (Exception) {
                return false;
            }

            var selectedIndex = variants[AlienAddon.VariantIndex];
            return selectedIndex == addonOption.Index;
        }

        return false;
    }

    private int? GetSelectedVariant(CustomPawn pawn, int variantIndex) {
        if (pawn.AlienRace == null) {
            return null;
        }

        var alienComp = pawn.Pawn.AllComps.FirstOrDefault(comp => {
            return comp.GetType().Name == "AlienComp";
        });
        if (alienComp == null) {
            return null;
        }

        var variantsField = ReflectionUtil.GetPublicField(alienComp, "addonVariants");
        if (variantsField == null) {
            return null;
        }

        List<int> variants = null;
        try {
            variants = (List<int>)variantsField.GetValue(alienComp);
        }
        catch (Exception) {
            return null;
        }

        return variants[variantIndex];
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        if (pawn.AlienRace == null) {
            return null;
        }

        var alienComp = pawn.Pawn.AllComps.FirstOrDefault(comp => {
            return comp.GetType().Name == "AlienComp";
        });
        if (alienComp == null) {
            return null;
        }

        var variantsField = ReflectionUtil.GetPublicField(alienComp, "addonVariants");
        if (variantsField == null) {
            return null;
        }

        List<int> variants = null;
        try {
            variants = (List<int>)variantsField.GetValue(alienComp);
        }
        catch (Exception) {
            return null;
        }

        return variants[AlienAddon.VariantIndex];
    }

    public override PawnLayerOption GetSelectedOption(CustomPawn pawn) {
        var selectedIndex = GetSelectedIndex(pawn);
        if (selectedIndex == null) {
            return null;
        }

        return options[selectedIndex.Value];
    }

    public override void SelectOption(CustomPawn pawn, PawnLayerOption option) {
        var addonOption = option as PawnLayerOptionAlienAddon;
        if (addonOption == null) {
            return;
        }

        if (pawn.AlienRace != null) {
            var alienComp = pawn.Pawn.AllComps.FirstOrDefault(comp => {
                return comp.GetType().Name == "AlienComp";
            });
            if (alienComp == null) {
                return;
            }

            var variantsField = ReflectionUtil.GetPublicField(alienComp, "addonVariants");
            if (variantsField == null) {
                return;
            }

            List<int> variants = null;
            try {
                variants = (List<int>)variantsField.GetValue(alienComp);
            }
            catch (Exception) {
                return;
            }

            variants[AlienAddon.VariantIndex] = addonOption.Index;
            pawn.MarkPortraitAsDirty();
        }
    }

    public override Color GetSelectedColor(CustomPawn pawn) {
        if (Hair) {
            return pawn.HairColor;
        }

        if (Skin) {
            return pawn.SkinColor;
        }

        return Color.white;
    }

    public override void SelectColor(CustomPawn pawn, Color color) {
        if (Hair) {
            pawn.HairColor = color;
        }
        else if (Skin) {
            pawn.SkinColor = color;
        }
    }
}
