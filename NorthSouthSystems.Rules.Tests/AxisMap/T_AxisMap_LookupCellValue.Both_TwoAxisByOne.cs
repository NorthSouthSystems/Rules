public partial class T_AxisMap_LookupCellValue
{
    [Fact]
    public void Both_TwoAxisByOne()
    {
        Test(
            [1, 1],
            [1],
            (new(H0_: "A", H1_: "A", V0_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "B"), false),
            (new(H0_: "A", H1_: "B", V0_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A"), false)
        );

        Test(
            [1, 2],
            [1],
            (new(H0_: "A", H1_: "A", V0_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "B"), false),
            (new(H0_: "A", H1_: "B", V0_: "A"), true),
            (new(H0_: "A", H1_: "B", V0_: "B"), false),
            (new(H0_: "A", H1_: "C", V0_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A"), false)
        );

        Test(
            [1, 1],
            [2],
            (new(H0_: "A", H1_: "A", V0_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "C"), false),
            (new(H0_: "A", H1_: "B", V0_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A"), false)
        );

        Test(
            [2, 2],
            [2],
            (new(H0_: "A", H1_: "A", V0_: "A"), true),
            (new(H0_: "A", H1_: "A", V0_: "B"), true),
            (new(H0_: "A", H1_: "A", V0_: "C"), false),
            (new(H0_: "A", H1_: "B", V0_: "A"), true),
            (new(H0_: "A", H1_: "B", V0_: "B"), true),
            (new(H0_: "A", H1_: "B", V0_: "C"), false),
            (new(H0_: "A", H1_: "C", V0_: "A"), false),
            (new(H0_: "B", H1_: "A", V0_: "A"), true),
            (new(H0_: "B", H1_: "A", V0_: "B"), true),
            (new(H0_: "B", H1_: "A", V0_: "C"), false),
            (new(H0_: "B", H1_: "B", V0_: "A"), true),
            (new(H0_: "B", H1_: "B", V0_: "B"), true),
            (new(H0_: "B", H1_: "B", V0_: "C"), false),
            (new(H0_: "B", H1_: "C", V0_: "B"), false),
            (new(H0_: "C", H1_: "A", V0_: "A"), false)
        );

        // Copy-pasted with H and V swaps

        Test(
            [1],
            [1, 1],
            (new(V0_: "A", V1_: "A", H0_: "A"), true),
            (new(V0_: "A", V1_: "A", H0_: "B"), false),
            (new(V0_: "A", V1_: "B", H0_: "A"), false),
            (new(V0_: "B", V1_: "A", H0_: "A"), false)
        );

        Test(
            [1],
            [1, 2],
            (new(V0_: "A", V1_: "A", H0_: "A"), true),
            (new(V0_: "A", V1_: "A", H0_: "B"), false),
            (new(V0_: "A", V1_: "B", H0_: "A"), true),
            (new(V0_: "A", V1_: "B", H0_: "B"), false),
            (new(V0_: "A", V1_: "C", H0_: "A"), false),
            (new(V0_: "B", V1_: "A", H0_: "A"), false)
        );

        Test(
            [2],
            [1, 1],
            (new(V0_: "A", V1_: "A", H0_: "A"), true),
            (new(V0_: "A", V1_: "A", H0_: "B"), true),
            (new(V0_: "A", V1_: "A", H0_: "C"), false),
            (new(V0_: "A", V1_: "B", H0_: "A"), false),
            (new(V0_: "B", V1_: "A", H0_: "A"), false)
        );

        Test(
            [2],
            [2, 2],
            (new(V0_: "A", V1_: "A", H0_: "A"), true),
            (new(V0_: "A", V1_: "A", H0_: "B"), true),
            (new(V0_: "A", V1_: "A", H0_: "C"), false),
            (new(V0_: "A", V1_: "B", H0_: "A"), true),
            (new(V0_: "A", V1_: "B", H0_: "B"), true),
            (new(V0_: "A", V1_: "B", H0_: "C"), false),
            (new(V0_: "A", V1_: "C", H0_: "A"), false),
            (new(V0_: "B", V1_: "A", H0_: "A"), true),
            (new(V0_: "B", V1_: "A", H0_: "B"), true),
            (new(V0_: "B", V1_: "A", H0_: "C"), false),
            (new(V0_: "B", V1_: "B", H0_: "A"), true),
            (new(V0_: "B", V1_: "B", H0_: "B"), true),
            (new(V0_: "B", V1_: "B", H0_: "C"), false),
            (new(V0_: "B", V1_: "C", H0_: "B"), false),
            (new(V0_: "C", V1_: "A", H0_: "A"), false)
        );
    }
}