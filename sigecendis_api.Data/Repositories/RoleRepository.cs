using Dapper;
using Npgsql;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.Role;
using sigecendis_api.Model;
using sigecendis_api.Data.Helpers;
using sigecendis_api.Data.Exceptions;
using Microsoft.AspNetCore.Http;

namespace sigecendis_api.Data.Repositories {
  public class RoleRepository : IRoleRepository {
    PostgreSQLConnection _connection;

    public RoleRepository(PostgreSQLConnection connection) => _connection = connection;

    protected NpgsqlConnection DbConnection() => new (_connection.ConnectionString);

    #region FindAll
    public async Task<IEnumerable<Role>> FindAll(int userId) {
      string sqlQuery = "select * from fun_role_findAll(p_userId:= @userId);";

      IEnumerable<Role>? roles = await RoleQueryFirstOrDefaultAsync(sqlQuery, new { userId });

      return roles;
    }
    #endregion

    #region FindOne
    public async Task<Role> FindOne(int roleId, int userId) {
      string sqlQuery = "select * from fun_role_findOne(p_roleId := @roleId, p_userId := @userId);";
      Role? target = null;
      IEnumerable<Role> result = await RoleQueryFirstOrDefaultAsync(sqlQuery, new {
        roleId,
        userId
      });

      target = result.FirstOrDefault();

      if (target == null) throw new HttpResponseException(StatusCodes.Status404NotFound, "No se ha encontrado el rol.");

      return target;
    }
    #endregion

    #region ToggleRoleStatus
    public async Task<Role> ToggleRoleStatus(int roleId, int userId) {
      string sqlQuery = "select * from fun_role_toggleStatus(p_roleId := @roleId, p_userId := @userId);";
      Role? role = null;
      IEnumerable<Role> result = await RoleQueryFirstOrDefaultAsync(sqlQuery, new {
        roleId,
        userId
      });

      role = result.FirstOrDefault();

      if (role == null) throw new HttpResponseException(StatusCodes.Status404NotFound, "No se ha encontrado el rol.");

      return role;
    }
    #endregion

    #region RoleQueryFirstOrDefaultAsync
    public async Task<IEnumerable<Role>> RoleQueryFirstOrDefaultAsync (string sql, object? parameters = null) {
      IEnumerable<Role> target;
      using NpgsqlConnection db = DbConnection();
      await db.OpenAsync();
      try {
        target = await db.QueryAsync<Role>(sql, parameters);
      } catch (PostgresException ex) {
        // Si no es un error de los esperados (en este caso el UNAUTHORIZED) entonces tira un 500
        if (ex.SqlState != DB_ERRORS.UNAUTHORIZED)
          throw new HttpResponseException(StatusCodes.Status500InternalServerError);

        throw new HttpResponseException(StatusCodes.Status401Unauthorized, ex.MessageText);
      } catch (Exception) {// En caso de que la excepcion de tipo PostgresException

        throw new HttpResponseException(StatusCodes.Status500InternalServerError);
      }

      await db.CloseAsync();
      return target;
    }
    #endregion
  }
}
