using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordApparelV4 : IExposable {
    public string apparel;
    public Color color;
    public string layer;
    public string stuff;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref layer, "layer", "");
        Scribe_Values.Look<string>(ref apparel, "apparel", "");
        Scribe_Values.Look<string>(ref stuff, "stuff", "");
        Scribe_Values.Look(ref color, "color", Color.white);
    }
}
