internal enum T_AxisMapInputEnum
{
    A,
    B,
    C,
    D
}

internal sealed class T_AxisMapInput
{
    public bool TheBool { get; set; }
    public bool? TheBoolNullable { get; set; }

    public short TheShort { get; set; }

    public int TheInt { get; set; }
    public int? TheIntNullable { get; set; }

    public long TheLong { get; set; }

    public decimal TheDecimal { get; set; }

    public string TheString { get; set; }

    public T_AxisMapInputEnum TheEnum { get; set; }
    public T_AxisMapInputEnum? TheEnumNullable { get; set; }

    internal static string GetPropertyName(Type t)
    {
        if (t == typeof(bool)) return nameof(TheBool);
        else if (t == typeof(bool?)) return nameof(TheBoolNullable);
        else if (t == typeof(short)) return nameof(TheShort);
        else if (t == typeof(int)) return nameof(TheInt);
        else if (t == typeof(int?)) return nameof(TheIntNullable);
        else if (t == typeof(long)) return nameof(TheLong);
        else if (t == typeof(decimal)) return nameof(TheDecimal);
        else if (t == typeof(string)) return nameof(TheString);
        else if (t == typeof(T_AxisMapInputEnum)) return nameof(TheEnum);
        else if (t == typeof(T_AxisMapInputEnum?)) return nameof(TheEnumNullable);
        else throw new NotSupportedException(t.ToString());
    }

    internal static T_AxisMapInput ConstructWithProperty(Type t, object value)
    {
        if (t == typeof(bool)) return new() { TheBool = (bool)value };
        else if (t == typeof(bool?)) return new() { TheBoolNullable = (bool?)value };
        else if (t == typeof(short)) return new() { TheShort = (short)value };
        else if (t == typeof(int)) return new() { TheInt = (int)value };
        else if (t == typeof(int?)) return new() { TheIntNullable = (int?)value };
        else if (t == typeof(long)) return new() { TheLong = (long)value };
        else if (t == typeof(decimal)) return new() { TheDecimal = (decimal)value };
        else if (t == typeof(string)) return new() { TheString = (string)value };
        else if (t == typeof(T_AxisMapInputEnum)) return new() { TheEnum = (T_AxisMapInputEnum)value };
        else if (t == typeof(T_AxisMapInputEnum?)) return new() { TheEnumNullable = (T_AxisMapInputEnum?)value };
        else throw new NotSupportedException(t.ToString());
    }
}
