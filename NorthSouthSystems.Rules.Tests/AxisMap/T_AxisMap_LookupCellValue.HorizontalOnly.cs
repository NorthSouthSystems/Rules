public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    public void HorizontalOnly_OneAxis()
    {
        Test(
            [1],
            [],
            (new(H0_: "A"), true),
            (new(H0_: "B"), false)
        );

        Test(
            [2],
            [],
            (new(H0_: "A"), true),
            (new(H0_: "B"), true),
            (new(H0_: "C"), false)
        );

        Test(
            [3],
            [],
            (new(H0_: "A"), true),
            (new(H0_: "B"), true),
            (new(H0_: "C"), true),
            (new(H0_: "D"), false)
        );
    }

    [Fact]
    public void HorizontalOnly_TwoAxis_CountsOneByX()
    {
        Test(
            [1, 1],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), false),
            (new(H0_: "B", H1_: "A"), false)
        );

        Test(
            [1, 2],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), false),
            (new(H0_: "B", H1_: "A"), false),
            (new(H0_: "B", H1_: "B"), false)
        );

        Test(
            [1, 3],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), true),
            (new(H0_: "A", H1_: "D"), false),
            (new(H0_: "B", H1_: "A"), false),
            (new(H0_: "B", H1_: "B"), false),
            (new(H0_: "B", H1_: "C"), false)
        );
    }

    [Fact]
    public void HorizontalOnly_TwoAxis_CountsXByOne()
    {
        Test(
            [2, 1],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "C", H1_: "A"), false),
            (new(H0_: "A", H1_: "B"), false),
            (new(H0_: "B", H1_: "B"), false)
        );

        Test(
            [3, 1],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "C", H1_: "A"), true),
            (new(H0_: "D", H1_: "A"), false),
            (new(H0_: "A", H1_: "B"), false),
            (new(H0_: "B", H1_: "B"), false),
            (new(H0_: "C", H1_: "B"), false)
        );
    }

    [Fact]
    public void HorizontalOnly_TwoAxis_CountsXAndX()
    {
        Test(
            [2, 2],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), false),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "B", H1_: "B"), true),
            (new(H0_: "B", H1_: "C"), false),
            (new(H0_: "C", H1_: "A"), false)
        );

        Test(
            [2, 3],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), true),
            (new(H0_: "A", H1_: "D"), false),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "B", H1_: "B"), true),
            (new(H0_: "B", H1_: "C"), true),
            (new(H0_: "B", H1_: "D"), false),
            (new(H0_: "C", H1_: "A"), false)
        );

        Test(
            [3, 2],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), false),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "B", H1_: "B"), true),
            (new(H0_: "B", H1_: "C"), false),
            (new(H0_: "C", H1_: "A"), true),
            (new(H0_: "C", H1_: "B"), true),
            (new(H0_: "C", H1_: "C"), false),
            (new(H0_: "D", H1_: "A"), false)
        );

        Test(
            [3, 3],
            [],
            (new(H0_: "A", H1_: "A"), true),
            (new(H0_: "A", H1_: "B"), true),
            (new(H0_: "A", H1_: "C"), true),
            (new(H0_: "A", H1_: "D"), false),
            (new(H0_: "B", H1_: "A"), true),
            (new(H0_: "B", H1_: "B"), true),
            (new(H0_: "B", H1_: "C"), true),
            (new(H0_: "B", H1_: "D"), false),
            (new(H0_: "C", H1_: "A"), true),
            (new(H0_: "C", H1_: "B"), true),
            (new(H0_: "C", H1_: "C"), true),
            (new(H0_: "C", H1_: "D"), false),
            (new(H0_: "D", H1_: "A"), false)
        );
    }
}