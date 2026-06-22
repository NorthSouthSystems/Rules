public class T_AxisMapTableKeystone
{
    [Theory]
    [MemberData(nameof(ParseTestCases))]
    public void ParseCsvLine(string keystone,
        Type[] expectedVerticalAxesTypes,
        Type[] expectedHorizontalAxesTypes,
        Type expectedCellValueType)
    {
        if (keystone.Contains(','))
            keystone = string.Create(InvariantCulture, $"\"{keystone}\"");

        Test(keystone + "\n", keystone.Length + 1);
        Test(keystone + "\n\n", keystone.Length + 1);

        Test(keystone + "\r\n", keystone.Length + 2);
        Test(keystone + "\r\n\r\n", keystone.Length + 2);

        Test(keystone + ",\n", keystone.Length + 2);
        Test(keystone + ",\n\n", keystone.Length + 2);

        Test(keystone + ",\r\n", keystone.Length + 3);
        Test(keystone + ",\r\n\r\n", keystone.Length + 3);

        Test(keystone + ",\n,", keystone.Length + 2);
        Test(keystone + ",\n,\n", keystone.Length + 2);

        Test(keystone + ",\r\n,", keystone.Length + 3);
        Test(keystone + ",\r\n,\r\n", keystone.Length + 3);

        Test(keystone + ",,\n,,", keystone.Length + 3);
        Test(keystone + ",,\n,,\n", keystone.Length + 3);

        Test(keystone + ",,\r\n,,", keystone.Length + 4);
        Test(keystone + ",,\r\n,,\r\n", keystone.Length + 4);

        void Test(string csvLine, int expectedFirstLineLength)
        {
            (int resultFirstLineLength, var resultKeystone) = AxisMapTableKeystone.ParseCsvLine(csvLine);

            resultFirstLineLength.Should().Be(expectedFirstLineLength);

            resultKeystone.VerticalAxesTypes.Should().Equal(expectedVerticalAxesTypes);
            resultKeystone.HorizontalAxesTypes.Should().Equal(expectedHorizontalAxesTypes);
            resultKeystone.CellValueType.Should().Be(expectedCellValueType);
        }
    }

    [Fact]
    public void ParseCsvLine_Exceptions()
    {
        Action act;

        foreach (string keystone in ParseTestCases().Select(tc => tc.Data.Item1))
        {
            act = () => AxisMapTableKeystone.ParseCsvLine(keystone);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Newline not found*");
        }

        foreach (string keystoneNotFound in new[] { "\n", "\r\n" })
        {
            act = () => AxisMapTableKeystone.ParseCsvLine(keystoneNotFound);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone not found*");
        }

        foreach (string invalid in new[] { "INVALID\n", "INVALID\r\n", "invalid,\n", "invalid,\r\n" })
        {
            act = () => AxisMapTableKeystone.ParseCsvLine(invalid);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone indicates that Table contains invalid cell value*");
        }

        foreach (string keystoneIsBlank in new[] { ",\n", ",\r\n", " ,\n", " ,\r\n" })
        {
            act = () => AxisMapTableKeystone.ParseCsvLine(keystoneIsBlank);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone is blank*");
        }

        foreach (string followedNotEmpty in new[] { "keystone,a\n", "keystone,a\r\n", "keystone, \n", "keystone, \r\n" })
        {
            act = () => AxisMapTableKeystone.ParseCsvLine(followedNotEmpty);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone must be followed by empty cells on its row*");
        }
    }

    // HORIZONTAL AND VERTICAL PARAMETER ORDER IS FLIPPED (from our normal alphabetical ordering elsewhere)
    // IN ORDER TO ALIGN WITH HOW KEYSTONE IS WRITTEN IN THE TABLE!
    //
    // We are using MemberData instead of InlineData because we encountered the following Exception with the
    // latter (only in the Tests Output Window; the Exception was not blocking AFAICT):
    // Unable to serialize argument 'System.Object[]' for test method 'NorthSouthSystems.Rules.AxisMapTableKeystoneTests.Parse'
    // System.Runtime.Serialization.SerializationException: Type 'Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.NonErrorNamedTypeSymbol' in Assembly 'Microsoft.CodeAnalysis.CSharp, Version=4.14.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' is not marked as serializable.
    [Theory]
    [MemberData(nameof(ParseTestCases))]
    public void Parse(string keystone,
        Type[] expectedVerticalAxesTypes,
        Type[] expectedHorizontalAxesTypes,
        Type expectedCellValueType)
    {
        foreach (string keystoneMutation in MutateKeystone(keystone))
        {
            var result = AxisMapTableKeystone.Parse(keystoneMutation);

            result.VerticalAxesTypes.Should().Equal(expectedVerticalAxesTypes);
            result.HorizontalAxesTypes.Should().Equal(expectedHorizontalAxesTypes);
            result.CellValueType.Should().Be(expectedCellValueType);
        }

        static IEnumerable<string> MutateKeystone(string ks)
        {
            yield return ks;
            yield return ks.ToUpperInvariant();

            string mutated = (ks + " ")
                .Replace("[", "  [  ")
                .Replace("]", "  ]  ")
                .Replace(", ", "  ,  ")
                .Replace(" x ", "  x  ")
                .Replace("==", "  ==  ");

            yield return mutated;
            yield return mutated.ToUpperInvariant();
        }
    }

    [Fact]
    public void Parse_Exceptions()
    {
        Action act;

        foreach (string improper in new[] { "[int] [] == string", "[int] == string", "[int x ] == string", "[int] x [] = string", "[int] x [] ==", "[int] x []" })
        {
            act = () => AxisMapTableKeystone.Parse(improper);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone not properly specified*");
        }

        act = () => AxisMapTableKeystone.Parse("[] x [] == string");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone does not contain any Axes*");

        act = () => AxisMapTableKeystone.Parse("[byte] x [] == string");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Keystone includes unsupported Type names*");
    }

    // HORIZONTAL AND VERTICAL PARAMETER ORDER IS FLIPPED (from our normal alphabetical ordering elsewhere)
    // IN ORDER TO ALIGN WITH HOW KEYSTONE IS WRITTEN IN THE TABLE!
    public static IEnumerable<TheoryDataRow<string, Type[], Type[], Type>> ParseTestCases()
    {
        yield return new
        (
            "[int] x [] == string",
            [typeof(int)],
            [],
            typeof(string)
        );
        yield return new
        (
            "[] x [decimal] == string",
            [],
            [typeof(decimal)],
            typeof(string)
        );
        yield return new
        (
            "[int] x [decimal] == string",
            [typeof(int)],
            [typeof(decimal)],
            typeof(string)
        );
        yield return new
        (
            "[int] x [decimal, bool] == string",
            [typeof(int)],
            [typeof(decimal), typeof(bool)],
            typeof(string)
        );
        yield return new
        (
            "[int, string] x [decimal] == double",
            [typeof(int), typeof(string)],
            [typeof(decimal)],
            typeof(double)
        );
        yield return new
        (
            "[int, string] x [decimal, bool] == double",
            [typeof(int), typeof(string)],
            [typeof(decimal), typeof(bool)],
            typeof(double)
        );
        yield return new
        (
            "[int, string, short] x [decimal, bool, long] == double",
            [typeof(int), typeof(string), typeof(short)],
            [typeof(decimal), typeof(bool), typeof(long)],
            typeof(double)
        );
    }
}