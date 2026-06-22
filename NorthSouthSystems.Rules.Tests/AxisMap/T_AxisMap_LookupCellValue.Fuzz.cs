public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    [Trait("Duration", "Long")]
    public void Fuzz()
    {
        int fuzzAxisMapCount = FuzzAxisMapCount;

        while (fuzzAxisMapCount-- > 0)
        {
            (int[] horizontalBoundCounts, int[] verticalBoundCounts) = GetFuzzBoundCounts();

            int testCaseCount = AxisMap.BoundCountsAggregateMultiply(horizontalBoundCounts)
                * AxisMap.BoundCountsAggregateMultiply(verticalBoundCounts)
                / 2;

            testCaseCount = Math.Max(testCaseCount, 2);

            var testCases = Enumerable.Range(0, testCaseCount)
                .Select(_ => (GetFuzzInput(horizontalBoundCounts, verticalBoundCounts), true));

            Test(
                horizontalBoundCounts,
                verticalBoundCounts,
                [.. testCases]
            );
        }
    }

    private static int FuzzAxisMapCount =>
#if GITHUB_ACTIONS
        200;
#else
        1_000;
#endif

    private static (int[] HorizontalBoundCounts, int[] VerticalBoundCounts) GetFuzzBoundCounts()
    {
        const int maxBoundCount = 4;

        int axesCount = Random.Shared.Next(AxisMap.AxesTotalCountMax) + 1;

        int horizontalAxesCount = Math.Max(
            Random.Shared.Next(Math.Min(axesCount, AxisMap.AxesOrientationCountMax) + 1),
            axesCount - AxisMap.AxesOrientationCountMax);

        int verticalAxesCount = axesCount - horizontalAxesCount;

        horizontalAxesCount.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax); // Sanity
        verticalAxesCount.Should().BeLessThanOrEqualTo(AxisMap.AxesOrientationCountMax);   // Sanity

        return (GetFuzzBoundCounts(horizontalAxesCount), GetFuzzBoundCounts(verticalAxesCount));

        static int[] GetFuzzBoundCounts(int axesCount) =>
            [.. Enumerable.Range(0, axesCount).Select(static _ => Random.Shared.Next(maxBoundCount) + 1)];
    }

    private static TheInput GetFuzzInput(int[] horizontalBoundCounts, int[] verticalBoundCounts)
    {
        return new(
            H0_: GetSuffix(true, 0),
            H1_: GetSuffix(true, 1),
            H2_: GetSuffix(true, 2),
            H3_: GetSuffix(true, 3),
            V0_: GetSuffix(false, 0),
            V1_: GetSuffix(false, 1),
            V2_: GetSuffix(false, 2),
            V3_: GetSuffix(false, 3)
        );

        string GetSuffix(bool isOrientationHorizontal, int orientationRelativeIndex) =>
            FuzzChar(
                GetBoundCount(
                    isOrientationHorizontal
                        ? horizontalBoundCounts
                        : verticalBoundCounts,
                    orientationRelativeIndex));

        static int? GetBoundCount(int[] boundCounts, int axisIndex) =>
            axisIndex < boundCounts.Length
                ? boundCounts[axisIndex]
                : null;

        static string FuzzChar(int? boundCount) =>
            boundCount is not null
                ? ((char)('A' + Random.Shared.Next(boundCount.Value))).ToString()
                : null;
    }
}