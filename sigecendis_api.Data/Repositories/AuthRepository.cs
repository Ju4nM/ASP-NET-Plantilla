using Npgsql;
using Microsoft.IdentityModel.Tokens;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Dapper;
using sigecendis_api.Model;
using BC = BCrypt.Net.BCrypt;
using sigecendis_api.Data.Exceptions;
using Microsoft.AspNetCore.Http;

namespace sigecendis_api.Data.Repositories {
  public class AuthRepository : IAuthRepository {

    PostgreSQLConnection _connection;
    UserRepository _userRepository;

    public AuthRepository(PostgreSQLConnection connection) {
      _connection = connection;
      _userRepository = new UserRepository(_connection);
    }
    protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection.ConnectionString);

    /// <summary>
    /// Logica del inicio de sesión
    /// </summary>
    /// <param name="loginDto">Objeto con el email y contraseña</param>
    /// <returns>Un token en caso de que el email y contraseña sean correctos; string vacio en el caso contrario</returns>
    public async Task<LoginResponse> Login(LoginDto loginDto) {

      User? targetUser = await FindUserByEmail(loginDto.Email);

      if (targetUser == null) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

      bool passwordIsValid = BC.EnhancedVerify(loginDto.Password, targetUser.Contrasena);

      if (!passwordIsValid) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

      string[] tokens = GenerateTokens(
        Convert.ToString(targetUser.IdUsuario)
      );

      await UpdateRT(targetUser.IdUsuario, tokens[1]);

      return new LoginResponse {
        User = targetUser,
        Role = targetUser.Rol,
        accessToken = tokens[0],
        refreshToken = tokens[1]
      };
    }

    /// <summary>
    /// Elimina el refresh token (lo setea a null) del registro del usuario con la Id dada
    /// </summary>
    /// <param name="userId">Id del usuario al que se le "cerrara la sesion"</param>
    /// <returns></returns>
    public async Task<bool> LogOut(int userId) {
      string sqlQuery = "select * from fun_auth_clearRT(p_userId := @userId);";

      using (NpgsqlConnection database = DbConnection()) {
        await database.OpenAsync();
        await database.QueryAsync(
          sqlQuery,
          param: new {
            userId
          }
        );
        await database.CloseAsync();
      }
      return true;
    }

    /// <summary>
    /// Busca un usuario en la base de datos en base a un email dado
    /// </summary>
    /// <param name="email">Email del usuario a buscar</param>
    /// <returns></returns>
    private async Task<User?> FindUserByEmail(string email) {
      string sqlQuery = "select * from view_usuario where emailUsuario = @email and usuarioActivo = true;";

      User? targetUser = (await _userRepository.UserQueryAsync(sqlQuery, new { email })).FirstOrDefault();
      return targetUser;
    }

    /// <summary>
    /// Busca un usuario en la base de datos en base a un di dado
    /// </summary>
    /// <param name="userId">id del usuario a buscar</param>
    /// <returns></returns>
    private async Task<User?> FindUserById (int userId) {
      string sqlQuery = "select * from view_usuario where idUsuario = @userId;";

      User? targetUser = (await _userRepository.UserQueryAsync(sqlQuery, new { userId })).FirstOrDefault();
      return targetUser;
    }

    /// <summary>
    /// Genera un token que guarda el id del usuario
    /// </summary>
    /// <param name="userId">Id del usuario que guardara el token</param>
    /// <returns>Token con el token del usuario como payload</returns>
    private string[] GenerateTokens(string userId) {
      //string userId = Convert.ToString(user.IdUsuario);
      //string cendiId = Convert.ToString(user.Cendi.IdCendi);

      JwtSecurityTokenHandler tokenHandler = new();
      var jwtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));
      var rtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("RT_SECRET"));
      SymmetricSecurityKey jwtSigningKey = new(jwtKey);
      SymmetricSecurityKey rtSigningKey = new(rtKey);

      SecurityTokenDescriptor jwtDescriptor = new() {
        Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("UserId", userId),
                }),
        SigningCredentials = new SigningCredentials(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        Expires = DateTime.UtcNow.AddHours(1)
      };

      SecurityTokenDescriptor rtDescriptor = new() {
        Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("UserId", userId),
                }),
        SigningCredentials = new SigningCredentials(rtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        Expires = DateTime.UtcNow.AddDays(1)
      };

      SecurityToken jwtToken = tokenHandler.CreateToken(jwtDescriptor);
      SecurityToken rtToken = tokenHandler.CreateToken(rtDescriptor);
      string jwt = tokenHandler.WriteToken(jwtToken);
      string rt = tokenHandler.WriteToken(rtToken);
      return [jwt, rt];
    }

    /// <summary>
    /// Hashea el refreshToken y lo inserta en el campo "refreshToken" del usuario con la id dada
    /// </summary>
    /// <param name="userId">Id del usuario al que se le seteara / actualizara el refreshToken</param>
    /// <param name="refreshToken">RefreshToken que sera seteado despues de hashearlo</param>
    /// <returns></returns>
    private async Task UpdateRT(int userId, string refreshToken) {
      string sqlQuery = "select * from fun_auth_updateRT(p_userId := @userId, p_refreshtoken := @refreshToken);";

      refreshToken = BC.EnhancedHashPassword(refreshToken);

      using (NpgsqlConnection database = DbConnection()) {
        await database.OpenAsync();
        await database.QueryAsync(
          sqlQuery,
          param: new {
            userId,
            refreshToken
          }
        );
        await database.CloseAsync();
      }
    }

    /// <summary>
    /// Obtiene los claims del refreshToken, en caso de que no haya claims se retornara null. 
    /// En caso de que existan los claims 
    /// necesarios se extraeran los id de usaurio y cendi para generar nuevos tokens 
    /// y setearlos en base de datos
    /// </summary>
    /// <param name="refreshTokenDto">string del refreshToken</param>
    /// <returns></returns>
    public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenDto refreshTokenDto) {
      List<Claim> claims = GetClaimsFromToken(refreshTokenDto.RefreshToken);
      if (claims.Count == 0) return null;

      string userId = claims.FirstOrDefault(claim => claim.Type == "UserId").Value;
      string cendiId = claims.FirstOrDefault(claim => claim.Type == "CendiId").Value;

      if (userId == null || cendiId == null) return null;

      string[] tokens = GenerateTokens(userId);

      User user = await FindUserById(Convert.ToInt32(userId));

      if (user.RefreshToken == null) throw new HttpResponseException(StatusCodes.Status401Unauthorized);

      bool rtMatches = BC.EnhancedVerify(refreshTokenDto.RefreshToken, user.RefreshToken);

      if (!rtMatches) {
        await LogOut(user.IdUsuario);
        throw new HttpResponseException(StatusCodes.Status401Unauthorized);
      }

      await UpdateRT(Convert.ToInt32(userId), tokens[1]);

      return new RefreshTokenResponse {
        AccessToken = tokens[0],
        RefreshToken = tokens[1],
      };
    }

    /// <summary>
    /// Valida el refreshToken (incluye la validacion de la expiracion) y si este es valido retorna
    /// una lista de los claims del token, si no, retorna una lista vacia
    /// </summary>
    /// <param name="refreshToken">string del refreshtoken</param>
    /// <returns></returns>
    private List<Claim> GetClaimsFromToken (string refreshToken) {
      var rtKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("RT_SECRET"));
      SymmetricSecurityKey rtSigningKey = new(rtKey);

      TokenValidationParameters validationParameters = new () {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = rtSigningKey,
        ValidateLifetime = true,
        LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) => {
          return expires.HasValue && expires > DateTime.UtcNow;
        }
      };

      try {
        ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validationParameters, out _);
        return principal.Claims.ToList();
      } catch (SecurityTokenValidationException error) {
        return [];
      }

    }
  }
}
