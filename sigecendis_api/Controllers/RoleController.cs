using Microsoft.AspNetCore.Mvc;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizeAttributes;
using sigecendis_api.Data.Filters;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.Role;
using sigecendis_api.Model;
using System.Security.Claims;

namespace sigecendis_api.Controllers {

  [ServiceFilter<ExceptionFilter>]
  [RoleAuthorize(ROLES.COORDINATION)]
  [Route("api/[controller]")]
  [ApiController]
  public class RoleController : ControllerBase {
    private readonly IRoleRepository _roleRepository;

    public RoleController(IRoleRepository rolRepository) => _roleRepository = rolRepository;

    [HttpGet]
    public async Task<IActionResult> FindAll() {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      IEnumerable<Role> roles = await _roleRepository.FindAll(userId);
      return Ok(new {
        data = roles,
        total = roles.Count()
      });
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> FindOne(int roleId) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      Role role = await _roleRepository.FindOne(roleId, userId);
      return Ok(role);
    }

    [HttpPut("ToggleStatus/{roleId}")]
    public async Task<IActionResult> ToggleRole(int roleId) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      Role roleToggled = await _roleRepository.ToggleRoleStatus(roleId, userId);
      return Ok(roleToggled);
    }
  }
}
