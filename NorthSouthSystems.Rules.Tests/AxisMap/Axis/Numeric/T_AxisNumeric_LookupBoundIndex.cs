public partial class T_AxisNumeric_LookupBoundIndex
{
    private static void Test(ImmutableArray<string> boundsRaw, params IReadOnlyList<(int Value, int? ExpectedIndex)> testCases)
    {
        Test_ValidateBookendManipulationPrereqs(boundsRaw, testCases);

        TestCore(boundsRaw, testCases);

        int nullCountFront = testCases.TakeWhile(tc => tc.ExpectedIndex is null).Count();
        int nullCountBack = testCases.ReverseNoBuffer().TakeWhile(tc => tc.ExpectedIndex is null).Count();

        if (nullCountFront > 0)
        {
            int value = testCases[nullCountFront - 1].Value;

            TestCore(
                [SwapOp(true, boundsRaw[0]), .. boundsRaw],
                [.. testCases.Select(tc => (tc.Value, tc.Value <= value ? 0 : (tc.ExpectedIndex + 1)))]);
        }

        while (nullCountFront > 0)
        {
            int value = testCases[nullCountFront - 1].Value;

            Test(
                [B("==", value), .. boundsRaw],
                [.. testCases.Select(tc => (tc.Value, tc.Value == value ? 0 : (tc.ExpectedIndex + 1)))]);

            TestCore(
                [B("<", value), .. boundsRaw],
                [.. testCases.Select(tc => (tc.Value, tc.Value < value ? 0 : (tc.ExpectedIndex + 1)))]);

            TestCore(
                [B("<=", value), .. boundsRaw],
                [.. testCases.Select(tc => (tc.Value, tc.Value <= value ? 0 : (tc.ExpectedIndex + 1)))]);

            nullCountFront--;
        }

        if (nullCountBack > 0)
        {
            int value = testCases[^nullCountBack].Value;

            TestCore(
                [.. boundsRaw, SwapOp(false, boundsRaw[^1])],
                [.. testCases.Select(tc => (tc.Value, tc.Value >= value ? boundsRaw.Length : tc.ExpectedIndex))]);
        }

        while (nullCountBack > 0)
        {
            int value = testCases[^nullCountBack].Value;

            Test(
                [.. boundsRaw, B("==", value)],
                [.. testCases.Select(tc => (tc.Value, tc.Value == value ? boundsRaw.Length : tc.ExpectedIndex))]);

            TestCore(
                [.. boundsRaw, B(">", value)],
                [.. testCases.Select(tc => (tc.Value, tc.Value > value ? boundsRaw.Length : tc.ExpectedIndex))]);

            TestCore(
                [.. boundsRaw, B(">=", value)],
                [.. testCases.Select(tc => (tc.Value, tc.Value >= value ? boundsRaw.Length : tc.ExpectedIndex))]);

            nullCountBack--;
        }

        static string SwapOp(bool isFront, string boundRaw)
        {
            var bound = BoundNumeric<int>.Parse(boundRaw);

            string op = bound.Operator switch
            {
                BoundNumericOperator.EqualTo =>
                    isFront ? "<" : ">",
                BoundNumericOperator.LessThan =>
                    isFront ? throw new NotSupportedException() : ">=",
                BoundNumericOperator.LessThanOrEqualTo =>
                    isFront ? throw new NotSupportedException() : ">",
                BoundNumericOperator.GreaterThan =>
                    isFront ? "<=" : throw new NotSupportedException(),
                BoundNumericOperator.GreaterThanOrEqualTo =>
                    isFront ? "<" : throw new NotSupportedException(),

                _ => throw new NotSupportedException(bound.Operator.ToString())
            };

            return B(op, bound.Value);
        }

        static string B(string op, int value) => string.Create(InvariantCulture, $"{op} {value}");
    }

    private static void Test(ImmutableArray<string> boundsRaw, params IEnumerable<(int? Value, int? ExpectedIndex)> testCases) =>
        TestCore(boundsRaw, testCases);

    private static void TestCore<T>(ImmutableArray<string> boundsRaw, IEnumerable<(T Value, int? ExpectedIndex)> testCases)
    {
        var expectedBoundToStrings = boundsRaw
            .Select(br => br.Contains(' ') ? br : ("== " + br))
            .ToImmutableArray();

        T_Axis.LookupBoundIndex_Test(boundsRaw, expectedBoundToStrings, testCases);
    }

    private static void Test_ValidateBookendManipulationPrereqs(
        IReadOnlyList<string> boundsRaw, IReadOnlyList<(int Value, int? ExpectedIndex)> testCases)
    {
        // We require all BoundStrings to be in ascending order. Ascending vs. descending is irrelevant to test
        // coverage because T_Axis.LookupBoundIndex_Test automatically performs a boundStrings "reverse" test.
        if (boundsRaw.Count > 1)
        {
            var boundFirst = BoundNumeric<int>.Parse(boundsRaw[0]);
            var boundLast = BoundNumeric<int>.Parse(boundsRaw[^1]);

            BoundNumeric<int>.IsValueAscending([boundFirst, boundLast]).Should().BeTrue();
        }

        // We require all testCases' Values to be in ascending order.
        for (int testCaseIndex = 0; testCaseIndex < testCases.Count - 1; testCaseIndex++)
            testCases[testCaseIndex].Value.Should().BeLessThan(testCases[testCaseIndex + 1].Value);
    }
}