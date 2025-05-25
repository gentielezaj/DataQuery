namespace shlabs.DataQuery.Abstractions;

public interface IQueryBuilder
{
    int? Take { get; }
    int? Skip { get; }
}