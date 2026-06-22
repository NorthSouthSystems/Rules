using System.IO;

namespace NorthSouthSystems.Rules;

public static class AxisMapTemplateX
{
#pragma warning disable CA1034 // False positive; analyzer bug for C# 14.
    // Besides their "extension" nature, these are extension methods in order to have consistency with the
    // Nss.Bcl/Nerdbank.MessagePack/MessagePackable.cs serialization extensions which must use such syntax.
    extension(AxisMap)
    {
        public static AxisMap FromTemplate(string template) =>
            AxisMapTemplateReader.ParseValidateAndConstruct(template);

        public static AxisMap ReadTemplate(Stream stream) =>
            AxisMapTemplateReader.ParseValidateAndConstruct(stream);
    }
#pragma warning restore

    public static string ToTemplate(this AxisMap map) =>
        AxisMapTemplateWriter.Write(map).ToString();

    public static void WriteTemplate(this AxisMap map, Stream stream) =>
        AxisMapTemplateWriter.Write(map, stream);
}