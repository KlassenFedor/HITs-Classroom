using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Util;
using HITs_classroom.Models.Token;
using HITs_classroom.Models.User;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static System.Net.WebRequestMethods;

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
                _logger.LogInformation("Successfully logged in.");
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogInformation("An error was found when executing the request 'login'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.Logout();
            _logger.LogInformation("Successfully logged out.");
        }

        [HttpPost("getCode")]
        public IActionResult GetCode()
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return BadRequest();
            }
        }

        [HttpPost("testAuth")]
        public IActionResult TestAuth([FromQuery] string code)
        {
            try
            {
                //get the access token 
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
                webRequest.Method = "POST";
                var parameters =
                    "code=" + code +
                    "&client_id=" + "661958257383-e3ftp6ndnksr4hae0cvg8nv83me6vurk.apps.googleusercontent.com" +
                    "&client_secret=" + "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" +
                    "&scope=" + "https://www.googleapis.com/auth/classroom.profile.emails https://www.googleapis.com/auth/classroom.courses https://www.googleapis.com/auth/classroom.rosters" +
                    "&redirect_uri=" + "http://localhost:7284/pages/courses.html" + "&grant_type=authorization_code";
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;
                Stream postStream = webRequest.GetRequestStream();
                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                WebResponse response = webRequest.GetResponse();
                postStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(postStream);
                string responseFromServer = reader.ReadToEnd();

                GooglePlusAccessToken serStatus = JsonConvert.DeserializeObject<GooglePlusAccessToken>(responseFromServer);

                if (serStatus != null)
                {
                    string accessToken = string.Empty;
                    accessToken = serStatus.access_token;

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        // This is where you want to add the code if login is successful.
                        // getgoogleplususerdataSer(accessToken);
                        return new JsonResult(serStatus);
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                return BadRequest(e.Message);
            }
        }

        private async void GetGooglePlusUserDataSer(string access_token)
        {
            try
            {
                HttpClient client = new HttpClient();
                var urlProfile = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + access_token;

                client.CancelPendingRequests();
                HttpResponseMessage output = await client.GetAsync(urlProfile);

                if (output.IsSuccessStatusCode)
                {
                    string outputData = await output.Content.ReadAsStringAsync();
                    GoogleUserOutputData serStatus = JsonConvert.DeserializeObject<GoogleUserOutputData>(outputData);

                    if (serStatus != null)
                    {
                        // You will get the user information here.
                    }
                }
            }
            catch (Exception ex)
            {
                //catching the exception
                _logger.LogInformation(ex.Message);
            }
        }
    }
}
