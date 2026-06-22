using System.IO;

namespace NorthSouthSystems.Rules;

internal static class AxisMapTemplateWriter
{
    internal static void Write(AxisMap map, Stream stream)
    {
        Throw.IfNull(map);

        using var writer = new StreamWriter(Throw.IfNull(stream), leaveOpen: true);

        writer.Write(Write(map));
    }

    internal static StringBuilder Write(AxisMap map)
    {
        Throw.IfNull(map);

        var builder = new StringBuilder();

        string typeName = TypeX.CSharpKeywordsByType.TryGetValue(map.CellValueType, out string? keyword)
            ? keyword
            : map.CellValueType.Name;

        builder.AppendLine(InvariantCulture, $"{typeName}:{map.CellValueScaleForFormatting}");

        foreach (var axis in map.Axes)
        {
            builder.AppendLine();
            builder.AppendLine(new AxisMapTemplateAxisHeader(axis).ToString());

            foreach (string boundToString in axis.BoundToStrings)
                builder.AppendLine(boundToString);
        }

        builder.AppendLine();
        builder.AppendLine(CellAxesBoundsHashPipeValueBase64Boundary);
        map.AppendCellAxesBoundsHashValueBase64Csv(builder);

        return builder;
    }

    internal const string CellAxesBoundsHashPipeValueBase64Boundary = "--" + nameof(CellAxesBoundsHashPipeValueBase64Boundary);
}