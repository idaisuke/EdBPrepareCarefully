using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully;

public class LabelTrimmer {
    private readonly Dictionary<string, string> cache = new();
    private readonly string suffix = "...";
    private float width;

    public Rect Rect {
        set {
            if (width != value.width) {
                cache.Clear();
            }

            width = value.width;
        }
    }

    public float Width {
        get => width;
        set {
            if (width != value) {
                cache.Clear();
            }

            width = value;
        }
    }

    public string TrimLabelIfNeeded(string name) {
        return TrimLabelIfNeeded(new DefaultLabelProvider(name));
    }

    public string TrimLabelIfNeeded(LabelProvider provider) {
        var label = provider.Current;
        if (Text.CalcSize(label).x <= width) {
            return label;
        }

        if (cache.TryGetValue(label, out var shorter)) {
            return shorter;
        }

        return TrimLabel(provider);
    }

    public string TrimLabel(LabelProvider provider) {
        var original = provider.Current;
        var shorter = original;
        while (!shorter.NullOrEmpty()) {
            var length = shorter.Length;
            shorter = provider.Trim();
            // The trimmer should always return a shorter length.  If it doesn't we bail--it's a bad implementation.
            if (shorter.Length >= length) {
                break;
            }

            var withSuffix = provider.CurrentWithSuffix(suffix);
            var size = Text.CalcSize(withSuffix);
            if (size.x <= width) {
                cache.Add(original, withSuffix);
                return shorter;
            }
        }

        return original;
    }

    public interface LabelProvider {
        string Current {
            get;
        }

        string Trim();
        string CurrentWithSuffix(string suffix);
    }

    public struct DefaultLabelProvider : LabelProvider {
        private bool trimmed;

        public DefaultLabelProvider(string label) {
            Current = label;
            trimmed = false;
        }

        public string Trim() {
            var length = Current.Length;
            if (length == 0) {
                return "";
            }

            Current = Current.Substring(0, length - 1).TrimEnd();
            trimmed = true;
            return Current;
        }

        public string Current { get; private set; }

        public string CurrentWithSuffix(string suffix) {
            if (trimmed) {
                return Current + suffix;
            }

            return Current;
        }
    }
}
