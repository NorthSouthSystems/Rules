public partial class T_AxisNumeric_LookupBoundIndex
{
    [Fact]
    public void TwoBounds_Twins()
    {
        Test(["1", "> 1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, null),
            (1, 0),
            (2, 1),
            (3, 1)
        );

        Test(["< 1", "1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 1),
            (2, null),
            (3, null)
        );
    }

    [Fact]
    public void TwoBounds_Twins_Mixed()
    {
        Test(["< 1", "> 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, null),
            (2, 1),
            (3, 1)
        );

        Test(["< 1", ">= 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 1),
            (2, 1),
            (3, 1)
        );

        Test(["<= 1", "> 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, 1)
        );
    }

    [Fact]
    public void TwoBounds_NonAdjacent_Equal()
    {
        Test(["-1", "1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, null),
            (1, 1),
            (2, null),
            (3, null)
        );
    }

    [Fact]
    public void TwoBounds_NonAdjacent_GreaterThan()
    {
        Test(["-1", "> 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, null),
            (1, null),
            (2, 1),
            (3, 1)
        );

        Test(["> -1", "1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["> -1", "> 1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, 1)
        );

        Test(["-1", ">= 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, null),
            (1, 1),
            (2, 1),
            (3, 1)
        );

        Test([">= -1", "1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 0),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test([">= -1", ">= 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 0),
            (1, 1),
            (2, 1),
            (3, 1)
        );

        Test(["> -1", ">= 1"],
            (-3, null),
            (-2, null),
            (-1, null),
            (0, 0),
            (1, 1),
            (2, 1),
            (3, 1)
        );

        Test([">= -1", "> 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, 1)
        );
    }

    [Fact]
    public void TwoBounds_NonAdjacent_LessThan()
    {
        Test(["-1", "< 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 1),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["< -1", "1"],
            (-3, 0),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["< -1", "< 1"],
            (-3, 0),
            (-2, 0),
            (-1, 1),
            (0, 1),
            (1, null),
            (2, null),
            (3, null)
        );

        Test(["-1", "<= 1"],
            (-3, null),
            (-2, null),
            (-1, 0),
            (0, 1),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["<= -1", "1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, null),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["<= -1", "<= 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 1),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["< -1", "<= 1"],
            (-3, 0),
            (-2, 0),
            (-1, 1),
            (0, 1),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["<= -1", "< 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, 1),
            (1, null),
            (2, null),
            (3, null)
        );
    }

    [Fact]
    public void TwoBounds_NonAdjacent_Mixed()
    {
        Test(["< -1", "> 1"],
            (-3, 0),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, null),
            (2, 1),
            (3, 1)
        );

        Test(["< -1", ">= 1"],
            (-3, 0),
            (-2, 0),
            (-1, null),
            (0, null),
            (1, 1),
            (2, 1),
            (3, 1)
        );

        Test(["<= -1", "> 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, null),
            (1, null),
            (2, 1),
            (3, 1)
        );

        Test(["<= -1", ">= 1"],
            (-3, 0),
            (-2, 0),
            (-1, 0),
            (0, null),
            (1, 1),
            (2, 1),
            (3, 1)
        );
    }

    [Fact]
    public void TwoBounds_Adjacent()
    {
        Test(["1", "2"],
            (-1, null),
            (0, null),
            (1, 0),
            (2, 1),
            (3, null)
        );

        Test(["1", "> 2"],
            (-1, null),
            (0, null),
            (1, 0),
            (2, null),
            (3, 1)
        );

        Test(["1", ">= 2"],
            (-1, null),
            (0, null),
            (1, 0),
            (2, 1),
            (3, 1)
        );

        Test(["< 1", "2"],
            (-1, 0),
            (0, 0),
            (1, null),
            (2, 1),
            (3, null)
        );

        Test(["<= 1", "2"],
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, null)
        );
    }

    [Fact]
    public void TwoBounds_Adjacent_SameSign()
    {
        Test(["> 1", "> 2"],
            (-1, null),
            (0, null),
            (1, null),
            (2, 0),
            (3, 1)
        );

        Test([">= 1", ">= 2"],
            (-1, null),
            (0, null),
            (1, 0),
            (2, 1),
            (3, 1)
        );

        Test(["< 1", "< 2"],
            (-1, 0),
            (0, 0),
            (1, 1),
            (2, null),
            (3, null)
        );

        Test(["<= 1", "<= 2"],
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, null)
        );
    }

    [Fact]
    public void TwoBounds_Adjacent_Mixed()
    {
        Test(["< 1", "> 2"],
            (-1, 0),
            (0, 0),
            (1, null),
            (2, null),
            (3, 1)
        );

        Test(["< 1", ">= 2"],
            (-1, 0),
            (0, 0),
            (1, null),
            (2, 1),
            (3, 1)
        );

        Test(["<= 1", "> 2"],
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, null),
            (3, 1)
        );

        Test(["<= 1", ">= 2"],
            (-1, 0),
            (0, 0),
            (1, 0),
            (2, 1),
            (3, 1)
        );
    }
}