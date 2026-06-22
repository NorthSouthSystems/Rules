using MoreLinq;
using static T_AxisMapMessagePackX;
using static T_AxisMapTemplateX;
using static T_AxisMapXlsxX;

public class T_AxisMap
{
    [Fact]
    [Trait("Duration", "Long")]
    public void Fuzz()
    {
        int fuzzAxisMapCount = FuzzAxisMapCount;

        while (fuzzAxisMapCount-- > 0)
        {
            var (boundses, axes) = FuzzAxes();
            var (cellValueType, inputs) = FuzzInput.Construct(boundses);

            byte cellValueScaleForFormatting = cellValueType.IsFloatingPoint() ? (byte)15 : (byte)0; // Only relevant for Excel UI.

            var tempMap = AxisMap.ParseValidateAndConstructFromTemplate(cellValueType, axes,
                cellValueScaleForFormatting, ImmutableDictionary<string, object>.Empty);

            tempMap.ThrowIfAxesInputTypeMismatches(typeof(FuzzInput));

            var cellValuesByAxesBoundsHashBase64 = inputs
                .Select(input => (Input: input, Hash: tempMap.LookupCellAxesBoundsHashBase64(input)))
                .Where(iAndH => iAndH.Hash is not null)
                .ToImmutableDictionary(iAndH => iAndH.Hash, iAndH => iAndH.Input.CellValue);

            var map = AxisMap.ParseValidateAndConstructFromTemplate(cellValueType, axes,
                cellValueScaleForFormatting, cellValuesByAxesBoundsHashBase64);

            map.ThrowIfAxesInputTypeMismatches(typeof(FuzzInput));

            WithMessagePackRoundTrip(map, typeof(FuzzInput), RoundTripTest);
            WithTemplateRoundTrip(map, typeof(FuzzInput), RoundTripTest);
            WithXlsxRoundTrip(map, typeof(FuzzInput), RoundTripTest);
            WithXlsxCsvRoundTrip(map, typeof(FuzzInput), RoundTripTest);

            void RoundTripTest(AxisMap roundTripMap)
            {
                foreach (var input in inputs)
                {
                    var result = roundTripMap.LookupCellValue(input);

                    result.Status.Should().Be(input.CellValue is null
                        ? AxisMapLookupCellValueStatus.CellIsNullOrWhiteSpace
                        : AxisMapLookupCellValueStatus.Success);

                    if (input.CellValue is not null)
                        result.Value.Should().Be(input.CellValue);
                }
            }
        }
    }

    private static int FuzzAxisMapCount =>
#if GITHUB_ACTIONS
        200;
#else
        1_000;
#endif

    private static (ImmutableArray<ImmutableArray<object>> Boundses, ImmutableArray<Axis> Axes) FuzzAxes()
    {
        const int maxBoundCount = 4;

        int axesCount = Random.Shared.Next(Axis.SupportedBoundTypes.Count) + 1;

        var boundTypes = Axis.SupportedBoundTypes
            .Shuffle(Random.Shared)
            .Take(axesCount)
            .ToImmutableArray();

        int horizontalAxesCount = Math.Max(
            Random.Shared.Next(Math.Min(axesCount, AxisMap.AxesOrientationCountMax) + 1),
            axesCount - AxisMap.AxesOrientationCountMax);

        int verticalAxesCount = axesCount - horizontalAxesCount;

        horizontalAxesCount.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax); // Sanity
        verticalAxesCount.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax);   // Sanity

        var boundses = new List<ImmutableArray<object>>(axesCount);
        var axes = new List<Axis>(axesCount);

        foreach (var boundType in boundTypes)
        {
            int suggestedBoundCount = Random.Shared.Next(maxBoundCount) + 1;

            var bounds = FuzzInput.GetBounds(boundType, suggestedBoundCount);
            var boundStrings = bounds.Select(b => b.ToString());

            var axis = Axis.ParseValidateAndConstruct(boundType, FuzzInput.BoundTypeToPropertyName(boundType),
                horizontalAxesCount > 0, horizontalAxesCount > 0 ? --horizontalAxesCount : --verticalAxesCount,
                [.. boundStrings]);

            boundses.Add(bounds);
            axes.Add(axis);
        }

        axes.Count.Should().Be(axesCount); // Sanity

        return ([.. boundses], [.. axes]);
    }

    private class FuzzInput
    {
        internal static string BoundTypeToPropertyName(Type boundType)
        {
            if (boundType == typeof(bool)) return nameof(TheBool);
            else if (boundType == typeof(short)) return nameof(TheShort);
            else if (boundType == typeof(int)) return nameof(TheInt);
            else if (boundType == typeof(long)) return nameof(TheLong);
            else if (boundType == typeof(decimal)) return nameof(TheDecimal);
            else if (boundType == typeof(string)) return nameof(TheString);
            else throw new NotSupportedException(boundType.ToString());
        }

        internal static ImmutableArray<object> GetBounds(Type boundType, int suggestedCount)
        {
            int scale = boundType.IsFloatingPoint()
                ? Random.Shared.Next(DecimalMaxScale) + 1
                : 0;

            if (boundType == typeof(bool)) return [.. new[] { true, false }.Shuffle(Random.Shared)];
            else if (boundType == typeof(short)) return Get(() => (short)Random.Shared.Next());
            else if (boundType == typeof(int)) return Get(() => (int)Random.Shared.NextInt64());
            else if (boundType == typeof(long)) return Get(() => GetRandomLongForExcel());
            else if (boundType == typeof(decimal)) return Get(() => GetRandomDecimalForExcel(scale));
            else if (boundType == typeof(string)) return [.. _boundStrings.Shuffle(Random.Shared).Take(suggestedCount)];
            else throw new NotSupportedException(boundType.ToString());

            ImmutableArray<object> Get(Func<object> selector) =>
            [
                .. Enumerable.Range(0, int.MaxValue)
                    .Select(_ => selector())
                    .Distinct()
                    .Take(suggestedCount) // Take before OrderBy else we'd be ordering 2B items.
                    .OrderBy(x => x)      // AxisNumeric BoundsNumeric must be ordered.
            ];
        }

        // Excel rounds to 15 significant digits. long.MaxValue has more than that. 48-bit should cover
        // most if not all use cases.
        private static long GetRandomLongForExcel() =>
            (Random.Shared.NextInt64() >> 16) * (Random.Shared.NextBool() ? 1 : -1);

        // Excel rounds to 15 significant digits. We are going to generate decimals with exactly 15.
        //
        // We use a decimal DecimalMultiplier with 6 fractional digits to ensure that we have at least
        // 6 fractional digits in case they are desired via scale == 6 (otherwise trailing zero's could
        // get stripped in double to decimal conversion - doubles have no concept of trailing zeros -
        // and cause BoundNumeric<decimal> to not round-trip successfully).
        private const decimal DecimalMultiplier = 100_000_000.000_000m;
        private const int DecimalMaxScale = 6;

        private static decimal GetRandomDecimalForExcel(int scale = DecimalMaxScale) =>
            Math.Round((decimal)Random.Shared.NextDouble() * DecimalMultiplier, scale);

        private static readonly ImmutableArray<string> _boundStrings =
        [
            "foo",
            "bar",
            "FOOBAR",
            "01234",
            "2025-09-22"
            //"'singlequote" ClosedXML automatically removes single quote prefixes from string cells.
        ];

        internal static (Type CellValueType, ImmutableArray<FuzzInput> Inputs)
            Construct(ImmutableArray<ImmutableArray<object>> boundses)
        {
            // Create a new array of List<object> with the only element in the array being an empty list.
            var cartesian = new[] { new List<object>() };

            foreach (var bounds in boundses)
                cartesian = [.. cartesian.Cartesian(bounds, (list, bound) => new List<object>(list) { bound })];

            var cellValueType = RandomCellValueType();
            var inputs = cartesian.Select(list => new FuzzInput(list, cellValueType)).ToImmutableArray();

            return (cellValueType, inputs);
        }

        private static Type RandomCellValueType() =>
            AxisMap.SupportedCellValueTypes
                .Shuffle(Random.Shared)
                .First();

        private FuzzInput(List<object> propertyValues, Type cellValueType)
        {
            foreach (object value in propertyValues)
            {
                if (value is bool @bool) TheBool = @bool;
                else if (value is short @short) TheShort = @short;
                else if (value is int @int) TheInt = @int;
                else if (value is long @long) TheLong = @long;
                else if (value is decimal @decimal) TheDecimal = @decimal;
                else if (value is string @string) TheString = @string;
                else throw new NotSupportedException(value.GetType().ToString());
            }

            // 33% of the time, leave CellValue == null.
            if (Random.Shared.Next(3) == 0)
                return;

            // NOTE : Round-tripping fractional decimals through Excel and having bit-for-bit MessagePack
            // matching is not possible becaseu Excel converts all numbers to 15-significant digit doubles.
            // The issues include doubles not being able to represent many fractional numbers precisely and
            // also trailing zeros being stripped automatically when converted to double.
            if (cellValueType == typeof(bool)) CellValue = Random.Shared.NextBool();
            else if (cellValueType == typeof(short)) CellValue = (short)Random.Shared.Next();
            else if (cellValueType == typeof(int)) CellValue = (int)Random.Shared.NextInt64();
            else if (cellValueType == typeof(long)) CellValue = GetRandomLongForExcel();
            else if (cellValueType == typeof(double)) CellValue = (double)GetRandomDecimalForExcel();
            else if (cellValueType == typeof(decimal)) CellValue = GetRandomLongForExcel(); // Intentional; see comment above.
            else if (cellValueType == typeof(string)) CellValue = _boundStrings.Shuffle(Random.Shared).First();
            else throw new NotSupportedException(cellValueType.ToString());
        }

        public bool TheBool { get; }
        public short TheShort { get; }
        public int TheInt { get; }
        public long TheLong { get; }
        public decimal TheDecimal { get; }
        public string TheString { get; }

        public object CellValue { get; }
    }
}