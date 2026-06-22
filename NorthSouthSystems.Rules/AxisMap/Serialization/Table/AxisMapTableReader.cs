using MoreLinq;

namespace Nss.Rules;

internal static class AxisMapTableReader
{
    internal static AxisMap ParseValidateAndConstruct(IAxisMapTable table)
    {
        Throw.IfNull(table);

        ThrowIfVoidAreaContainsData(table);

        var axesHorizontal = ParseAxesForOrientation(table, true);
        var axesVertical = ParseAxesForOrientation(table, false);

        ThrowIfRowOrColumnCountMismatch(table, axesHorizontal, axesVertical);

        var axes = axesHorizontal.Concat(axesVertical).ToImmutableArray();

        var cellValuesRaw = ParseCellValuesRaw(table);

        return AxisMap.ParseValidateAndConstructFromTable(table.Keystone.CellValueType,
            axes, table.Keystone.CellValueScaleForFormatting, cellValuesRaw);
    }

    private static void ThrowIfVoidAreaContainsData(IAxisMapTable table)
    {
        for (int rowIndex = 0; rowIndex < table.Keystone.HorizontalAxesTypes.Length * 2; rowIndex++)
            for (int columnIndex = 0; columnIndex < table.Keystone.VerticalAxesTypes.Length * 2; columnIndex++)
                if (!string.IsNullOrWhiteSpace(table.GetString(rowIndex, columnIndex)))
                    throw new ArgumentException("Void area must not contain any data.", nameof(table));
    }

    private static void ThrowIfRowOrColumnCountMismatch(IAxisMapTable table,
        List<Axis> axesHorizontal, List<Axis> axesVertical)
    {
        int axesRowCount = (2 * axesHorizontal.Count) + AxisMap.BoundCountsAggregateMultiply(axesVertical);
        int axesColumnCount = (2 * axesVertical.Count) + AxisMap.BoundCountsAggregateMultiply(axesHorizontal);

        if (table.RowCount != axesRowCount)
            throw new ArgumentException(string.Create(InvariantCulture, $"Table RowCount and Axes mismatch. Table: {table.RowCount}, Axes: {axesRowCount}"));

        if (table.ColumnCount != axesColumnCount)
            throw new ArgumentException(string.Create(InvariantCulture, $"Table ColumnCount and Axes mismatch. Table: {table.ColumnCount}, Axes: {axesColumnCount}"));
    }

    private static List<Axis> ParseAxesForOrientation(IAxisMapTable table, bool isOrientationHorizontal)
    {
        var keystone = table.Keystone;

        var axesTypes = isOrientationHorizontal ? keystone.HorizontalAxesTypes : keystone.VerticalAxesTypes;
        var axes = new List<Axis>(axesTypes.Length);

        int expectedRepeatQuotient = 1;

        for (int axisIndex = 0; axisIndex < axesTypes.Length; axisIndex++)
        {
            var propertyPathColumn = ParseAxisGetStrings(table, isOrientationHorizontal, axisIndex, true);

            string propertyPath = propertyPathColumn[0];

            ArgumentException NewArgumentException(string messagePrefix, int? boundIndex = null, string? boundString = null) =>
                ParseAxisNewArgumentException(isOrientationHorizontal, axisIndex, propertyPath, messagePrefix, boundIndex, boundString);

            if (string.IsNullOrWhiteSpace(propertyPath))
                throw NewArgumentException("Axis property path must be non-null and non-whitespace.");

            if (!propertyPathColumn.Skip(1).All(string.IsNullOrWhiteSpace))
                throw NewArgumentException("Axis property path must be followed by void cells in its row or column.");

            var boundStringsWithVoids = ParseAxisGetStrings(table, isOrientationHorizontal, axisIndex, false);
            var boundStringsWithRepeats = ParseBoundStringsWithVoids(boundStringsWithVoids, NewArgumentException);
            var boundStrings = ParseBoundStringsWithRepeats(boundStringsWithRepeats, NewArgumentException, ref expectedRepeatQuotient);

            var axis = Axis.ParseValidateAndConstruct(axesTypes[axisIndex], propertyPath, isOrientationHorizontal, axisIndex, boundStrings);
            axes.Add(axis);
        }

        return axes;
    }

    private static ImmutableArray<string> ParseAxisGetStrings(IAxisMapTable table,
        bool isOrientationHorizontal, int orientationRelativeIndex, bool isForPropertyPath)
    {
        Func<int, IEnumerable<string>> func = isOrientationHorizontal ? table.GetRowStrings : table.GetColumnStrings;
        int rowOrColumnIndex = (orientationRelativeIndex * 2) + (isForPropertyPath ? 0 : 1);
        int skip = isOrientationHorizontal ? table.Keystone.CellValuesColumnOffset : table.Keystone.CellValuesRowOffset;

        return [.. func(rowOrColumnIndex).Skip(skip)];
    }

    private static ArgumentException ParseAxisNewArgumentException(
        bool isOrientationHorizontal, int orientationRelativeIndex, string propertyPath,
        string messagePrefix, int? boundIndex, string? boundString)
    {
        var builder = new StringBuilder(messagePrefix);

        builder.Append(InvariantCulture, $" Axis: {(isOrientationHorizontal ? 'H' : 'V')}{orientationRelativeIndex}({propertyPath})");

        if (boundIndex is not null || boundString is not null)
            builder.Append(InvariantCulture, $" Bound: {boundIndex}({boundString})");

        return new(builder.ToString());
    }

    private delegate ArgumentException ParseAxisNewArgumentExceptionCurried(
        string messagePrefix, int? boundIndex = null, string? boundString = null);

    private static ImmutableArray<string> ParseBoundStringsWithVoids(ImmutableArray<string> boundStringsWithVoids,
        ParseAxisNewArgumentExceptionCurried newArgumentException)
    {
        int interBoundVoidCount = boundStringsWithVoids.ReverseNoBuffer()
            .TakeWhile(string.IsNullOrWhiteSpace)
            .Count();

        if (boundStringsWithVoids.Length % (interBoundVoidCount + 1) != 0)
            throw newArgumentException("Axis and inter-bound void counts disagree.");

        int voidCount = interBoundVoidCount;
        var boundStringsWithRepeats = new List<string>(boundStringsWithVoids.Length / (interBoundVoidCount + 1));

        foreach (string bound in boundStringsWithVoids)
        {
            if (string.IsNullOrWhiteSpace(bound))
            {
                if (voidCount >= interBoundVoidCount)
                    throw newArgumentException("Axis has unexpected void bound.");

                voidCount++;
            }
            else
            {
                if (voidCount != interBoundVoidCount)
                    throw newArgumentException("Axis missing an expected void bound.");

                voidCount = 0;

                boundStringsWithRepeats.Add(bound);
            }
        }

        return [.. boundStringsWithRepeats];
    }

    private static ImmutableArray<string> ParseBoundStringsWithRepeats(ImmutableArray<string> boundStringsWithRepeats,
        ParseAxisNewArgumentExceptionCurried newArgumentException, ref int expectedRepeatQuotient)
    {
        string firstBound = boundStringsWithRepeats[0];

        int firstRepeatIndex = boundStringsWithRepeats.Skip(1)
                .TakeWhile(b => firstBound != b)
                .Count()
            + 1;

        if (firstRepeatIndex == 1)
        {
            if (boundStringsWithRepeats.Any(b => firstBound != b))
                throw newArgumentException("Axis early bound repeat.", 0, firstBound);

            expectedRepeatQuotient *= firstRepeatIndex;

            return [firstBound];
        }

        if (firstRepeatIndex >= boundStringsWithRepeats.Length)
        {
            if (boundStringsWithRepeats.Duplicates().Any())
                throw newArgumentException("Axis missing bound repeat.", 0, firstBound);

            expectedRepeatQuotient *= firstRepeatIndex;

            return boundStringsWithRepeats;
        }

        if (boundStringsWithRepeats.Length % firstRepeatIndex != 0)
            throw newArgumentException("Axis and bound repeat counts disagree.");

        if (boundStringsWithRepeats.Length / firstRepeatIndex != expectedRepeatQuotient)
            throw newArgumentException("Axis has missing or unexpected bound repetition.");

        expectedRepeatQuotient *= firstRepeatIndex;

        // Recheck i == 0 for sanity (we can skip because of the checks above, but don't).
        for (int i = 0; (i + firstRepeatIndex) < boundStringsWithRepeats.Length; i++)
            if (boundStringsWithRepeats[i] != boundStringsWithRepeats[i + firstRepeatIndex])
                throw newArgumentException("Axis inconsistent bound repeats.", i, boundStringsWithRepeats[i]);

        return [.. boundStringsWithRepeats[..firstRepeatIndex]];
    }

    private static ImmutableArray<object?> ParseCellValuesRaw(IAxisMapTable table)
    {
        var keystone = table.Keystone;

        int capacity = (table.RowCount - keystone.CellValuesRowOffset)
            * (table.ColumnCount - keystone.CellValuesColumnOffset);

        var values = new List<object?>(capacity);

        // All rows have the same number of columns.
        for (int rowIndex = keystone.CellValuesRowOffset; rowIndex < table.RowCount; rowIndex++)
            for (int columnIndex = keystone.CellValuesColumnOffset; columnIndex < table.ColumnCount; columnIndex++)
                values.Add(table.GetObject(rowIndex, columnIndex));

        return [.. values];
    }
}