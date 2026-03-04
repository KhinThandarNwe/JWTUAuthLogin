using JWTUAuthLogin.DTO.Login_Module;
using JWTUAuthLogin.Infrastructure.Repository.Login_Module;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace JWTUAuthLogin.API.Login_Module
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Login")]
    public class UserDataApi : ControllerBase
    {
        private readonly IProgramAccessChecker _programAccessChecker;

        public UserDataApi(IProgramAccessChecker programAccessChecker)
        {
            _programAccessChecker = programAccessChecker;
        }
        [HttpPost("DoLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> login(UserDataDTO userData)
        {
            ServiceActionResult result = await _programAccessChecker.login(userData);
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
         public IActionResult renewToken(string email, string password)
        {
            ServiceActionResult result = _programAccessChecker.renewToken(email, password);
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
        [AllowAnonymous]
        public IActionResult logout(string email, string deviceId)
        {
            ServiceActionResult result = _programAccessChecker.logout(email, deviceId);
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

        [AllowAnonymous]
        public IActionResult changePassword(string userid, string oldPassword, string newPassword)
        {
            ServiceActionResult result = _programAccessChecker.changePassword(userid, oldPassword, newPassword);
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
