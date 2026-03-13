using JWTUAuthLogin.DTO.System_Module;
using JWTUAuthLogin.Infrastructure.Repository.System_Module;
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace JWTUAuthLogin.API.System_Module
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Admin-API")]
    [Authorize]
    public class SysAttachmentApi : ControllerBase
    {
        private readonly ISysAttachmentController _controller;
        public SysAttachmentApi(ISysAttachmentController controller)
        {
            _controller = controller;
        }
        [HttpPost("UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            try
            {
                //recordType, string recordId, string photoCategory
                var boundary = HeaderUtilities
                    .RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary)
                    .Value;

                var recordType = Request.Headers["recordType"].ToString();
                var recordId = Request.Headers["recordId"].ToString();
                var recordCategory = Request.Headers["recordCategory"].ToString();
                var reader = new MultipartReader(boundary, Request.Body);
                var section = await reader.ReadNextSectionAsync();
                string response = string.Empty;

                ServiceActionResult result = await _controller.UploadFile(
                            reader,
                            section,
                            recordType,
                            recordId,
                            recordCategory
                        );
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

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("UploadBase64FileAsync")]
        public async Task<IActionResult> UploadBase64FileAsync(AttachmentDTO attachment)
        {
            try
            {
                ServiceActionResult result = await _controller.UploadBase64FileAsync(attachment);
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Remove/{recordid}")]
        public IActionResult Remove(string recordid)
        {
            ServiceActionResult result = _controller.Remove(recordid);
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
