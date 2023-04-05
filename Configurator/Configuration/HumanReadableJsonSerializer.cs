using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Configurator.Configuration
{
    public interface IHumanReadableJsonSerializer
    {
        T Deserialize<T>(string input);
        string Serialize<T>(T input);
    }

    public class HumanReadableJsonSerializer : IHumanReadableJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        static HumanReadableJsonSerializer()
        {
            Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Working as of 4/4/2023")]
        public T Deserialize<T>(string input)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(input, Options)!;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Working as of 4/4/2023")]
        public string Serialize<T>(T input)
        {
            return System.Text.Json.JsonSerializer.Serialize(input, Options);
        }
    }
}
