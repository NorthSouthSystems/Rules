using System.ComponentModel;

namespace Nss.Rules.Excel;

internal static class ExcelRounding
{
    private const int _excelSignificantDigits = 15;
    private static readonly string _excelNumberFormat = string.Create(InvariantCulture, $"G{_excelSignificantDigits}");

    // WARNING: Here be dragons if you try to improve performance!
    //
    // Attempting to create a fast version using Math.Log10 was a one-off nightmare due to double's
    // precision issues. ChatGPT was good; however, it always had gaps. I was good; however, I too
    // always had gaps. After an hour of attempts, it became clear that the juice was not worth the
    // squeeze because this is being used on non-performance critical paths, such as round-trip tests.
    [Obsolete("WARNING: Here be dragons!", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static double RoundSignificantDigitsFast(double value) =>
        throw new NotSupportedException();

    internal static double RoundSignificantDigitsSlow(double value) =>
        double.Parse(
            value.ToString(_excelNumberFormat, InvariantCulture),
            InvariantCulture);
}