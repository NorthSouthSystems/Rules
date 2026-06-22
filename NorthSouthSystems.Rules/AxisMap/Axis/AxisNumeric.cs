using Nerdbank.MessagePack;
using NorthSouthSystems.Reflection;
using PolyType;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Numerics;

namespace NorthSouthSystems.Rules;

// SEE COMMENTS ABOVE AXIS BASE CLASS!

public sealed class AxisNumeric<TBoundValue> : Axis
    where TBoundValue : struct, INumber<TBoundValue>
{
    internal static AxisNumeric<TBoundValue> ParseValidateAndConstruct(
        string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex,
        ImmutableArray<string> boundNumericsRaw)
    {
        if (!SupportedBoundTypes.Contains(typeof(TBoundValue)))
            throw new NotSupportedException(string.Create(InvariantCulture, $"{nameof(TBoundValue)} == {typeof(TBoundValue)}"));

        Validate(propertyPath, orientationRelativeIndex, boundNumericsRaw);

        var boundNumerics = boundNumericsRaw.Select(BoundNumeric<TBoundValue>.Parse).ToImmutableArray();
        BoundNumeric<TBoundValue>.Validate(boundNumerics);

        // This is not a performance critical method, so this is quick, easy, and acceptable.
        byte boundScale = boundNumericsRaw
            .Select(BoundNumeric<decimal>.Parse)
            .Max(bn => bn.Value.Scale);

        ValidateBoundScale(boundScale);

        return new(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundNumerics, boundScale);
    }

    [ExcludeFromCodeCoverage]
    private static void ValidateBoundScale(byte boundScale)
    {
        // This Exception is unreachable because the preceding call to BoundNumeric<TBoundValue>.Parse
        // throws a FormatException whenever attempting to parse a "floating point number string" as an
        // integral Type.
        if (!typeof(TBoundValue).IsFloatingPoint())
            Throw.IfGreaterThan(boundScale, 0);
    }

    [ConstructorShape]
    private AxisNumeric(string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex,
        ImmutableArray<BoundNumeric<TBoundValue>> boundNumerics, byte boundScale) :
        base(propertyPath, isOrientationHorizontal, orientationRelativeIndex)
    {
        BoundNumerics = boundNumerics;
        _boundValueIsAscending = BoundNumeric<TBoundValue>.IsValueAscending(boundNumerics);

        BoundScale = boundScale;
        _boundNumberFormat = string.Create(InvariantCulture, $"N{boundScale}");
    }

    [Key(5)] public ImmutableArray<BoundNumeric<TBoundValue>> BoundNumerics { get; }
    private readonly bool _boundValueIsAscending;

    [Key(6)] public byte BoundScale { get; }
    private readonly string _boundNumberFormat;

    internal override Type BoundType => typeof(TBoundValue);
    internal override int BoundCount => BoundNumerics.Length;

    internal override string GetBoundToString(int boundIndex) =>
        BoundNumerics[boundIndex].ToString(_boundNumberFormat);

    internal override int? LookupBoundIndex(object input)
    {
        object? propertyValue = PropertyInfoX.GetValueCompiled(input, PropertyPath);

        if (propertyValue is null)
            return null;

        var propertyValueTyped = (TBoundValue)propertyValue;

        bool previousResult = false;
        int boundIndex = 0;

        foreach (var bound in BoundNumerics)
        {
            // We "flip" the CompareTo operands so that the Operators "align" with the result.
            int compareTo = propertyValueTyped.CompareTo(bound.Value);

            bool result = bound.Operator switch
            {
                BoundNumericOperator.EqualTo => compareTo == 0,
                BoundNumericOperator.GreaterThan => compareTo > 0,
                BoundNumericOperator.GreaterThanOrEqualTo => compareTo >= 0,
                BoundNumericOperator.LessThan => compareTo < 0,
                BoundNumericOperator.LessThanOrEqualTo => compareTo <= 0,

                _ => throw new UnreachableException(bound.Operator.ToString())
            };

            if (result)
            {
                if (boundIndex == BoundCount - 1)
                    return boundIndex;

                bool shortCircuit = bound.Operator switch
                {
                    BoundNumericOperator.EqualTo => true,
                    BoundNumericOperator.GreaterThan
                        or BoundNumericOperator.GreaterThanOrEqualTo => !_boundValueIsAscending,
                    BoundNumericOperator.LessThan
                        or BoundNumericOperator.LessThanOrEqualTo => _boundValueIsAscending,

                    _ => throw new UnreachableException(bound.Operator.ToString())
                };

                if (shortCircuit)
                    return boundIndex;
            }
            else
            {
                bool shortCircuit = compareTo switch
                {
                    0 => previousResult,
                    > 0 => !_boundValueIsAscending,
                    < 0 => _boundValueIsAscending
                };

                if (shortCircuit)
                    return previousResult ? boundIndex - 1 : null;
            }

            previousResult = result;
            boundIndex++;
        }

        return null;
    }

    internal override void AppendBoundDescription(StringBuilder builder, int boundIndex)
    {
        var bound = BoundNumerics[boundIndex];

        if (bound.Operator == BoundNumericOperator.EqualTo)
        {
            AppendBoundDescription(builder, _boundNumberFormat, bound);
        }
        else if (_boundValueIsAscending == bound.Operator.IsInGreaterThanFamily())
        {
            if ((boundIndex + 1) >= BoundCount)
                AppendBoundDescription(builder, _boundNumberFormat, bound);
            else
                AppendBoundDescription(builder, _boundNumberFormat, bound, BoundNumerics[boundIndex + 1]);
        }
        else
        {
            if ((boundIndex - 1) < 0)
                AppendBoundDescription(builder, _boundNumberFormat, bound);
            else
                AppendBoundDescription(builder, _boundNumberFormat, bound, BoundNumerics[boundIndex - 1]);
        }
    }

    private static void AppendBoundDescription(StringBuilder builder, string boundNumberFormat,
        BoundNumeric<TBoundValue> bound) =>
        builder.Append(bound.ToString(boundNumberFormat));

    private static void AppendBoundDescription(StringBuilder builder, string boundNumberFormat,
        BoundNumeric<TBoundValue> bound, BoundNumeric<TBoundValue> sibling)
    {
        bool boundInclusive = bound.Operator.IsInEqualToFamily();
        bool siblingInclusive = !sibling.Operator.IsInEqualToFamily();

        int compareTo = bound.Value.CompareTo(sibling.Value);

        if (compareTo == 0)
        {
            // This may be unreachable code.
            AppendBoundDescription(builder, boundNumberFormat, bound);
            return;
        }

        var lesserValue = compareTo < 0 ? bound.Value : sibling.Value;
        bool lesserInclusive = compareTo < 0 ? boundInclusive : siblingInclusive;

        var greaterValue = compareTo > 0 ? bound.Value : sibling.Value;
        bool greaterInclusive = compareTo > 0 ? boundInclusive : siblingInclusive;

        if (typeof(TBoundValue).IsIntegral())
        {
            if (!lesserInclusive)
                lesserValue += TBoundValue.One;

            if (!greaterInclusive)
                greaterValue -= TBoundValue.One;

            // Unneccesary safeguard from any future code that might depend on these variables.
            lesserInclusive = greaterInclusive = true;

            if (lesserValue == greaterValue)
                builder.Append(InvariantCulture, $"== {lesserValue.ToString(boundNumberFormat, InvariantCulture)}");
            else
                builder.Append(InvariantCulture, $"[{lesserValue.ToString(boundNumberFormat, InvariantCulture)}, {greaterValue.ToString(boundNumberFormat, InvariantCulture)}]");
        }
        else
            builder.Append(InvariantCulture, $"{(lesserInclusive ? '[' : '(')}{lesserValue.ToString(boundNumberFormat, InvariantCulture)}, {greaterValue.ToString(boundNumberFormat, InvariantCulture)}{(greaterInclusive ? ']' : ')')}");
    }

    protected override void AppendBoundHash(XxHash128 hasher, int boundIndex) =>
        BoundNumerics[boundIndex].AppendHash(hasher);
}