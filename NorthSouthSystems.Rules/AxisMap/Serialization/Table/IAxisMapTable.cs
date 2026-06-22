namespace NorthSouthSystems.Rules;

internal interface IAxisMapTable
{
    AxisMapTableKeystone Keystone { get; }

    int RowCount { get; }
    int ColumnCount { get; }

    string GetString(int rowIndex, int columnIndex);
    object? GetObject(int rowIndex, int columnIndex);

    IEnumerable<string> GetRowStrings(int rowIndex);
    IEnumerable<string> GetColumnStrings(int columnIndex);
}