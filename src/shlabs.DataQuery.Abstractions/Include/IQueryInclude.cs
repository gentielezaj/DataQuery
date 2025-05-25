namespace shlabs.DataQuery.Abstractions;

public interface IQueryInclude<TEntity> : IQueryInclude
    where TEntity : class;