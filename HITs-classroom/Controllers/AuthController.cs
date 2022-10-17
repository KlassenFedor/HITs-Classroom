using HITs_classroom.Models.Password;
using HITs_classroom.Models.Tsu;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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

        [HttpGet("tsuLogin")]
        public async Task<IActionResult> Login([FromQuery] string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                var myConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var applicationId = myConfig.GetValue<string>("TsuApplicationId");
                string? secretKey;
                using (StreamReader reader = new StreamReader("../Keys/tsu-secret-key.txt"))
                {
                    secretKey = reader.ReadToEnd();
                }
                var values = new Dictionary<string, string>
                {
                    { "token", token },
                    { "applicationId", applicationId },
                    { "secretKey", secretKey }
                };

                var content = new StringContent(values.ToString(), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://accounts.tsu.ru/api/Account/", content);
                var responseString = await response.Content.ReadFromJsonAsync<TsuAuthData>();

                string accountId = responseString.AccountId;
                string accessToken = responseString.AccessToken;
                await _authService.Login(accountId, accessToken);

                return Redirect("https://localhost:7284/pages/courses.html");
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