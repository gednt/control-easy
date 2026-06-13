using System.Data;
using DBTools.Abstractions;
using DBTools.Models;

namespace ControlEasyReborn.UnitTests.TestDoubles;

public sealed class FakeAsyncSqlClient : IAsyncSqlClient
{
    private readonly List<IQueryInterceptor> _interceptors = new();
    private readonly List<FakeOperation> _operations = new();

    public IReadOnlyList<FakeOperation> Operations => _operations;

    public Func<DataTable>? SelectResultFactory { get; set; }
    public Func<bool>? InsertResultFactory { get; set; }
    public Func<bool>? UpdateResultFactory { get; set; }
    public Func<bool>? DeleteResultFactory { get; set; }

    public ISqlQueryBuilder QueryBuilderInstance => null!;
    public IDbConfiguration Configuration => null!;
    public string ConnectionString => "Fake";
    public string Error => string.Empty;

    public FakeAsyncSqlClient AddInterceptor(IQueryInterceptor interceptor)
    {
        _interceptors.Add(interceptor);
        return this;
    }

    public Task<DataTable> SelectAsync(string fields, string table, string whereClause, object[] parameters, CancellationToken ct = default)
    {
        var sql = string.IsNullOrEmpty(whereClause)
            ? $"SELECT {fields} FROM {table}"
            : $"SELECT {fields} FROM {table} WHERE {whereClause}";

        var context = new QueryInterceptionContext
        {
            Sql = sql,
            Parameters = parameters?.ToList() ?? new List<object>(),
            OperationType = QueryOperationType.Select,
            TableName = table
        };

        RunBeforeInterceptors(context);

        _operations.Add(new FakeOperation("Select", context.Sql, context.Parameters.ToList()));
        return Task.FromResult(SelectResultFactory?.Invoke() ?? new DataTable());
    }

    public Task<DataTable> SelectAsync(string queryWithoutSelect, object[] parameters, CancellationToken ct = default)
    {
        var sql = "SELECT " + queryWithoutSelect;
        var context = new QueryInterceptionContext
        {
            Sql = sql,
            Parameters = parameters?.ToList() ?? new List<object>(),
            OperationType = QueryOperationType.Select
        };

        RunBeforeInterceptors(context);

        _operations.Add(new FakeOperation("Select", context.Sql, context.Parameters.ToList()));
        return Task.FromResult(SelectResultFactory?.Invoke() ?? new DataTable());
    }

    public Task<bool> InsertAsync(string[] fields, string table, object[] values, string primaryKeyName = null!, bool autoIncrement = true, CancellationToken ct = default)
    {
        var paramPlaceholders = string.Join(",", fields.Select((_, i) => $"@param{i}"));
        var fieldList = string.Join(",", fields);
        var sql = $"INSERT INTO {table}({fieldList}) VALUES({paramPlaceholders})";

        var context = new QueryInterceptionContext
        {
            Sql = sql,
            Parameters = values?.ToList() ?? new List<object>(),
            OperationType = QueryOperationType.Insert,
            TableName = table
        };

        RunBeforeInterceptors(context);

        _operations.Add(new FakeOperation("Insert", context.Sql, context.Parameters.ToList()));
        return Task.FromResult(InsertResultFactory?.Invoke() ?? true);
    }

    public Task<bool> UpdateAsync(string[] fields, string table, string[] values, string whereClause, object[] whereParameters, CancellationToken ct = default)
    {
        var setClause = string.Join(",", fields.Select((f, i) => $"{f}=@param{i}"));
        var sql = $"UPDATE {table} SET {setClause} WHERE {whereClause}";

        var allParams = new List<object>();
        for (int i = 0; i < values.Length; i++)
            allParams.Add((object?)values[i] ?? DBNull.Value);
        if (whereParameters != null)
            allParams.AddRange(whereParameters);

        var context = new QueryInterceptionContext
        {
            Sql = sql,
            Parameters = allParams,
            OperationType = QueryOperationType.Update,
            TableName = table
        };

        RunBeforeInterceptors(context);

        _operations.Add(new FakeOperation("Update", context.Sql, context.Parameters.ToList()));
        return Task.FromResult(UpdateResultFactory?.Invoke() ?? true);
    }

    public Task<bool> DeleteAsync(string table, string whereClause, object[] parameters, CancellationToken ct = default)
    {
        var sql = $"DELETE FROM {table} WHERE {whereClause}";
        var context = new QueryInterceptionContext
        {
            Sql = sql,
            Parameters = parameters?.ToList() ?? new List<object>(),
            OperationType = QueryOperationType.Delete,
            TableName = table
        };

        RunBeforeInterceptors(context);

        _operations.Add(new FakeOperation("Delete", context.Sql, context.Parameters.ToList()));
        return Task.FromResult(DeleteResultFactory?.Invoke() ?? true);
    }

    public Task ExecuteQueryAsync(string query, CancellationToken ct = default)
    {
        _operations.Add(new FakeOperation("Execute", query, new List<object>()));
        return Task.CompletedTask;
    }

    public Task<DataTable> SelectRawAsync(string fullSql, object[] parameters, CancellationToken ct = default)
    {
        _operations.Add(new FakeOperation("SelectRaw", fullSql, parameters?.ToList() ?? new List<object>()));
        return Task.FromResult(SelectResultFactory?.Invoke() ?? new DataTable());
    }

        public Task<object> ExecuteScalarAsync(string query, object[] parameters, CancellationToken ct = default)
    {
        _operations.Add(new FakeOperation("Scalar", query, parameters?.ToList() ?? new List<object>()));
        return Task.FromResult((object?)null)!;
    }

    private void RunBeforeInterceptors(QueryInterceptionContext context)
    {
        foreach (var interceptor in _interceptors)
            interceptor.BeforeExecute(context);
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public sealed record FakeOperation(string OperationType, string Sql, IReadOnlyList<object> Parameters);