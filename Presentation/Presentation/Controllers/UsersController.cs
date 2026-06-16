using Application.DTO;
using Application.Services.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("auth")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUser user, CancellationToken token)
        {
            user.Role ??= UserRole.User;
            var userInfo = await _userService.RegisterUser(user, token);
            return NoContent();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Authorize([FromBody] UserAuthentication user, CancellationToken token)
        {
            return Ok(await _userService.Enter(user.Login, user.Password, token));
        }
    }
}
