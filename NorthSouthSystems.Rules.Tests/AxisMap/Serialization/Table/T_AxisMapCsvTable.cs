public class T_AxisMapCsvTable
{
    [Fact]
    public void ParseRows_Exceptions()
    {
        Action act;

        act = static () => AxisMapCsvTable.Parse("[int] x [] == int:0\n");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Csv is empty after the keystone*");

        act = static () => AxisMapCsvTable.Parse("[int] x [] == int:0\n,");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Csv must contain at least one non-empty non-whitespace row after the keystone*");

        act = static () => AxisMapCsvTable.Parse("[int] x [] == int:0\n,\n0,0");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Rows with after keystone 0-based indices are completely empty or whitespace*");

        act = static () => AxisMapCsvTable.Parse("[int] x [] == int:0\n0,0\n1");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Rows with after keystone 0-based indices have invalid number of columns*");
    }
}