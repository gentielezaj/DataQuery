namespace shabs.DataQuery.Abstractions;

public interface IQueryInclude<TEntity> : IQueryInclude
    where TEntity : class;