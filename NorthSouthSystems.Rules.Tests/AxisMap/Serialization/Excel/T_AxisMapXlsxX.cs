using NorthSouthSystems.MessagePackable;
using System.IO;

internal static class T_AxisMapXlsxX
{
    internal static void WithXlsxRoundTrip(AxisMap map, Type inputType, Action<AxisMap> action)
    {
        action(map);

        var messagePackBefore = map.ToMessagePack(Cancel);

        using (var memory = new MemoryStream())
        {
            map.WriteXlsx(memory);
            memory.Position = 0;

            map = AxisMap.ReadXlsx(memory);
        }

        map.ThrowIfAxesInputTypeMismatches(inputType);
        map.ToMessagePack(Cancel).Should().Equal(messagePackBefore);

        action(map);
    }

    internal static void WithXlsxCsvRoundTrip(AxisMap map, Type inputType, Action<AxisMap> action)
    {
        action(map);

        var messagePackBefore = map.ToMessagePack(Cancel);

        // No Stream input overload because ToXlsxToCsv is for testing purposes only.
        string csv = map.ToXlsxToCsv();

        if (Random.Shared.NextBool())
        {
            using var memory = new MemoryStream();
            using var writer = new StreamWriter(memory);

            writer.Write(csv);
            writer.Flush();
            memory.Position = 0;

            map = AxisMap.ReadCsv(memory);
        }
        else
            map = AxisMap.FromCsv(csv);

        map.ThrowIfAxesInputTypeMismatches(inputType);
        map.ToMessagePack(Cancel).Should().Equal(messagePackBefore);

        action(map);
    }

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;
}