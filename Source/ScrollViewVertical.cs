using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class ScrollViewVertical {
    public static readonly float ScrollbarSize = 16;
    private readonly bool consumeScrollEvents = true;
    private Rect contentRect;
    private Vector2 position = Vector2.zero;
    private Vector2? scrollTo;
    private Rect viewRect;

    public ScrollViewVertical() {
    }

    public ScrollViewVertical(bool consumeScrollEvents) {
        this.consumeScrollEvents = consumeScrollEvents;
    }

    public float ViewHeight => viewRect.height;

    public float ViewWidth => viewRect.width;

    // The current width of the view, adjusted based on whether or not the scrollbars are visible
    public float CurrentViewWidth => !ScrollbarsVisible ? viewRect.width : viewRect.width - ScrollbarSize;

    public float ContentWidth => contentRect.width;

    public float ContentHeight { get; private set; }

    public Vector2 Position {
        get => position;
        set => position = value;
    }

    public bool ScrollbarsVisible => ContentHeight > ViewHeight;

    public void Begin(Rect viewRect) {
        this.viewRect = viewRect;
        contentRect = new Rect(0, 0, viewRect.width - 16, ContentHeight);
        if (consumeScrollEvents) {
            Widgets.BeginScrollView(viewRect, ref position, contentRect);
        }
        else {
            BeginScrollView(viewRect, ref position, contentRect);
        }
    }

    public void End(float yPosition) {
        ContentHeight = yPosition;
        Widgets.EndScrollView();
        if (scrollTo != null) {
            var newPosition = scrollTo.Value;
            if (newPosition.y < 0) {
                newPosition.y = 0;
            }
            else if (newPosition.y > ContentHeight - ViewHeight - 1) {
                newPosition.y = ContentHeight - ViewHeight - 1;
            }

            Position = newPosition;
            scrollTo = null;
        }
    }

    public void ScrollToTop() {
        scrollTo = new Vector2(0, 0);
    }

    public void ScrollToBottom() {
        scrollTo = new Vector2(0, float.MaxValue);
    }

    public void ScrollTo(float y) {
        scrollTo = new Vector2(0, y);
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
