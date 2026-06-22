public partial class T_AxisNumeric_LookupBoundIndex
{
    [Fact]
    public void Nullable()
    {
        Test(["1", "2"],
            (-1, null),
            (0, null),
            (1, 0),
            (2, 1),
            (3, null),
            ((int?)null, null)
        );
    }
}