using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph.Models;
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

        private static List<UserModel> _users = new List<UserModel>();
        public AuthorizationController(JWTSettings jWTSettings, IConfiguration config)
        {
            _jwtSettings = jWTSettings;
            _config = config;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserModel user)
        {
            var registeredUser = _users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (registeredUser == null)
            {
                return Unauthorized("Credenciales incorrectas");
            }
            var token = GenerateJwtToken(user.Username);
            return Ok(new { token });
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

        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserModel user)
        {
            // Verifica si el usuario ya existe
            if (_users.Any(u => u.Username == user.Username))
            {
                return BadRequest("El usuario ya existe");
            }

            // Registra el nuevo usuario (en memoria)
            _users.Add(user);

            // Crea un token JWT para el nuevo usuario
            var token = GenerateJwtToken(user.Username);

            return Ok(new { token });
        }

    }
}
