using MoreLinq;
using System.IO;

namespace NorthSouthSystems.Rules;

internal static class AxisMapTemplateReader
{
    internal static AxisMap ParseValidateAndConstruct(Stream stream)
    {
        using var reader = new StreamReader(Throw.IfNull(stream), leaveOpen: true);

        return ParseValidateAndConstruct(reader.ReadToEnd());
    }

    internal static AxisMap ParseValidateAndConstruct(string template)
    {
        Throw.IfNullOrWhiteSpace(template);

        (var axesLines, string? cellAxesBoundsHashValueBase64Csv) = ParseSplit(template);

        var cellValueTypeAndScaleLines = axesLines[0];

        if (cellValueTypeAndScaleLines.Length > 1)
            throw new ArgumentException("The first line must be Cell Value Type and optional Scale followed by a blank line.", nameof(template));

        string[] cellValueTypeAndScaleParts = cellValueTypeAndScaleLines[0].Split(':');

        if (cellValueTypeAndScaleParts.Length > 2)
            throw new ArgumentException("Cell Value Type and optional Scale improperly specified.", nameof(template));

        string cellValueTypeRaw = cellValueTypeAndScaleParts[0];
        string cellValueScaleRaw = cellValueTypeAndScaleParts.Skip(1).SingleOrDefault() ?? "0";

        var cellValueType = AxisMapTableKeystone.ParseType(AxisMap.SupportedCellValueTypes, cellValueTypeRaw)
            ?? throw new ArgumentException(string.Create(InvariantCulture, $"Cell Value Type is not supported: {cellValueTypeRaw}"), nameof(template));

        byte cellValueScaleForFormatting = byte.Parse(cellValueScaleRaw, InvariantCulture);

        var axes = axesLines.Skip(1)
            .Select(ConstructAxis)
            .ToImmutableArray();

        var cellValuesRawByAxesBoundsHashBase64 = ParseCellAxesBoundsHashValueBase64Csv(
            cellAxesBoundsHashValueBase64Csv, cellValueType);

        return AxisMap.ParseValidateAndConstructFromTemplate(cellValueType,
            axes, cellValueScaleForFormatting, cellValuesRawByAxesBoundsHashBase64);
    }

    private static (ImmutableArray<ImmutableArray<string>> AxesLines, string? CellAxesBoundsHashValueBase64Csv)
        ParseSplit(string template)
    {
        var axesLinesSplit = new List<string>();
        string? cellAxesBoundsHashPipeValueBase64Lines = null;

        using (var reader = new StringReader(template))
        {
            string? line = reader.ReadLine();

            while (line is not null)
            {
                if (line == AxisMapTemplateWriter.CellAxesBoundsHashPipeValueBase64Boundary)
                {
                    cellAxesBoundsHashPipeValueBase64Lines = reader.ReadToEnd();
                    break;
                }

                axesLinesSplit.Add(line.Trim());

                line = reader.ReadLine();
            }
        }

        var axesLines = axesLinesSplit
            .Segment(string.IsNullOrEmpty)
            .Select(lines => lines
                .Where(string.IsNotNullAndNotEmpty)
                .ToImmutableArray())
            .Where(lines => lines.Length > 0)
            .ToImmutableArray();

        return (axesLines, cellAxesBoundsHashPipeValueBase64Lines);
    }

    private static Axis ConstructAxis(ImmutableArray<string> axisLines)
    {
        var header = AxisMapTemplateAxisHeader.Parse(axisLines[0]);

        return Axis.ParseValidateAndConstruct(header.BoundType,
            header.PropertyPath, header.IsOrientationHorizontal, header.OrientationRelativeIndex,
            [.. axisLines.Skip(1)]);
    }

    private static ImmutableDictionary<string, object> ParseCellAxesBoundsHashValueBase64Csv(
        string? csv, Type cellValueType) =>
        csv is null
            ? ImmutableDictionary<string, object>.Empty
            : csv.SplitQuotedRows(StringQuotedSignals.CsvNewRowTolerantWindowsPrimaryRFC4180)
                .Where(row => row.Any(string.IsNotNullAndNotEmpty))
                .ToImmutableDictionary(
                    row => row[0],
                    row =>
                    {
                        string valueBase64 = row[1];

                        if (cellValueType == typeof(bool)) return (object)BinaryRoundTrip.ReadBase64Bool(valueBase64);
                        else if (cellValueType == typeof(short)) return BinaryRoundTrip.ReadBase64Short(valueBase64);
                        else if (cellValueType == typeof(int)) return BinaryRoundTrip.ReadBase64Int(valueBase64);
                        else if (cellValueType == typeof(long)) return BinaryRoundTrip.ReadBase64Long(valueBase64);
                        else if (cellValueType == typeof(double)) return BinaryRoundTrip.ReadBase64Double(valueBase64);
                        else if (cellValueType == typeof(decimal)) return BinaryRoundTrip.ReadBase64Decimal(valueBase64);
                        else if (cellValueType == typeof(string)) return BinaryRoundTrip.ReadBase64String(valueBase64);
                        else throw new NotSupportedException(cellValueType.ToString());
                    });
}