public class T_AxisBool_ValidateAndConstruct
{
    [Fact]
    public void Exceptions()
    {
        Action act;

        act = static () => Construct("false");
        act.Should().ThrowExactly<ArgumentOutOfRangeException>();

        act = static () => Construct("true", "True");
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Duplicate bounds*");

        static AxisBool Construct(params string[] boundBoolsRaw) =>
            AxisBool.ParseValidateAndConstruct(T_AxisMapInput.GetPropertyName(typeof(bool)), true, 0, [.. boundBoolsRaw]);
    }
}

public class T_AxisBool_LookupBoundIndex
{
    [Fact]
    public void Full()
    {
        T_Axis.LookupBoundIndex_Test(["false", "true"], [false.ToString(), true.ToString()],
            (false, 0),
            (true, 1)
        );

        T_Axis.LookupBoundIndex_Test(["false", "true"], [false.ToString(), true.ToString()],
            ((bool?)null, null),
            (false, 0),
            (true, 1)
        );
    }
}

public class T_AxisBool_AppendBoundDescription
{
    [Fact]
    public void Full()
    {
        T_Axis.AppendBoundDescription_Test<bool>(["false", "true"],
            (0, "== False"),
            (1, "== True")
        );

        T_Axis.AppendBoundDescription_Test<bool?>(["true", "false"],
            (0, "== True"),
            (1, "== False")
        );
    }
}