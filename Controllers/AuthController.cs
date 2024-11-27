using Authentication.Helpers;
using Authentication.Modals;
using Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
namespace Authentication.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly JwtHelper _jwtHelper;

        public AuthController(MongoDBService mongoDBService, JwtHelper jwtHelper)
        {
            _mongoDBService = mongoDBService;
            _jwtHelper = jwtHelper;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _mongoDBService.GetUserByUsername(request.Username) != null)
                return BadRequest("Username already exists");

            var salt = Guid.NewGuid().ToString();
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt)); 
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = salt  
            };

            await _mongoDBService.CreateUser(user);
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _mongoDBService.GetUserByUsername(request.Username);
            if (user == null) return Unauthorized("Invalid username or password");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(user.Salt)); 
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            if (computedHash != user.PasswordHash)
                return Unauthorized("Invalid username or password");

            var token = _jwtHelper.GenerateToken(user.Username);
            return Ok(new { Token = token });
        }

    }
}
