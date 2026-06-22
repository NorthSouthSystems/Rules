using System.IO;

namespace NorthSouthSystems.Rules;

public static class AxisMapCsvX
{
#pragma warning disable CA1034 // False positive; analyzer bug for C# 14.
    // Besides their "extension" nature, these are extension methods in order to have consistency with the
    // Nss.Bcl/Nerdbank.MessagePack/MessagePackable.cs serialization extensions which must use such syntax.
    extension(AxisMap)
    {
        public static AxisMap FromCsv(string csv) =>
            AxisMapTableReader.ParseValidateAndConstruct(
                AxisMapCsvTable.Parse(csv));

        public static AxisMap ReadCsv(Stream stream) =>
            AxisMapTableReader.ParseValidateAndConstruct(
                AxisMapCsvTable.Parse(stream));
    }
#pragma warning restore
}