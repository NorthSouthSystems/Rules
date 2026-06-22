using MoreLinq;
using Nerdbank.MessagePack;
using NorthSouthSystems.IO.Hashing;
using NorthSouthSystems.Reflection;
using PolyType;
using System.IO.Hashing;
using System.Text.RegularExpressions;

namespace NorthSouthSystems.Rules;

// SEE COMMENTS ABOVE AXIS BASE CLASS!

public sealed class AxisString : Axis
{
    internal static AxisString ParseValidateAndConstruct(
        string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex,
        ImmutableArray<string> boundStrings)
    {
        Validate(propertyPath, orientationRelativeIndex, boundStrings);

        if (boundStrings.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentNullException(nameof(boundStrings), "All strings must be non-null and non-whitespace.");

        if (boundStrings.Any(bound => bound.Length > _boundLengthMax))
            throw new ArgumentOutOfRangeException(nameof(boundStrings), string.Create(InvariantCulture, $"Each string must be less than or equal to {_boundLengthMax} characters."));

        var boundStringsWithInvalidWhiteSpace =
            boundStrings.Where(bound => bound.Any(c => char.IsWhiteSpace(c) && c != ' '));

        ArgumentExceptionX.ThrowIfAny(boundStringsWithInvalidWhiteSpace,
            "Space is the only whitespace character allowed.",
            originalParamName: nameof(boundStrings));

        ArgumentExceptionX.ThrowIfAny(boundStrings.Duplicates(),
            "Duplicate bounds not allowed.", originalParamName: nameof(boundStrings));

        if (boundStrings.Any(static s => s == "*") && boundStrings[^1] != "*")
            throw new ArgumentException("A 'Match All' bound, if included, must be last.", nameof(boundStrings));

        return new(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundStrings);
    }

    private const int _boundLengthMax = 1_000;

    [ConstructorShape]
    private AxisString(string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex,
        ImmutableArray<string> boundStrings)
        : base(propertyPath, isOrientationHorizontal, orientationRelativeIndex)
    {
        BoundStrings = boundStrings;

        _boundRegexes =
        [
            .. BoundStrings.Select(static bound =>
            {
                bool ignoreCase = bound[0] == '~';

                if (ignoreCase && bound.Length > 1)
                    bound = bound[1..];

                // We support very simple "glob" patterns: ? is any single character, and * is 0 to many characters.
                return (ignoreCase || bound.Any(static c => c is '?' or '*'))
                    ? new Regex(ToRegexPattern(bound), GetRegexOptions(ignoreCase), _regexMatchTimeout)
                    : null;
            })
        ];

        static string ToRegexPattern(string bound) =>
            "^"
            + Regex.Escape(bound)
                .Replace(@"\?", ".", StringComparison.Ordinal)
                .Replace(@"\*", ".*", StringComparison.Ordinal)
            + "$";

        static RegexOptions GetRegexOptions(bool ignoreCase) =>
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.NonBacktracking | (ignoreCase ? RegexOptions.IgnoreCase : 0);
    }

    // SonarQube correctly warned about the potential for user input DoS attacks prior to passing the Regex constructor
    // a matchTimeout argument. ChatGPT 5 on 2025-09-08 stated that the potential for catastrophic backtracking was the
    // primary risk of user input; however, our simple Regex support here doesn't require or allow backtracking.
    // On ChatGPT's guidance, we added RegexOptions.NonBacktracking to be explicit. ChatGPT also suggested a matchTimeout
    // of 10-25ms, which would be excessively long for our use case, especially after we added validation to limit all
    // boundStrings to 1_000 characters or less (each).
    private const int _regexMatchTimeoutMs = 25;
    private static readonly TimeSpan _regexMatchTimeout = TimeSpan.FromMilliseconds(_regexMatchTimeoutMs);

    [Key(7)] public ImmutableArray<string> BoundStrings { get; }
    private readonly ImmutableArray<Regex?> _boundRegexes;

    internal override Type BoundType => typeof(string);
    internal override int BoundCount => BoundStrings.Length;

    internal override string GetBoundToString(int boundIndex) =>
        BoundStrings[boundIndex];

    internal override int? LookupBoundIndex(object input)
    {
        object? propertyValue = PropertyInfoX.GetValueCompiled(input, PropertyPath);

        if (propertyValue is null)
            return null;

        if (propertyValue is not string propertyValueString)
            propertyValueString = propertyValue.ToString() ?? string.Empty;

        int falseCount = _boundRegexes
            .TakeWhile((regex, index) =>
                regex is not null
                    ? !regex.IsMatch(propertyValueString)
                    : !string.Equals(BoundStrings[index], propertyValueString, StringComparison.Ordinal))
            .Count();

        return falseCount < BoundCount ? falseCount : null;
    }

    internal override void AppendBoundDescription(StringBuilder builder, int boundIndex)
    {
        var regex = _boundRegexes[boundIndex];
        builder.Append(InvariantCulture, $"{(regex is null ? "==" : "=~")} '{BoundStrings[boundIndex]}'");
    }

    protected override void AppendBoundHash(XxHash128 hasher, int boundIndex) =>
        hasher.Append(BoundStrings[boundIndex]);
}