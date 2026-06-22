public class T_AxisMapTemplateAxisHeader
{
    [Theory]
    [InlineData("H:0:TheProperty:string", true, 0, "TheProperty", typeof(string))]
    [InlineData("H:1:TheProperty:int", true, 1, "TheProperty", typeof(int))]
    [InlineData("V:0:TheProperty:bool", false, 0, "TheProperty", typeof(bool))]
    [InlineData("V:1:TheProperty:decimal", false, 1, "TheProperty", typeof(decimal))]
    public void Parse(string header,
        bool expectedIsOrientationHorizontal, int expectedOrientationRelativeIndex,
        string expectedPropertyPath, Type expectedBoundType)
    {
        foreach (string headerMutation in MutateHeader(header))
        {
            var result = AxisMapTemplateAxisHeader.Parse(headerMutation);

            result.IsOrientationHorizontal.Should().Be(expectedIsOrientationHorizontal);
            result.OrientationRelativeIndex.Should().Be(expectedOrientationRelativeIndex);
            result.PropertyPath.Should().Be(expectedPropertyPath);
            result.BoundType.Should().Be(expectedBoundType);
        }

        static IEnumerable<string> MutateHeader(string h)
        {
            yield return h;
            yield return h + " ";
            yield return " " + h;

            yield return h.Replace(":", ": ");
            yield return h.Replace(":", " :");
            yield return h.Replace(":", " : ");
        }
    }

    [Fact]
    public void Parse_Exceptions()
    {
        Action act;

        foreach (string improper in new[]
                 {
                     "H:01:TheProperty", "H:10:TheProperty", "H:V:TheProperty", "H0TheProperty", "H0:TheProperty", "H:0TheProperty", "H:TheProperty", "0:TheProperty", "H:0", "H:0:", "u:0:TheProperty",
                     "H:01:TheProperty:string", "H:10:TheProperty:string", "H:V:TheProperty:string", "H0TheProperty:string", "H0:TheProperty:string", "H:0TheProperty:string", "H:TheProperty:string", "0:TheProperty:string", "H:0:string", "H:0::string", "u:0:TheProperty:string"
                 })
        {
            act = () => AxisMapTemplateAxisHeader.Parse(improper);
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Header not properly specified*");
        }

        act = () => AxisMapTemplateAxisHeader.Parse("H:0:TheProperty:Foobar");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Header unsupported Bound Type*");
    }
}