public class T_AxisMapTemplateReader_ParseValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        act = static () => AxisMapTemplateReader.ParseValidateAndConstruct(_firstLineGroupInvalid);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("The first line must be Cell Value Type and optional Scale followed by a blank line*");

        act = static () => AxisMapTemplateReader.ParseValidateAndConstruct(_firstLineSplitInvalid);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Cell Value Type and optional Scale improperly specified*");

        act = static () => AxisMapTemplateReader.ParseValidateAndConstruct(_cellValueTypeNotSupported);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Cell Value Type is not supported*");
    }

    private static readonly string _firstLineGroupInvalid =
        """
        double:3
        H:0:TheInt
        0
        """;

    private static readonly string _firstLineSplitInvalid =
        """
        double:3:

        H:0:TheInt
        0
        """;

    private static readonly string _cellValueTypeNotSupported =
        """
        float:3

        H:0:TheInt
        0
        """;
}