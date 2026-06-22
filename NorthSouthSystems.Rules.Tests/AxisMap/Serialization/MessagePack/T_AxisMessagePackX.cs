using NorthSouthSystems.MessagePackable;
using System.IO;

internal static class T_AxisMessagePackX
{
    internal static void WithMessagePackRoundTrip(Axis axis, Action<Axis> action)
    {
        action(axis);

        if (Random.Shared.NextBool())
        {
            using var memory = new MemoryStream();

            axis.WriteMessagePack(memory, Cancel);
            memory.Position = 0;

            axis = Axis.ReadMessagePack(memory, Cancel);
        }
        else
            axis = Axis.FromMessagePack(axis.ToMessagePack(Cancel));

        action(axis);
    }

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;
}