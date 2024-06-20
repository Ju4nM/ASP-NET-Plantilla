using Dapper;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizationRequirements;
using sigecendis_api.Model;

namespace sigecendis_api.Data.AuthorizationPolicies.AuthorizationHandlers {

  public class RoleHandler : AuthorizationHandler<RoleRequirement> {
    PostgreSQLConnection _connection;

    public RoleHandler(PostgreSQLConnection connection) => _connection = connection;

    protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);
    protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement) {

      var userId = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserId");

      if (userId == null) {
        return Task.CompletedTask;
      }

      bool hasRole = await HasRole (Convert.ToInt32(userId.Value), requirement.Roles);

      if (hasRole) context.Succeed(requirement);

      return Task.CompletedTask;

    }

    /// <summary>
    /// Verifica si el usuario tiene un rol en especifico ejecutando un procedimiento almacenado 
    /// que devuelve el registro del rol en caso de que lo tenga, si no, el metodo devuelve null
    /// </summary>
    /// <param name="userId">Id del usuario</param>
    /// <param name="roleKey">Clave unica del rol</param>
    /// <returns>La variable rol es diferente de nulo? => Tiene el rol?</returns>
    private async Task<bool> HasRole(int userId, string[] possibleRoles) {

      Role role;

      string sqlQuery = "select * from fun_user_findUserRole (p_userId := @userId);";

      using (NpgsqlConnection database = DbConnection()) {
        await database.OpenAsync();

        role = await database.QueryFirstOrDefaultAsync<Role>(sqlQuery, new {
          userId
        });

        await database.CloseAsync();
      }

      if (role == null) return false;

      foreach (string roleKey in possibleRoles) {
        if (role.Llave == roleKey) return true;
      }

      return false;
    }

  }
}
