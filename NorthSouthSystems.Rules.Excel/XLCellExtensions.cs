using ClosedXML.Excel;

namespace Nss.Rules.Excel;

internal static class XLCellExtensions
{
    internal static IXLRange AsTopLeftOfRange(this IXLCell cell, int totalRowCount, int totalColumnCount)
    {
        Throw.IfNull(cell);
        Throw.IfLessThan(totalRowCount, 1);
        Throw.IfLessThan(totalColumnCount, 1);

        var lastCell = cell.Worksheet.Cell(
            cell.Address.RowNumber + totalRowCount - 1,
            cell.Address.ColumnNumber + totalColumnCount - 1);

        return cell.Worksheet.Range(cell, lastCell);
    }
}