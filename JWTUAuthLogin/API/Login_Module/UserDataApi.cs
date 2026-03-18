using JWTUAuthLogin.DTO.Login_Module;
using JWTUAuthLogin.Infrastructure.Repository.Login_Module;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTUAuthLogin.API.Login_Module
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[ApiExplorerSettings(GroupName = "Admin-API")]
    public class UserDataApi : ControllerBase
    {
        private readonly IProgramAccessChecker _programAccessChecker;

        public UserDataApi(IProgramAccessChecker programAccessChecker)
        {
            _programAccessChecker = programAccessChecker;
        }

        [HttpPost("DoRegister")]
        [AllowAnonymous]
        public async Task<IActionResult> register(UserDataDTO userData)
        {
            ServiceActionResult result = await _programAccessChecker.register(userData);
            switch (result.Status)
            {
                case ReturnStatus.success:
                    return Ok(result);
                case ReturnStatus.InternalServerError:
                    return BadRequest(result);
                default:
                    return StatusCode(500, result);
            }
        }

        [HttpPost("RenewToken")]
        [AllowAnonymous]
        public async Task<IActionResult> renewToken(string email, string password)
        {
            ServiceActionResult result = await _programAccessChecker.renewToken(email, password);
            switch (result.Status)
            {
                case ReturnStatus.success:
                    return Ok(result);
                case ReturnStatus.InternalServerError:
                    return BadRequest(result);
                default:
                    return StatusCode(500, result);
            }
        }
        [HttpPost("DoLogout")]
        public async Task<IActionResult> logout(string email, string deviceId)
        {
            ServiceActionResult result = await _programAccessChecker.logout(email, deviceId);
            switch (result.Status)
            {
                case ReturnStatus.success:
                    return Ok(result);
                case ReturnStatus.InternalServerError:
                    return BadRequest(result);
                default:
                    return StatusCode(500, result);
            }
        }
        [HttpPost("ChangePassword")]
        public IActionResult changePassword(int userId, string oldPassword, string newPassword)
        {
            ServiceActionResult result = _programAccessChecker.changePassword(userId, oldPassword, newPassword);
            switch (result.Status)
            {
                case ReturnStatus.success:
                    return Ok(result);
                case ReturnStatus.InternalServerError:
                    return BadRequest(result);
                default:
                    return StatusCode(500, result);
            }
        }
    }
}
