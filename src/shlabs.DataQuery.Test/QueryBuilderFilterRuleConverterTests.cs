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
                    "field": "nullableEnum",
                    "operator": "equal",
                    "value": "Value2"
                },
                {
                    "field": "nullableInteger",
                    "operator": "equal",
                    "value": "45"
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
                        "field": "enumValue",
                        "operator": "equal",
                        "value": "Value2"
                    },
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
        Assert.Collection(andFilter.Rules!,
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
                Assert.Equal("nullableEnum", criteria.Field);
                Assert.Equal(QueryBuilderCriteriaConditions.Equal, criteria.Operator);
                Assert.Equal("Value2", criteria.Value);
            },
            rule =>
            {
                var criteria = Assert.IsType<QueryBuilderFilterCriteria>(rule);
                Assert.Equal("nullableInteger", criteria.Field);
                Assert.Equal(QueryBuilderCriteriaConditions.Equal, criteria.Operator);
                Assert.Equal("45", criteria.Value);
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
        Assert.True(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value2, NullableEnum = TestModelEnums.Value2, NullableInteger = 45 }));
        Assert.False(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value1, NullableEnum = TestModelEnums.Value2, NullableInteger = 45 }));
        Assert.False(matches(new TestModel { EstablishmentId = 43, EnumValue = TestModelEnums.Value2, NullableEnum = TestModelEnums.Value2, NullableInteger = 45 }));
        Assert.False(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value2, NullableEnum = null, NullableInteger = 45 }));
        Assert.False(matches(new TestModel { EstablishmentId = 42, EnumValue = TestModelEnums.Value2, NullableEnum = TestModelEnums.Value2, NullableInteger = null }));
    }


    [Theory]
    [InlineData("nullableInteger", "equal", "45", true, false)]
    [InlineData("nullableInteger", "greater", "44", true, false)]
    [InlineData("nullableEnum", "equal", "value2", true, false)]
    [InlineData("nullableEnum", "equal", "1", true, false)]
    [InlineData("nullableInteger", "equal", null, false, true)]
    [InlineData("nullableEnum", "equal", null, false, true)]
    [InlineData("nullableInteger", "null", null, false, true)]
    [InlineData("nullableEnum", "notNull", null, true, false)]
    [InlineData("nullableInteger", "null", "ignored", false, true)]
    [InlineData("nullableEnum", "notNull", "ignored", true, false)]
    public void Nullable_criteria_convert_values_and_handle_nulls(
        string field, string @operator, string? value, bool matchesValue, bool matchesNull)
    {
        var json = JsonSerializer.Serialize(new { field, @operator, value });
        var rule = JsonSerializer.Deserialize<QueryBuilderFilterRule>(json, CreateOptions());
        Assert.NotNull(rule);
        var matches = rule.ToQueryBuilderFilter<TestModel>().Compile();

        Assert.Equal(matchesValue, matches(new TestModel
        {
            NullableInteger = 45,
            NullableEnum = TestModelEnums.Value2
        }));
        Assert.Equal(matchesNull, matches(new TestModel()));
    }

    [Fact]
    public void Nullable_date_conversion_preserves_requested_kind()
    {
        var criteria = new QueryBuilderFilterCriteria(
            "nullableDate", QueryBuilderCriteriaConditions.Equal, "2026-10-12T10:00:00");
        var matches = criteria.ToQueryBuilderFilter<TestModel>(new QueryBuilderFilterRuleConvertorOptions
        {
            DateTimeKind = DateTimeKind.Utc
        }).Compile();

        Assert.True(matches(new TestModel
        {
            NullableDate = new DateTime(2026, 10, 12, 10, 0, 0, DateTimeKind.Utc)
        }));
        Assert.False(matches(new TestModel()));
    }

    private class TestModel
    {
        public int EstablishmentId { get; set; }
        public string Name { get; set; } = string.Empty;

        public TestModelEnums EnumValue { get; set; }

        public int? NullableInteger { get; set; }
        public TestModelEnums? NullableEnum { get; set; }
        public DateTime? NullableDate { get; set; }
    }

    private enum TestModelEnums
    {
        Value1,
        Value2
    }
}
