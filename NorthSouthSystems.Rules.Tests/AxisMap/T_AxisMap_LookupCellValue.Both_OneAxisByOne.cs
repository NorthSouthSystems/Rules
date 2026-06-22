public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    public void Both_OneAxisByOne()
    {
        Test(
            [1],
            [1],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), false),
            (new(H0_: "B", V0_: "A"), false),
            (new(H0_: "B", V0_: "B"), false)
        );

        Test(
            [1],
            [2],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), true),
            (new(H0_: "A", V0_: "C"), false),
            (new(H0_: "B", V0_: "A"), false),
            (new(H0_: "B", V0_: "B"), false),
            (new(H0_: "B", V0_: "C"), false)
        );

        Test(
            [2],
            [1],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), false),
            (new(H0_: "B", V0_: "A"), true),
            (new(H0_: "B", V0_: "B"), false),
            (new(H0_: "C", V0_: "A"), false),
            (new(H0_: "C", V0_: "B"), false)
        );

        Test(
            [2],
            [2],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), true),
            (new(H0_: "A", V0_: "C"), false),
            (new(H0_: "B", V0_: "A"), true),
            (new(H0_: "B", V0_: "B"), true),
            (new(H0_: "B", V0_: "C"), false),
            (new(H0_: "C", V0_: "A"), false),
            (new(H0_: "C", V0_: "B"), false),
            (new(H0_: "C", V0_: "C"), false)
        );

        Test(
            [2],
            [3],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), true),
            (new(H0_: "A", V0_: "C"), true),
            (new(H0_: "A", V0_: "D"), false),
            (new(H0_: "B", V0_: "A"), true),
            (new(H0_: "B", V0_: "B"), true),
            (new(H0_: "B", V0_: "C"), true),
            (new(H0_: "B", V0_: "D"), false),
            (new(H0_: "C", V0_: "A"), false),
            (new(H0_: "C", V0_: "B"), false),
            (new(H0_: "C", V0_: "C"), false),
            (new(H0_: "C", V0_: "D"), false)
        );

        Test(
            [3],
            [2],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), true),
            (new(H0_: "A", V0_: "C"), false),
            (new(H0_: "B", V0_: "A"), true),
            (new(H0_: "B", V0_: "B"), true),
            (new(H0_: "B", V0_: "C"), false),
            (new(H0_: "C", V0_: "A"), true),
            (new(H0_: "C", V0_: "B"), true),
            (new(H0_: "C", V0_: "C"), false),
            (new(H0_: "D", V0_: "A"), false),
            (new(H0_: "D", V0_: "B"), false),
            (new(H0_: "D", V0_: "C"), false)
        );

        Test(
            [3],
            [3],
            (new(H0_: "A", V0_: "A"), true),
            (new(H0_: "A", V0_: "B"), true),
            (new(H0_: "A", V0_: "C"), true),
            (new(H0_: "A", V0_: "D"), false),
            (new(H0_: "B", V0_: "A"), true),
            (new(H0_: "B", V0_: "B"), true),
            (new(H0_: "B", V0_: "C"), true),
            (new(H0_: "B", V0_: "D"), false),
            (new(H0_: "C", V0_: "A"), true),
            (new(H0_: "C", V0_: "B"), true),
            (new(H0_: "C", V0_: "C"), true),
            (new(H0_: "D", V0_: "A"), false),
            (new(H0_: "D", V0_: "B"), false),
            (new(H0_: "D", V0_: "C"), false),
            (new(H0_: "D", V0_: "D"), false)
        );
    }
}