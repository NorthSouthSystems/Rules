using NorthSouthSystems.IO;
using System.IO;

public class T_AxisMapTableReader_ParseValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        foreach (var kvp in _exceptionMessagesByFileName)
        {
            act = () => ParseValidateAndConstruct(kvp.Key);
            act.Should().ThrowExactly<ArgumentException>().WithMessage(kvp.Value);
        }
    }

    private static AxisMap ParseValidateAndConstruct(string fileName)
    {
        var map = AxisMapTableReader.ParseValidateAndConstruct(Parse(fileName));
        map.ThrowIfAxesInputTypeMismatches(typeof(T_AxisMapInput));

        return map;
    }

    private static AxisMapXlsxTable Parse(string fileName)
    {
        using var file = File.OpenRead(
            PathX.GetFullPathRelativeToCallerFilePath(
                Path.Combine(
                    nameof(T_AxisMapTableReader_ParseValidateAndConstruct),
                    fileName)));

        return AxisMapXlsxTable.Parse(file);
    }

    // In order of runtime enforcement.
    private static readonly Dictionary<string, string> _exceptionMessagesByFileName = new()
    {
        // ThrowIfVoidAreaContainsData
        ["VoidAreaContainsData.xlsx"] = VoidAreaContainsData,

        // ParseAxesForOrientation
        ["AxisPropertyPathEmptyH.xlsx"] = AxisPropertyPathEmpty,
        ["AxisPropertyPathEmptyV.xlsx"] = AxisPropertyPathEmpty,
        ["AxisPropertyPathFollowersNotVoidH.xlsx"] = AxisPropertyPathFollowersNotVoid,
        ["AxisPropertyPathFollowersNotVoidV.xlsx"] = AxisPropertyPathFollowersNotVoid,
        ["UnableToConstructAxis.xlsx"] = UnableToConstructAxis, // Axis.ParseValidateAndConstruct

        // ParseBoundStringsWithVoids
        ["InterBoundVoidCountsDisagreeH.xlsx"] = InterBoundVoidCountsDisagree,
        ["InterBoundVoidCountsDisagreeV.xlsx"] = InterBoundVoidCountsDisagree,
        ["UnexpectedVoidBound.xlsx"] = UnexpectedVoidBound,
        ["MissingExpectedVoidBound.xlsx"] = MissingExpectedVoidBound,

        // ParseBoundStringsWithRepeats
        ["EarlyBoundRepeat.xlsx"] = EarlyBoundRepeat,
        ["MissingBoundRepeat.xlsx"] = MissingBoundRepeat,
        ["BoundRepeatCountsDisagree.xlsx"] = BoundRepeatCountsDisagree,
        ["UnexpectedBoundRepeat.xlsx"] = UnexpectedBoundRepeat,
        ["InconsistentBoundRepeat.xlsx"] = InconsistentBoundRepeat,

        // ThrowIfRowOrColumnCountMismatch
        ["TableColumnCountAndAxesMismatch.xlsx"] = TableColumnCountAndAxesMismatch,
        ["TableRowCountAndAxesMismatch.xlsx"] = TableRowCountAndAxesMismatch
    };

    private const string VoidAreaContainsData = "Void area must not contain any data*";

    private const string AxisPropertyPathEmpty = "Axis property path must be non-null and non-whitespace*";
    private const string AxisPropertyPathFollowersNotVoid = "Axis property path must be followed by void cells in its row or column*";
    private const string UnableToConstructAxis = "Unable to construct Axis*";

    private const string InterBoundVoidCountsDisagree = "Axis and inter-bound void counts disagree*";
    private const string UnexpectedVoidBound = "Axis has unexpected void bound*";
    private const string MissingExpectedVoidBound = "Axis missing an expected void bound*";

    private const string EarlyBoundRepeat = "Axis early bound repeat*";
    private const string MissingBoundRepeat = "Axis missing bound repeat*";
    private const string BoundRepeatCountsDisagree = "Axis and bound repeat counts disagree*";
    private const string UnexpectedBoundRepeat = "Axis has missing or unexpected bound repetition*";
    private const string InconsistentBoundRepeat = "Axis inconsistent bound repeats*";

    private const string TableColumnCountAndAxesMismatch = "Table ColumnCount and Axes mismatch*";
    private const string TableRowCountAndAxesMismatch = "Table RowCount and Axes mismatch*";
}