using MoreLinq;
using Nerdbank.MessagePack;
using NorthSouthSystems.MessagePackable;
using NorthSouthSystems.Reflection;
using PolyType;
using System.ComponentModel;
using System.IO.Hashing;
using System.Reflection;

namespace Nss.Rules;

public enum AxisMapLookupCellValueStatus
{
    CellNotFound,
    CellIsNullOrWhiteSpace,
    Success
}

// Nerdbank.MessagePack.KeyAttribute is used throughout to optimize the MessagePack size.
// All KeyAttribute values must be unique throughout an entire class hierarchy.
//
// PolyType.DerivedTypeShape Tag is used to optimize the MessagePack size.
//
// DerivedTypeShape(AxisMap<>) Name properties provided because of the PolyType build error:
// error PT0012: Polymorphic type 'NorthSouthSystems.Rules.AxisMap' uses duplicate assignments for name 'AxisMap'.
//
// PolyType does not work with generic types, so we use this base class to workaround
// the limitation and to provide a common base to consumers who might be storing a collection
// of AxisMaps with different generic type arguments.
[GenerateShape]
[DerivedTypeShape(typeof(AxisMap<bool>), Name = nameof(AxisMap) + nameof(Boolean), Tag = 1)]
[DerivedTypeShape(typeof(AxisMap<short>), Name = nameof(AxisMap) + nameof(Int16), Tag = 2)]
[DerivedTypeShape(typeof(AxisMap<int>), Name = nameof(AxisMap) + nameof(Int32), Tag = 3)]
[DerivedTypeShape(typeof(AxisMap<long>), Name = nameof(AxisMap) + nameof(Int64), Tag = 4)]
[DerivedTypeShape(typeof(AxisMap<double>), Name = nameof(AxisMap) + nameof(Double), Tag = 5)]
[DerivedTypeShape(typeof(AxisMap<decimal>), Name = nameof(AxisMap) + nameof(Decimal), Tag = 6)]
[DerivedTypeShape(typeof(AxisMap<string>), Name = nameof(AxisMap) + nameof(String), Tag = 7)]
public abstract partial class AxisMap : IMessagePackable
{
    public static MessagePackSerializer MessagePack { get; } = new();

    // Anymore would be unmanageable complexity for the end user, and we'd like to put a hard limit on our testing
    // surface area even though from the code's perspective 5 is NOT different from 4 is NOT different from 3
    // (unlike how 3 IS different from 2 IS different from 1 IS different from 0).
    public static int AxesOrientationCountMax { get; } = 4;
    public static int AxesTotalCountMax { get; } = 2 * AxesOrientationCountMax;

    // No rhyme or reason at this time other than potential performance concerns for Excel and Csv serialization.
    public static int CellValuesCountMax { get; } = 1_000_000;

    public static ImmutableHashSet<Type> SupportedCellValueTypes { get; } =
        [typeof(bool), typeof(short), typeof(int), typeof(long), typeof(double), typeof(decimal), typeof(string)];

    internal static int BoundCountsAggregateMultiply(IEnumerable<Axis> axes) =>
        BoundCountsAggregateMultiply(axes.Select(static axis => axis.BoundCount));

    internal static int BoundCountsAggregateMultiply(IEnumerable<int> boundCounts) =>
        boundCounts.Aggregate(1, static (accumulator, count) => accumulator * count);

    internal static AxisMap ParseValidateAndConstructFromTable(Type cellValueType,
        ImmutableArray<Axis> axes, byte cellValueScaleForFormatting, ImmutableArray<object?> cellValuesRaw) =>
        (AxisMap)typeof(AxisMap<>)
            .MakeGenericType(cellValueType)
            .GetMethod(nameof(ParseValidateAndConstructFromTable),
                BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(null, [axes, cellValueScaleForFormatting, cellValuesRaw])!;

    internal static AxisMap ParseValidateAndConstructFromTemplate(Type cellValueType,
        ImmutableArray<Axis> axes, byte cellValueScaleForFormatting, ImmutableDictionary<string, object> cellValuesRawByAxesBoundsHashBase64) =>
        (AxisMap)typeof(AxisMap<>)
            .MakeGenericType(cellValueType)
            .GetMethod(nameof(ParseValidateAndConstructFromTemplate),
                BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(null, [axes, cellValueScaleForFormatting, cellValuesRawByAxesBoundsHashBase64])!;

    [ConstructorShape]
    protected AxisMap(ImmutableArray<Axis> axes, byte cellValueScaleForFormatting)
    {
        // This is potentially "inefficient" for deserialization; however, there are catostrophic effects to this
        // Axes ordering not being preserved when calling CalculateAxesMultipliers whose results are used by
        // LookupCellValue, so we must do it every construction. Doing so also protects us in the case that we
        // change our algorithms later. Only the OrderBy affects correctness; ThenBy makes Descriptions stable for tests.
        Axes =
        [
            .. axes.OrderBy(static a => a.OrientationRelativeIndex)
                .ThenByDescending(static a => a.IsOrientationHorizontal)
        ];

        (AxesHorizontalMultipliers, AxesVerticalMultipliers) = CalculateAxesMultipliers();

        AxesHorizontalBoundCountsAggregateMultiply = BoundCountsAggregateMultiply(
            Axes.Where(static axis => axis.IsOrientationHorizontal));

        AxesVerticalBoundCountsAggregateMultiply = BoundCountsAggregateMultiply(
            Axes.Where(static axis => !axis.IsOrientationHorizontal));

        // We validate this in AxisMap<TCellValue>.
        CellValueScaleForFormatting = cellValueScaleForFormatting;
    }

    [Key(1)] public ImmutableArray<Axis> Axes { get; }

    internal ImmutableArray<int> AxesHorizontalMultipliers { get; }
    internal ImmutableArray<int> AxesVerticalMultipliers { get; }

    internal int AxesHorizontalBoundCountsAggregateMultiply { get; }
    internal int AxesVerticalBoundCountsAggregateMultiply { get; }

    public abstract Type CellValueType { get; }
    [Key(2)] public byte CellValueScaleForFormatting { get; }

    // Identical to calling BoundCountsAggregateMultiply(Axes), but more efficient.
    internal int CellValuesExpectedCount =>
        AxesHorizontalBoundCountsAggregateMultiply * AxesVerticalBoundCountsAggregateMultiply;

    private (ImmutableArray<int> Horizontal, ImmutableArray<int> Vertical) CalculateAxesMultipliers()
    {
        int[]? horizontal = null;
        int[]? vertical = null;

        foreach (var axis in Axes.ReverseNoBuffer())
        {
            if (axis.IsOrientationHorizontal)
                horizontal ??= new int[axis.OrientationRelativeIndex + 1];
            else
                vertical ??= new int[axis.OrientationRelativeIndex + 1];

            int[] multipliers = axis.IsOrientationHorizontal ? horizontal! : vertical!;

            if (multipliers[0] == 0)
                Array.Fill(multipliers, 1);

            if (axis.OrientationRelativeIndex > 0)
                multipliers[axis.OrientationRelativeIndex - 1] = multipliers[axis.OrientationRelativeIndex] * axis.BoundCount;
        }

        return (horizontal?.ToImmutableArray() ?? [], vertical?.ToImmutableArray() ?? []);
    }

    public void ThrowIfAxesInputTypeMismatches(Type inputType)
    {
        var axesTypeMismatches = Axes
            .Select(a =>
            (
                Axis: a,
                GetterReturnType: PropertyInfoX.GetGetterReturnTypeOrThrow(inputType, a.PropertyPath)
            ))
            .Where(ag =>
            {
                var type = ag.GetterReturnType;

                if (type.IsEnum)
                    type = typeof(string);

                return ag.Axis.BoundType != type;
            })
            .Select(ag => ag.Axis.PropertyPath);

        ArgumentExceptionX.ThrowIfAny(axesTypeMismatches,
            "Axes Types must match their corresponding inputType's Property's Type.");
    }

    public (AxisMapLookupCellValueStatus Status, object? Value) LookupCellValue(object input, StringBuilder? descriptionBuilder = null)
    {
        Throw.IfNull(input);

        int descriptionStartingLength = descriptionBuilder?.Length ?? 0;

        int horizontalIndex = 0;
        int verticalIndex = 0;

        foreach (var axis in Axes)
        {
            int? boundIndex = axis.LookupBoundIndex(input);

            if (boundIndex is null)
            {
                if (descriptionBuilder is not null)
                    descriptionBuilder.Length = descriptionStartingLength;

                return (AxisMapLookupCellValueStatus.CellNotFound, null);
            }

            if (descriptionBuilder is not null)
            {
                if (descriptionBuilder.Length > 0)
                    descriptionBuilder.Append(" and ");

                axis.AppendAxisBoundDescription(descriptionBuilder, boundIndex.Value);
            }

            if (axis.IsOrientationHorizontal)
                horizontalIndex += boundIndex.Value * AxesHorizontalMultipliers[axis.OrientationRelativeIndex];
            else
                verticalIndex += boundIndex.Value * AxesVerticalMultipliers[axis.OrientationRelativeIndex];
        }

        int cellIndex = (AxesHorizontalBoundCountsAggregateMultiply * verticalIndex) + horizontalIndex;

        return GetCellIsNullOrWhiteSpace(cellIndex)
            ? (AxisMapLookupCellValueStatus.CellIsNullOrWhiteSpace, null)
            : (AxisMapLookupCellValueStatus.Success, GetCellValue(cellIndex));
    }

    internal abstract bool GetCellIsNullOrWhiteSpace(int cellIndex);
    internal abstract object? GetCellValue(int cellIndex);

    internal void AppendCellAxesBoundsHashValueBase64Csv(StringBuilder builder) =>
        GetCellAxesBoundsHashBase64s().ForEach((hashBase64, cellIndex) =>
        {
            if (GetCellIsNullOrWhiteSpace(cellIndex))
                return;

            builder.AppendLine();
            builder.Append(hashBase64);
            builder.Append(',');
            AppendCellValueBase64(builder, cellIndex);
        });

    // ReSharper disable once InconsistentNaming // Plural, not a new word for UpperCamelCase.
    protected IEnumerable<string> GetCellAxesBoundsHashBase64s()
    {
        var hasher = new XxHash128();

        for (int cellIndex = 0; cellIndex < CellValuesExpectedCount; cellIndex++)
        {
            foreach (var axisBoundIndex in GetCellAxesBoundIndices(cellIndex))
                axisBoundIndex.Axis.AppendAxisBoundHash(hasher, axisBoundIndex.BoundIndex);

            byte[] hash = hasher.GetHashAndReset();

            yield return Convert.ToBase64String(hash);
        }
    }

    // Internal for testing purposes. Both directions of an AxisMapTemplateWriter + Reader round-trip test
    // use GetCellAxesBoundsHashBase64s under the hood, and many potential bugs in it would be of a nature
    // that they would be "hidden" / "cancelled-out" by the round-trip.
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IEnumerable<(Axis Axis, int BoundIndex)> GetCellAxesBoundIndices(int cellIndex)
    {
        int horizontalIndex = cellIndex % AxesHorizontalBoundCountsAggregateMultiply;
        int verticalIndex = cellIndex / AxesHorizontalBoundCountsAggregateMultiply;

        // We use our default Axes Ordering, which is stable (required!); however, Cell Value hashes will
        // not "match" after Axes reordering or re-orientation, which technically could be supported by
        // sorting by Axis.PropertyPath; however, the value of such a feature is likely zero. The real
        // value is in a user being able to add and remove Bounds to/from an existing Axis without having
        // to reenter existing Cell Values, which our default Axes ordering allows.
        return Axes.Select(axis =>
            (axis,
                axis.IsOrientationHorizontal
                    ? horizontalIndex / AxesHorizontalMultipliers[axis.OrientationRelativeIndex] % axis.BoundCount
                    : verticalIndex / AxesVerticalMultipliers[axis.OrientationRelativeIndex] % axis.BoundCount));
    }

    protected abstract void AppendCellValueBase64(StringBuilder builder, int cellIndex);

    // For testing purposes only. This can be used to generate Templates that can be used to construct
    // AxisMaps used fuzz serialization. Much of this code overlaps with LookupCellValue, so it is less
    // valuable as a tool for testing that stack of methods (incl. Axis.LookupBoundIndex); however, those
    // methods are heavily covered by unit tests.
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal string? LookupCellAxesBoundsHashBase64(object input)
    {
        Throw.IfNull(input);

        var hasher = new XxHash128();

        foreach (var axis in Axes)
        {
            int? boundIndex = axis.LookupBoundIndex(input);

            if (boundIndex is null)
                return null;

            axis.AppendAxisBoundHash(hasher, boundIndex.Value);
        }

        byte[] hash = hasher.GetHashAndReset();

        return Convert.ToBase64String(hash);
    }
}