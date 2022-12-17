using System;
using System.Reflection;

namespace EdB.PrepareCarefully;

public class QuietReflectionUtil {
    public static MethodInfo Method(Type type, string name) {
        var info = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        return null;
    }

    public static MethodInfo MethodWithParameters(Type type, string name, Type[] argumentTypes) {
        var info = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any,
            argumentTypes, null);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any,
            argumentTypes, null);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any,
            argumentTypes, null);
        if (info != null) {
            return info;
        }

        info = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any,
            argumentTypes, null);
        if (info != null) {
            return info;
        }

        return null;
    }

    public static MethodInfo RequiredMethod(Type type, string name, Type[] argumentTypes = null) {
        var info = argumentTypes == null ? Method(type, name) : MethodWithParameters(type, name, argumentTypes);
        if (info != null) {
            return info;
        }

        throw new MissingMethodException("Prepare Carefully requires but could not find the method " + type.Name + "."
                                         + name + " via reflection");
    }

    public static PropertyInfo Property(Type type, string name) {
        var info = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        info = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        return null;
    }

    public static FieldInfo Field(Type type, string name) {
        var info = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        if (info != null) {
            return info;
        }

        info = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        info = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (info != null) {
            return info;
        }

        return null;
    }

    public static FieldInfo RequiredField(Type type, string name) {
        var info = Field(type, name);
        if (info != null) {
            return info;
        }

        throw new MissingFieldException("Prepare Carefully requires but could not find the field " + type.Name + "."
                                        + name + " via reflection");
    }


    public static FieldInfo GetNonPublicField(Type type, string name) {
        return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static FieldInfo GetNonPublicStaticField(Type type, string name) {
        return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static T GetNonPublicStatic<T>(Type type, string name) {
        var field = GetNonPublicStaticField(type, name);
        if (field == null) {
            return default;
        }

        return (T)field.GetValue(null);
    }

    public static FieldInfo GetNonPublicField(object target, string name) {
        return GetNonPublicField(target.GetType(), name);
    }

    public static bool SetNonPublicField(object target, string name, object value) {
        var info = GetNonPublicField(target.GetType(), name);
        if (info == null) {
            return false;
        }

        info.SetValue(target, value);
        return true;
    }

    public static FieldInfo GetPublicField(object target, string name) {
        return GetPublicField(target.GetType(), name);
    }

    public static T GetFieldValue<T>(object target, string name) {
        if (target == null) {
            return default;
        }

        var field = Field(target.GetType(), name);
        if (field == null) {
            return default;
        }

        var o = field.GetValue(target);
        if (o == null) {
            return default;
        }

        if (typeof(T).IsAssignableFrom(o.GetType())) {
            return (T)field.GetValue(target);
        }

        return default;
    }

    public static T GetPropertyValue<T>(object target, string name) {
        if (target == null) {
            return default;
        }

        var property = Property(target.GetType(), name);
        if (property == null) {
            return default;
        }

        var o = property.GetValue(target);
        if (o == null) {
            return default;
        }

        if (typeof(T).IsAssignableFrom(o.GetType())) {
            return (T)o;
        }

        return default;
    }

    public static bool InvokeActionMethod(object target, string name, object[] args = null) {
        var method = Method(target.GetType(), name);
        if (method == null) {
            return false;
        }

        method.Invoke(target, args ?? new object[] { });
        return true;
    }

    public static T InvokeFunctionMethod<T>(object target, string name, object[] args = null) {
        var method = Method(target.GetType(), name);
        if (method == null) {
            return default;
        }

        var o = method.Invoke(target, args ?? new object[] { });
        if (o == null) {
            return default;
        }

        if (typeof(T).IsAssignableFrom(o.GetType())) {
            return (T)o;
        }

        return default;
    }

    public static FieldInfo GetPublicField(Type type, string name) {
        return type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
    }

    public static bool SetFieldValue(object target, string name, object value) {
        if (target == null) {
            return false;
        }

        var info = Field(target.GetType(), name);
        if (info == null) {
            return false;
        }

        info.SetValue(target, value);
        return true;
    }

    public static bool SetPublicField(object target, string name, object value) {
        var info = GetPublicField(target.GetType(), name);
        if (info == null) {
            return false;
        }

        info.SetValue(target, value);
        return true;
    }
}
