using NorthSouthSystems.IO;
using System.IO;

public class T_AxisMapXlsxTable
{
    [Fact]
    public void Parse_Exceptions()
    {
        Action act;

        act = static () => OpenAndParse("WorksheetNotFound.xlsx");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Worksheet * not found*");

        act = static () => OpenAndParse("CellDataTypeNotSupported.xlsx");
        act.Should().ThrowExactly<NotSupportedException>().WithMessage("Row Num: *, Col Num: *, Data Type: *");

        static void OpenAndParse(string fileName)
        {
            string xlsxFilePath =
                PathX.GetFullPathRelativeToCallerFilePath(
                    Path.Combine(
                        nameof(T_AxisMapXlsxTable),
                        fileName));

            using var file = File.OpenRead(xlsxFilePath);

            AxisMapXlsxTable.Parse(file);
        }
    }
}