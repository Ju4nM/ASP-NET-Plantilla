using sigecendis_api.DTOs.User;
using sigecendis_api.Model;

namespace sigecendis_api.Data.Interfaces {
  public interface IUserRepository {

    // userId => Id del usuario que quiere realizar la accion (registro, consulta, acuatlizacion, eliminacion)
    // targetId => Id del usuario que sera afectado (usuario que sera consultado, actualizado o eliminado)
    Task<IEnumerable<User>> FindAll(int userId);
    Task<User> FindOne(int targetId, int userId);
    Task<User> Create(CreateUserDto createUserDto, int userId);
    Task<User> Update(int targetId, UpdateUserDto updateUserDto, int userId);
    Task<User> Remove(int targetId, int userId);

    // En base a una consulta sql que retorna una estrucutra igual que la vista view_usuario
    // mapeara los datos retornados por la consulta
    Task<IEnumerable<User>> UserQueryAsync(string sqlQuery, object? parameters = null);
  }
}
