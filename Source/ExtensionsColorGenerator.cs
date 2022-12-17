using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public static class ExtensionsColorGenerator {
    public static List<Color> GetColorList(this ColorGenerator generator) {
        var options = generator as ColorGenerator_Options;
        if (options != null) {
            return options.options.Where(option => {
                return option.only.a > -1.0f;
            }).Select(option => {
                return option.only;
            }).ToList();
        }

        var single = generator as ColorGenerator_Single;
        if (single != null) {
            return new List<Color> { single.color };
        }

        var white = generator as ColorGenerator_White;
        if (white != null) {
            return new List<Color> { Color.white };
        }

        return new List<Color>();
    }
}
