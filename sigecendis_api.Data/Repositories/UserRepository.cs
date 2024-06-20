using Npgsql;
using Dapper;
using sigecendis_api.Model;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.User;
using BC = BCrypt.Net.BCrypt;
using sigecendis_api.Data.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Dynamic;

namespace sigecendis_api.Data.Repositories {
  public class UserRepository : IUserRepository {
    private PostgreSQLConnection _connectionString;

    public UserRepository(PostgreSQLConnection connectionString) {
      _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString.ConnectionString);

    #region FindAll
    /// <summary>
    /// Obtiene todos los registros de los usuarios mediante la vista de usuarios (la vista oculta informacion como la contraseña, refreshtoken, entre otros).
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    public async Task<IEnumerable<User>> FindAll(int userId) {
      string sqlQuery = "select * from fun_user_findAll(p_userId := @userId);";

      return await UserQueryAsync(sqlQuery, new { userId });
    }
    #endregion

    #region FindOne
    /// <summary>
    /// Obtiene el registro de un usuario en especifico.
    /// </summary>
    /// <param name="targetId">Id del usuario objetivo</param>
    /// <param name="userId">Id del usuario que solicita la busqueda del objetivo</param>
    /// <returns>Usuario objetivo</returns>
    /// <exception cref="HttpResponseException">Excepcion para enviarle mensaje al cliente con error 404</exception>
    public async Task<User> FindOne(int targetId, int userId) {
      string sqlQuery = "select * from fun_user_findOne(p_targetId := @targetId, p_userId := @userId);";
      User? user = (await UserQueryAsync(sqlQuery, new { userId, targetId })).FirstOrDefault();

      return user ?? throw new HttpResponseException(StatusCodes.Status404NotFound, "No se ha encontrado el usuario.");
    }
    #endregion

    #region Create
    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    /// <param name="createUserDto">Datos del nuevo usuario</param>
    /// <param name="userId">Id del usuario que solicita el registro</param>
    /// <returns>Registro del nuevo usuario</returns>
    public async Task<User> Create(CreateUserDto createUserDto, int userId) {
      string sqlQuery = "select * from fun_user_create (" +
          "p_names := @names," +
          "p_firstLastName := @firstLastName," +
          "p_secondLastName := @secondLastName," +
          "p_email := @email," +
          "p_password := @password," +
          "p_userId := @userId," +
          "p_roleId := @roleId" +
      ");";

      await CheckUserUniqueFieldsAsync(createUserDto);

      // EnhancedHashPassword mejora la aleatoriedad del hash
      string hashedPassword = BC.EnhancedHashPassword(createUserDto.Password);

      IEnumerable<User> result = await UserQueryAsync(sqlQuery, new {
        names = createUserDto.Name,
        firstLastName = createUserDto.FirstName,
        secondLastName = createUserDto.SecondName,
        email = createUserDto.Email,
        password = hashedPassword,
        userId,
        roleId = createUserDto.RoleId
      });

      return result.FirstOrDefault();
    }
    #endregion

    #region Update
    /// <summary>
    /// Actualiza el registro de un usuario en especifico.
    /// </summary>
    /// <param name="targetId">Id del usuario que sera actualizado</param>
    /// <param name="updateUserDto">Nuevos datos del usuario</param>
    /// <param name="userId">Id del usuario que solicita la actualizacion</param>
    /// <returns>Registro del usuario ya actualizado</returns>
    public async Task<User> Update(int targetId, UpdateUserDto updateUserDto, int userId) {
      string sqlQuery = "select * from fun_user_update (" +
          "p_targetId := @targetId," +
          "p_userId := @userId," +
          "p_names := @names," +
          "p_firstLastName := @firstLastName," +
          "p_secondLastName := @secondLastName," +
          "p_email := @email," +
          "p_password := @password" +
      ");";

      await CheckUserUniqueFieldsAsync(updateUserDto, targetId);

      string? hashedPassword = null;

      if (updateUserDto.Password != null)
        hashedPassword = BC.EnhancedHashPassword(updateUserDto.Password);

      IEnumerable<User> result = await UserQueryAsync(sqlQuery, new {
        targetId,
        userId,
        names = updateUserDto.Name,
        firstLastName = updateUserDto.FirstName,
        secondLastName = updateUserDto.SecondName,
        email = updateUserDto.Email,
        password = hashedPassword
      });

      return result.FirstOrDefault() ?? throw new HttpResponseException(StatusCodes.Status404NotFound);
    }
    #endregion

    #region Remove
    /// <summary>
    /// Elimina un usuario en especifico.
    /// </summary>
    /// <param name="targetId">Id del usuario que sera eliminado</param>
    /// <param name="userId">Id del usuario que solicita la "eliminacion"</param>
    /// <returns>Registro del usuario que se acaba de eliminar</returns>
    /// <exception cref="HttpResponseException">404 sera enviado al cliente en caso de que el usuario eliminado resultante sea nulo (quiere decir que no se encontro)</exception>
    public async Task<User> Remove(int targetId, int userId) {
      string sqlQuery = "select * from fun_user_remove (p_targetId := @targetId, p_userId := @userId);";
      User? userRemoved = (await UserQueryAsync(sqlQuery, new { userId, targetId })).FirstOrDefault();

      return userRemoved ?? throw new HttpResponseException(StatusCodes.Status404NotFound);
    }
    #endregion

    #region CheckUserUniqueFieldsAsync
    /// <summary>
    /// Permite crear una consulta en base a los datos proporcionados que son unicos en los usuarios
    /// (Email, RFC, CURP, Numero de empleado, todo campo unico e irrepetible).
    /// </summary>
    /// <param name="dto">Pretende esperar ya sea un CreateUserDto o UpdateUserDto ya que estos tienen campos que en base de datos son cosiderados unicos</param>
    /// <param name="targetId">Id del usuario objetivo (esto en caso de ser una actualizacion)</param>
    /// <returns>En caso de que no encuentre registros detiene la ejecucion del metodo para seguir con el flujo principal (flujo principal == creacion o actualizacion)</returns>
    private async Task CheckUserUniqueFieldsAsync(object dto, int? targetId = null) {
      User parameters = new ();

      List<string> filters = [];
      string sqlQuery = "select * from view_usuario where ";

      PropertyInfo[] properties = dto.GetType().GetProperties();
      // Recorre todas las propiedades buscando los campos unicos
      foreach (PropertyInfo property in properties) {
        // si encuentra un campo unico no nulo agrega el filtro y guarda el valor que tiene

        if (property.Name == "Email" && property.GetValue(dto) != null) {
          filters.Add("emailUsuario = @email");
          parameters.EmailUsuario = (string) property.GetValue(dto);
        }
      }

      if (filters.Count() == 0) return;

      // Termina de construir la consulta en base a los filtros
      sqlQuery += "(" + (filters.Count() == 1 ? filters[0] : String.Join(" or ", filters)) + ")";
      sqlQuery += targetId != null ? " and idUsuario != @userId" : "" + ";";

      IEnumerable<User> matches = await UserQueryAsync(sqlQuery, new {
        userId = targetId,
        email = parameters.EmailUsuario,
      });

      if (matches.Count() == 0) return;

      ProcessUniqueFieldsMatches(
        parameters,
        matches
      );
    }

    /// <summary>
    /// En caso de que CheckUserUniqueFieldsAsync encuentre registros con datos como los que se proporcionaron (quiere decir que hay registros que ya estan usando ciertos datos unicos)
    /// este metodo se encargara de recorrer las coincidencias (usuarios encontrados) para extraer y formar un objeto con errores (errores sobre campos unicos como "el correo ya esta uso, escribe otro")
    /// y retornarlo al cliente
    /// </summary>
    /// <param name="userParams">datos que anteriormente fueron pasados en los dtos</param>
    /// <param name="matches">Coincidencias encontradas (usuarios encontrados con estos datos unicos)</param>
    /// <exception cref="HttpResponseException">Envio de errores al cliente con un status code 409</exception>
    private void ProcessUniqueFieldsMatches (User userParams, IEnumerable<User> matches) {
      dynamic conflicts = new ExpandoObject();

      foreach (User match in matches) {
        if (match.EmailUsuario == userParams.EmailUsuario) conflicts.Email = new string[] { "El correo electronico que escribio ya se encuentra en uso" };
      }

      throw new HttpResponseException(StatusCodes.Status409Conflict, conflicts);
    }
    #endregion

    #region UserQueryAsync
    /// <summary>
    /// Permite ejecutar consultas sql con parametros en ellas.
    /// Mapea los datos regresados (los retornados view_usuarios unicamente) de los usuarios.
    /// </summary>
    /// <param name="sqlQuery">Consulta sql</param>
    /// <param name="parameters">Parametros de la consulta</param>
    /// <returns>IEnumerable (puede ser usado como si fuese una lista) con los datos resultantes de la ejecucion de la consulta sql</returns>
    public async Task<IEnumerable<User>> UserQueryAsync(string sqlQuery, object? parameters = null) {

      IEnumerable<User> users = [];

      using NpgsqlConnection database = DbConnection();

      try {
        await database.OpenAsync();
        var result = await database.QueryAsync<User, Role, User>(
          sqlQuery,
          param: parameters,
          map: (user, role) => {
            user.Rol = role;
            return user;
          },
          splitOn: "idRol"
        );

        users = result.Distinct();

        await database.CloseAsync();
      } catch (PostgresException ex) {
        if (ex.SqlState == DB_ERRORS.UNAUTHORIZED)
          throw new HttpResponseException(StatusCodes.Status401Unauthorized, ex.MessageText);

        if (ex.SqlState == DB_ERRORS.CONFLICT)
          throw new HttpResponseException(StatusCodes.Status409Conflict, ex.MessageText);

        if (ex.SqlState == DB_ERRORS.BAD_REQUEST)
          throw new HttpResponseException(StatusCodes.Status400BadRequest, ex.MessageText);

        throw new HttpResponseException(StatusCodes.Status500InternalServerError);
      } catch (Exception) {
        throw new HttpResponseException(StatusCodes.Status500InternalServerError);
      }

      return users;
    }
    #endregion
  }
}
