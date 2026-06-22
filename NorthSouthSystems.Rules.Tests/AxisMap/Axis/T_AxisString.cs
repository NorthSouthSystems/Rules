public class T_AxisString_ValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        act = static () => Construct([]);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("boundsRaw.Length");

        act = static () => Construct([null]);
        act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("boundStrings");

        act = static () => Construct(" ");
        act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("boundStrings");

        act = static () => Construct(new string('f', 1_001));
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("boundStrings");

        foreach (string invalidWhiteSpace in new[] { "foo\tbar", "foo\rbar", "foo\nbar", "foo\r\nbar" })
        {
            act = () => Construct(invalidWhiteSpace);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Space is the only whitespace character allowed*");
        }

        act = static () => Construct("foo bar");
        act.Should().NotThrow();

        act = static () => Construct("foo", "bar*", "bar*", "foobar");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Duplicate bounds*");

        act = static () => Construct("foo", "bar*", "*", "foobar");
        act.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Contain("Match All");

        static AxisString Construct(params string[] boundStrings) =>
            AxisString.ParseValidateAndConstruct(T_AxisMapInput.GetPropertyName(typeof(string)), true, 0, [.. boundStrings]);
    }
}

public class T_AxisString_LookupBoundIndex
{
    [Fact]
    public void Full()
    {
        // Exact

        Test(["FOO", "bar"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", null),
            ("FOObar", null)
        );

        // Case-insensitive

        Test(["FOO", "~bar"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", null)
        );

        // Single character wildcard prefix

        Test(["?OO", "~bar"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", null),

            ("fOO", 0),
            ("bOO", 0),
            ("foO", null),
            ("fOOO", null)
        );

        // Multi character wildcard prefixed

        Test(["FOO", "~*bar"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", 1),

            ("baz", null),
            ("BAZ", null)
        );

        // Single character wildcard suffix

        Test(["FOO", "~ba?"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", null),

            ("baz", 1),
            ("BAZ", 1)
        );

        // Multi character wildcard suffix

        Test(["FOO*", "~bar"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", 0)
        );

        // Both wildcards suffixed

        Test(["FOO*", "~ba?"],
            ("FOO", 0),
            ("bar", 1),
            ("foo", null),
            ("BAR", 1),
            ("FOObar", 0),

            ("baz", 1),
            ("BAZ", 1),

            ("FO", null),
            ("bars", null)
        );

        // Both wildcards inner

        Test(["FOO*r", "~ba?s"],
            ("FOO", null),
            ("bar", null),
            ("foo", null),
            ("BAR", null),
            ("FOObar", 0),

            ("baz", null),
            ("BAZ", null),

            ("FO", null),

            ("FOObr", 0),
            ("FOOr", 0),
            ("FOO!r", 0),

            ("bazs", 1),
            ("BAZS", 1),
            ("bas", null),
            ("BAS", null)
        );

        // Overlap

        Test(["FOO", "~foo*"], true,
            ("FOO", 0),
            ("bar", null),
            ("foo", 1),
            ("BAR", null),
            ("FOObar", 1)
        );
    }

    private static void Test(string[] boundStrings, params (string Value, int? ExpectedIndex)[] testCases) =>
        Test(boundStrings, false, testCases);

    private static void Test(string[] boundStrings, bool omitReverse, params (string Value, int? ExpectedIndex)[] testCases)
    {
        var expandedTestCases = testCases
            .Append((null, null))
            .Append((string.Empty, null))
            .Append((" ", null));

        T_Axis.LookupBoundIndex_Test([.. boundStrings], [.. boundStrings], omitReverse, expandedTestCases);

        T_Axis.LookupBoundIndex_Test([.. boundStrings.Append("*")], [.. boundStrings.Append("*")], true,
            TestCasesWithWildcard(expandedTestCases, boundStrings.Length));
    }

    internal static IEnumerable<(T Value, int? ExpectedIndex)> TestCasesWithWildcard<T>(
        IEnumerable<(T Value, int? ExpectedIndex)> testCases, int wildcardIndex) =>
            testCases.Select(tc => (tc.Value, tc.Value is not null ? (tc.ExpectedIndex ?? wildcardIndex) : tc.ExpectedIndex));
}

public class T_AxisString_AppendBoundDescription
{
    [Fact]
    public void Full()
    {
        T_Axis.AppendBoundDescription_Test<string>(["FOO", "bar"],
            (0, "== 'FOO'"),
            (1, "== 'bar'")
        );

        T_Axis.AppendBoundDescription_Test<string>(["FOO", "~bar"],
            (0, "== 'FOO'"),
            (1, "=~ '~bar'")
        );

        T_Axis.AppendBoundDescription_Test<string>(["FOO", "bar?"],
            (0, "== 'FOO'"),
            (1, "=~ 'bar?'")
        );

        T_Axis.AppendBoundDescription_Test<string>(["FOO*", "*"], true,
            (0, "=~ 'FOO*'"),
            (1, "=~ '*'")
        );
    }
}

public class T_AxisString_Enum_LookupBoundIndex
{
    [Fact]
    public void Enum()
    {
        Test([T_AxisMapInputEnum.A, T_AxisMapInputEnum.B],
            (T_AxisMapInputEnum.A, 0),
            (T_AxisMapInputEnum.B, 1),
            (T_AxisMapInputEnum.C, null),
            (T_AxisMapInputEnum.D, null)
        );

        Test([T_AxisMapInputEnum.B, T_AxisMapInputEnum.A],
            (T_AxisMapInputEnum.A, 1),
            (T_AxisMapInputEnum.B, 0),
            (T_AxisMapInputEnum.C, null),
            (T_AxisMapInputEnum.D, null)
        );

        Test([T_AxisMapInputEnum.B, T_AxisMapInputEnum.C],
            (T_AxisMapInputEnum.A, null),
            (T_AxisMapInputEnum.B, 0),
            (T_AxisMapInputEnum.C, 1),
            (T_AxisMapInputEnum.D, null)
        );

        Test([T_AxisMapInputEnum.C, T_AxisMapInputEnum.B],
            (T_AxisMapInputEnum.A, null),
            (T_AxisMapInputEnum.B, 1),
            (T_AxisMapInputEnum.C, 0),
            (T_AxisMapInputEnum.D, null)
        );

        Test([T_AxisMapInputEnum.A, T_AxisMapInputEnum.B, T_AxisMapInputEnum.C],
            (T_AxisMapInputEnum.A, 0),
            (T_AxisMapInputEnum.B, 1),
            (T_AxisMapInputEnum.C, 2),
            (T_AxisMapInputEnum.D, null)
        );

        Test([T_AxisMapInputEnum.D, T_AxisMapInputEnum.C, T_AxisMapInputEnum.B],
            (T_AxisMapInputEnum.A, null),
            (T_AxisMapInputEnum.B, 2),
            (T_AxisMapInputEnum.C, 1),
            (T_AxisMapInputEnum.D, 0)
        );
    }

    [Fact]
    public void EnumNullable()
    {
        Test_Nullable([T_AxisMapInputEnum.C, T_AxisMapInputEnum.A],
            (T_AxisMapInputEnum.A, 1),
            (T_AxisMapInputEnum.B, null),
            (T_AxisMapInputEnum.C, 0),
            (T_AxisMapInputEnum.D, null)
        );
    }

    private static void Test(T_AxisMapInputEnum[] boundEnums, params (T_AxisMapInputEnum Value, int? ExpectedIndex)[] testCases)
    {
        var boundStrings = boundEnums.Select(b => b.ToString()).ToImmutableArray();

        T_Axis.LookupBoundIndex_Test(boundStrings, boundStrings, testCases);

        boundStrings = [.. boundStrings.Append("*")];

        T_Axis.LookupBoundIndex_Test(boundStrings, boundStrings, true,
            T_AxisString_LookupBoundIndex.TestCasesWithWildcard(testCases, boundEnums.Length));
    }

    private static void Test_Nullable(T_AxisMapInputEnum[] boundEnums, params (T_AxisMapInputEnum? Value, int? ExpectedIndex)[] testCases)
    {
        var boundStrings = boundEnums.Select(b => b.ToString()).ToImmutableArray();

        T_Axis.LookupBoundIndex_Test(boundStrings, boundStrings, testCases);

        boundStrings = [.. boundStrings.Append("*")];

        T_Axis.LookupBoundIndex_Test(boundStrings, boundStrings, true,
            T_AxisString_LookupBoundIndex.TestCasesWithWildcard(testCases, boundEnums.Length));
    }
}

public class T_AxisString_Enum_AppendBoundDescription
{
    [Fact]
    public void Full()
    {
        T_Axis.AppendBoundDescription_Test<T_AxisMapInputEnum>([T_AxisMapInputEnum.A.ToString(), T_AxisMapInputEnum.B.ToString()],
            (0, "== 'A'"),
            (1, "== 'B'")
        );

        T_Axis.AppendBoundDescription_Test<T_AxisMapInputEnum?>([T_AxisMapInputEnum.A.ToString(), T_AxisMapInputEnum.B.ToString()],
            (0, "== 'A'"),
            (1, "== 'B'")
        );
    }
}