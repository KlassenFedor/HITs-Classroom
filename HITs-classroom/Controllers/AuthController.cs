﻿using HITs_classroom.Models.Password;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService _authService;
        private readonly ILogger _logger;
        public AuthController(IAuthService service, ILogger<AuthController> logger)
        {
            _authService = service;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    { "token", token },
                    { "applicationId", "" },
                    { "secretKey", "" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://accounts.tsu.ru/api/Account/", content);
                var responseString = await response.Content.ReadAsStringAsync();

                string accountId = "";
                string accessToken = "";
                await _authService.Login(accountId, accessToken);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpPost("passwordLogin")]
        public async Task<IActionResult> LoginWithPassword([FromBody] PasswordModel passwordModel)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var result = await _authService.LoginWithPassword(passwordModel.Password);

                if (result)
                {
                    return Ok();
                }
                return BadRequest("Incorrect password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Error during authorization.");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.Logout();
            _logger.LogInformation("Successfully logged out.");
        }
    }
}