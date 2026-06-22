using NorthSouthSystems.MessagePackable;
using System.IO;
using System.Runtime.InteropServices;

internal static class T_AxisMapMessagePackX
{
    internal static void WithMessagePackRoundTrip(AxisMap map, Type inputType, Action<AxisMap> action)
    {
        action(map);

        ImmutableArray<byte> messagePackBefore;

        if (Random.Shared.NextBool())
        {
            using var memory = new MemoryStream();

            map.WriteMessagePack(memory, Cancel);
            memory.Position = 0;

            messagePackBefore = ImmutableCollectionsMarshal.AsImmutableArray(memory.ToArray());
            map = AxisMap.ReadMessagePack(memory, Cancel);
        }
        else
        {
            messagePackBefore = map.ToMessagePack(Cancel);
            map = AxisMap.FromMessagePack(messagePackBefore);
        }

        map.ThrowIfAxesInputTypeMismatches(inputType);
        map.ToMessagePack(Cancel).Should().Equal(messagePackBefore);

        action(map);
    }

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;
}