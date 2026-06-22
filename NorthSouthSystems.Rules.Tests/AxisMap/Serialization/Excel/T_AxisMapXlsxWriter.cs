using NorthSouthSystems.IO;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using VerifyXunit;
using static T_AxisMapMessagePackX;
using static T_AxisMapTemplateX;
using static T_AxisMapXlsxX;

public class T_AxisMapXlsxWriter
{
    // 1 x 0
    [Fact] public Task OneHCellDouble() => Test(_oneHCellDouble);
    [Fact] public Task OneVCellInt() => Test(_oneVCellInt);
    // 2 x 0
    [Fact] public Task TwoHCellShort() => Test(_twoHCellShort);
    [Fact] public Task TwoVCellLong() => Test(_twoVCellLong);
    // 1 x 1
    [Fact] public Task OneHOneVCellBool() => Test(_oneHOneVCellBool);
    // 2 x 1
    [Fact] public Task TwoHOneVCellDecimal() => Test(_twoHOneVCellDecimal);
    [Fact] public Task OneHTwoVCellString() => Test(_oneHTwoVCellString);
    // 2 x 2
    [Fact] public Task TwoHTwoVCellBool() => Test(_twoHTwoVCellBool);
    // 3 x 3
    [Fact] public Task ThreeHThreeVCellDecimal() => Test(_threeHThreeVCellDecimal);
    // 4 x 3
    [Fact] public Task FourHThreeVCellString() => Test(_fourHThreeVCellString);

    [Theory]
    [MemberData(nameof(Templates))]
    public void WithRoundTrips(string template)
    {
        // NOTE : Our AxisNumeric ParseValidateAndConstruct + BoundToString code standardizes decimal.Scale
        // to the max of all of its Bounds. This means that if in a template there are two Bounds with
        // different decimal.Scale, the Axis will NOT round trip "successfully"; however, that is a feature
        // and not a bug.
        var map = AxisMap.FromTemplate(template);
        map.ThrowIfAxesInputTypeMismatches(typeof(T_AxisMapInput));

        // With*RoundTrip methods all assert the MessagePack bytes afterwards, hence the no-op
        // (no further assertions needed).
        WithMessagePackRoundTrip(map, typeof(T_AxisMapInput), m => { });
        WithTemplateRoundTrip(map, typeof(T_AxisMapInput), m => { });
        WithXlsxRoundTrip(map, typeof(T_AxisMapInput), m => { });
        WithXlsxCsvRoundTrip(map, typeof(T_AxisMapInput), m => { });
    }

    public static IEnumerable<TheoryDataRow<string>> Templates =>
        typeof(T_AxisMapXlsxWriter)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => new TheoryDataRow<string>((string)f.GetValue(null)) { Label = f.Name });

    #region Template Strings

    private const string _oneHCellDouble =
        """
        double:3

        H:0:TheInt:int
        > 10
        > 20
        > 30
        """;

    private const string _oneVCellInt =
        """
        int:0

        V:0:TheDecimal:decimal
        > 80.00
        > 85.00
        > 90.00
        """;

    private const string _twoHCellShort =
        """
        short:0

        H:0:TheInt:int
        <= 10
        <= 20
        <= 30

        H:1:TheBool:bool
        true
        false
        """;

    private const string _twoVCellLong =
        """
        long:0

        V:0:TheDecimal:decimal
        >= 80.00
        >= 90.00

        V:1:TheShort:short
        1
        2
        3
        4
        """;

    private const string _oneHOneVCellBool =
        """
        bool:0

        H:0:TheDecimal:decimal
        > 80.00
        > 85.00
        > 90.00

        V:0:TheInt:int
        10
        20
        30
        """;

    private const string _twoHOneVCellDecimal =
        """
        decimal:2

        H:0:TheString:string
        ~foo
        bar?
        *

        H:1:TheShort:short
        <= 0
        1
        < 100

        V:0:TheBool:bool
        false
        true
        """;

    private const string _oneHTwoVCellString =
        """
        string:0

        H:0:TheInt:int
        <= -100
        < 0
        <= 100

        V:0:TheBool:bool
        true
        false

        V:1:TheShort:short
        >= 0
        >= 700
        >= 800
        999
        """;

    private const string _twoHTwoVCellBool =
        """
        bool:0

        H:0:TheEnum:string
        A
        B
        C
        D

        H:1:TheBool:bool
        false
        true

        V:0:TheInt:int
        > 100000
        > 1000000

        V:1:TheString:string
        Loan
        Mortgage
        Rate
        """;

    private const string _threeHThreeVCellDecimal =
        """
        decimal:4

        H:0:TheBool:bool
        true
        false

        H:1:TheShort:short
        < -1000
        <= 0
        < 1000

        H:2:TheInt:int
        > 100000
        > 1000000

        V:0:TheLong:long
        <= 0
        < 3000000000

        V:1:TheDecimal:decimal
        1.10
        2.20
        3.33

        V:2:TheString:string
        FOO
        bar
        42
        """;

    private const string _fourHThreeVCellString =
        """
        string:0

        H:0:TheEnum:string
        A
        B
        C
        D

        H:1:TheShort:short
        < -1000
        < 1000

        H:2:TheInt:int
        >= 100000
        >= 1000000

        H:3:TheBool:bool
        true
        false

        V:0:TheLong:long
        <= 0
        < 3000000000

        V:1:TheDecimal:decimal
        1.10
        2.20
        3.33

        V:2:TheString:string
        foobar
        *
        """;

    #endregion

    private static Task Test(string template)
    {
        var map = AxisMap.FromTemplate(template);
        map.ThrowIfAxesInputTypeMismatches(typeof(T_AxisMapInput));

        using var memory = new MemoryStream();

        map.WriteXlsx(memory);
        memory.Position = 0;

        string xlsxFilePath =
            PathX.GetFullPathRelativeToCallerFilePath(
                Path.Combine(
                    nameof(T_AxisMapXlsxWriter),
                    string.Create(InvariantCulture, $"{TestContext.Current.TestCase.TestMethodName}.received.xlsx")));

        File.WriteAllBytes(xlsxFilePath, memory.ToArray());

        return Verifier
            .VerifyZip(memory.ToArray(), IsIncluded, fileScrubber: Scrubber)
            .DisableDiff();
    }

    private static bool IsIncluded(ZipArchiveEntry entry)
    {
        string fileName = Path.GetFileName(entry.FullName);

        if (fileName is ".rels")
            return false;

        string extension = Path.GetExtension(fileName);

        if (extension is ".psmdcp")
            return false;

        return true;
    }

    private static void Scrubber(string filePath, StringBuilder builder)
    {
        if (Path.GetFileName(filePath).Equals("sheet1.xml"))
        {
            string raw = builder.ToString();
            builder.Clear();

            string scrubbed = _sheetProtectionRegex.Replace(raw, m => string.Empty);
            builder.Append(scrubbed);
        }
    }

    private static readonly Regex _sheetProtectionRegex = new(@"<x:sheetProtection\s+[^>]*>");
}