// src/AcuPay.Data/Repositories/InvoiceRepository.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Acupay.Data;

namespace Acupay.Data.Repositories;

public sealed class InvoiceRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public InvoiceRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // The procedure returns result set 1 = total count, result set 2 = invoice page.
    public async Task<(int TotalCount, DataTable Items)> GetInvoicesAsync(
        string? status, string? vendorCode, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_GetInvoices", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value =
            (object?)status ?? DBNull.Value;
        command.Parameters.Add("@VendorCode", SqlDbType.NVarChar, 50).Value =
            (object?)vendorCode ?? DBNull.Value;
        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int totalCount = 0;
        if (await reader.ReadAsync(cancellationToken))
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

        var items = new DataTable();
        if (await reader.NextResultAsync(cancellationToken))
            items.Load(reader);

        return (totalCount, items);
    }

    public async Task<DataTable> GetInvoiceByIdAsync(
        int invoiceId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_GetInvoiceById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public async Task<int> InsertInvoiceAsync(
        string invoiceNumber, int vendorId, int? poId, decimal invoiceAmount,
        DateTime invoiceDate, string createdBy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_InsertInvoice", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 50).Value = invoiceNumber;
        command.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
        command.Parameters.Add("@PoId", SqlDbType.Int).Value = (object?)poId ?? DBNull.Value;

        var amount = command.Parameters.Add("@InvoiceAmount", SqlDbType.Decimal);
        amount.Precision = 18;
        amount.Scale = 2;
        amount.Value = invoiceAmount;

        command.Parameters.Add("@InvoiceDate", SqlDbType.Date).Value = invoiceDate.Date;
        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = createdBy;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async Task UpdateInvoiceMatchAsync(
        int invoiceId, string status, string matchNote,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_UpdateInvoiceMatch", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
        command.Parameters.Add("@MatchNote", SqlDbType.NVarChar, 100).Value = matchNote;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DecideInvoiceAsync(
        int invoiceId, string status, string decidedBy, string? decisionReason,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_DecideInvoice", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
        command.Parameters.Add("@DecidedBy", SqlDbType.NVarChar, 100).Value = decidedBy;
        command.Parameters.Add("@DecisionReason", SqlDbType.NVarChar, 500).Value =
            (object?)decisionReason ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<DataTable> GetSpendByVendorAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = new SqlCommand("dbo.usp_GetSpendByVendor", connection);
        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
