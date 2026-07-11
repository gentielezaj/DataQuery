using shlabs.DataQuery.Abstractions.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace shlabs.DataQuery.Abstractions.Utils
{
    public class QueryBuilderFilterRuleConverter : JsonConverter<QueryBuilderFilterRule>
    {
        public override QueryBuilderFilterRule? Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            // If it has "rules", it's a QueryBuilderFilter (group)
            if (root.TryGetProperty("rules", out _) || root.TryGetProperty("Rules", out _))
            {
                return JsonSerializer.Deserialize<QueryBuilderFilter>(root.GetRawText(), options);
            }

            // Otherwise it's a leaf criteria
            return JsonSerializer.Deserialize<QueryBuilderFilterCriteria>(root.GetRawText(), options);
        }

        public override void Write(
            Utf8JsonWriter writer, QueryBuilderFilterRule value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
