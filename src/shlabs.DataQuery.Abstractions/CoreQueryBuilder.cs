namespace shlabs.DataQuery.Abstractions;

public abstract class CoreQueryBuilder : IQueryBuilder
{
    public int? Take { get; set; }
    public int? Skip { get; set; }
}