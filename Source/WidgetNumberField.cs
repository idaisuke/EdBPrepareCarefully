using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully;

public class WidgetNumberField {
    private static readonly int ClickDelay = 250;
    private readonly string id;
    private string focusedControl = "";
    private int maxValue = int.MaxValue;
    private int minValue;
    private int? newValue;
    private bool shouldFocusField;
    private bool showTextField;
    private GUIStyle textFieldStyle;
    private int ticksSinceClick;

    public WidgetNumberField() {
        id = "CONTROL-" + Guid.NewGuid();
        DragSlider.minValue = minValue;
        DragSlider.maxValue = maxValue;
    }

    public Action<int> UpdateAction { get; set; } = null;

    public int MinValue {
        get => minValue;
        set {
            minValue = value;
            DragSlider.minValue = value;
        }
    }

    public int MaxValue {
        get => maxValue;
        set {
            maxValue = value;
            DragSlider.maxValue = value;
        }
    }

    public DragSlider DragSlider { get; set; } = new();

    protected void Update(int value) {
        if (value < minValue) {
            value = minValue;
        }
        else if (value > maxValue) {
            value = maxValue;
        }

        if (UpdateAction != null) {
            UpdateAction(value);
        }

        newValue = null;
    }

    public void Draw(Rect rect, int value) {
        GUI.color = Style.ColorText;
        Text.Font = GameFont.Small;

        var dragging = false;
        var currentControl = GUI.GetNameOfFocusedControl();
        if (currentControl != focusedControl) {
            if (focusedControl == id && currentControl != id) {
                if (newValue != null) {
                    if (newValue == value) {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    }
                    else if (newValue >= minValue && newValue <= maxValue) {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        Update(newValue.Value);
                    }
                    else {
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        Update(newValue.Value);
                    }
                }

                showTextField = false;
            }

            focusedControl = currentControl;
        }

        if (showTextField) {
            if (textFieldStyle == null) {
                textFieldStyle = new GUIStyle(Text.CurTextFieldStyle);
                textFieldStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (shouldFocusField) {
                newValue = value;
            }

            GUI.SetNextControlName(id);
            var previousText = newValue == null ? "" : newValue.Value + "";
            var text = GUI.TextField(rect, previousText, textFieldStyle);
            if (shouldFocusField) {
                shouldFocusField = false;
                GUI.FocusControl(id);
            }

            if (previousText != text) {
                if (string.IsNullOrEmpty(text)) {
                    newValue = null;
                }
                else {
                    try {
                        newValue = int.Parse(text);
                    }
                    catch (Exception) { }
                }
            }

            if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Tab ||
                Event.current.keyCode == KeyCode.KeypadEnter) {
                GUI.FocusControl(null);
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.Ignore) {
                if (!rect.Contains(Event.current.mousePosition)) {
                    GUI.FocusControl(null);
                }
            }
        }
        else {
            Widgets.DrawAtlas(rect, Textures.TextureFieldAtlas);
            DragSlider.OnGUI(rect, value, v => {
                Update(v);
            });
            if (rect.Contains(Event.current.mousePosition)) {
                if (Event.current.type == EventType.MouseDown) {
                    ticksSinceClick = Environment.TickCount;
                }
                else if (Event.current.type == EventType.MouseUp) {
                    var newTicks = Environment.TickCount;
                    if (newTicks - ticksSinceClick < ClickDelay) {
                        showTextField = true;
                        shouldFocusField = true;
                    }

                    ticksSinceClick = 0;
                }
            }

            dragging = DragSlider.IsDragging();
        }

        // Draw the decrement button.
        var buttonRect = new Rect(rect.x - 17, rect.y + 6, 16, 16);
        if (value == minValue) {
            GUI.color = Style.ColorButtonDisabled;
        }
        else {
            if (!dragging && !showTextField) {
                Style.SetGUIColorForButton(buttonRect);
            }
            else {
                GUI.color = Style.ColorButton;
            }
        }

        GUI.DrawTexture(buttonRect, Textures.TextureButtonPrevious);
        if (value != minValue) {
            if (Widgets.ButtonInvisible(buttonRect, false)) {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                var amount = Event.current.shift ? 10 : 1;
                var newValue = value - amount;
                Update(newValue);
            }
        }

        // Draw the increment button.
        buttonRect = new Rect(rect.x + rect.width + 1, rect.y + 6, 16, 16);
        if (value == maxValue) {
            GUI.color = Style.ColorButtonDisabled;
        }
        else {
            if (!dragging && !showTextField) {
                Style.SetGUIColorForButton(buttonRect);
            }
            else {
                GUI.color = Style.ColorButton;
            }
        }

        if (value != maxValue) {
            GUI.DrawTexture(buttonRect, Textures.TextureButtonNext);
            if (Widgets.ButtonInvisible(buttonRect, false)) {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                var amount = Event.current.shift ? 10 : 1;
                var newValue = value + amount;
                Update(newValue);
            }
        }

        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }
}
