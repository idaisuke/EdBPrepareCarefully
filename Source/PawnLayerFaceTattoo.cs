using System.Collections.Generic;
using Verse;

namespace EdB.PrepareCarefully;

public class PawnLayerFaceTattoo : PawnLayer {
    private List<PawnLayerOption> options = new();

    public override List<PawnLayerOption> Options {
        get => options;
        set => options = value;
    }

    public override ColorSelectorType ColorSelectorType => ColorSelectorType.None;

    public override bool IsOptionSelected(CustomPawn pawn, PawnLayerOption option) {
        if (!(option is PawnLayerOptionTattoo aOption)) {
            return false;
        }

        return pawn.FaceTattoo == aOption.TattooDef;
    }

    public override int? GetSelectedIndex(CustomPawn pawn) {
        var selectedIndex = options.FirstIndexOf(option => {
            var layerOption = option as PawnLayerOptionTattoo;
            if (layerOption == null) {
                return false;
            }

            return layerOption.TattooDef == pawn.FaceTattoo;
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
            pawn.FaceTattoo = layerOption.TattooDef;
        }
    }
}
