public partial class T_AxisNumeric_AppendBoundDescription
{
    [Fact]
    public void Dec_OneBound_ScaleZero()
    {
        Test_Dec(["-2"],
            (0, "== -2")
        );

        Test_Dec(["2"],
            (0, "== 2")
        );

        Test_Dec(["> -2"],
            (0, "> -2")
        );

        Test_Dec(["> 2"],
            (0, "> 2")
        );

        Test_Dec([">= -2"],
            (0, ">= -2")
        );

        Test_Dec([">= 2"],
            (0, ">= 2")
        );

        Test_Dec(["< -2"],
            (0, "< -2")
        );

        Test_Dec(["< 2"],
            (0, "< 2")
        );

        Test_Dec(["<= -2"],
            (0, "<= -2")
        );

        Test_Dec(["<= 2"],
            (0, "<= 2")
        );
    }

    [Fact]
    public void Dec_OneBound_ScaleOne()
    {
        Test_Dec(["-2.0"],
            (0, "== -2.0")
        );

        Test_Dec(["2.0"],
            (0, "== 2.0")
        );

        Test_Dec(["> -2.0"],
            (0, "> -2.0")
        );

        Test_Dec(["> 2.0"],
            (0, "> 2.0")
        );

        Test_Dec([">= -2.0"],
            (0, ">= -2.0")
        );

        Test_Dec([">= 2.0"],
            (0, ">= 2.0")
        );

        Test_Dec(["< -2.0"],
            (0, "< -2.0")
        );

        Test_Dec(["< 2.0"],
            (0, "< 2.0")
        );

        Test_Dec(["<= -2.0"],
            (0, "<= -2.0")
        );

        Test_Dec(["<= 2.0"],
            (0, "<= 2.0")
        );
    }

    [Fact]
    public void Dec_OneBound_ScaleTwo()
    {
        Test_Dec(["-2.00"],
            (0, "== -2.00")
        );

        Test_Dec(["2.00"],
            (0, "== 2.00")
        );

        Test_Dec(["> -2.00"],
            (0, "> -2.00")
        );

        Test_Dec(["> 2.00"],
            (0, "> 2.00")
        );

        Test_Dec([">= -2.00"],
            (0, ">= -2.00")
        );

        Test_Dec([">= 2.00"],
            (0, ">= 2.00")
        );

        Test_Dec(["< -2.00"],
            (0, "< -2.00")
        );

        Test_Dec(["< 2.00"],
            (0, "< 2.00")
        );

        Test_Dec(["<= -2.00"],
            (0, "<= -2.00")
        );

        Test_Dec(["<= 2.00"],
            (0, "<= 2.00")
        );
    }
}