﻿using HITs_classroom.Models.Token;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private IAuthService _authService;
        private readonly ILogger _logger;
        public AuthController(IAuthService service, ILogger<AuthController> logger)
        {
            _authService = service;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JwtIdGoogleToken token)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("An model error was found when executing the request 'login'.");
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                await _authService.Login(token.Token);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogInformation("An error was found when executing the request 'login'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.Logout();
        }
    }
}