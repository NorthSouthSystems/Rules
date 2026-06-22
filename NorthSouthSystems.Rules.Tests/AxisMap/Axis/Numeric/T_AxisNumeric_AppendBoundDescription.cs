public partial class T_AxisNumeric_AppendBoundDescription
{
    private static void Test_Int(ImmutableArray<string> boundsRaw,
            params (int Index, string ExpectedDescription)[] testCases) =>
        T_Axis.AppendBoundDescription_Test<int>(boundsRaw, testCases);

    private static void Test_Dec(ImmutableArray<string> boundsRaw,
            params (int Index, string ExpectedDescription)[] testCases) =>
        T_Axis.AppendBoundDescription_Test<decimal>(boundsRaw, testCases);
}