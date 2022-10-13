using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersSearchController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITeachersSearchservice _service;
        public TeachersSearchController(ILogger<AuthController> logger, ITeachersSearchservice service)
        {
            _logger = logger;
            _service = service;
        }

        [Authorize]
        [HttpGet("findTeachers/{namePart}")]
        public async Task<IActionResult> GetSuitableTeachers(string namePart)
        {
            try
            {
                var result = await _service.SearchTeachers(namePart);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'findTeachers/{namePart}'. {error}",
                    namePart, e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        [Authorize]
        [HttpGet("findTeachers")]
        public async Task<IActionResult> GetAllTeachers()
        {
            try
            {
                var result = await _service.SearchTeachers("");
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'findTeachers'. {error}",
                    e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }
    }
}
