using MoreLinq;
using Nerdbank.MessagePack;
using PolyType;
using System.IO.Hashing;

namespace NorthSouthSystems.Rules;

// SEE COMMENTS ABOVE AXIS BASE CLASS!

public sealed class AxisBool : Axis
{
    internal static AxisBool ParseValidateAndConstruct(
        string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex,
        ImmutableArray<string> boundBoolsRaw)
    {
        Validate(propertyPath, orientationRelativeIndex, boundBoolsRaw);
        Throw.IfNotEqual(boundBoolsRaw.Length, 2);

        var boundBools = boundBoolsRaw.Select(bool.Parse).ToImmutableArray();

        ArgumentExceptionX.ThrowIfAny(boundBools.Duplicates(),
            "Duplicate bounds not allowed.", originalParamName: nameof(boundBools));

        return new(propertyPath, isOrientationHorizontal, orientationRelativeIndex, boundBools[0]);
    }

    [ConstructorShape]
    private AxisBool(string propertyPath, bool isOrientationHorizontal, int orientationRelativeIndex, bool boundBoolIndex0)
        : base(propertyPath, isOrientationHorizontal, orientationRelativeIndex) =>
        BoundBoolIndex0 = boundBoolIndex0;

    [Key(4)] public bool BoundBoolIndex0 { get; }

    internal override Type BoundType => typeof(bool);
    internal override int BoundCount => 2;

    internal override string GetBoundToString(int boundIndex) =>
        boundIndex == 0 ? BoundBoolIndex0.ToString() : (!BoundBoolIndex0).ToString();

    internal override int? LookupBoundIndex(object input)
    {
        object? propertyValue = PropertyInfoX.GetValueCompiled(input, PropertyPath);

        if (propertyValue is null)
            return null;

        return (bool)propertyValue == BoundBoolIndex0 ? 0 : 1;
    }

    internal override void AppendBoundDescription(StringBuilder builder, int boundIndex)
    {
        bool boundValue = boundIndex == 0 ? BoundBoolIndex0 : !BoundBoolIndex0;

        builder.Append(InvariantCulture, $"== {boundValue}");
    }

    protected override void AppendBoundHash(XxHash128 hasher, int boundIndex) =>
        hasher.Append(boundIndex == 0 ? BoundBoolIndex0 : !BoundBoolIndex0);
}