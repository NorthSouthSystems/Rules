using NorthSouthSystems.MessagePackable;
using System.IO;

internal static class T_AxisMapTemplateX
{
    internal static void WithTemplateRoundTrip(AxisMap map, Type inputType, Action<AxisMap> action)
    {
        action(map);

        var messagePackBefore = map.ToMessagePack(Cancel);

        if (Random.Shared.NextBool())
        {
            using var memory = new MemoryStream();

            map.WriteTemplate(memory);
            memory.Position = 0;

            map = AxisMap.ReadTemplate(memory);
        }
        else
            map = AxisMap.FromTemplate(map.ToTemplate());

        map.ThrowIfAxesInputTypeMismatches(inputType);
        map.ToMessagePack(Cancel).Should().Equal(messagePackBefore);

        action(map);
    }

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;
}