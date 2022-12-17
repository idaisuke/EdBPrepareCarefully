using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

internal class ScrollViewHorizontal {
    public static readonly float ScrollbarSize = 15;
    private readonly bool consumeScrollEvents = true;
    private Rect contentRect;
    private Vector2 position = Vector2.zero;
    private Rect viewRect;

    public ScrollViewHorizontal() {
    }

    public ScrollViewHorizontal(bool consumeScrollEvents) {
        this.consumeScrollEvents = consumeScrollEvents;
    }

    public float ViewHeight => viewRect.height;

    public float ViewWidth => viewRect.width;

    public float ContentWidth { get; private set; }

    public float ContentHeight => contentRect.height;

    public Vector2 Position {
        get => position;
        set => position = value;
    }

    public bool ScrollbarsVisible => ContentWidth > ViewWidth;

    public void Begin(Rect viewRect) {
        this.viewRect = viewRect;
        contentRect = new Rect(0, 0, ContentWidth, viewRect.height - 16);
        if (consumeScrollEvents) {
            Widgets.BeginScrollView(viewRect, ref position, contentRect);
        }
        else {
            BeginScrollView(viewRect, ref position, contentRect);
        }
    }

    public void End(float xPosition) {
        if (Event.current.type == EventType.Layout) {
            ContentWidth = xPosition;
        }

        Widgets.EndScrollView();
    }

    protected static void BeginScrollView(Rect outRect, ref Vector2 scrollPosition, Rect viewRect) {
        var vector = scrollPosition;
        var vector2 = GUI.BeginScrollView(outRect, scrollPosition, viewRect);
        Vector2 vector3;
        if (Event.current.type == EventType.MouseDown) {
            vector3 = vector;
        }
        else {
            vector3 = vector2;
        }

        if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect)) {
            vector3 += Event.current.delta * 40;
        }

        scrollPosition = vector3;
    }
}
