using System;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully; 

public class CustomHeadType {
    public string AlienCrownType {
        get;
        set;
    }

    public CrownType CrownType { get; set; }

    public string GraphicPath { get; set; }

    public string AlternateGraphicPath { get; set; }

    public Gender? Gender { get; set; }

    public string Label {
        get;
        set;
    }

    protected static string GetHeadLabel(string path) {
        try {
            var pathValues = path.Split('/');
            var crownType = pathValues[pathValues.Length - 1];
            var values = crownType.Split('_');
            return values[values.Count() - 2] + ", " + values[values.Count() - 1];
        }
        catch (Exception) {
            Logger.Warning("Could not determine head type label from graphics path: " + path);
            return "EdB.PC.Common.Default".Translate();
        }
    }

    public override string ToString() {
        return "{ label = \"" + Label + "\", graphicsPath = \"" + GraphicPath + "\", crownType = " + CrownType +
               "\", AlienCrownType = " + AlienCrownType + ", gender = " + Gender + "}";
    }
}
