public partial class T_AxisNumeric_AppendBoundDescription
{
    [Fact]
    public void Int_OneBound()
    {
        Test_Int(["-2"],
            (0, "== -2")
        );

        Test_Int(["2"],
            (0, "== 2")
        );

        Test_Int(["> -2"],
            (0, "> -2")
        );

        Test_Int(["> 2"],
            (0, "> 2")
        );

        Test_Int([">= -2"],
            (0, ">= -2")
        );

        Test_Int([">= 2"],
            (0, ">= 2")
        );

        Test_Int(["< -2"],
            (0, "< -2")
        );

        Test_Int(["< 2"],
            (0, "< 2")
        );

        Test_Int(["<= -2"],
            (0, "<= -2")
        );

        Test_Int(["<= 2"],
            (0, "<= 2")
        );
    }
}