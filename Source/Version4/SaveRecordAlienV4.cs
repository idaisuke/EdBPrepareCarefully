using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class SaveRecordAlienV4 : IExposable {
    public string crownType;
    public Color hairColorSecond;
    public Color skinColor;
    public Color skinColorSecond;

    public void ExposeData() {
        Scribe_Values.Look<string>(ref crownType, "crownType", "");
        Scribe_Values.Look(ref skinColor, "skinColor", Color.white);
        Scribe_Values.Look(ref skinColorSecond, "skinColorSecond", Color.white);
        Scribe_Values.Look(ref hairColorSecond, "hairColorSecond", Color.white);
    }
}
