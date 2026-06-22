using MoreLinq;
using static T_AxisMapMessagePackX;
using static T_AxisMapTemplateX;
using static T_AxisMapXlsxX;

public partial class T_AxisMap_LookupCellValue
{
    private sealed record TheInput(
        string H0_ = null, string H1_ = null, string H2_ = null, string H3_ = null, string H4_ = null,
        string V0_ = null, string V1_ = null, string V2_ = null, string V3_ = null, string V4_ = null)
    {
        public const char ValuePartsDelimiter = '|';

        public string H0 => Concat(nameof(H0), H0_);
        public string H1 => Concat(nameof(H1), H1_);
        public string H2 => Concat(nameof(H2), H2_);
        public string H3 => Concat(nameof(H3), H3_);
        public string H4 => Concat(nameof(H4), H4_);

        public string V0 => Concat(nameof(V0), V0_);
        public string V1 => Concat(nameof(V1), V1_);
        public string V2 => Concat(nameof(V2), V2_);
        public string V3 => Concat(nameof(V3), V3_);
        public string V4 => Concat(nameof(V4), V4_);

        private static string Concat(string prefix, string suffix) =>
            suffix is not null ? (prefix + suffix) : null;

        public string ExpectedValue =>
            string.Join(ValuePartsDelimiter, GetParts());

        // This ordering simulates the line in AxisMap's constructor that provides guaranteed ordering:
        // axes.OrderBy(a => a.OrientationRelativeIndex).ThenByDescending(a => a.IsOrientationHorizontal)
        public string ExpectedDescription =>
            string.Join(" and ",
                GetParts()
                    .OrderBy(p => p[1])
                    .ThenBy(p => p[0])
                    .Select(p => $"{p[..2]} == '{p}'"));

        private IEnumerable<string> GetParts() =>
            new[] { H0, H1, H2, H3, H4, /**/ V0, V1, V2, V3, V4 }.Where(string.IsNotNullAndNotEmpty);
    }

    private static void Test(int[] horizontalBoundCounts, int[] verticalBoundCounts,
        params (TheInput Input, bool ExpectedFound)[] testCases)
    {
        horizontalBoundCounts.Length.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax);
        verticalBoundCounts.Length.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax);

        var hAxes = horizontalBoundCounts
            .Select((count, index) => ConstructAxis(true, index, count))
            .ToImmutableArray();

        var vAxes = verticalBoundCounts
            .Select((count, index) => ConstructAxis(false, index, count))
            .ToImmutableArray();

        var hCartesian = CreateOrientationCartesian(hAxes.Select(a => a.BoundStrings));
        var vCartesian = CreateOrientationCartesian(vAxes.Select(a => a.BoundStrings));

        var cellValues = vCartesian
            .Cartesian(hCartesian, (v, h) => (object)string.Join(TheInput.ValuePartsDelimiter, h.Concat(v)))
            .ToImmutableArray();

        // Sanity checks
        hCartesian.Length.Should().Be(AxisMap.BoundCountsAggregateMultiply(horizontalBoundCounts));
        hCartesian.Length.Should().Be(AxisMap.BoundCountsAggregateMultiply(hAxes));

        vCartesian.Length.Should().Be(AxisMap.BoundCountsAggregateMultiply(verticalBoundCounts));
        vCartesian.Length.Should().Be(AxisMap.BoundCountsAggregateMultiply(vAxes));

        cellValues.Length.Should().Be(hCartesian.Length * vCartesian.Length);

        // TODO : Test null cell values.
        var map = AxisMap<string>.ParseValidateAndConstructFromTable(
            [.. hAxes.Concat(vAxes).Cast<Axis>()], 0, cellValues);

        map.ThrowIfAxesInputTypeMismatches(typeof(TheInput));

        WithMessagePackRoundTrip(map, typeof(TheInput), RoundTripTest);
        WithTemplateRoundTrip(map, typeof(TheInput), RoundTripTest);
        WithXlsxRoundTrip(map, typeof(TheInput), RoundTripTest);
        WithXlsxCsvRoundTrip(map, typeof(TheInput), RoundTripTest);

        // DO NOT CAPTURE "map" IN ANY CLOSURES!
        void RoundTripTest(AxisMap roundTripMap)
        {
            TestCellAxesBoundIndices(roundTripMap);

            TestCase(hAxes.Length, vAxes.Length, roundTripMap, new(), false, -1); // Empty

            int testCaseIndex = 0;

            foreach ((var input, bool expectedFound) in testCases)
                TestCase(hAxes.Length, vAxes.Length, roundTripMap, input, expectedFound, testCaseIndex++);
        }

        static AxisString ConstructAxis(bool isOrientationHorizontal, int orientationRelativeIndex, int boundCount)
        {
            var bounds = Enumerable.Range(0, boundCount)
                .Select(boundIndex => CreateBound(isOrientationHorizontal, orientationRelativeIndex, boundIndex));

            return AxisString.ParseValidateAndConstruct(
                $"{(isOrientationHorizontal ? "H" : "V")}{orientationRelativeIndex}",
                isOrientationHorizontal, orientationRelativeIndex, [.. bounds]);
        }

        static string CreateBound(bool isOrientationHorizontal, int orientationRelativeIndex, int boundIndex) =>
            $"{(isOrientationHorizontal ? "H" : "V")}{orientationRelativeIndex}{(char)('A' + boundIndex)}";

        static List<string>[] CreateOrientationCartesian(IEnumerable<ImmutableArray<string>> boundses)
        {
            // Create a new array of List<string> with the only element in the array being an empty list.
            var cartesian = new[] { new List<string>() };

            foreach (var bounds in boundses)
                cartesian = [.. cartesian.Cartesian(bounds, (list, bound) => new List<string>(list) { bound })];

            return cartesian;
        }

        static void TestCellAxesBoundIndices(AxisMap m)
        {
            for (int cellIndex = 0; cellIndex < m.CellValuesExpectedCount; cellIndex++)
            {
                var axesBounds = m.GetCellAxesBoundIndices(cellIndex)
                    .Select(abi => CreateBound(abi.Axis.IsOrientationHorizontal, abi.Axis.OrientationRelativeIndex, abi.BoundIndex))
                    .OrderBy(axisBound => axisBound);

                string.Join(TheInput.ValuePartsDelimiter, axesBounds).Should().Be((string)m.GetCellValue(cellIndex));
            }
        }

        static void TestCase(int hAxesCount, int vAxesCount, AxisMap m,
            TheInput input, bool expectedFound, int testCaseIndex)
        {
            var description = new StringBuilder();

            Assert(m.LookupCellValue(input, description));

            Assert(m.LookupCellValue(CreateSuperfluousHorizontalInput(input), description));
            Assert(m.LookupCellValue(CreateSuperfluousVerticalInput(input), description));
            Assert(m.LookupCellValue(CreateSuperfluousBothInput(input), description));

            Assert(m.LookupCellValue(CreateRandomIncompleteInput(input), description), false);

            void Assert((AxisMapLookupCellValueStatus Status, object Value) result, bool? expectedFoundOverride = null)
            {
                string because = string.Create(InvariantCulture, $"{nameof(testCaseIndex)}: {testCaseIndex}");

                result.Status.Should().Be(
                    (expectedFoundOverride ?? expectedFound)
                        ? AxisMapLookupCellValueStatus.Success
                        : AxisMapLookupCellValueStatus.CellNotFound,
                    because);

                result.Value.Should().Be(
                    result.Status == AxisMapLookupCellValueStatus.Success
                        ? input.ExpectedValue
                        : null,
                    because);

                description.ToString().Should().Be(
                    result.Status == AxisMapLookupCellValueStatus.Success
                        ? input.ExpectedDescription
                        : string.Empty,
                    because);

                description.Clear();
            }

            TheInput CreateSuperfluousBothInput(TheInput example) =>
                CreateSuperfluousHorizontalInput(
                    CreateSuperfluousVerticalInput(example));

            TheInput CreateSuperfluousHorizontalInput(TheInput example) =>
                hAxesCount switch
                {
                    0 => example with { H0_ = "A" },
                    1 => example with { H1_ = "A" },
                    2 => example with { H2_ = "A" },
                    3 => example with { H3_ = "A" },
                    4 => example with { H4_ = "A" },

                    // AxisMap.AxesOrientationCountMax == 4.
                    _ => throw new NotSupportedException()
                };

            TheInput CreateSuperfluousVerticalInput(TheInput example) =>
                vAxesCount switch
                {
                    0 => example with { V0_ = "A" },
                    1 => example with { V1_ = "A" },
                    2 => example with { V2_ = "A" },
                    3 => example with { V3_ = "A" },
                    4 => example with { V4_ = "A" },

                    // AxisMap.AxesOrientationCountMax == 4.
                    _ => throw new NotSupportedException()
                };

            TheInput CreateRandomIncompleteInput(TheInput example)
            {
                bool targetH = hAxesCount > 0 && (vAxesCount == 0 || Random.Shared.NextBool());
                int targetIndex = Random.Shared.Next(targetH ? hAxesCount : vAxesCount);

                return targetIndex switch
                {
                    0 => targetH ? example with { H0_ = null } : example with { V0_ = null },
                    1 => targetH ? example with { H1_ = null } : example with { V1_ = null },
                    2 => targetH ? example with { H2_ = null } : example with { V2_ = null },
                    3 => targetH ? example with { H3_ = null } : example with { V3_ = null },
                    _ => targetH ? example with { H4_ = null } : example with { V4_ = null }
                };
            }
        }
    }
}