using MoreLinq;
using Nerdbank.MessagePack;
using PolyType;

namespace NorthSouthSystems.Rules;

// SEE COMMENTS ABOVE AXISMAP BASE CLASS!

public class AxisMap<TCellValue> : AxisMap
{
    internal static AxisMap<TCellValue> ParseValidateAndConstructFromTable(
        ImmutableArray<Axis> axes, byte cellValueScaleForFormatting, ImmutableArray<object?> cellValuesRaw)
    {
        Validate(axes, cellValueScaleForFormatting);

        Throw.IfEqual(cellValuesRaw.IsDefault, true);
        Throw.IfZero(cellValuesRaw.Length);
        Throw.IfGreaterThan(cellValuesRaw.Length, CellValuesCountMax);

        ThrowIfAxesAndCellValuesCountsDisagree(axes, cellValuesRaw.Length);

        var (cellValues, cellIsNullOrWhiteSpaceMask) = ParseCellValues(cellValuesRaw);

        return new(axes, cellValueScaleForFormatting, cellValues, cellIsNullOrWhiteSpaceMask);
    }

    internal static AxisMap<TCellValue> ParseValidateAndConstructFromTemplate(
        ImmutableArray<Axis> axes, byte cellValueScaleForFormatting, ImmutableDictionary<string, object> cellValuesRawByAxesBoundsHashBase64)
    {
        Validate(axes, cellValueScaleForFormatting);

        return new(axes, cellValueScaleForFormatting, cellValuesRawByAxesBoundsHashBase64);
    }

    private static void Validate(ImmutableArray<Axis> axes, byte cellValueScaleForFormatting)
    {
        if (!SupportedCellValueTypes.Contains(typeof(TCellValue)))
            throw new NotSupportedException(string.Create(InvariantCulture, $"{nameof(TCellValue)} == {typeof(TCellValue)}"));

        // Short-circuits AxesOrientationCountMax validation.
        Throw.IfEqual(axes.IsDefault, true);
        Throw.IfZero(axes.Length);
        Throw.IfGreaterThan(axes.Length, AxesTotalCountMax);

        ThrowIfAxesOrientationCountExceeded(axes);
        ThrowIfAxesOutOfOrder(axes);
        ThrowIfAxesDuplicatePropertyPaths(axes);

        if (!typeof(TCellValue).IsFloatingPoint())
            Throw.IfGreaterThan(cellValueScaleForFormatting, 0);
    }

    private static void ThrowIfAxesOrientationCountExceeded(IEnumerable<Axis> axes)
    {
        var axesOrientationCountExceeded = axes.CountBy(a => a.IsOrientationHorizontal)
            .Where(oc => oc.Value > AxesOrientationCountMax)
            .Select(oc => oc.Key ? "Horizontal" : "Vertical");

        ArgumentExceptionX.ThrowIfAny(axesOrientationCountExceeded,
            "Axes count for orientation exceeded.",
            originalParamName: nameof(axes));
    }

    private static void ThrowIfAxesOutOfOrder(IEnumerable<Axis> axes)
    {
        int nextHorizontalIndex = 0;
        int nextVerticalIndex = 0;

        foreach (var axis in axes.OrderBy(a => a.OrientationRelativeIndex))
        {
            if (axis.IsOrientationHorizontal && axis.OrientationRelativeIndex != nextHorizontalIndex++)
                throw new ArgumentException(string.Create(InvariantCulture, $"Axis out of order: {axis.PropertyPath}"));

            if (!axis.IsOrientationHorizontal && axis.OrientationRelativeIndex != nextVerticalIndex++)
                throw new ArgumentException(string.Create(InvariantCulture, $"Axis out of order: {axis.PropertyPath}"));
        }
    }

    private static void ThrowIfAxesDuplicatePropertyPaths(IEnumerable<Axis> axes)
    {
        var axesDuplicatePropertyPaths = axes
            .Select(a => a.PropertyPath)
            .Duplicates();

        ArgumentExceptionX.ThrowIfAny(axesDuplicatePropertyPaths,
            "Axes must have unique PropertyPaths.",
            originalParamName: nameof(axes));
    }

    private static void ThrowIfAxesAndCellValuesCountsDisagree(IEnumerable<Axis> axes, int cellValuesCount)
    {
        int axesCellValuesCount = BoundCountsAggregateMultiply(axes);

        if (axesCellValuesCount != cellValuesCount)
            throw new ArgumentException(
                string.Create(InvariantCulture, $"Axes calculation and CellValues count disagree. Expected: {axesCellValuesCount}, Actual: {cellValuesCount}"));
    }

    private static (ImmutableArray<TCellValue> CellValues, ImmutableArray<bool> CellIsNullOrWhiteSpaceMask)
        ParseCellValues(ImmutableArray<object?> cellValuesRaw)
    {
        var cellValues = new List<TCellValue>(cellValuesRaw.Length);
        var cellIsNullOrWhiteSpaceMask = new List<bool>(cellValuesRaw.Length);

        foreach (object? raw in cellValuesRaw)
        {
            if (raw is null)
            {
                cellValues.Add((TCellValue)DefaultCellValue);
                cellIsNullOrWhiteSpaceMask.Add(true);
            }
            else if (raw is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    cellValues.Add((TCellValue)DefaultCellValue);
                    cellIsNullOrWhiteSpaceMask.Add(true);
                }
                else
                {
                    cellValues.Add((TCellValue)ParseCellValue(text));
                    cellIsNullOrWhiteSpaceMask.Add(false);
                }
            }
            else
            {
                cellValues.Add((TCellValue)Convert.ChangeType(raw, typeof(TCellValue), InvariantCulture));
                cellIsNullOrWhiteSpaceMask.Add(false);
            }
        }

        return ([.. cellValues], [.. cellIsNullOrWhiteSpaceMask]);
    }

    private static object DefaultCellValue =>
        typeof(TCellValue) == typeof(string) ? string.Empty : default(TCellValue)!;

    private static object ParseCellValue(string value)
    {
        var type = typeof(TCellValue);

        if (type == typeof(bool)) return bool.Parse(value);
        else if (type == typeof(short)) return short.Parse(value, InvariantCulture);
        else if (type == typeof(int)) return int.Parse(value, InvariantCulture);
        else if (type == typeof(long)) return long.Parse(value, InvariantCulture);
        else if (type == typeof(double)) return double.Parse(value, InvariantCulture);
        else if (type == typeof(decimal)) return decimal.Parse(value, InvariantCulture);
        else if (type == typeof(string)) return value;
        else throw new NotSupportedException(type.ToString());
    }

    [ConstructorShape]
    private AxisMap(ImmutableArray<Axis> axes, byte cellValueScaleForFormatting,
        ImmutableArray<TCellValue> cellValues, ImmutableArray<bool> cellIsNullOrWhiteSpaceMask)
        : base(axes, cellValueScaleForFormatting)
    {
        CellValues = cellValues;
        CellIsNullOrWhiteSpaceMask = cellIsNullOrWhiteSpaceMask;
    }

    private AxisMap(ImmutableArray<Axis> axes, byte cellValueScaleForFormatting,
        ImmutableDictionary<string, object> cellValuesRawByAxesBoundsHashBase64)
        : base(axes, cellValueScaleForFormatting)
    {
        var cellValuesRaw = GetCellAxesBoundsHashBase64s()
            .Select(hash => cellValuesRawByAxesBoundsHashBase64.TryGetValue(hash, out object? value) ? value : null)
            .ToImmutableArray();

        (CellValues, CellIsNullOrWhiteSpaceMask) = ParseCellValues(cellValuesRaw);
    }

    // We store CellValues and CellIsNullOrWhiteSpaceMask separately for two reasons:
    // 1. To simplify the DerivedTypeShapes that we must declare on AxisMap; i.e. with our tactic,
    //    we do not need to specify both Nullable<> and non-Nullable<> variants of integral Types.
    // 2. To increase density for large AxisMaps. Nullable<> requires a struct that contains a
    //    bool indicating null or not. Due to CPU alignment requirements, in most cases, that bool
    //    requires 4 (8?) bytes.
    [Key(3)] public ImmutableArray<TCellValue> CellValues { get; }
    [Key(4)] public ImmutableArray<bool> CellIsNullOrWhiteSpaceMask { get; }

    public override Type CellValueType => typeof(TCellValue);

    internal override bool GetCellIsNullOrWhiteSpace(int cellIndex) => CellIsNullOrWhiteSpaceMask[cellIndex];
    internal override object? GetCellValue(int cellIndex) => CellValues[cellIndex];

    protected override void AppendCellValueBase64(StringBuilder builder, int cellIndex)
    {
        object value = CellValues[cellIndex]!;

        if (value is bool @bool) BinaryRoundTrip.WriteBase64Bool(@bool, s => builder.Append(s));
        else if (value is short @short) BinaryRoundTrip.WriteBase64Short(@short, s => builder.Append(s));
        else if (value is int @int) BinaryRoundTrip.WriteBase64Int(@int, s => builder.Append(s));
        else if (value is long @long) BinaryRoundTrip.WriteBase64Long(@long, s => builder.Append(s));
        else if (value is double @double) BinaryRoundTrip.WriteBase64Double(@double, s => builder.Append(s));
        else if (value is decimal @decimal) BinaryRoundTrip.WriteBase64Decimal(@decimal, s => builder.Append(s));
        else if (value is string @string) BinaryRoundTrip.WriteBase64String(@string, s => builder.Append(s));
        else throw new NotSupportedException(value.GetType().ToString());
    }
}