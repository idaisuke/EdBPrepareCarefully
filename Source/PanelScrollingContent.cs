using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class PanelScrollingContent : PanelBase {
    protected float ModuleWidth;

    protected Rect RectScrollFrame;
    protected Rect RectScrollView;
    protected ScrollViewVertical scrollView = new();

    public List<PanelModule> Modules { get; set; } = new();

    public override string PanelHeader => null;

    public void ScrollToTop() {
        scrollView.ScrollToTop();
    }

    public void ScrollToBottom() {
        scrollView.ScrollToBottom();
    }

    public override void Resize(Rect rect) {
        base.Resize(rect);

        var contentSize = new Vector2(PanelRect.width, BodyRect.height);

        RectScrollFrame = new Rect(0, BodyRect.y, contentSize.x, contentSize.y);
        RectScrollView = new Rect(0, 0, RectScrollFrame.width, RectScrollFrame.height);

        ResizeModules(rect.width);
    }

    protected void ResizeModules(float width) {
        ModuleWidth = width;
        Modules.ForEach(m => { m.Resize(ModuleWidth); });
    }

    protected override void DrawPanelContent(State state) {
        base.DrawPanelContent(state);
        var currentPawn = state.CurrentPawn;

        float y = 0;
        GUI.BeginGroup(RectScrollFrame);

        try {
            scrollView.Begin(RectScrollView);
            if (scrollView.CurrentViewWidth != ModuleWidth) {
                ResizeModules(scrollView.CurrentViewWidth);
            }

            try {
                var visibleModules = 0;
                foreach (var module in Modules) {
                    if (module.IsVisible(state)) {
                        if (visibleModules > 0) {
                            y += 6;
                            GUI.color = Style.ColorTabViewBackground;
                            GUI.DrawTexture(new Rect(0, y, PanelRect.width, 4), BaseContent.WhiteTex);
                            GUI.color = Color.white;
                            y += 2;
                        }

                        try {
                            y += module.Draw(state, y);
                            visibleModules++;
                        }
                        catch (Exception e) {
                            Logger.Error("Failed to draw module", e);
                        }
                    }
                }
            }
            finally {
                scrollView.End(y);
            }
        }
        finally {
            GUI.EndGroup();
        }
    }
}
