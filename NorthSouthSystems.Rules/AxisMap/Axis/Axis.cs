using Nerdbank.MessagePack;
using NorthSouthSystems.IO.Hashing;
using NorthSouthSystems.MessagePackable;
using PolyType;
using System.IO.Hashing;

namespace Nss.Rules;

// AxisNumeric<double> is explicitly NOT supported due to the challenges involved with floating
// point equality and comparison - those challenges are not worth the space savings (and potentially
// some calculation savings; however, those are likely to be offset by attempting to make equality
// and comparison correct). AxisMap<double> is supported because those challenges are left to
// consumers if they are even relevant to their use case, and the space savings are more meaningful.
//
// Nerdbank.MessagePack.KeyAttribute is used throughout to optimize the MessagePack size.
// All KeyAttribute values must be unique throughout an entire class hierarchy.
//
// The Axis[Enum|Numeric|String].Bound[EnumName|Numeric|String]s properties are all uniquely
// named within their respective classes so that we can use Nerdbank.MessagePackSerializer.GetJsonSchema
// without encountering:
// "System.ArgumentException : An item with the same key has already been added. Key: Bounds".
//
// PolyType.DerivedTypeShapeAttribute.Tag is used to optimize the MessagePack size.
//
// DerivedTypeShape(AxisNumeric<>) Name properties are provided because of the PolyType build error:
// error PT0012: Polymorphic type 'NorthSouthSystems.Rules.Axis' uses duplicate assignments for name 'AxisNumeric'.
[GenerateShape]
[DerivedTypeShape(typeof(AxisBool), Tag = 1)]
[DerivedTypeShape(typeof(AxisNumeric<short>), Name = nameof(Axis) + nameof(Int16), Tag = 2)]
[DerivedTypeShape(typeof(AxisNumeric<int>), Name = nameof(Axis) + nameof(Int32), Tag = 3)]
[DerivedTypeShape(typeof(AxisNumeric<long>), Name = nameof(Axis) + nameof(Int64), Tag = 4)]
[DerivedTypeShape(typeof(AxisNumeric<decimal>), Name = nameof(Axis) + nameof(Decimal), Tag = 5)]
[DerivedTypeShape(typeof(AxisString), Tag = 6)]
public abstract partial class Axis : IMessagePackable
{
    public static MessagePackSerializer MessagePack => AxisMap.MessagePack;

    public static ImmutableHashSet<Type> SupportedBoundTypes { get; } =
        [typeof(bool), typeof(short), typeof(int), typeof(long), /* NO! See comment above. typeof(double),*/ typeof(decimal), typeof(string)];

    internal static Axis ParseValidateAndConstruct(Type type,
        string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex, ImmutableArray<string> boundsRaw)
    {
        try
        {
            if (type == typeof(bool)) return AxisBool.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else if (type == typeof(short)) return AxisNumeric<short>.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else if (type == typeof(int)) return AxisNumeric<int>.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else if (type == typeof(long)) return AxisNumeric<long>.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else if (type == typeof(decimal)) return AxisNumeric<decimal>.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else if (type == typeof(string)) return AxisString.ParseValidateAndConstruct(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundsRaw);
            else throw new NotSupportedException(type.ToString());
        }
        catch (Exception innerException)
        {
            throw new ArgumentException(string.Create(InvariantCulture, $"Unable to construct Axis. Axis Property Path: {propertyPath}"), innerException);
        }
    }

    protected static void Validate(string propertyPath, int orientationRelativeIndex, ImmutableArray<string> boundsRaw)
    {
        if (Throw.IfNull(propertyPath).Any(char.IsWhiteSpace))
            throw new ArgumentException("Whitespace characters are not allowed.", nameof(propertyPath));

        Throw.IfLessThan(orientationRelativeIndex, 0);
        Throw.IfEqual(boundsRaw.IsDefault, true);
        Throw.IfZero(boundsRaw.Length);
    }

    [ConstructorShape]
    protected Axis(string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex)
    {
        PropertyPath = propertyPath;
        IsOrientationHorizontal = isOrientationHorizontal;
        OrientationRelativeIndex = orientationRelativeIndex;
    }

    [Key(1)] public string PropertyPath { get; }
    [Key(2)] public bool IsOrientationHorizontal { get; }
    [Key(3)] public int OrientationRelativeIndex { get; }

    internal abstract Type BoundType { get; }
    internal abstract int BoundCount { get; }

    internal IEnumerable<string> BoundToStrings =>
        Enumerable.Range(0, BoundCount).Select(GetBoundToString);

    internal abstract string GetBoundToString(int boundIndex);

    internal abstract int? LookupBoundIndex(object input);

    internal void AppendAxisBoundDescription(StringBuilder builder, int boundIndex)
    {
        builder.Append(PropertyPath);
        builder.Append(' ');

        AppendBoundDescription(builder, boundIndex);
    }

    internal void AppendAxisBoundHash(XxHash128 hasher, int boundIndex)
    {
        hasher.Append(PropertyPath);
        hasher.Append(_hashDelimiterBytes);

        AppendBoundHash(hasher, boundIndex);
        hasher.Append(_hashDelimiterBytes);
    }

    internal abstract void AppendBoundDescription(StringBuilder builder, int boundIndex);
    protected abstract void AppendBoundHash(XxHash128 hasher, int boundIndex);

    // This likely "wastes" performance due to being unneccessarily long, but it will drive the chance
    // of an inadvertent "Concat(PropertyPath, Bound) == Concat(OtherPropertyPath, OtherBound) to 0%.
    private static readonly string _hashDelimiter = string.Create(InvariantCulture, $"\n{nameof(AxisMap)}.{nameof(_hashDelimiter)}\n");
    private static readonly byte[] _hashDelimiterBytes = Encoding.UTF8.GetBytes(_hashDelimiter);
}