using System;
using Azure;
using Azure.Data.Tables;

public class ProductionEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset ProductionTime { get; set; }
    public int ItemsProduced { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
