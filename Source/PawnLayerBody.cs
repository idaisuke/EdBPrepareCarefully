using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerBody : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public override ColorSelectorType ColorSelectorType => ColorSelectorType.Skin;

    public override List<Color> ColorSwatches { get; set; } = null;

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        var bodyOption = option as PawnLayerOptionBody;
        if (bodyOption == null) {
            return false;
        }

        return pawn.BodyType == bodyOption.BodyTypeDef;
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        var selectedIndex = options.FirstIndexOf(option => {
            var bodyOption = option as PawnLayerOptionBody;
            if (bodyOption == null) {
                return false;
            }

            return bodyOption.BodyTypeDef == pawn.BodyType;
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
        var bodyOption = option as PawnLayerOptionBody;
        if (bodyOption != null) {
            pawn.BodyType = bodyOption.BodyTypeDef;
        }
    }

    public override Color GetSelectedColor(CustomPawn pawn) {
        return pawn.SkinColor;
    }

    public override void SelectColor(CustomPawn pawn, Color color) {
        pawn.SkinColor = color;
    }
}
