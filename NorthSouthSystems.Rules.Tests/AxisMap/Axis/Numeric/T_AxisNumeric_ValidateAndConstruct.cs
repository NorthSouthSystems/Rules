public class T_AxisNumeric_ValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        act = () => AxisNumeric<ulong>.ParseValidateAndConstruct("TheULong", true, 0, ["0"]);
        act.Should().ThrowExactly<NotSupportedException>().Which.Message.Should().Be("TBoundValue == System.UInt64");

        act = () => AxisNumeric<int>.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheInt), true, 0, []);
        act.Should().ThrowExactly<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("boundsRaw.Length");

        // This is a BoundNumeric<TBoundValue>.Parse Exception that is "blocking" the ValidateBoundScale Exception from ever being thrown.
        act = () => AxisNumeric<int>.ParseValidateAndConstruct(nameof(T_AxisMapInput.TheInt), true, 0, ["1.0"]);
        act.Should().ThrowExactly<FormatException>();
    }
}