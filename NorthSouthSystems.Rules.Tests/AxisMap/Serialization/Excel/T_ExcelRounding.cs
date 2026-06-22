public class T_ExcelRounding
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.23456789012345)]
    [InlineData(12.3456789012345)]
    [InlineData(1234567890123.45)]
    [InlineData(12345678901234.5)]
    [InlineData(9.99999999999999)]
    [InlineData(99.9999999999999)]
    [InlineData(9999999999999.99)]
    [InlineData(99999999999999.9)]
    [InlineData(1e13 + 0.5)]
    [InlineData(9e13 + 0.9)]
    public void NoRounding(double raw)
    {
        ExcelRounding.RoundSignificantDigitsSlow(raw).Should().Be(raw);
        ExcelRounding.RoundSignificantDigitsSlow(-raw).Should().Be(-raw);
    }
}