public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    public void VerticalOnly_OneAxis()
    {
        Test(
            [],
            [1],
            (new(V0_: "A"), true),
            (new(V0_: "B"), false)
        );

        Test(
            [],
            [2],
            (new(V0_: "A"), true),
            (new(V0_: "B"), true),
            (new(V0_: "C"), false)
        );

        Test(
            [],
            [3],
            (new(V0_: "A"), true),
            (new(V0_: "B"), true),
            (new(V0_: "C"), true),
            (new(V0_: "D"), false)
        );
    }

    [Fact]
    public void VerticalOnly_TwoAxis_CountsOneByX()
    {
        Test(
            [],
            [1, 1],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), false),
            (new(V0_: "B", V1_: "A"), false)
        );

        Test(
            [],
            [1, 2],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), false),
            (new(V0_: "B", V1_: "A"), false),
            (new(V0_: "B", V1_: "B"), false)
        );

        Test(
            [],
            [1, 3],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), true),
            (new(V0_: "A", V1_: "D"), false),
            (new(V0_: "B", V1_: "A"), false),
            (new(V0_: "B", V1_: "B"), false),
            (new(V0_: "B", V1_: "C"), false)
        );
    }

    [Fact]
    public void VerticalOnly_TwoAxis_CountsXByOne()
    {
        Test(
            [],
            [2, 1],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "C", V1_: "A"), false),
            (new(V0_: "A", V1_: "B"), false),
            (new(V0_: "B", V1_: "B"), false)
        );

        Test(
            [],
            [3, 1],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "C", V1_: "A"), true),
            (new(V0_: "D", V1_: "A"), false),
            (new(V0_: "A", V1_: "B"), false),
            (new(V0_: "B", V1_: "B"), false),
            (new(V0_: "C", V1_: "B"), false)
        );
    }

    [Fact]
    public void VerticalOnly_TwoAxis_CountsXAndX()
    {
        Test(
            [],
            [2, 2],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), false),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "B", V1_: "B"), true),
            (new(V0_: "B", V1_: "C"), false),
            (new(V0_: "C", V1_: "A"), false)
        );

        Test(
            [],
            [2, 3],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), true),
            (new(V0_: "A", V1_: "D"), false),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "B", V1_: "B"), true),
            (new(V0_: "B", V1_: "C"), true),
            (new(V0_: "B", V1_: "D"), false),
            (new(V0_: "C", V1_: "A"), false)
        );

        Test(
            [],
            [3, 2],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), false),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "B", V1_: "B"), true),
            (new(V0_: "B", V1_: "C"), false),
            (new(V0_: "C", V1_: "A"), true),
            (new(V0_: "C", V1_: "B"), true),
            (new(V0_: "C", V1_: "C"), false),
            (new(V0_: "D", V1_: "A"), false)
        );

        Test(
            [],
            [3, 3],
            (new(V0_: "A", V1_: "A"), true),
            (new(V0_: "A", V1_: "B"), true),
            (new(V0_: "A", V1_: "C"), true),
            (new(V0_: "A", V1_: "D"), false),
            (new(V0_: "B", V1_: "A"), true),
            (new(V0_: "B", V1_: "B"), true),
            (new(V0_: "B", V1_: "C"), true),
            (new(V0_: "B", V1_: "D"), false),
            (new(V0_: "C", V1_: "A"), true),
            (new(V0_: "C", V1_: "B"), true),
            (new(V0_: "C", V1_: "C"), true),
            (new(V0_: "C", V1_: "D"), false),
            (new(V0_: "D", V1_: "A"), false)
        );
    }
}