using ClosedXML.Excel;
using System.IO;

namespace Nss.Rules.Excel;

internal class AxisMapXlsxTable : IAxisMapTable
{
    internal static string WorksheetName { get; } = "NorthSouthSystems.Rules.AxisMap";

    internal static IXLWorksheet GetWorksheet(XLWorkbook workbook)
    {
        Throw.IfNull(workbook).Worksheets.TryGetWorksheet(WorksheetName, out var worksheet);

        return worksheet
            ?? throw new ArgumentException(string.Create(InvariantCulture, $"Worksheet '{WorksheetName}' not found."));
    }

    internal static AxisMapXlsxTable Parse(Stream stream)
    {
        using var workbook = new XLWorkbook(Throw.IfNull(stream));

        return new(GetWorksheet(workbook));
    }

    private AxisMapXlsxTable(IXLWorksheet worksheet)
    {
        var keystoneCell = worksheet.Cell(1, 1);
        keystoneCell.InvalidateFormula();

        Keystone = AxisMapTableKeystone.Parse(keystoneCell.GetText());

        _rows = ParseRows(worksheet);
    }

    public AxisMapTableKeystone Keystone { get; }

    private readonly ImmutableArray<ImmutableArray<(string, object?)>> _rows;

    public int RowCount => _rows.Length;
    public int ColumnCount => _rows[0].Length;

    public string GetString(int rowIndex, int columnIndex) => _rows[rowIndex][columnIndex].Item1;
    public object? GetObject(int rowIndex, int columnIndex) => _rows[rowIndex][columnIndex].Item2;

    public IEnumerable<string> GetRowStrings(int rowIndex) => _rows[rowIndex].Select(cell => cell.Item1);
    public IEnumerable<string> GetColumnStrings(int columnIndex) => _rows.Select(row => row[columnIndex].Item1);

    private static ImmutableArray<ImmutableArray<(string, object?)>> ParseRows(IXLWorksheet worksheet)
    {
        int lastRowNumber = worksheet.LastRowUsed(XLCellsUsedOptions.All)!.RowNumber();
        int lastColumnNumber = worksheet.LastColumnUsed(XLCellsUsedOptions.All)!.ColumnNumber();

        // We must exclude the Keystone row.
        var rows = new List<ImmutableArray<(string, object?)>>(lastRowNumber - 1);

        for (int rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
        {
            var row = new List<(string, object?)>(lastColumnNumber);

            for (int columnNumber = 1; columnNumber <= lastColumnNumber; columnNumber++)
            {
                var cell = worksheet.Cell(rowNumber, columnNumber);

                row.Add((CellToString(cell), CellToObject(cell)));
            }

            rows.Add([.. row]);
        }

        return [.. rows];
    }

    internal static string CellToString(IXLCell cell)
    {
        // We must first get the cell's Value in order to trigger any formula reevaluations that might affect
        // the cell's DataType. We noticed this problem with Keystone: if Value wasn't gotten, DataType would
        // return Blank, and we wouldn't call one of the Get* methods which implicitly gets Value...
        var value = cell.Value;

        return cell.DataType switch
        {
            // All of the AxisMap.Axes Bounds should be "Text" cells, so there is no "round-trip" concern.
            // For AxisMap.CellValues, we use format strings which best represent "round-trip"-able instead
            // of default formats (which are used by XLCellValue.ToString(CultureInfo)).
            XLDataType.Blank => string.Empty,
            XLDataType.Boolean => value.GetBoolean().ToString(InvariantCulture),
            XLDataType.Number => value.GetNumber().ToString("G17", InvariantCulture),
            XLDataType.Text => value.GetText(),

            // DateTime, Error, TimeSpan
            _ => throw CellDataTypeNotSupported(cell)
        };
    }

    private static object? CellToObject(IXLCell cell)
    {
        // We must first get the cell's Value in order to trigger any formula reevaluations that might affect
        // the cell's DataType. We noticed this problem with Keystone: if Value wasn't gotten, DataType would
        // return Blank, and we wouldn't call one of the Get* methods which implicitly gets Value...
        var value = cell.Value;

        return cell.DataType switch
        {
            XLDataType.Blank => null,
            XLDataType.Boolean => value.GetBoolean(),
            XLDataType.Number => value.GetNumber(),
            XLDataType.Text => value.GetText(),

            // DateTime, Error, TimeSpan
            _ => throw CellDataTypeNotSupported(cell)
        };
    }

    private static NotSupportedException CellDataTypeNotSupported(IXLCell cell) =>
        new(string.Create(InvariantCulture, $"Row Num: {cell.Address.RowNumber}, Col Num: {cell.Address.ColumnNumber}, Data Type: {cell.DataType}"));
}