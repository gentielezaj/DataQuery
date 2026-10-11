using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public class QueryBuilderFilterCriteria(
    string field,
    QueryBuilderCriteriaConditions @operator,
    string? value)
    : QueryBuilderFilterRule
{
    public string Field { get; set; } = field;

    [JsonConverter(typeof(QueryBuilderFilterCriteriaValueJsonConverter))]
    public string? Value { get; set; } = value;

    public QueryBuilderCriteriaConditions Operator { get; set; } = @operator;

    [Obsolete("This  constructior is used only by controller property resolver. Do not use it in your code.")]
    public QueryBuilderFilterCriteria() : this(string.Empty, QueryBuilderCriteriaConditions.Equal, null)
    {
    }

    public override Expression<Func<T, bool>> ToQueryBuilderFilter<T>(QueryBuilderFilterRuleConvertorOptions? options = null)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var member = GetNestedProperty(parameter, Field);
        var propType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
        if (Operator is QueryBuilderCriteriaConditions.Null or QueryBuilderCriteriaConditions.NotNull)
        {
            var nullConstant = Expression.Constant(null, member.Type);
            var nullComparison = Operator == QueryBuilderCriteriaConditions.Null
                ? Expression.Equal(member, nullConstant)
                : Expression.NotEqual(member, nullConstant);
            return Expression.Lambda<Func<T, bool>>(nullComparison, parameter);
        }

        var typedValue = Value is null
            ? null
            : propType.IsEnum
                ? Enum.Parse(propType, Value, ignoreCase: true)
                : Convert.ChangeType(Value, propType);

        if (propType == typeof(DateTime) && options?.DateTimeKind != null && Value != null
            && DateTime.TryParse(Value, null, DateTimeStyles.RoundtripKind, out var dateTime))
        {
            if (options?.DateTimeKind is not null)
            {
                dateTime = DateTime.SpecifyKind(dateTime, options.DateTimeKind.Value);
            }

            typedValue = dateTime;
        }

        Expression constant = typedValue is null
            ? Expression.Constant(null, member.Type)
            : Expression.Constant(typedValue, propType);
        if (typedValue is not null && propType != member.Type)
        {
            constant = Expression.Convert(constant, member.Type);
        }

        Expression body = Operator switch
        {
            QueryBuilderCriteriaConditions.Equal => Expression.Equal(member, constant),
            QueryBuilderCriteriaConditions.NotEqual => Expression.NotEqual(member, constant),
            QueryBuilderCriteriaConditions.Greater => Expression.GreaterThan(member, constant),
            QueryBuilderCriteriaConditions.Less => Expression.LessThan(member, constant),
            QueryBuilderCriteriaConditions.GreaterOrEqual => Expression.GreaterThanOrEqual(member, constant),
            QueryBuilderCriteriaConditions.LessOrEqual => Expression.LessThanOrEqual(member, constant),
            QueryBuilderCriteriaConditions.Contains => Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant),
            QueryBuilderCriteriaConditions.NotContains => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant)),
            _ => throw new Exception($"Condition type '{Operator}' is not Supported.")
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression GetNestedProperty(Expression parameter, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        Expression member = parameter;

        foreach (var property in properties)
        {
            member = Expression.Property(member, property);
        }

        return member;
    }
}

public sealed class QueryBuilderFilterCriteriaValueJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.Number:
                return reader.GetDecimal().ToString(CultureInfo.InvariantCulture);

            case JsonTokenType.True:
                return bool.TrueString;

            case JsonTokenType.False:
                return bool.FalseString;

            case JsonTokenType.Null:
                return null;

            case JsonTokenType.StartObject:
            case JsonTokenType.StartArray:
                using (var document = JsonDocument.ParseValue(ref reader))
                {
                    return document.RootElement.ToString();
                }

            default:
                return reader.TokenType == JsonTokenType.None ? null : reader.GetString();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}
