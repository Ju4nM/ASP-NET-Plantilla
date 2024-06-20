using sigecendis_api.DTOs.Role;
using sigecendis_api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigecendis_api.Data.Interfaces {
  public interface IRoleRepository {

    // userId => Id del usuario que quiere realizar la accion (obtener o cambiar estado en este caso)
    public Task<IEnumerable<Role>> FindAll(int userId);
    public Task<Role> FindOne(int roleId, int userId);
    public Task<Role> ToggleRoleStatus(int roleId, int userId);
  }
}
