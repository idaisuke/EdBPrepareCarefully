using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public delegate void DragSliderValueUpdateCallback(int value);

public class DragSlider {
    private const int DragTimeGrowMinimumOffset = 300;
    protected static FieldInfo draggingField;

    private static readonly SoundDef DragStartSound = SoundDef.Named("TradeSlider_DragStart");

    private static readonly SoundDef DragAmountChangedSound = SoundDef.Named("Drag_TradeSlider");

    private static readonly SoundDef DragEndSound = SoundDef.Named("TradeSlider_DragEnd");
    private readonly DragSliderCallback dragCompletedCallback;
    private readonly DragSliderCallback dragStartCallback;
    private readonly DragSliderCallback dragUpdateCallback;

    public int dragBaseAmount;

    public bool dragLimitWarningGiven;

    private float lastDragRealTime = -10000;

    public float lastUpdateTime;
    public float maxDelta = 400;
    public int maxValue = Int32.MaxValue;
    public int minValue = 0;
    public int multiplier = 10;
    public float scale = 15;

    public float stretch = 0.4f;

    protected int value;

    public DragSliderValueUpdateCallback valueUpdateCallback;

    public DragSlider() {
        dragStartCallback = DraggingStart;
        dragUpdateCallback = DraggingUpdate;
        dragCompletedCallback = DraggingCompleted;
    }

    public DragSlider(float stretch, float scale, float maxDelta) : this() {
        this.stretch = stretch;
        this.scale = scale;
        this.maxDelta = maxDelta;
    }

    public void Reset() {
        lastUpdateTime = 0;
    }

    protected void Update() {
        lastUpdateTime = lastUpdateTime += Time.deltaTime;
    }

    public void DraggingStart(float mouseOffX, float rateFactor) {
        DragStartSound.PlayOneShot(SoundInfo.OnCamera());
    }

    public void DraggingCompleted(float mouseOffX, float rateFactor) {
        DragEndSound.PlayOneShot(SoundInfo.OnCamera());
        valueUpdateCallback = null;
    }

    public void DraggingUpdate(float mouseOffX, float rateFactor) {
        Update();

        var amount = 0;
        var direction = 1;

        if (mouseOffX != 0) {
            var delta = Math.Abs(mouseOffX);
            if (delta > maxDelta) {
                delta = maxDelta;
            }

            delta = delta * stretch;
            delta = scale / (delta * delta);
            if (lastUpdateTime > delta) {
                amount = 0;
                while (lastUpdateTime > delta) {
                    lastUpdateTime -= delta;
                    amount++;
                }
            }
        }

        if (mouseOffX < 0) {
            direction = -1;
        }

        AcceptanceReport acceptanceReport = null;
        if (amount != 0) {
            if (Event.current.shift) {
                amount = amount * multiplier;
            }

            if (direction > 0) {
                if (value < maxValue) {
                    if (maxValue - value < amount) {
                        value = maxValue;
                    }
                    else {
                        value += amount;
                    }

                    acceptanceReport = true;
                }
                else {
                    acceptanceReport = false;
                }
            }
            else {
                if (value > minValue) {
                    if (minValue + amount > value) {
                        value = minValue;
                    }
                    else {
                        value -= amount;
                    }

                    acceptanceReport = true;
                }
                else {
                    acceptanceReport = false;
                }
            }

            if (acceptanceReport.Accepted) {
                DragAmountChangedSound.PlayOneShot(SoundInfo.OnCamera());
                lastDragRealTime = Time.realtimeSinceStartup;
            }
        }

        if (valueUpdateCallback != null) {
            valueUpdateCallback(value);
        }
    }

    public void OnGUI(Rect rect, int value, DragSliderValueUpdateCallback valueUpdateCallback) {
        var centerX = rect.x + (rect.width / 2);
        var y = rect.y + (rect.height / 2);
        var height = rect.height;
        var slRect = new Rect(rect);

        if (DragSliderManager.DragSlider(slRect, 1, dragStartCallback, dragUpdateCallback, dragCompletedCallback)) {
            Reset();
            this.value = value;
            dragBaseAmount = value;
            dragLimitWarningGiven = false;
            this.valueUpdateCallback = valueUpdateCallback;
        }

        var label = value.ToString();
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.MiddleCenter;
        rect.y += 1;
        Widgets.Label(rect, label);
        rect.y -= 1;
        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;
    }

    public static bool IsDragging() {
        if (draggingField == null) {
            draggingField =
                typeof(DragSliderManager).GetField("dragging", BindingFlags.Static | BindingFlags.NonPublic);
        }

        return (bool)draggingField.GetValue(null);
    }

    public void ForceStop() {
        DragSliderManager.ForceStop();
    }
}
