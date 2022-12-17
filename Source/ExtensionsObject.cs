using System.Reflection;

namespace EdB.PrepareCarefully;

public static class ExtensionsObject {
    public static void SetPrivateField(this object target, string name, object value) {
        var info = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (info != null) {
            info.SetValue(target, value);
        }
    }

    public static T GetPrivateField<T>(this object target, string name) {
        var info = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (info != null) {
            return (T)info.GetValue(target);
        }

        return default;
    }
}
