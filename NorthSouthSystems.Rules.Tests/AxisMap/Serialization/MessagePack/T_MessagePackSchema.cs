using PolyType;
using System.Text.Json;
using VerifyXunit;

public class T_MessagePackSchema
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    // The AxisMap schema will include the Axis schema; however, having both might help identify
    // the source of any unexpected changes.
    [Fact] public Task AxisSchema() => VerifySchema<Axis>();
    [Fact] public Task AxisMapSchema() => VerifySchema<AxisMap>();

    private static Task VerifySchema<T>()
        where T : IShapeable<T>
    {
        var schema = AxisMap.MessagePack.GetJsonSchema<T>();
        string schemaRaw = schema.ToJsonString(_jsonSerializerOptions);

        return Verifier.Verify(schemaRaw)
#if !GITHUB_ACTIONS
                .AutoVerify(false, true)
#endif
            ;
    }
}