using System.Text.Json;
using System.Text.Json.Serialization;
using shlabs.DataQuery.Abstractions;
using shlabs.DataQuery.Abstractions.Dynamic;
using shlabs.DataQuery.Abstractions.Utils;

namespace shlabs.DataQuery.Test;

public class QueryBuilderFilterRuleConverterTests
{
    private const string JsonFilterWithLowercaseValues = """
        {
          "order": [
            { "field": "name", "direction": "asc" }
          ],
          "filter": {
            "condition": "and",
            "rules": [
                {
                    "field": "enumValue",
                    "operator": "equal",
                    "value": "Value2"
                },
              {
                "field": "establishmentId",
                "operator": "equal",
                "value": "42"
              }
            ]
          }
        }
        """;

    private static JsonSerializerOptions CreateOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new QueryBuilderFilterRuleConverter()
        }
    };

    [Fact]
    public void Deserialize_filter_rule_supports_lowercase_condition_operator_and_number_value()
    {
        var json = """
            {
              "condition": "and",
              "rules": [
                {
                  "field": "establishmentId",
                  "operator": "equal",
                  "value": 42
                }
              ]
            }
            """;

        var filter = JsonSerializer.Deserialize<QueryBuilderFilterRule>(json, CreateOptions());

        var andFilter = Assert.IsType<QueryBuilderFilter>(filter);
        Assert.Equal(QueryBuilderFilter.Conditions.And, andFilter.Condition);
        var rule = Assert.Single(andFilter.Rules!);
        var criteria = Assert.IsType<QueryBuilderFilterCriteria>(rule);
        Assert.Equal("establishmentId", criteria.Field);
        Assert.Equal(QueryBuilderCriteriaConditions.Equal, criteria.Operator);
        Assert.Equal("42", criteria.Value);
    }

    [Fact]
    public void Deserialize_dynamic_query_builder_supports_json_from_api_payload()
    {
        var builder = JsonSerializer.Deserialize<QueryBuilder>(JsonFilterWithLowercaseValues, CreateOptions());

        Assert.NotNull(builder);
        Assert.NotNull(builder.Filter);
        Assert.Equal(QueryBuilderFilter.Conditions.And, builder.Filter.Condition);
        Assert.Collection(builder.Filter.Rules!,
            rule =>
            {
                var criteria = Assert.IsType<QueryBuilderFilterCriteria>(rule);
                Assert.Equal("enumValue", criteria.Field);
                Assert.Equal(QueryBuilderCriteriaConditions.Equal, criteria.Operator);
                Assert.Equal("Value2", criteria.Value);
            },
            rule =>
            {
                var criteria = Assert.IsType<QueryBuilderFilterCriteria>(rule);
                Assert.Equal("establishmentId", criteria.Field);
                Assert.Equal(QueryBuilderCriteriaConditions.Equal, criteria.Operator);
                Assert.Equal("42", criteria.Value);
            });

        var order = Assert.Single(builder.Order!);
        Assert.Equal("name", order.Field);
        Assert.Equal(QueryOrderDirections.Asc, order.Direction);

        var query = builder.ToQueryBuilder<TestModel>();
        Assert.NotNull(query.Filter);
        var matches = query.Filter.Compile();
        Assert.True(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value2 }));
        Assert.False(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value1 }));
        Assert.False(matches(new TestModel { EstablishmentId = 43, EnumValue = TestModelEnums.Value2 }));
    }


    private class TestModel
    {
        public int EstablishmentId { get; set; }
        public string Name { get; set; } = string.Empty;

        public TestModelEnums EnumValue { get; set; }
    }

    private enum TestModelEnums
    {
        Value1,
        Value2
    }
}
