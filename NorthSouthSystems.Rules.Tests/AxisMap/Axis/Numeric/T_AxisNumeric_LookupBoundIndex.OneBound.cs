public partial class T_AxisNumeric_LookupBoundIndex
{
    [Fact]
    public void OneBound()
    {
        // -2

        Test(["-2"],
            (-3, null),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["> -2"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test([">= -2"],
            (-3, null),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test(["< -2"],
            (-3, 0),
            (-2, null),
            (-1, null),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["<= -2"],
            (-3, 0),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        // -1

        Test(["-1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["> -1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test([">= -1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test(["< -1"],
            (-3, 0),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["<= -1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        // 0

        Test(["0"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["> 0"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test([">= 0"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test(["< 0"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, null),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["<= 0"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, null),
            (2, null),
            (3, null)
        );

        // 1

        Test(["1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, 0),
            (2, null),
            (3, null)
        );

        Test(["> 1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, null),
            (2, 0),
            (3, 0)
        );

        Test([">= 1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, 0),
            (2, 0),
            (3, 0)
        );

        Test(["< 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["<= 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, null),
            (3, null)
        );

        // 2

        Test(["2"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, null),
            (2, 0),
            (3, null)
        );

        Test(["> 2"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, null),
            (2, null),
            (3, 0)
        );

        Test([">= 2"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, null),
            (2, 0),
            (3, 0)
        );

        Test(["< 2"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, null),
            (3, null)
        );

        Test(["<= 2"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 0),
            (3, null)
        );
    }
}