using Acupay.Data;
using Microsoft.Data.SqlClient;
namespace Acupay.Data.Repositories;
public class DbConnectionFactory
{
    private readonly string _connectionString;
    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
