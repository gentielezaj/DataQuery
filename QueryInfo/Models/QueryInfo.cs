namespace QueryInfo.Models;

public class QueryInfo : CoreQueryInfo
{
    public QueryInfoFilter? Filter { get; set; } = new();
    public List<OrderInfo>? Order { get; set; }

    public void AddOrder(string field, OrderInfoDirections direction)
    {
        Order ??= new List<OrderInfo>();
        Order.Add(new OrderInfo(field, direction));
    }

    public QueryInfo<T> ToQueryInfo<T>()
        where T : class
    {
        return new QueryInfo<T>
        {
            OrderInfo = ToOrderInfo<T>(),
            Skip = Skip,
            Take = Take,
            Where = Filter?.ToQueryInfoFilter<T>()
        };
    }

    private IOrderInfo<T>? ToOrderInfo<T>()
    {
        // TODO: Implement the logic to convert OrderInfo to IOrderInfo<T>
        return null;
    }
}

