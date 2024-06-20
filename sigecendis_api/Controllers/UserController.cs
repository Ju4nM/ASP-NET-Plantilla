using Microsoft.AspNetCore.Mvc;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizeAttributes;
using sigecendis_api.Data.Filters;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.DTOs.User;
using sigecendis_api.Model;
using System.Security.Claims;

namespace sigecendis_api.Controllers {

  [ServiceFilter<ExceptionFilter>]
  [RoleAuthorize(ROLES.CENDI_MANAGEMENT, ROLES.COORDINATION)]
  [Route("api/[controller]")]
  [ApiController]
  public class UserController : ControllerBase {

    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpGet]
    public async Task<IActionResult> FindAll() {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      IEnumerable<User> users = await _userRepository.FindAll(userId);
      return Ok(new { 
        data = users,
        total = users.Count()
      });
    }

    [HttpGet("{targetId}")]
    public async Task<IActionResult> FindOne(int targetId) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      User user = await _userRepository.FindOne(targetId, userId);
      return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      User user = await _userRepository.Create(createUserDto, userId);
      return Created(nameof(Create), user);
    }

    [HttpPut("{targetId}")]
    public async Task<IActionResult> Update(int targetId, [FromBody] UpdateUserDto updateUserDto) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      User user = await _userRepository.Update(targetId, updateUserDto, userId);
      return Ok(user);
    }

    [HttpDelete("{targetId}")]
    public async Task<IActionResult> Delete(int targetId) {
      int userId = Convert.ToInt32(User.FindFirstValue("UserId"));
      User user = await _userRepository.Remove(targetId, userId);
      return Ok(user);
    }
  }
}
