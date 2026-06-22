using System.Text.RegularExpressions;

namespace Nss.Rules;

internal sealed partial class AxisMapTemplateAxisHeader
{
    [GeneratedRegex(@"(^\s*|\s*:\s*|\s*$)")]
    private static partial Regex HeaderTriviaRegex();

    [GeneratedRegex(
        @"^(?<orientation>[HV]):(?<orientationRelativeIndex>\d):(?<propertyPath>[^:]+):(?<boundTypeName>\w+)$",
        RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
    private static partial Regex HeaderRegex();

    internal static AxisMapTemplateAxisHeader Parse(string header)
    {
        header = HeaderTriviaRegex().Replace(header, m => m.Groups[0].Value.Trim());

        var match = HeaderRegex().Match(header);

        if (!match.Success)
            throw new ArgumentException(string.Create(InvariantCulture, $"Header not properly specified. '{header}'"), nameof(header));

        bool isOrientationHorizontal = match.Groups["orientation"].Value.Equals("H", StringComparison.OrdinalIgnoreCase);
        int orientationRelativeIndex = int.Parse(match.Groups["orientationRelativeIndex"].Value, InvariantCulture);
        string propertyPath = match.Groups["propertyPath"].Value;
        string boundTypeName = match.Groups["boundTypeName"].Value;

        var boundType = AxisMapTableKeystone.ParseType(Axis.SupportedBoundTypes, boundTypeName)
            ?? throw new ArgumentException("Header unsupported Bound Type.");

        return new(isOrientationHorizontal, orientationRelativeIndex, propertyPath, boundType);
    }

    public override string ToString() =>
        string.Create(InvariantCulture, $"{(IsOrientationHorizontal ? 'H' : 'V')}:{OrientationRelativeIndex}:{PropertyPath}:{TypeX.CSharpKeywordsByType[BoundType]}");

    internal AxisMapTemplateAxisHeader(Axis axis)
        : this(axis.IsOrientationHorizontal, axis.OrientationRelativeIndex, axis.PropertyPath, axis.BoundType)
    { }

    private AxisMapTemplateAxisHeader(bool isOrientationHorizontal, int orientationRelativeIndex,
        string propertyPath, Type boundType)
    {
        IsOrientationHorizontal = isOrientationHorizontal;
        OrientationRelativeIndex = orientationRelativeIndex;
        PropertyPath = propertyPath;
        BoundType = boundType;
    }

    internal bool IsOrientationHorizontal { get; }
    internal int OrientationRelativeIndex { get; }
    internal string PropertyPath { get; }
    internal Type BoundType { get; }
}