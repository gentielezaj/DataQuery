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

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Expected a filter object.");
            }

            if (HasRules(root))
            {
                var filter = new QueryBuilderFilter();

                if (TryGetProperty(root, "condition", "Condition", out var conditionElement))
                {
                    var conditionText = AsString(conditionElement);
                    if (!Enum.TryParse<QueryBuilderFilter.Conditions>(conditionText, true, out var condition))
                    {
                        throw new JsonException($"Unsupported filter condition '{conditionText}'.");
                    }

                    filter.Condition = condition;
                }

                if (TryGetProperty(root, "rules", "Rules", out var rulesElement) && rulesElement.ValueKind == JsonValueKind.Array)
                {
                    filter.Rules = new List<QueryBuilderFilterRule>();

                    foreach (var item in rulesElement.EnumerateArray())
                    {
                        var rule = DeserializeRule(item, options);
                        filter.Rules.Add(rule);
                    }
                }

                return filter;
            }

            var criteria = new QueryBuilderFilterCriteria();

            if (TryGetProperty(root, "field", "Field", out var fieldElement) && fieldElement.ValueKind != JsonValueKind.Null)
            {
                criteria.Field = fieldElement.ValueKind == JsonValueKind.String
                    ? fieldElement.GetString() ?? string.Empty
                    : fieldElement.ToString();
            }

            if (TryGetProperty(root, "operator", "Operator", out var operatorElement) && operatorElement.ValueKind != JsonValueKind.Null)
            {
                var operatorText = AsString(operatorElement);
                if (!Enum.TryParse<QueryBuilderCriteriaConditions>(operatorText, true, out var op))
                {
                    throw new JsonException($"Unsupported filter operator '{operatorText}'.");
                }

                criteria.Operator = op;
            }

            if (TryGetProperty(root, "value", "Value", out var valueElement) && valueElement.ValueKind != JsonValueKind.Null)
            {
                criteria.Value = AsString(valueElement) ?? string.Empty;
            }

            return criteria;
        }

        public override void Write(
            Utf8JsonWriter writer, QueryBuilderFilterRule value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static bool HasRules(JsonElement root)
        {
            return root.TryGetProperty("rules", out _) || root.TryGetProperty("Rules", out _);
        }

        private static bool TryGetProperty(JsonElement root, string lowerCaseName, string pascalCaseName, out JsonElement value)
        {
            if (root.TryGetProperty(lowerCaseName, out value))
            {
                return true;
            }

            return root.TryGetProperty(pascalCaseName, out value);
        }

        private static string? AsString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.ToString(),
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        private static QueryBuilderFilterRule DeserializeRule(JsonElement element, JsonSerializerOptions options)
        {
            var json = element.GetRawText();
            if (element.TryGetProperty("rules", out _) || element.TryGetProperty("Rules", out _))
            {
                var filter = JsonSerializer.Deserialize<QueryBuilderFilter>(json, options);
                if (filter is null)
                {
                    throw new JsonException("Invalid filter rule.");
                }

                return filter;
            }

            var criteria = JsonSerializer.Deserialize<QueryBuilderFilterCriteria>(json, options);
            if (criteria is null)
            {
                throw new JsonException("Invalid filter rule.");
            }

            return criteria;
        }

        // NOTE: this converter is intentionally not recursive; nested rules are deserialized
        // by the concrete QueryBuilderFilter or QueryBuilderFilterCriteria types.
    }
}