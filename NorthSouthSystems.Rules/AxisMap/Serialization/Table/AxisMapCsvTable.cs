using System.IO;

namespace Nss.Rules;

internal class AxisMapCsvTable : IAxisMapTable
{
    internal static AxisMapCsvTable Parse(Stream stream)
    {
        using var reader = new StreamReader(Throw.IfNull(stream), leaveOpen: true);

        return Parse(reader.ReadToEnd());
    }

    internal static AxisMapCsvTable Parse(string csv) => new(Throw.IfNullOrWhiteSpace(csv));

    private AxisMapCsvTable(string csv)
    {
        (int firstLineLength, var keystone) = AxisMapTableKeystone.ParseCsvLine(csv);
        Keystone = keystone;

        _rows = ParseRows(Keystone, csv.Skip(firstLineLength));
    }

    public AxisMapTableKeystone Keystone { get; }

    private readonly ImmutableArray<string[]> _rows;

    public int RowCount => _rows.Length;
    public int ColumnCount => _rows[0].Length;

    public string GetString(int rowIndex, int columnIndex) => _rows[rowIndex][columnIndex];
    public object? GetObject(int rowIndex, int columnIndex) => _rows[rowIndex][columnIndex];

    public IEnumerable<string> GetRowStrings(int rowIndex) => _rows[rowIndex];
    public IEnumerable<string> GetColumnStrings(int columnIndex) => _rows.Select(row => row[columnIndex]);

    private static ImmutableArray<string[]> ParseRows(AxisMapTableKeystone keystone, IEnumerable<char> csv)
    {
        var rowsRaw = csv
            .SplitQuotedRows(StringQuotedSignals.CsvNewRowTolerantWindowsPrimaryRFC4180)
            .ToImmutableArray();

        if (rowsRaw.Length == 0)
            throw new ArgumentException("Csv is empty after the keystone.", nameof(csv));

        int columnCount = rowsRaw[0].Length;

        // The "final" blank row when:
        // keystone.VerticalAxesTypes.Length == 0 && columnCount == 1 && string.IsNullOrEmpty(the single cell)
        // is indistinguisable from a missing / elided row. We must assume the "final" row is blank rather than
        // missing because downstream validation would always fail otherwise.
        if (keystone.VerticalAxesTypes.Length == 0 && columnCount == 1)
            rowsRaw = [.. rowsRaw.Append([string.Empty])];

        int rowsTrailingBlankCount = rowsRaw.ReverseNoBuffer()
            .TakeWhile(row => row.All(string.IsNullOrWhiteSpace))
            .Count();

        // An AxisMap with only horizontal Axes and with no CellValues will have a "valid" blank row.
        // This requires conditional logic for rowsTrailingBlankCount and blankRowIndices.
        int rowPossiblyBlankIndex = keystone.VerticalAxesTypes.Length == 0
            ? (2 * keystone.HorizontalAxesTypes.Length)
            : -1;

        rowsTrailingBlankCount = Math.Min(rowsTrailingBlankCount, rowsRaw.Length - rowPossiblyBlankIndex - 1);

        var rows = rowsTrailingBlankCount > 0
            ? [.. rowsRaw.Take(rowsRaw.Length - rowsTrailingBlankCount)]
            : rowsRaw;

        if (rows.Length == 0)
            throw new ArgumentException("Csv must contain at least one non-empty non-whitespace row after the keystone.", nameof(csv));

        var blankRowIndices = rows
            .Select((row, index) => row.Any(string.IsNotNullAndNotWhiteSpace) ? -1 : index)
            .Where(index => index >= 0 && index != rowPossiblyBlankIndex);

        ArgumentExceptionX.ThrowIfAny(blankRowIndices,
            "Rows with after keystone 0-based indices are completely empty or whitespace.",
            originalParamName: nameof(csv));

        var invalidRowIndices = rows
            .Select((row, index) => row.Length == columnCount ? -1 : index)
            .Where(index => index >= 0);

        ArgumentExceptionX.ThrowIfAny(invalidRowIndices,
            "Rows with after keystone 0-based indices have invalid number of columns.",
            originalParamName: nameof(csv));

        return rows;
    }
}