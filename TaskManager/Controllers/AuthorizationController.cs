using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.API.Hubs;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Models;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {

        private readonly JWTSettings _jwtSettings;
        private readonly IConfiguration _config;
        public AuthorizationController(JWTSettings jWTSettings, IConfiguration config)
        {
            _jwtSettings = jWTSettings;
            _config = config;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserModel user)
        {
            if (user.Username == "admin" && user.Password == "password")
            {
                var token = GenerateJwtToken(user.Username);
                return Ok(new { token });
            }
            return Unauthorized("Credenciales incorrectas");
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.Name, username) };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
