using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerBodyTattoo : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public override ColorSelectorType ColorSelectorType => ColorSelectorType.None;

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        var aOption = option as PawnLayerOptionTattoo;
        if (aOption == null) {
            return false;
        }

        return pawn.BodyTattoo == aOption.TattooDef;
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        var selectedIndex = options.FirstIndexOf(option => {
            if (!(option is PawnLayerOptionTattoo layerOption)) {
                return false;
            }

            return layerOption.TattooDef == pawn.BodyTattoo;
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
        if (option is PawnLayerOptionTattoo layerOption) {
            pawn.BodyTattoo = layerOption.TattooDef;
        }
    }
}
