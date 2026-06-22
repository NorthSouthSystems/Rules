using System.Text.RegularExpressions;

namespace Nss.Rules;

internal sealed partial class AxisMapTableKeystone
{
    internal const string InvalidSentinel = "INVALID";

    [GeneratedRegex(@"(^\s*|\s*\[\s*|\s*,\s*|\s*:\s*|\s*\]\s*|\s*==\s*|\s*$)")]
    private static partial Regex KeystoneTriviaRegex();

    [GeneratedRegex(
        @"^\[(?<vTypesCsv>([a-z]+,?)*)\]x\[(?<hTypesCsv>([a-z]+,?)*)\]==(?<cType>[a-z]+)(:(?<cTypeDefaultScale>\d{1,2}))?$",
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex KeystoneRegex();

    internal static (int FirstLineLength, AxisMapTableKeystone Keystone) ParseCsvLine(string csv)
    {
        int firstNewlineIndex = csv.IndexOf('\n', StringComparison.Ordinal);

        if (firstNewlineIndex < 0)
            throw new ArgumentException("Newline not found.", nameof(csv));

        string firstLine = csv[..firstNewlineIndex].TrimEnd('\r');

        if (string.IsNullOrWhiteSpace(firstLine))
            throw new ArgumentException("Keystone not found.", nameof(csv));

        if (firstLine.StartsWith(InvalidSentinel, StringComparison.InvariantCultureIgnoreCase))
            throw new ArgumentException("Keystone indicates that Table contains invalid cell value(s).");

        string[] keystoneRow = firstLine.SplitQuotedRow(StringQuotedSignals.CsvNewRowTolerantWindowsPrimaryRFC4180);
        string keystone = keystoneRow[0];

        if (string.IsNullOrWhiteSpace(keystone))
            throw new ArgumentException("Keystone is blank.", nameof(csv));

        if (keystoneRow.Skip(1).Any(string.IsNotNullAndNotEmpty))
            throw new ArgumentException("Keystone must be followed by empty cells on its row.", nameof(csv));

        return (firstNewlineIndex + 1, Parse(keystone));
    }

    internal static AxisMapTableKeystone Parse(string keystone)
    {
        keystone = KeystoneTriviaRegex().Replace(keystone, m => m.Groups[0].Value.Trim());

        var match = KeystoneRegex().Match(keystone);

        if (!match.Success)
            throw new ArgumentException(string.Create(InvariantCulture, $"Keystone not properly specified. '{keystone}'"), nameof(keystone));

        var hTypes = ParseAxesTypes(match.Groups["hTypesCsv"].Value);
        var vTypes = ParseAxesTypes(match.Groups["vTypesCsv"].Value);

        if (hTypes.Length == 0 && vTypes.Length == 0)
            throw new ArgumentException(string.Create(InvariantCulture, $"Keystone does not contain any Axes. '{keystone}'"), nameof(keystone));

        string cTypeName = match.Groups["cType"].Value;
        var cType =
        (
            Name: cTypeName,
            Type: ParseType(AxisMap.SupportedCellValueTypes, cTypeName)
        );

        var invalidTypeNames = hTypes.Concat(vTypes).Append(cType)
            .Where(static t => t.Type is null)
            .Select(static t => t.Name);

        ArgumentExceptionX.ThrowIfAny(invalidTypeNames,
            "Keystone includes unsupported Type names.",
            originalParamName: nameof(keystone));

        string? cTypeDefaultScaleRaw = match.Groups["cTypeDefaultScale"]?.Value;
        byte cTypeDefaultScale = byte.TryParse(cTypeDefaultScaleRaw, out byte scale) ? scale : (byte)0;

        return new([.. hTypes.Select(static x => x.Type!)], [.. vTypes.Select(static x => x.Type!)],
            cType.Type!, cTypeDefaultScale);
    }

    private static ImmutableArray<(string Name, Type? Type)> ParseAxesTypes(string typeNamesCsv) =>
    [
        .. typeNamesCsv.Split([",", " "], StringSplitOptions.RemoveEmptyEntries)
            .Select(static name =>
            (
                name,
                ParseType(Axis.SupportedBoundTypes, name)
            ))
    ];

    // "internal" so that it can be reused by AxisMapTemplateReader.
    internal static Type? ParseType(IEnumerable<Type> supportedTypes, string typeName) =>
        supportedTypes.SingleOrDefault(type =>
        {
            if (type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                return true;

            if (TypeX.CSharpKeywordsByType.TryGetValue(type, out string? keyword)
                && keyword.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        });

    private static IEnumerable<Type> GetBoundTypes(AxisMap map, bool isOrientationHorizontal) =>
        map.Axes.Where(a => a.IsOrientationHorizontal == isOrientationHorizontal).Select(a => a.BoundType);

    internal AxisMapTableKeystone(AxisMap map)
        : this([.. GetBoundTypes(map, true)], [.. GetBoundTypes(map, false)],
            map.CellValueType, map.CellValueScaleForFormatting)
    { }

    private AxisMapTableKeystone(ImmutableArray<Type> horizontalAxesTypes, ImmutableArray<Type> verticalAxesTypes,
        Type cellValueType, byte cellValueScaleForFormatting)
    {
        if (!cellValueType.IsFloatingPoint())
            Throw.IfGreaterThan(cellValueScaleForFormatting, 0);

        HorizontalAxesTypes = horizontalAxesTypes;
        VerticalAxesTypes = verticalAxesTypes;

        CellValueType = cellValueType;
        CellValueScaleForFormatting = cellValueScaleForFormatting;
    }

    internal ImmutableArray<Type> HorizontalAxesTypes { get; }
    internal ImmutableArray<Type> VerticalAxesTypes { get; }

    internal Type CellValueType { get; }
    internal byte CellValueScaleForFormatting { get; }

    internal int CellValuesRowOffset => HorizontalAxesTypes.Length * 2;
    internal int CellValuesColumnOffset => VerticalAxesTypes.Length * 2;

    public override string ToString() => string.Create(InvariantCulture,
        $"[{AxesTypesToString(VerticalAxesTypes)}] x [{AxesTypesToString(HorizontalAxesTypes)}] == {TypeX.CSharpKeywordsByType[CellValueType]}:{CellValueScaleForFormatting}");

    private static string AxesTypesToString(ImmutableArray<Type> axesTypes) =>
        string.Join(", ", axesTypes.Select(t => TypeX.CSharpKeywordsByType[t]));
}