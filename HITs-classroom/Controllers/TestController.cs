using HITs_classroom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IServicesAccountsService _servicesAccountsService;

        public TestController(IServicesAccountsService servicesAccountsService)
        {
            _servicesAccountsService = servicesAccountsService;
        }

        [HttpGet("impersonate")]
        public IActionResult TestImpersonateUser()
        {
            _servicesAccountsService.GetCourse("hitscourse@gmail.com");
            return Ok();
        }
    }
}
