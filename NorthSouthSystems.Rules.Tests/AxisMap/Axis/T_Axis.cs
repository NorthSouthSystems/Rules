using Xunit.Sdk;
using static T_AxisMessagePackX;

public class T_Axis
{
    [Fact]
    public void Constructor_Exceptions()
    {
        Action act;

        act = static () => Axis.ParseValidateAndConstruct(typeof(double), "ThePropertyName", true, 0, ["1.0", "2.0"]);
        act.Should().ThrowExactly<ArgumentException>().WithInnerExceptionExactly<NotSupportedException>().WithMessage(typeof(double).ToString());

        act = static () => AxisBool.ParseValidateAndConstruct(null, true, 0, ["false", "true"]);
        act.Should().ThrowExactly<ArgumentNullException>();

        act = static () => AxisBool.ParseValidateAndConstruct("ThePropertyName", true, -1, ["false", "true"]);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>();

        act = static () => AxisBool.ParseValidateAndConstruct("The Property Name", true, 0, ["false", "true"]);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Whitespace characters are not allowed*");
    }

    internal static void AppendBoundDescription_Test<T>(ImmutableArray<string> boundsRaw,
        params IEnumerable<(int Index, string ExpectedDescription)> testCases) =>
        AppendBoundDescription_Test<T>(boundsRaw, false, testCases);

    internal static void AppendBoundDescription_Test<T>(ImmutableArray<string> boundsRaw, bool omitReverse,
        params IEnumerable<(int Index, string ExpectedDescription)> testCases)
    {
        var builder = new StringBuilder();

        var axis = Construct(typeof(T), boundsRaw);

        WithMessagePackRoundTrip(axis, a =>
        {
            foreach ((int index, string expectedDescription) in testCases)
            {
                a.AppendBoundDescription(builder, index);
                builder.ToString().Should().Be(expectedDescription);

                builder.Clear();
            }
        });

        if (!omitReverse)
        {
            axis = Construct(typeof(T), [.. boundsRaw.ReverseNoBuffer()]);

            WithMessagePackRoundTrip(axis, a =>
            {
                foreach ((int index, string expectedDescription) in testCases)
                {
                    a.AppendBoundDescription(builder, boundsRaw.Length - 1 - index);
                    builder.ToString().Should().Be(expectedDescription);

                    builder.Clear();
                }
            });
        }
    }

    internal static void LookupBoundIndex_Test<T>(ImmutableArray<string> boundsRaw, ImmutableArray<string> expectedBoundToStrings,
        params IEnumerable<(T Value, int? ExpectedIndex)> testCases) =>
        LookupBoundIndex_Test(boundsRaw, expectedBoundToStrings, false, testCases);

    internal static void LookupBoundIndex_Test<T>(ImmutableArray<string> boundsRaw, ImmutableArray<string> expectedBoundToStrings, bool omitReverse,
        params IEnumerable<(T Value, int? ExpectedIndex)> testCases)
    {
        var axis = Construct(typeof(T), boundsRaw);

        WithMessagePackRoundTrip(axis, a =>
        {
            TestBounds(a, expectedBoundToStrings);

            foreach ((var value, int? expectedIndex) in testCases)
                TestLookup(a, value, expectedIndex);
        });

        // Only relevant for AxisBool and AxisNumeric.
        axis = Construct(typeof(T), [.. expectedBoundToStrings]);

        WithMessagePackRoundTrip(axis, a =>
        {
            TestBounds(a, expectedBoundToStrings);

            foreach ((var value, int? expectedIndex) in testCases)
                TestLookup(a, value, expectedIndex);
        });

        if (!omitReverse)
        {
            axis = Construct(typeof(T), [.. boundsRaw.ReverseNoBuffer()]);

            WithMessagePackRoundTrip(axis, a =>
            {
                TestBounds(a, expectedBoundToStrings.ReverseNoBuffer());

                foreach ((var value, int? expectedIndex) in testCases)
                    TestLookup(a, value,
                        expectedIndex is not null
                            ? boundsRaw.Length - 1 - expectedIndex
                            : null);
            });
        }

        static void TestBounds(Axis a, IEnumerable<string> expectedBs)
        {
            a.BoundCount.Should().Be(expectedBs.Count());
            a.BoundToStrings.Should().Equal(expectedBs);
        }

        static void TestLookup(Axis a, T value, int? expected) =>
            a.LookupBoundIndex(T_AxisMapInput.ConstructWithProperty(typeof(T), value)).Should().Be(expected);
    }

    private static Axis Construct(Type t, ImmutableArray<string> boundsRaw)
    {
        var flattenedT = t.UnwrapNullable();

        return Axis.ParseValidateAndConstruct(flattenedT.IsEnum ? typeof(string) : flattenedT, T_AxisMapInput.GetPropertyName(t),
            true, 0, boundsRaw);
    }
}