// T4-generated @ 2026-06-24 03:04:44 UTC
#nullable enable

using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.IO.Hashing;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NorthSouthSystems.Rules;

[ExcludeFromCodeCoverage]
internal static class ArgumentExceptionX
{
    public static void ThrowIfAny<T>(IEnumerable<T>? enumerable,
        string? messagePrefix = null, bool messageIncludeIndices = false,
        string? originalParamName = null, [CallerArgumentExpression(nameof(enumerable))] string? paramName = null)
    {
        if (enumerable is null)
            return;

        StringBuilder? message = null;
        int index = 0;

        foreach (var t in enumerable)
        {
            if (message is null)
            {
                message = new(messagePrefix);

                if (!string.IsNullOrEmpty(messagePrefix) && !messagePrefix.EndsWith('\n'))
                    message.AppendLine();
            }
            else
                message.AppendLine();

            if (messageIncludeIndices)
            {
                message.Append(index++.ToString(InvariantCulture));
                message.Append(": ");
            }

            message.Append(t?.ToString());
        }

        if (message is null)
            return;

        throw new ArgumentException(message.ToString(), originalParamName ?? paramName);
    }

    public static void ThrowIfDefault<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : struct
    {
        if (argument is null)
            throw new ArgumentNullException(paramName, "Value cannot be null.");

        ThrowIfDefault(argument.Value, paramName);
    }

    public static void ThrowIfDefault<T>(T argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : struct
    {
        if (argument.Equals(default(T)))
            throw new ArgumentException("Value cannot be default.", paramName);
    }
}

[ExcludeFromCodeCoverage]
internal static class ObjectX
{
    public static T DeferredNew<T>(ref T? obj)
        where T : class, new()
    {
        return obj ??= new();
    }
}

[ExcludeFromCodeCoverage]
internal static class RandomX
{
#pragma warning disable CA5394 // We are explicitly extending Random rather than a secure RNG.
    public static bool NextBool(this Random random) =>
        random == null
            ? throw new ArgumentNullException(nameof(random))
            : random.Next(2) == 1;
#pragma warning restore
}

#pragma warning disable CA1716 // Accepted as the preferred design.
[ExcludeFromCodeCoverage]
internal static class Throw
#pragma warning restore
{
    public static T IfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);

        return argument;
    }

    public static T IfDefault<T>(T argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : struct
    {
        ArgumentExceptionX.ThrowIfDefault(argument, paramName);

        return argument;
    }

    public static T IfDefault<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : struct
    {
        ArgumentExceptionX.ThrowIfDefault(argument, paramName);

        return argument.Value;
    }

    #region String (ArgumentException pass-through)

    public static string IfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);

        return argument;
    }

    public static string IfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);

        return argument;
    }

    #endregion

    #region INumberBase<T> (ArgumentOutOfRangeException pass-through)

    public static T IfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfZero(value, paramName);

        return value;
    }

    public static T IfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);

        return value;
    }

    public static T IfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);

        return value;
    }

    #endregion

    #region Equality (ArgumentOutOfRangeException pass-through)

    public static T IfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName);

        return value;
    }

    public static T IfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, paramName);

        return value;
    }

    #endregion

    #region IComparable<T> (ArgumentOutOfRangeException pass-through)

    public static T IfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName);

        return value;
    }

    public static T IfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName);

        return value;
    }

    public static T IfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName);

        return value;
    }

    public static T IfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName);

        return value;
    }

    #endregion

    #region Comparer<TEnum> (custom)

    public static TEnum IfGreaterThanEnum<TEnum>(TEnum value, TEnum other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
    {
        if (Comparer<TEnum>.Default.Compare(value, other) > 0)
            ComparerTEnumThrow(value, other, paramName, "less than or equal to");

        return value;
    }

    public static TEnum IfGreaterThanOrEqualEnum<TEnum>(TEnum value, TEnum other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
    {
        if (Comparer<TEnum>.Default.Compare(value, other) >= 0)
            ComparerTEnumThrow(value, other, paramName, "less than");

        return value;
    }

    public static TEnum IfLessThanEnum<TEnum>(TEnum value, TEnum other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
    {
        if (Comparer<TEnum>.Default.Compare(value, other) < 0)
            ComparerTEnumThrow(value, other, paramName, "greater than or equal to");

        return value;
    }

    public static TEnum IfLessThanOrEqualEnum<TEnum>(TEnum value, TEnum other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
    {
        if (Comparer<TEnum>.Default.Compare(value, other) <= 0)
            ComparerTEnumThrow(value, other, paramName, "greater than");

        return value;
    }

    // Extracted in order to make the successful path smaller and more likely to inline.
    [DoesNotReturn]
    private static void ComparerTEnumThrow<TEnum>(TEnum value, TEnum other, string? paramName, string inverseOperatorFriendlyName)
        where TEnum : struct, Enum =>
        throw new ArgumentOutOfRangeException(paramName, value,
            string.Create(InvariantCulture, $"{paramName} ('{value}') must be {inverseOperatorFriendlyName} '{other}'."));

    #endregion

    #region IComparable<T> Between (custom)

    public static T IfBetween<T>(T value, T leftInclusive, T rightInclusive, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T> =>
        BetweenHelper(value, leftInclusive, rightInclusive, paramName, true);

    public static T IfNotBetween<T>(T value, T leftInclusive, T rightInclusive, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T> =>
        BetweenHelper(value, leftInclusive, rightInclusive, paramName, false);

    private static T BetweenHelper<T>(T value, T leftInclusive, T rightInclusive, string? paramName, bool isBetweenThrows)
        where T : IComparable<T>
    {
        // The ArgumentOutOfRangeException.Throw* methods do NOT validate their parameters and allow
        // NullReferenceExceptions to occur accordingly.

        if (leftInclusive.CompareTo(rightInclusive) > 0)
            (leftInclusive, rightInclusive) = (rightInclusive, leftInclusive);

        bool isBetween = value.CompareTo(leftInclusive) >= 0 && value.CompareTo(rightInclusive) <= 0;

        if (isBetween == isBetweenThrows)
            BetweenHelperThrow(value, leftInclusive, rightInclusive, paramName, isBetweenThrows);

        return value;
    }

    // Extracted in order to make the successful path smaller and more likely to inline.
    [DoesNotReturn]
    private static void BetweenHelperThrow<T>(T value, T leftInclusive, T rightInclusive, string? paramName, bool isBetweenThrows)
        where T : IComparable<T> =>
        throw new ArgumentOutOfRangeException(paramName, value,
            string.Create(InvariantCulture, $"{paramName} ('{value}') must {(isBetweenThrows ? "not " : string.Empty)}be between '{leftInclusive}' and '{rightInclusive}' inclusively."));

    #endregion

    #region Comparer<TEnum> Between (custom)

    public static TEnum IfBetweenEnum<TEnum>(TEnum value, TEnum leftInclusive, TEnum rightInclusive, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum =>
        BetweenEnumHelper(value, leftInclusive, rightInclusive, paramName, true);

    public static TEnum IfNotBetweenEnum<TEnum>(TEnum value, TEnum leftInclusive, TEnum rightInclusive, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum =>
        BetweenEnumHelper(value, leftInclusive, rightInclusive, paramName, false);

    private static TEnum BetweenEnumHelper<TEnum>(TEnum value, TEnum leftInclusive, TEnum rightInclusive, string? paramName, bool isBetweenThrows)
        where TEnum : struct, Enum
    {
        var comparer = Comparer<TEnum>.Default;

        if (comparer.Compare(leftInclusive, rightInclusive) > 0)
            (leftInclusive, rightInclusive) = (rightInclusive, leftInclusive);

        bool isBetween = comparer.Compare(value, leftInclusive) >= 0 && comparer.Compare(value, rightInclusive) <= 0;

        if (isBetween == isBetweenThrows)
            BetweenEnumHelperThrow(value, leftInclusive, rightInclusive, paramName, isBetweenThrows);

        return value;
    }

    // Extracted in order to make the successful path smaller and more likely to inline.
    [DoesNotReturn]
    private static void BetweenEnumHelperThrow<TEnum>(TEnum value, TEnum leftInclusive, TEnum rightInclusive, string? paramName, bool isBetweenThrows)
        where TEnum : struct, Enum =>
        throw new ArgumentOutOfRangeException(paramName, value,
            string.Create(InvariantCulture, $"{paramName} ('{value}') must {(isBetweenThrows ? "not " : string.Empty)}be between '{leftInclusive}' and '{rightInclusive}' inclusively."));

    #endregion
}

[ExcludeFromCodeCoverage]
internal static class TypeX
{
    public static object? Default(this Type type) =>
        Throw.IfNull(type).IsValueType
            ? RuntimeHelpers.GetUninitializedObject(type)
            : null;

    // Unfortunately, there is no simpler method to determine this. All Systems.Numerics interfaces
    // are recursive generics (i.e. IInterface<T> where T : IInterface<T>), so they can't be used
    // with "is" or "as" operators on instances or with Type.IsAssignable for Types (without Reflection).
    public static bool IsFloatingPoint(this Type type) =>
        FloatingPointTypes.Contains(Throw.IfNull(type));

    public static ImmutableHashSet<Type> FloatingPointTypes { get; } =
    [
        typeof(Half),
        typeof(float),
        typeof(double),
        typeof(decimal)
    ];

    // Unfortunately, there is no simpler method to determine this. All Systems.Numerics interfaces
    // are recursive generics (i.e. IInterface<T> where T : IInterface<T>), so they can't be used
    // with "is" or "as" operators on instances or with Type.IsAssignable for Types (without Reflection).
    public static bool IsIntegral(this Type type) =>
        IntegralTypes.Contains(Throw.IfNull(type));

    public static ImmutableHashSet<Type> IntegralTypes { get; } =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),

        typeof(Int128),
        typeof(BigInteger)
    ];

    internal static bool CanBeEnumUnderlyingType(this Type type) =>
        EnumUnderlyingTypes.Contains(Throw.IfNull(type));

    public static ImmutableHashSet<Type> EnumUnderlyingTypes { get; } =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong)
    ];

    public static ImmutableDictionary<Type, string> CSharpKeywordsByType { get; } =
        new Dictionary<Type, string>
            {
                [typeof(bool)] = "bool",
                [typeof(byte)] = "byte",
                [typeof(sbyte)] = "sbyte",
                [typeof(short)] = "short",
                [typeof(ushort)] = "ushort",
                [typeof(int)] = "int",
                [typeof(uint)] = "uint",
                [typeof(long)] = "long",
                [typeof(ulong)] = "ulong",
                [typeof(float)] = "float",
                [typeof(double)] = "double",
                [typeof(decimal)] = "decimal",

                [typeof(char)] = "char",
                [typeof(string)] = "string",

                [typeof(object)] = "object"
            }
            .ToImmutableDictionary();

    public static bool IsGenericNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;
    public static Type FlattenGenericNullable(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsSubTypeOfGeneric(this Type type, Type genericTypeDefinition) =>
        GetSubTypeOfGeneric(type, genericTypeDefinition) is not null;

    public static Type? GetSubTypeOfGeneric(this Type type, Type genericTypeDefinition)
    {
        Throw.IfNull(type);

        if (!Throw.IfNull(genericTypeDefinition).IsGenericTypeDefinition)
            throw new ArgumentException("Must be a generic type definition.", nameof(genericTypeDefinition));

        var types = genericTypeDefinition.IsInterface
            ? type.GetInterfaces()
            : SelfAndBaseTypes(type);

        return types.SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition);
    }

    public static IEnumerable<Type> SelfAndBaseTypes(Type? t)
    {
        while (true)
        {
            if (t is null)
                yield break;

            yield return t;

            t = t.BaseType;
        }
    }
}

[ExcludeFromCodeCoverage]
internal static class BinaryRoundTrip
{
    // ReadBase64 Adapters

    public static bool ReadBase64Bool(string value) => ReadBool(Convert.FromBase64String(value));
    public static byte ReadBase64Byte(string value) => ReadByte(Convert.FromBase64String(value));
    public static sbyte ReadBase64SByte(string value) => ReadSByte(Convert.FromBase64String(value));
    public static short ReadBase64Short(string value) => ReadShort(Convert.FromBase64String(value));
    public static ushort ReadBase64UShort(string value) => ReadUShort(Convert.FromBase64String(value));
    public static int ReadBase64Int(string value) => ReadInt(Convert.FromBase64String(value));
    public static uint ReadBase64UInt(string value) => ReadUInt(Convert.FromBase64String(value));
    public static long ReadBase64Long(string value) => ReadLong(Convert.FromBase64String(value));
    public static ulong ReadBase64ULong(string value) => ReadULong(Convert.FromBase64String(value));
    public static double ReadBase64Double(string value) => ReadDouble(Convert.FromBase64String(value));
    public static decimal ReadBase64Decimal(string value) => ReadDecimal(Convert.FromBase64String(value));
    public static string ReadBase64String(string value) => ReadString(Convert.FromBase64String(value));

    // WriteBase64 Adapters

    public static string WriteBase64Bool(bool value) { string? s = null; WriteBase64Bool(value, b64 => s = b64); return s!; }
    public static string WriteBase64Byte(byte value) { string? s = null; WriteBase64Byte(value, b64 => s = b64); return s!; }
    public static string WriteBase64SByte(sbyte value) { string? s = null; WriteBase64SByte(value, b64 => s = b64); return s!; }
    public static string WriteBase64Short(short value) { string? s = null; WriteBase64Short(value, b64 => s = b64); return s!; }
    public static string WriteBase64UShort(ushort value) { string? s = null; WriteBase64UShort(value, b64 => s = b64); return s!; }
    public static string WriteBase64Int(int value) { string? s = null; WriteBase64Int(value, b64 => s = b64); return s!; }
    public static string WriteBase64UInt(uint value) { string? s = null; WriteBase64UInt(value, b64 => s = b64); return s!; }
    public static string WriteBase64Long(long value) { string? s = null; WriteBase64Long(value, b64 => s = b64); return s!; }
    public static string WriteBase64ULong(ulong value) { string? s = null; WriteBase64ULong(value, b64 => s = b64); return s!; }
    public static string WriteBase64Double(double value) { string? s = null; WriteBase64Double(value, b64 => s = b64); return s!; }
    public static string WriteBase64Decimal(decimal value) { string? s = null; WriteBase64Decimal(value, b64 => s = b64); return s!; }
    public static string WriteBase64String(string value) { string? s = null; WriteBase64String(value, b64 => s = b64); return s!; }

    public static void WriteBase64Bool(bool value, Action<string> writer) => WriteBool(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Byte(byte value, Action<string> writer) => WriteByte(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64SByte(sbyte value, Action<string> writer) => WriteSByte(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Short(short value, Action<string> writer) => WriteShort(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64UShort(ushort value, Action<string> writer) => WriteUShort(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Int(int value, Action<string> writer) => WriteInt(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64UInt(uint value, Action<string> writer) => WriteUInt(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Long(long value, Action<string> writer) => WriteLong(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64ULong(ulong value, Action<string> writer) => WriteULong(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Double(double value, Action<string> writer) => WriteDouble(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64Decimal(decimal value, Action<string> writer) => WriteDecimal(value, bytes => writer(Convert.ToBase64String(bytes)));
    public static void WriteBase64String(string value, Action<string> writer) => WriteString(value, bytes => writer(Convert.ToBase64String(bytes)));

    // bool

    public static bool ReadBool(ReadOnlySpan<byte> bytes)
    {
        Throw.IfZero(bytes.Length);
        return bytes[0] > 0;
    }

    public static void WriteBool(bool value, Action<ReadOnlySpan<byte>> writer) =>
        Throw.IfNull(writer)([value ? (byte)1 : (byte)0]);

    // byte

    public static byte ReadByte(ReadOnlySpan<byte> bytes)
    {
        Throw.IfZero(bytes.Length);
        return bytes[0];
    }

    public static void WriteByte(byte value, Action<ReadOnlySpan<byte>> writer) =>
        Throw.IfNull(writer)([value]);

    // sbyte

    public static sbyte ReadSByte(ReadOnlySpan<byte> bytes)
    {
        Throw.IfZero(bytes.Length);
        return unchecked((sbyte)bytes[0]);
    }

    public static void WriteSByte(sbyte value, Action<ReadOnlySpan<byte>> writer) =>
        Throw.IfNull(writer)([(byte)value]);

    // short

    public static short ReadShort(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadInt16LittleEndian(bytes);

    public static void WriteShort(short value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(bytes, value);
        writer(bytes);
    }

    // ushort

    public static ushort ReadUShort(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes);

    public static void WriteUShort(ushort value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        writer(bytes);
    }

    // int

    public static int ReadInt(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadInt32LittleEndian(bytes);

    public static void WriteInt(int value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        writer(bytes);
    }

    // uint

    public static uint ReadUInt(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes);

    public static void WriteUInt(uint value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        writer(bytes);
    }

    // long

    public static long ReadLong(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadInt64LittleEndian(bytes);

    public static void WriteLong(long value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        writer(bytes);
    }

    // ulong

    public static ulong ReadULong(ReadOnlySpan<byte> bytes) =>
        BinaryPrimitives.ReadUInt64LittleEndian(bytes);

    public static void WriteULong(ulong value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        writer(bytes);
    }

    // double

    public static double ReadDouble(ReadOnlySpan<byte> bytes) =>
        BitConverter.UInt64BitsToDouble(ReadULong(bytes));

    public static void WriteDouble(double value, Action<ReadOnlySpan<byte>> writer) =>
        WriteULong(BitConverter.DoubleToUInt64Bits(value), writer);

    // decimal

    public static decimal ReadDecimal(ReadOnlySpan<byte> bytes)
    {
        Span<int> parts =
        [
            BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(0, 4)),
            BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(4, 4)),
            BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(8, 4)),
            BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(12, 4))
        ];

        return new(parts);
    }

    public static void WriteDecimal(decimal value, Action<ReadOnlySpan<byte>> writer)
    {
        Throw.IfNull(writer);

        Span<int> parts = stackalloc int[4];
        if (decimal.GetBits(value, parts) != 4)
            throw new UnreachableException();

        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(0, 4), parts[0]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(4, 4), parts[1]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(8, 4), parts[2]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.Slice(12, 4), parts[3]);

        writer(bytes);
    }

    // string

    public static string ReadString(ReadOnlySpan<byte> bytes) =>
        Encoding.UTF8.GetString(bytes);

    public static void WriteString(string value, Action<ReadOnlySpan<byte>> writer) =>
        Throw.IfNull(writer)(
            Encoding.UTF8.GetBytes(
                Throw.IfNull(value)));
}

[ExcludeFromCodeCoverage]
internal static class PathX
{
    public static string? CrawlToSolutionDirectory(string? currentDirectoryOverride = null)
    {
        var directory = new DirectoryInfo(currentDirectoryOverride ?? Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            bool hasSolution = directory.GetFiles("*.sln?")
                .Select(f => f.Extension)
                .Any(e => e.Equals(".sln", StringComparison.OrdinalIgnoreCase)
                    || e.Equals(".slnx", StringComparison.OrdinalIgnoreCase));

            if (hasSolution)
                break;

            directory = directory.Parent;
        }

        return directory?.FullName;
    }

    public static string GetDirectoryNameOfCallerFilePath([CallerFilePath] string? callerFilePath = null)
    {
        if (!Path.IsPathRooted(Throw.IfNull(callerFilePath)))
            throw new ArgumentOutOfRangeException(nameof(callerFilePath));

        string? directory = Path.GetDirectoryName(callerFilePath);

        if (string.IsNullOrEmpty(directory))
            throw new ArgumentOutOfRangeException(nameof(callerFilePath));

        return directory;
    }

    public static string GetFullPathRelativeToCallerFilePath(string relativePath, [CallerFilePath] string? callerFilePath = null)
    {
        if (Path.IsPathRooted(Throw.IfNull(relativePath)))
            throw new ArgumentOutOfRangeException(nameof(relativePath));

        // We use our helper method here because of its validations (DRY).
        string directory = GetDirectoryNameOfCallerFilePath(callerFilePath);

        return Path.Combine(directory, relativePath);
    }
}

[ExcludeFromCodeCoverage]
internal static class XxHash128X
{
    // We leverage our BinaryRoundTrip helper for "stable" binary representations of all primitives
    // except for string. BinaryRoundTrip uses Encoding.UTF8.GetBytes for string, which is likely
    // slower than the very simple method below (possibly a premature optimization). However, both
    // Encoding.UTF8.GetBytes and MemoryMarshal.AsBytes return no bytes for string.Empty, thereby
    // causing it not to affect the hash. Our method "solves" this potential problem.

#pragma warning disable CA1062 // A NullReferenceException here would be "expected" by the caller.
    public static void Append(this XxHash128 hasher, bool value) => BinaryRoundTrip.WriteBool(value, hasher.Append);
    public static void Append(this XxHash128 hasher, byte value) => BinaryRoundTrip.WriteByte(value, hasher.Append);
    public static void Append(this XxHash128 hasher, sbyte value) => BinaryRoundTrip.WriteSByte(value, hasher.Append);
    public static void Append(this XxHash128 hasher, short value) => BinaryRoundTrip.WriteShort(value, hasher.Append);
    public static void Append(this XxHash128 hasher, ushort value) => BinaryRoundTrip.WriteUShort(value, hasher.Append);
    public static void Append(this XxHash128 hasher, int value) => BinaryRoundTrip.WriteInt(value, hasher.Append);
    public static void Append(this XxHash128 hasher, uint value) => BinaryRoundTrip.WriteUInt(value, hasher.Append);
    public static void Append(this XxHash128 hasher, long value) => BinaryRoundTrip.WriteLong(value, hasher.Append);
    public static void Append(this XxHash128 hasher, ulong value) => BinaryRoundTrip.WriteULong(value, hasher.Append);
    public static void Append(this XxHash128 hasher, double value) => BinaryRoundTrip.WriteDouble(AppendNormalize(value), hasher.Append);
    public static void Append(this XxHash128 hasher, decimal value) => BinaryRoundTrip.WriteDecimal(value, hasher.Append);
#pragma warning restore

    public static void Append(this XxHash128 hasher, string value)
    {
        Throw.IfNull(hasher);
        Throw.IfNull(value);

        // This line results in 0 bytes for string.Empty, which does not cause the hash to change.
        // Since we don't allow null values, this will not allow the hash to distinguish between
        // null and string.Empty without intervention. We choose to hash a single sentinel byte
        // for string.Empty, which because we are using UTF16, this sentinel cannot collide with
        // any character that is potentially hashed afterwards (it can collide with other inputs though).
        var utf16 = MemoryMarshal.AsBytes(value.AsSpan());

        if (utf16.Length > 0)
            hasher.Append(utf16);
        else
            hasher.Append(_stringEmptySentinel);
    }

    // Our intuition is that this is better than either 0 or 255 for reducing potential collisions.
    private const byte _stringEmptySentinel = 0b_0101_0101;

    private static double AppendNormalize(double value)
    {
        // double has multiple bit patterns for NaN and both -0.0 and +0.0; we normalize them.
        if (double.IsNaN(value)) return double.NaN;
        else if (value == double.NegativeZero) return 0.0;
        else return value;
    }
}

[ExcludeFromCodeCoverage]
internal static class ReverseNoBufferExtensions
{
    public static IEnumerable<T> ReverseNoBuffer<T>(this IReadOnlyList<T> source) =>
        ReverseNoBufferCore(Throw.IfNull(source));

    private static IEnumerable<T> ReverseNoBufferCore<T>(IReadOnlyList<T> source)
    {
        for (int i = source.Count - 1; i >= 0; i--)
            yield return source[i];
    }
}

// Adapted and simplified from a ChatGPT conversation on 2025-08-21.
[ExcludeFromCodeCoverage]
internal static class PropertyInfoX
{
    private static readonly ConcurrentDictionary<GetterCacheKey, Lazy<Func<object, object>>> _getterCache = new();
[ExcludeFromCodeCoverage]
    private record struct GetterCacheKey(Type Type, string PropertyPath, bool IncludeNonPublic);

    public static object GetValueCompiled(object obj, string propertyPath, bool includeNonPublic = false)
    {
        Throw.IfNull(obj);

        var getter = GetGetterCompiled(obj.GetType(), propertyPath, includeNonPublic);

        return getter(obj);
    }

    public static Func<object, object> GetGetterCompiled(Type type, string propertyPath, bool includeNonPublic = false)
    {
        Throw.IfNull(type);
        Throw.IfNullOrEmpty(propertyPath);

        if (type.IsValueType)
            throw new ArgumentException("Value types are not allowed.", nameof(type));

        var cacheKey = new GetterCacheKey(type, propertyPath, includeNonPublic);
        var getter = _getterCache.GetOrAdd(cacheKey, ck => new(() => CompileGetter(ck)));

        return getter.Value;
    }

    public static Type GetGetterReturnTypeOrThrow(Type objType, string propertyPath, bool includeNonPublic = false)
    {
        Throw.IfNull(objType);
        Throw.IfNullOrEmpty(propertyPath);

        string[] propertyPathSegments = SplitPropertyPath(propertyPath);

        var currentType = objType;

        foreach (string propertyName in propertyPathSegments)
        {
            var getter = GetGetterMethodOrThrow(currentType, propertyName, includeNonPublic);

            currentType = getter.ReturnType;
        }

        return currentType;
    }

    public static MethodInfo GetGetterMethodOrThrow(Type objType, string propertyName, bool includeNonPublic = false)
    {
        Throw.IfNull(objType);
        Throw.IfNullOrEmpty(propertyName);

        var flattenFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        var declaredFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        if (includeNonPublic)
        {
            flattenFlags |= BindingFlags.NonPublic;
            declaredFlags |= BindingFlags.NonPublic;
        }

        var property = objType.GetProperty(propertyName, flattenFlags)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {objType} with the requested accessibility.", nameof(propertyName));

        // We must get the PropertyInfo from the DeclaringType or else Expression.Property will throw:
        // "System.ArgumentException : The method '...' is not a property accessor (Parameter 'propertyAccessor')"
        if (property.DeclaringType != objType && property.DeclaringType is not null)
            property = property.DeclaringType.GetProperty(propertyName, declaredFlags)!;

        return property.GetGetMethod(includeNonPublic)
            ?? throw new ArgumentException($"Property '{propertyName}' on {objType} does not have a getter with the requested accessibility.", nameof(propertyName));
    }

    private static Func<object, object> CompileGetter(GetterCacheKey key)
    {
        string[] propertyPathSegments = SplitPropertyPath(key.PropertyPath);

        var objParameter = Expression.Parameter(typeof(object));

        Expression currentValue = Expression.Convert(objParameter, key.Type);
        var currentValueType = key.Type;

        foreach (string propertyName in propertyPathSegments)
        {
            var getter = GetGetterMethodOrThrow(currentValueType, propertyName, key.IncludeNonPublic);

            currentValue = Expression.Property(currentValue, getter);
            currentValueType = getter.ReturnType;
        }

        if (currentValueType.IsValueType)
            currentValue = Expression.Convert(currentValue, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(currentValue, objParameter);

        return lambda.Compile();
    }

    private static string[] SplitPropertyPath(string propertyPath) =>
        propertyPath.Split(['.'], StringSplitOptions.TrimEntries);
}
