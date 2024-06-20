using Dapper;
using Npgsql;

namespace sigecendis_api.Data.Helpers
{

  public static class DatabaseHelper
  {
    public static async Task<T> DatabaseQueryFirstOrDefault<T>(PostgreSQLConnection connectionString, string sqlQuery, object? parameters = null)
    {
      T target;
      using (NpgsqlConnection database = new NpgsqlConnection(connectionString.ConnectionString))
      {
        await database.OpenAsync();
        target = await database.QueryFirstOrDefaultAsync<T>(sqlQuery, parameters);
        await database.CloseAsync();
      }
      return target;
    }
  }
}