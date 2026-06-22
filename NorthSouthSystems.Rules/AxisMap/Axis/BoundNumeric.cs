using Nerdbank.MessagePack;
using NorthSouthSystems.IO.Hashing;
using PolyType;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Numerics;

namespace NorthSouthSystems.Rules;

internal static class BoundNumericOperatorExtensions
{
    internal static bool IsInEqualToFamily(this BoundNumericOperator @operator) =>
        @operator is BoundNumericOperator.EqualTo or BoundNumericOperator.GreaterThanOrEqualTo or BoundNumericOperator.LessThanOrEqualTo;

    internal static bool IsInGreaterThanFamily(this BoundNumericOperator @operator) =>
        @operator is BoundNumericOperator.GreaterThan or BoundNumericOperator.GreaterThanOrEqualTo;

    internal static bool IsInLessThanFamily(this BoundNumericOperator @operator) =>
        @operator is BoundNumericOperator.LessThan or BoundNumericOperator.LessThanOrEqualTo;

    internal static string ToStringSyntax(this BoundNumericOperator @operator) =>
        OperatorSyntax[(int)@operator].Syntax;

    // WARNING - DO NOT CHANGE THIS ORDERING. ToStringSyntax relies on it for indexing.
    //
    // 1. This Property is used by ToStringSyntax with direct indexing into the array.
    // 2. It is used by BoundNumeric<>.Parse to scan for matching Syntax to find the Operator.
    //    Because it is only 5 items, the scan is faster than a Dictionary lookup, which could
    //    not be used for the first use case anyways.
    internal static ImmutableArray<(BoundNumericOperator Operator, string Syntax)> OperatorSyntax { get; } =
    [
        (BoundNumericOperator.EqualTo, "=="),
        (BoundNumericOperator.GreaterThan, ">"),
        (BoundNumericOperator.GreaterThanOrEqualTo, ">="),
        (BoundNumericOperator.LessThan, "<"),
        (BoundNumericOperator.LessThanOrEqualTo, "<=")
    ];
}

// WARNING - DO NOT CHANGE THESE VALUES (or Names). MessagePack serializes as Enum.UnderlyingType by default.
// WARNING - DO NOT CHANGE THESE VALUES. Extensions.ToStringSyntax relies on them for indexing into its array.
public enum BoundNumericOperator
{
    EqualTo = 0,
    GreaterThan = 1,
    GreaterThanOrEqualTo = 2,
    LessThan = 3,
    LessThanOrEqualTo = 4
}

internal enum BoundNumericsValidationError
{
    MixedAscendingAndDescending,
    OverlappingGreaterThanAndLessThan,
    OverlappingTwins,
    OverlappingOneOffs
}

internal static class BoundNumeric
{
    internal const NumberStyles ParseIntegralNumberStyles =
        NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowThousands | NumberStyles.AllowTrailingWhite;
}

public readonly struct BoundNumeric<TBoundValue> : IEquatable<BoundNumeric<TBoundValue>>
    where TBoundValue : struct, INumber<TBoundValue>
{
    internal static bool IsValueAscending(ImmutableArray<BoundNumeric<TBoundValue>> boundNumerics)
    {
        Throw.IfZero(boundNumerics.Length);

        var boundFirst = boundNumerics[0];
        var boundLast = boundNumerics[^1];

        return boundFirst.Value.CompareTo(boundLast.Value) switch
        {
            0 => boundFirst.Operator.IsInLessThanFamily() || boundLast.Operator.IsInGreaterThanFamily(),
            > 0 => false,
            < 0 => true
        };
    }

    internal static BoundNumeric<TBoundValue> Parse(string boundRaw)
    {
        string operatorAndSpaces = Throw.IfNullOrWhiteSpace(boundRaw)
            .TakeWhile(c => c is '=' or '>' or '<' or ' ')
            .ToNewString();

        var @operator = string.IsNullOrWhiteSpace(operatorAndSpaces)
            ? BoundNumericOperator.EqualTo
            : BoundNumericOperatorExtensions.OperatorSyntax
                .Single(os => os.Syntax == operatorAndSpaces.Trim())
                .Operator;

        var valueSpan = boundRaw.AsSpan()[operatorAndSpaces.Length..];

        // We call Bound.Value.ToString with N# formatting where # = Scale. This results in integral numbers having
        // thousands separators, and by default, Parse and TryParse will throw an Exception for those string arguments
        // (i.e. they will not round-trip successfully by default).
        // See: https://learn.microsoft.com/en-us/dotnet/standard/base-types/parsing-numeric
        // "By default, the Parse and TryParse methods can successfully convert strings that contain integral decimal digits only to integer values.
        // They can successfully convert strings that contain integral and fractional decimal digits, group separators, and a decimal separator to floating-point values."
        var value = typeof(TBoundValue).IsIntegral()
            ? TBoundValue.Parse(valueSpan, BoundNumeric.ParseIntegralNumberStyles, InvariantCulture)
            : TBoundValue.Parse(valueSpan, InvariantCulture);

        return new(@operator, value);
    }

    [ConstructorShape]
    private BoundNumeric(BoundNumericOperator @operator, TBoundValue value)
    {
        Operator = @operator;
        Value = value;
    }

    [Key(0)] public BoundNumericOperator Operator { get; }
    [Key(1)] public TBoundValue Value { get; }

    public override string ToString() =>
        string.Create(InvariantCulture, $"{Operator.ToStringSyntax()} {Value}");

    public string ToString(string format) =>
        string.Create(InvariantCulture, $"{Operator.ToStringSyntax()} {Value.ToString(format, InvariantCulture)}");

    public bool Equals(BoundNumeric<TBoundValue> other) => Operator.Equals(other.Operator) && Value.Equals(other.Value);

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is BoundNumeric<TBoundValue> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Operator, Value);

    public static bool operator ==(BoundNumeric<TBoundValue> left, BoundNumeric<TBoundValue> right) => left.Equals(right);
    public static bool operator !=(BoundNumeric<TBoundValue> left, BoundNumeric<TBoundValue> right) => !left.Equals(right);

    internal void AppendHash(XxHash128 hasher)
    {
        hasher.Append((int)Operator);

        if (Value is short @short) hasher.Append(@short);
        else if (Value is int @int) hasher.Append(@int);
        else if (Value is long @long) hasher.Append(@long);
        else if (Value is double @double) hasher.Append(@double);
        else if (Value is decimal @decimal) hasher.Append(@decimal);
        else throw new NotSupportedException(typeof(TBoundValue).ToString());
    }

    internal static void Validate(ImmutableArray<BoundNumeric<TBoundValue>> boundNumerics)
    {
        List<(BoundNumericsValidationError, int)>? errorBoundIndices = null;

        void AddErrorBoundIndices(IEnumerable<BoundNumericsValidationError>? errors, int index)
        {
            if (errors is not null && errors.Any())
                ObjectX.DeferredNew(ref errorBoundIndices).AddRange(errors.Select(e => (e, index)));
        }

        bool? isAscending = null;

        for (int boundIndex = 1; boundIndex < boundNumerics.Length; boundIndex++)
        {
            var errors = boundNumerics[boundIndex - 1].ValidateNextSiblingPhase1Mixed(
                boundNumerics[boundIndex], ref isAscending);

            AddErrorBoundIndices(errors, boundIndex);
        }

        ArgumentExceptionX.ThrowIfAny(errorBoundIndices, originalParamName: nameof(boundNumerics));

        for (int boundIndex = 1; boundIndex < boundNumerics.Length; boundIndex++)
        {
            var errors = boundNumerics[boundIndex - 1].ValidateNextSiblingPhase2Overlapping(
                boundNumerics[boundIndex], isAscending);

            AddErrorBoundIndices(errors, boundIndex);
        }

        ArgumentExceptionX.ThrowIfAny(errorBoundIndices, originalParamName: nameof(boundNumerics));
    }

    // ValidateNextSibling needs to take place in two phases because Phase 2 is dependent upon the isAscending
    // calculation that takes place during Phase 1. Furthermore, if Phase 1 has errors, Phase 2 errors will
    // only add noise, so we will skip Phase 2 in such cases.
    private readonly List<BoundNumericsValidationError>? ValidateNextSiblingPhase1Mixed(
        BoundNumeric<TBoundValue> nextSibling, ref bool? isAscending)
    {
        List<BoundNumericsValidationError>? errors = null;

        int compareTo = Value.CompareTo(nextSibling.Value);

        if (IsNextSiblingMixedAscendingAndDescending(ref isAscending, compareTo))
            ObjectX.DeferredNew(ref errors).Add(BoundNumericsValidationError.MixedAscendingAndDescending);

        return errors;
    }

    private readonly List<BoundNumericsValidationError>? ValidateNextSiblingPhase2Overlapping(
        BoundNumeric<TBoundValue> nextSibling, bool? isAscending)
    {
        List<BoundNumericsValidationError>? errors = null;

        int compareTo = Value.CompareTo(nextSibling.Value);

        if (IsNextSiblingOverlappingGreaterThanAndLessThan(nextSibling, compareTo))
            ObjectX.DeferredNew(ref errors).Add(BoundNumericsValidationError.OverlappingGreaterThanAndLessThan);

        if (IsNextSiblingOverlappingTwins(nextSibling, isAscending, compareTo))
            ObjectX.DeferredNew(ref errors).Add(BoundNumericsValidationError.OverlappingTwins);

        if (IsNextSiblingOverlappingOneOffs(nextSibling, compareTo))
            ObjectX.DeferredNew(ref errors).Add(BoundNumericsValidationError.OverlappingOneOffs);

        return errors;
    }

    private static bool IsNextSiblingMixedAscendingAndDescending(
        ref bool? isAscending, int compareTo)
    {
        if (compareTo == 0)
            return false;

        bool compareToIsAscending = compareTo < 0;

        isAscending ??= compareToIsAscending;

        return isAscending != compareToIsAscending;
    }

    private readonly bool IsNextSiblingOverlappingGreaterThanAndLessThan(
        BoundNumeric<TBoundValue> nextSibling, int compareTo) =>
        (compareTo < 0 && Operator.IsInGreaterThanFamily() && nextSibling.Operator.IsInLessThanFamily())
        || (compareTo > 0 && Operator.IsInLessThanFamily() && nextSibling.Operator.IsInGreaterThanFamily());

    private readonly bool IsNextSiblingOverlappingTwins(
        BoundNumeric<TBoundValue> nextSibling, bool? isAscending, int compareTo)
    {
        if (compareTo != 0)
            return false;

        return (Operator.IsInGreaterThanFamily() && isAscending.GetValueOrDefault())
            || (Operator.IsInLessThanFamily() && !isAscending.GetValueOrDefault(true))
            || (nextSibling.Operator.IsInGreaterThanFamily() && !isAscending.GetValueOrDefault(true))
            || (nextSibling.Operator.IsInLessThanFamily() && isAscending.GetValueOrDefault())
            || (Operator.IsInEqualToFamily() && nextSibling.Operator.IsInEqualToFamily());
    }

    private readonly bool IsNextSiblingOverlappingOneOffs(
        BoundNumeric<TBoundValue> nextSibling, int compareTo)
    {
        if (compareTo == 0 || !typeof(TBoundValue).IsIntegral())
            return false;

        // We allow OverlappingOneOffs when both Operators are the same because we do not
        // think doing so increases the potential of mistakes: e.g. >= 1, >= 2, >= 10, >= 20.
        // It actually should be simpler for some users to understand.
        if (Operator == nextSibling.Operator)
            return false;

        if (compareTo < 0)
        {
            if (Value + TBoundValue.One != nextSibling.Value)
                return false;

            if (Operator.IsInGreaterThanFamily() && nextSibling.Operator.IsInEqualToFamily())
                return true;
            if (Operator.IsInEqualToFamily() && nextSibling.Operator.IsInLessThanFamily())
                return true;
        }
        else
        {
            if (Value - TBoundValue.One != nextSibling.Value)
                return false;

            if (Operator.IsInLessThanFamily() && nextSibling.Operator.IsInEqualToFamily())
                return true;
            if (Operator.IsInEqualToFamily() && nextSibling.Operator.IsInGreaterThanFamily())
                return true;
        }

        return false;
    }
}