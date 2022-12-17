using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerHair : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public override ColorSelectorType ColorSelectorType => ColorSelectorType.RGB;

    public override List<Color> ColorSwatches { get; set; } = null;

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        var hairOption = option as PawnLayerOptionHair;
        if (hairOption == null) {
            return false;
        }

        return pawn.Pawn.story.hairDef == hairOption.HairDef;
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        var selectedIndex = options.FirstIndexOf(option => {
            var hairOption = option as PawnLayerOptionHair;
            if (hairOption == null) {
                return false;
            }

            return hairOption.HairDef == pawn.Pawn.story.hairDef;
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
        var hairOption = option as PawnLayerOptionHair;
        if (hairOption != null) {
            pawn.Pawn.story.hairDef = hairOption.HairDef;
            pawn.MarkPortraitAsDirty();
        }
    }

    public override Color GetSelectedColor(CustomPawn pawn) {
        return pawn.HairColor;
    }

    public override void SelectColor(CustomPawn pawn, Color color) {
        pawn.HairColor = color;
    }
}
