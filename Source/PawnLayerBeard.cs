using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerBeard : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public override ColorSelectorType ColorSelectorType => ColorSelectorType.RGB;

    public override List<Color> ColorSwatches { get; set; } = null;

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        var aOption = option as PawnLayerOptionBeard;
        if (aOption == null) {
            return false;
        }

        return pawn.Beard == aOption.BeardDef;
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        var selectedIndex = options.FirstIndexOf(option => {
            if (!(option is PawnLayerOptionBeard beardOption)) {
                return false;
            }

            return beardOption.BeardDef == pawn.Beard;
        });
        if (selectedIndex > -1) {
            return selectedIndex;
        }

        return null;
    }

    public override PawnLayerOption GetSelectedOption(CustomPawn pawn) {
        var selectedIndex = GetSelectedIndex(pawn);
        if (selectedIndex == null) {
            return null;
        }

        if (selectedIndex.Value >= 0 && selectedIndex.Value < options.Count) {
            return options[selectedIndex.Value];
        }

        return null;
    }

    public override void SelectOption(CustomPawn pawn, PawnLayerOption option) {
        if (option is PawnLayerOptionBeard beardOption) {
            pawn.Beard = beardOption.BeardDef;
        }
    }

    public override Color GetSelectedColor(CustomPawn pawn) {
        return pawn.HairColor;
    }

    public override void SelectColor(CustomPawn pawn, Color color) {
        pawn.HairColor = color;
    }
}
