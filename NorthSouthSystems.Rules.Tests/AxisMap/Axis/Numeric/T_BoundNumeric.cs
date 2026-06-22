// Most BoundNumeric and OperatorExtension methods are tested via AxisNumericTests.
public class T_BoundNumeric
{
    [Fact]
    public void Parse_Exceptions()
    {
        Action act;

        act = static () => BoundNumeric<int>.Parse(null);
        act.Should().ThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("boundRaw");

        act = static () => BoundNumeric<int>.Parse(" ");
        act.Should().ThrowExactly<ArgumentException>().Which.ParamName.Should().Be("boundRaw");

        act = static () => BoundNumeric<int>.Parse("> = 0");
        act.Should().ThrowExactly<InvalidOperationException>().Which.StackTrace.Should().Contain("Single");

        act = static () => BoundNumeric<int>.Parse(">= 1 0");
        act.Should().ThrowExactly<FormatException>();

        act = static () => BoundNumeric<int>.Parse(" >=1 ");
        act.Should().NotThrow();

        // An int.Parse Exception.
        act = static () => BoundNumeric<int>.Parse("0.0");
        act.Should().ThrowExactly<FormatException>();
    }

    [Fact]
    public void Validate_Exceptions()
    {
        Test(BoundNumericsValidationError.MixedAscendingAndDescending, "1", "3", "2");

        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, "> 1", "< 3");
        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, ">= 1", "< 3");
        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, "> 1", "<= 3");
        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, ">= 1", "<= 3");
        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, "0", "> 1", "< 3");
        Test(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan, "0", "> 1", "< 3", "4");

        Test(BoundNumericsValidationError.OverlappingTwins, "1", "1");
        Test(BoundNumericsValidationError.OverlappingTwins, "> 1", "1", "2");
        Test(BoundNumericsValidationError.OverlappingTwins, "< 1", "1", "0");
        Test(BoundNumericsValidationError.OverlappingTwins, "1", "> 1", "0");
        Test(BoundNumericsValidationError.OverlappingTwins, "1", "< 1", "2");
        Test(BoundNumericsValidationError.OverlappingTwins, "== 1", ">= 1");
        Test(BoundNumericsValidationError.OverlappingTwins, "<= 1", "== 1");
        Test(BoundNumericsValidationError.OverlappingTwins, "0", "== 1", ">= 1");
        Test(BoundNumericsValidationError.OverlappingTwins, "0", "== 1", ">= 1", "3");

        Test(BoundNumericsValidationError.OverlappingOneOffs, "1", "<= 2");
        Test(BoundNumericsValidationError.OverlappingOneOffs, ">= 1", "2");
        //Test(BoundNumericsValidationError.OverlappingOneOffs, ">= 1", ">= 2"); // Explicitly allowed and tested in LookupBoundIndex.
        //Test(BoundNumericsValidationError.OverlappingOneOffs, "<= 1", "<= 2"); // Explicitly allowed and tested in LookupBoundIndex.
        return;

        static void Test(BoundNumericsValidationError expectedError, params string[] boundRaws)
        {
            var forward = boundRaws.Select(BoundNumeric<int>.Parse).ToImmutableArray();
            var reverse = forward.ReverseNoBuffer().ToImmutableArray();

            foreach (var boundNumerics in new[] { forward, reverse })
            {
                var act = () => BoundNumeric<int>.Validate(boundNumerics);

                var exception = act.Should().ThrowExactly<ArgumentException>().Which;
                exception.ParamName.Should().Be("boundNumerics");
                exception.Message.Should().Contain(expectedError.ToString());
            }
        }
    }
}