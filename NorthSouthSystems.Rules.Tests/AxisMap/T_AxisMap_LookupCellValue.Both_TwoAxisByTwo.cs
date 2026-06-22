public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    public void Both_TwoAxisByTwo()
    {
        Test(
            [1, 1],
            [1, 1],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), false)
        );

        Test(
            [1, 2],
            [1, 1],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "C", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), false)
        );

        Test(
            [2, 1],
            [1, 1],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "A"), false)
        );

        Test(
            [1, 1],
            [1, 2],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "C"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), false)
        );

        Test(
            [1, 1],
            [2, 1],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "C", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), false)
        );

        Test(
            [1, 2],
            [2, 1],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "C", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "C", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "B", V1_: "A"), true),
            (new(H0_: "A", H1_: "B", V0_: "C", V1_: "A"), false),
            (new(H0_: "A", H1_: "C", V0_: "B", V1_: "A"), false)
        );

        Test(
            [2, 1],
            [1, 2],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "C"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "C"), false),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "B"), false)
        );

        Test(
            [2, 2],
            [2, 2],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "C"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "C", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "C", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "A"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "B"), true),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "B", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "B", V0_: "B", V1_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "B", V1_: "B"), true),
            (new(H0_: "B", H1_: "B", V0_: "A", V1_: "B"), true),
            (new(H0_: "B", H1_: "B", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "B", V0_: "B", V1_: "B"), true)
        );

        Test(
            [3, 3],
            [3, 3],
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "C"), true),
            (new(H0_: "A", H1_: "A", V0_: "A", V1_: "D"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "C", V1_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "D", V1_: "A"), false),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "C", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "D", V0_: "A", V1_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "A"), true),
            (new(H0_: "D", H1_: "A", V0_: "A", V1_: "A"), false),
            (new(H0_: "A", H1_: "A", V0_: "B", V1_: "B"), true),
            (new(H0_: "A", H1_: "B", V0_: "A", V1_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "A", V1_: "B"), true),
            (new(H0_: "A", H1_: "B", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "A", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "B", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "B", V0_: "B", V1_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "B", V1_: "B"), true),
            (new(H0_: "B", H1_: "B", V0_: "A", V1_: "B"), true),
            (new(H0_: "B", H1_: "B", V0_: "B", V1_: "A"), true),
            (new(H0_: "B", H1_: "B", V0_: "B", V1_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "C", V1_: "C"), true),
            (new(H0_: "A", H1_: "C", V0_: "A", V1_: "C"), true),
            (new(H0_: "C", H1_: "A", V0_: "A", V1_: "C"), true),
            (new(H0_: "A", H1_: "C", V0_: "C", V1_: "A"), true),
            (new(H0_: "C", H1_: "A", V0_: "C", V1_: "A"), true),
            (new(H0_: "C", H1_: "C", V0_: "A", V1_: "A"), true),
            (new(H0_: "A", H1_: "C", V0_: "C", V1_: "C"), true),
            (new(H0_: "C", H1_: "A", V0_: "C", V1_: "C"), true),
            (new(H0_: "C", H1_: "C", V0_: "A", V1_: "C"), true),
            (new(H0_: "C", H1_: "C", V0_: "C", V1_: "A"), true),
            (new(H0_: "C", H1_: "C", V0_: "C", V1_: "C"), true)
        );
    }
}