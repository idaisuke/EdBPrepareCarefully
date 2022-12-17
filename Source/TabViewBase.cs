using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public abstract class TabViewBase : ITabView {
    public Rect TabViewRect = new(float.MinValue, float.MinValue, float.MinValue, float.MinValue);

    public TabRecord TabRecord {
        get;
        set;
    }

    public abstract string Name {
        get;
    }

    public virtual void Draw(State state, Rect rect) {
        if (rect != TabViewRect) {
            Resize(rect);
        }
    }

    protected virtual void Resize(Rect rect) {
        TabViewRect = rect;
    }
}
