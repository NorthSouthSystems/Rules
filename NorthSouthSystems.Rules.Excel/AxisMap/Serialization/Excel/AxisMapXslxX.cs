using ClosedXML.Excel;
using System.ComponentModel;
using System.IO;

namespace Nss.Rules.Excel;

public static class AxisMapXlsxX
{
#pragma warning disable CA1034 // False positive; analyzer bug for C# 14.
    extension(AxisMap)
    {
        public static AxisMap ReadXlsx(Stream stream) =>
            AxisMapTableReader.ParseValidateAndConstruct(
                AxisMapXlsxTable.Parse(stream));
    }
#pragma warning restore

    public static void WriteXlsx(this AxisMap map, Stream stream) =>
        AxisMapXlsxWriter.Write(map, stream);

    // For testing purposes only. We store a copy each Xlsx file as a Csv for simplified diff'ing;
    // however, we do NOT use AxisMap as an intermediary.
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static string ToXlsxToCsv(this AxisMap map)
    {
        using var memory = new MemoryStream();

        map.WriteXlsx(memory);
        memory.Position = 0;

        using var workbook = new XLWorkbook(memory);

        return workbook.ToCsv();
    }

    public static string ToCsv(this XLWorkbook workbook)
    {
        var worksheet = AxisMapXlsxTable.GetWorksheet(workbook);

        var keystoneCell = worksheet.Cell(1, 1);
        keystoneCell.InvalidateFormula();

        int lastRowNumber = worksheet.LastRowUsed(XLCellsUsedOptions.All)!.RowNumber();
        int lastColumnNumber = worksheet.LastColumnUsed(XLCellsUsedOptions.All)!.ColumnNumber();

        return Enumerable.Range(1, lastRowNumber)
            .Select(rowNumber =>
                Enumerable.Range(1, lastColumnNumber)
                    .Select(columnNumber => worksheet.Cell(rowNumber, columnNumber))
                    .Select(AxisMapXlsxTable.CellToString))
            .JoinQuotedRows(StringQuotedSignals.CsvNewRowTolerantWindowsPrimaryRFC4180);
    }
}