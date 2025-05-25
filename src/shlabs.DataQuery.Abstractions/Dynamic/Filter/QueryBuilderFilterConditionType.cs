namespace shlabs.DataQuery.Abstractions.Dynamic;

public enum QueryBuilderCriteriaConditions
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual,
    Contains,
    NotContains,
    In, // TODO implement QueryBuilderCriteriaConditions.In
    NotIn, // TODO implement QueryBuilderCriteriaConditions.NotIn
    Null,
    NotNull
}