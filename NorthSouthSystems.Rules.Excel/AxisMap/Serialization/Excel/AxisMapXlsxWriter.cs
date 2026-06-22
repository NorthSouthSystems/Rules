using ClosedXML.Excel;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace Nss.Rules.Excel;

internal static class AxisMapXlsxWriter
{
    // EvaluateFormulasBeforeSaving or KeystoneCell.InvalidateFormula (prior to reading Value) is required
    // for KeystoneCell to function properly. We do both (reads occur in AxisMapXlsxTable and ToCsvExtensions).
    private static readonly SaveOptions _saveOptions = new() { EvaluateFormulasBeforeSaving = true, ValidatePackage = true };

    private const double _nameVerticalColumnWidth = 2.5;
    private const double _boundColumnWidth = 12.5;

    private sealed class Context
    {
        internal Context(AxisMap map, XLWorkbook workbook)
        {
            Map = map;
            Keystone = new(map);

            Workbook = workbook;
            Worksheet = Workbook.AddWorksheet(AxisMapXlsxTable.WorksheetName);

            var cellValuesTopLeftCell = Worksheet.Cell(
                2 + Keystone.CellValuesRowOffset,     // Excel is 1-based + Keystone
                1 + Keystone.CellValuesColumnOffset); // Excel is 1-based

            CellValuesRange = cellValuesTopLeftCell.AsTopLeftOfRange(
                Map.AxesVerticalBoundCountsAggregateMultiply,
                Map.AxesHorizontalBoundCountsAggregateMultiply);
        }

        internal AxisMap Map { get; }
        internal AxisMapTableKeystone Keystone { get; }

        internal XLWorkbook Workbook { get; }
        internal IXLWorksheet Worksheet { get; }

        internal IXLRange CellValuesRange { get; }
    }

    internal static void Write(AxisMap map, Stream stream)
    {
        Throw.IfNull(map);
        Throw.IfNull(stream);

        using var workbook = new XLWorkbook();

        var context = new Context(map, workbook);
        WriteWorksheet(context);

        workbook.SaveAs(stream, _saveOptions);
    }

    private static void WriteWorksheet(Context context)
    {
        context.Worksheet.ShowGridLines = true;

        WriteKeystone(context);
        WriteVoid(context);

        WriteAxesHorizontal(context);
        WriteAxesVertical(context);

        // Excel Data Validation does not "run" during paste (or fill) operations, so besides Data Validation,
        // we must also alert the user to invalid data using Conditional Formatting.
        WriteCellValues(context);
        WriteCellNumberFormats(context);
        WriteCellDataValidations(context);
        WriteCellConditionalFormats(context);

        context.CellValuesRange.FirstCell().Select();
        context.CellValuesRange.Style.Protection.Locked = false;

        context.Worksheet.Protect(nameof(AxisMap),
            XLProtectionAlgorithm.Algorithm.SHA512,
            XLSheetProtectionElements.FormatColumns | XLSheetProtectionElements.SelectEverything);
    }

    private static void WriteKeystone(Context context)
    {
        var map = context.Map;
        var keystone = context.Keystone;

        var keystoneCell = context.Worksheet.Cell(1, 1);

        string isInvalidPredicate = GetIsInvalidPredicate(map.CellValueType, context.CellValuesRange.RangeAddress);

        keystoneCell.FormulaA1 = string.Create(InvariantCulture, $"IF({isInvalidPredicate}, \"{AxisMapTableKeystone.InvalidSentinel}\", \"{keystone}\")");

        var keystoneRange = keystoneCell.AsTopLeftOfRange(
            1,
            keystone.CellValuesColumnOffset + map.AxesHorizontalBoundCountsAggregateMultiply);

        keystoneRange.Merge();
        keystoneRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        keystoneRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
        keystoneRange.Style.Font.FontSize = 6;

        keystoneRange.AddConditionalFormat()
            .WhenIsTrue(_invalidSentinelPredicate)
            .Fill.SetBackgroundColor(XLColor.LightPink)
            .Font.SetBold()
            .Font.SetFontColor(XLColor.DarkRed)
            .Font.SetFontSize(18);
    }

    private static void WriteVoid(Context context)
    {
        var keystone = context.Keystone;

        if (keystone.CellValuesRowOffset == 0 || keystone.CellValuesColumnOffset == 0)
            return;

        var voidCell = context.Worksheet.Cell(2, 1);

        var voidRange = voidCell.AsTopLeftOfRange(
            keystone.CellValuesRowOffset,
            keystone.CellValuesColumnOffset);

        voidRange.Merge();
        voidRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);

        voidRange.AddConditionalFormat()
            .WhenIsTrue(_invalidSentinelPredicate)
            .Fill.SetBackgroundColor(XLColor.LightPink);
    }

    private static readonly string _invalidSentinelPredicate = string.Create(InvariantCulture, $"A1 = \"{AxisMapTableKeystone.InvalidSentinel}\"");

    private static void WriteAxesHorizontal(Context context)
    {
        var map = context.Map;
        var keystone = context.Keystone;
        var worksheet = context.Worksheet;

        worksheet
            .Columns(
                1 + keystone.CellValuesColumnOffset,
                1 + keystone.CellValuesColumnOffset + map.AxesHorizontalBoundCountsAggregateMultiply)
            .Width = _boundColumnWidth;

        var repeatingLeftBorderColumnNumbers = new List<int>();
        int repeatNext = 1;

        foreach (var axis in map.Axes.Where(a => a.IsOrientationHorizontal))
        {
            var propertyPathCell = worksheet.Cell(
                2 + (2 * axis.OrientationRelativeIndex),
                1 + keystone.CellValuesColumnOffset);

            propertyPathCell.Value = axis.PropertyPath;
            propertyPathCell.Style.Font.Bold = true;

            var propertyPathRange = propertyPathCell.AsTopLeftOfRange(
                1,
                map.AxesHorizontalBoundCountsAggregateMultiply);

            propertyPathRange.Merge();
            propertyPathRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            propertyPathRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            propertyPathRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            propertyPathRange.Style.Border.SetBottomBorder(XLBorderStyleValues.None);

            WriteAxisHorizontal(context, axis, propertyPathCell.CellBelow(),
                repeatingLeftBorderColumnNumbers, ref repeatNext);
        }

        WriteAxesHorizontalRepeatingLeftBorders(context, repeatingLeftBorderColumnNumbers);
    }

    private static void WriteAxisHorizontal(Context context, Axis axis, IXLCell boundCell,
        List<int> repeatingLeftBorderColumnNumbers, ref int repeatNext)
    {
        int repeat = repeatNext;
        repeatNext *= axis.BoundCount;

        bool isRepeating = false;

        while (repeat-- > 0)
        {
            foreach ((string boundToString, bool isFirstBound, bool isLastBound) in axis.BoundToStrings.TagFirstLast())
            {
                boundCell.Value = boundToString;
                boundCell.Style.Font.Bold = true;

                var boundRange = boundCell.AsTopLeftOfRange(
                    1,
                    context.Map.AxesHorizontalMultipliers[axis.OrientationRelativeIndex]);

                boundRange.Merge();
                boundRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                boundRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                if (isFirstBound)
                {
                    if (isRepeating)
                        repeatingLeftBorderColumnNumbers.Add(boundCell.Address.ColumnNumber);
                    else
                        boundRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                }
                else if (isLastBound)
                    boundRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);

                boundRange.Style.Border.SetTopBorder(XLBorderStyleValues.None);
                boundRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);

                boundCell = boundRange.LastCell().CellRight();
            }

            isRepeating = true;
        }
    }

    private static void WriteAxesHorizontalRepeatingLeftBorders(Context context, List<int> repeatingLeftBorderColumnNumbers)
    {
        foreach (var columnNumber in repeatingLeftBorderColumnNumbers.CountBy(n => n).OrderBy(n => n.Value))
        {
            var borderStyle = GetBorderStyleByRepeatCount(columnNumber.Value);

            context.Worksheet.Range(
                    1, columnNumber.Key,
                    context.CellValuesRange.RangeAddress.LastAddress.RowNumber, columnNumber.Key)
                .Style
                .Border.SetLeftBorder(borderStyle);
        }
    }

    private static void WriteAxesVertical(Context context)
    {
        var map = context.Map;

        var repeatingTopBorderRowNumbers = new List<int>();
        int repeatNext = 1;

        foreach (var axis in map.Axes.Where(a => !a.IsOrientationHorizontal))
        {
            var propertyPathCell = context.Worksheet.Cell(
                2 + context.Keystone.CellValuesRowOffset,
                1 + (2 * axis.OrientationRelativeIndex));

            propertyPathCell.Value = axis.PropertyPath;
            propertyPathCell.Style.Font.Bold = true;
            propertyPathCell.WorksheetColumn().Width = _nameVerticalColumnWidth;
            propertyPathCell.CellRight().WorksheetColumn().Width = _boundColumnWidth;

            var propertyPathRange = propertyPathCell.AsTopLeftOfRange(
                map.AxesVerticalBoundCountsAggregateMultiply,
                1);

            propertyPathRange.Merge();
            propertyPathRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            propertyPathRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            propertyPathRange.Style.Alignment.SetTextRotation(90);
            propertyPathRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            propertyPathRange.Style.Border.SetRightBorder(XLBorderStyleValues.None);

            WriteAxisVertical(context, axis, propertyPathCell.CellRight(),
                repeatingTopBorderRowNumbers, ref repeatNext);
        }

        WriteAxesVerticalRepeatingTopBorders(context, repeatingTopBorderRowNumbers);
    }

    private static void WriteAxisVertical(Context context, Axis axis, IXLCell boundCell,
        List<int> repeatingTopBorderRowNumbers, ref int repeatNext)
    {
        int repeat = repeatNext;
        repeatNext *= axis.BoundCount;

        bool isRepeating = false;

        while (repeat-- > 0)
        {
            foreach ((string boundToString, bool isFirstBound, bool isLastBound) in axis.BoundToStrings.TagFirstLast())
            {
                boundCell.Value = boundToString;
                boundCell.Style.Font.Bold = true;

                var boundRange = boundCell.AsTopLeftOfRange(
                    context.Map.AxesVerticalMultipliers[axis.OrientationRelativeIndex],
                    1);

                boundRange.Merge();
                boundRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                boundRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                if (isFirstBound)
                {
                    if (isRepeating)
                        repeatingTopBorderRowNumbers.Add(boundCell.Address.RowNumber);
                    else
                        boundRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                }
                else if (isLastBound)
                    boundRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);

                boundRange.Style.Border.SetLeftBorder(XLBorderStyleValues.None);
                boundRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);

                boundCell = boundRange.LastCell().CellBelow();
            }

            isRepeating = true;
        }
    }

    private static void WriteAxesVerticalRepeatingTopBorders(Context context, List<int> repeatingTopBorderRowNumbers)
    {
        foreach (var rowNumber in repeatingTopBorderRowNumbers.CountBy(n => n).OrderBy(n => n.Value))
        {
            var borderStyle = GetBorderStyleByRepeatCount(rowNumber.Value);

            context.Worksheet.Range(
                    rowNumber.Key, 1,
                    rowNumber.Key, context.CellValuesRange.RangeAddress.LastAddress.ColumnNumber)
                .Style
                .Border.SetTopBorder(borderStyle);
        }
    }

    private static XLBorderStyleValues GetBorderStyleByRepeatCount(int count) => count switch
    {
        <= 0 => throw new UnreachableException(),
        1 => XLBorderStyleValues.Thin,
        2 => XLBorderStyleValues.Double,
        >= 3 => XLBorderStyleValues.Thick
    };

    private static void WriteCellValues(Context context)
    {
        var map = context.Map;
        var cellValuesRange = context.CellValuesRange;

        for (int cellIndex = 0; cellIndex < map.CellValuesExpectedCount; cellIndex++)
            WriteCellValue(context, cellIndex);

        cellValuesRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
    }

    private static void WriteCellValue(Context context, int cellIndex)
    {
        var map = context.Map;

        var cell = context.CellValuesRange.Cell(
            1 + (cellIndex / map.AxesHorizontalBoundCountsAggregateMultiply),
            1 + (cellIndex % map.AxesHorizontalBoundCountsAggregateMultiply));

        object cellValue = map.GetCellValue(cellIndex)!;

        if (map.GetCellIsNullOrWhiteSpace(cellIndex)) cell.Value = Blank.Value;
        else if (map.CellValueType == typeof(bool)) cell.Value = (bool)cellValue;
        else if (map.CellValueType == typeof(short)) cell.Value = (short)cellValue;
        else if (map.CellValueType == typeof(int)) cell.Value = (int)cellValue;
        else if (map.CellValueType == typeof(long)) cell.Value = (long)cellValue;
        else if (map.CellValueType == typeof(double)) cell.Value = (double)cellValue;
        else if (map.CellValueType == typeof(decimal)) cell.Value = (decimal)cellValue;
        else if (map.CellValueType == typeof(string)) cell.Value = (string)cellValue;
        else throw new NotSupportedException(map.CellValueType.ToString());
    }

    private static void WriteCellNumberFormats(Context context)
    {
        var map = context.Map;

        if (map.CellValueType.IsIntegral() || map.CellValueType.IsFloatingPoint())
            context.CellValuesRange.Style.NumberFormat.Format = GetNumberFormat(map.CellValueScaleForFormatting);

        static string GetNumberFormat(int scale)
        {
            string formatPositive = GetNumberFormatPositive(scale);

            return string.Create(InvariantCulture, $"{formatPositive}; -{formatPositive}");
        }

        static string GetNumberFormatPositive(int scale) =>
            scale == 0
                ? "#,##0"
                : string.Create(InvariantCulture, $"#,##0.{new('0', scale)}");
    }

    private static void WriteCellDataValidations(Context context)
    {
        var map = context.Map;

        var validation = context.CellValuesRange.CreateDataValidation();
        validation.IgnoreBlanks = true;

        if (map.CellValueType == typeof(bool)) WriteCellBoolValidation(validation);
        else if (map.CellValueType == typeof(short)) WriteCellWholeNumberValidation<short>(validation);
        else if (map.CellValueType == typeof(int)) WriteCellWholeNumberValidation<int>(validation);
        else if (map.CellValueType == typeof(long)) WriteCellWholeNumberValidation<long>(validation);
        else if (map.CellValueType == typeof(double)) WriteCellDecimalValidation(validation);
        else if (map.CellValueType == typeof(decimal)) WriteCellDecimalValidation(validation);
        else if (map.CellValueType == typeof(string)) WriteCellTextValidation(validation);
        else throw new NotSupportedException(map.CellValueType.ToString());
    }

    private static void WriteCellBoolValidation(IXLDataValidation validation)
    {
        // Excel's list validation XML expects quoted inline items; however, it will work for many unquoted
        // literals, but NOT for TRUE,FALSE because that literal appears to be an invalid formula when unquoted.
        validation.List("\"TRUE,FALSE\"");
        validation.InCellDropdown = true;
        validation.ErrorMessage = "Please enter TRUE or FALSE.";
    }

    private static void WriteCellWholeNumberValidation<T>(IXLDataValidation validation)
        where T : struct, IMinMaxValue<T>, INumber<T>
    {
        validation.WholeNumber.Between(T.MinValue.ToString(null, InvariantCulture), T.MaxValue.ToString(null, InvariantCulture));
        validation.ErrorMessage = string.Create(InvariantCulture, $"Please enter a whole number between {T.MinValue:N0} and {T.MaxValue:N0}.");
    }

    private static void WriteCellDecimalValidation(IXLDataValidation validation)
    {
        validation.Decimal.Between(-_decimalBetweenMaxValue, _decimalBetweenMaxValue);
        validation.ErrorMessage = "Please enter a whole or fractional number within the range supported by Excel.";
    }

    private static void WriteCellTextValidation(IXLDataValidation validation)
    {
        validation.TextLength.EqualOrLessThan(_textLengthMax);
        validation.ErrorMessage = string.Create(InvariantCulture, $"Please enter text up to {_textLengthMax:N0} characters in length.");
    }

    // While double (which Excel uses) can technically store a greater range, this is Excel's hard limit,
    // and Excel will detect the Data Validation as corrupt if this limit is exceeded.
    private const double _decimalBetweenMaxValue = 9.999_999_999_999_99e307;
    private const int _textLengthMax = 1_000;

    private static void WriteCellConditionalFormats(Context context)
    {
        var cellValuesRange = context.CellValuesRange;

        string isInvalidPredicate = GetIsInvalidPredicate(context.Map.CellValueType, cellValuesRange.FirstCell().Address);

        cellValuesRange.AddConditionalFormat()
            .WhenIsTrue(isInvalidPredicate)
            .Fill.SetBackgroundColor(XLColor.LightPink)
            .Font.SetFontColor(XLColor.DarkRed);
    }

    // SUMPRODUCT throws an error in Excel and in ClosedXML when the range is a single blank cell.
    private static string GetIsInvalidPredicate(Type cellValueType, IXLRangeAddress address) =>
        address.NumberOfCells > 1
            ? GetIsInvalidPredicate(cellValueType, address.ToString(XLReferenceStyle.A1), true)
            : GetIsInvalidPredicate(cellValueType, address.FirstAddress);

    private static string GetIsInvalidPredicate(Type cellValueType, IXLAddress address) =>
        GetIsInvalidPredicate(cellValueType, address.ToString(XLReferenceStyle.A1), false);

    private static string GetIsInvalidPredicate(Type cellValueType, string addressRaw, bool isRange)
    {
        var concatPredicateFormat = cellValueType.IsIntegral()
            ?
            [
                isRange
                    ? _isInvalidWholeNumberPredicateFormatRange
                    : _isInvalidWholeNumberPredicateFormatCell
            ]
            : ImmutableArray<string>.Empty;

        var predicates = GetIsInvalidPredicateFormats(cellValueType)
            .Concat(concatPredicateFormat)
            .Select(pf => string.Format(InvariantCulture, pf, addressRaw))
            .ToImmutableArray();

        return predicates.Length == 1
            ? predicates[0]
            : string.Create(InvariantCulture, $"OR({string.Join(", ", predicates)})");
    }

    private static ImmutableArray<string> GetIsInvalidPredicateFormats(Type cellValueType) =>
        _isInvalidPredicateFormatsByType.TryGetValue(cellValueType, out var formats)
            ? formats
            : throw new NotSupportedException(cellValueType.ToString());

    private static readonly ImmutableDictionary<Type, ImmutableArray<string>> _isInvalidPredicateFormatsByType =
        new Dictionary<Type, ImmutableArray<string>>
            {
                [typeof(bool)] = [_isInvalidBoolPredicateFormat],
                [typeof(short)] = GetIsInvalidNumberPredicateFormats<short>(),
                [typeof(int)] = GetIsInvalidNumberPredicateFormats<int>(),
                [typeof(long)] = GetIsInvalidNumberPredicateFormats<long>(),
                [typeof(double)] = [_isInvalidNumberPredicateFormat],
                [typeof(decimal)] = [_isInvalidNumberPredicateFormat],
                [typeof(string)] = [_isInvalidStringPredicateFormat]
            }
            .ToImmutableDictionary();

    private static ImmutableArray<string> GetIsInvalidNumberPredicateFormats<T>()
        where T : struct, IMinMaxValue<T>, INumber<T> =>
    [
        _isInvalidNumberPredicateFormat,
        string.Create(InvariantCulture, $"COUNTIF({{0}}, \"<{T.MinValue}\") <> 0"),
        string.Create(InvariantCulture, $"COUNTIF({{0}}, \">{T.MaxValue}\") <> 0")
    ];

    private const string _isInvalidBoolPredicateFormat = "COUNTIF({0}, \"<>\") <> (COUNTIF({0}, TRUE) + COUNTIF({0}, FALSE))";
    private const string _isInvalidNumberPredicateFormat = "COUNTIF({0}, \"<>\") <> COUNT({0})";
    private const string _isInvalidStringPredicateFormat = "FALSE";

    // WARNING : ClosedXML has inconsistent results when attempting to use SUMPRODUCT for our other validations,
    // hence why we are using COUNTIF above (which should be simpler as well). With that said, we could not find a
    // way to validate that a range only contains whole numbers without using SUMPRODUCT, and we've tested ours ok.
    //
    // Other attempts using SUM caused Excel to automatically add implicit intersection '@' to those ranges, which
    // broke the formula.
    //
    // THE FORMULA WE ARE USING IS TECHNICALLY INCORRECT AND WILL ALLOW FRACTIONAL VALUES TO "CANCEL" EACH OTHER;
    // HOWEVER, IT IS THE BEST THAT WE CAN DO AT THIS TIME.
    //
    // The correct formula would be: SUMPRODUCT({0}) <> SUMPRODUCT(INT({0})); however, ClosedXML errors for it.
    // INT always floors "down" (positives toward 0, negatives towards negative inifinity), which would make this
    // formula work great if ClosedXML could support it (we believe the issue is with function calls nested as
    // SUMPRODUCT arguments.
    private const string _isInvalidWholeNumberPredicateFormatRange = "SUMPRODUCT({0}) <> INT(SUMPRODUCT({0}))";
    private const string _isInvalidWholeNumberPredicateFormatCell = "IFERROR({0} <> INT({0}), FALSE)";
}