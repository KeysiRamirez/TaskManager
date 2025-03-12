using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.API.Hubs;
using TaskManager.Data.Entities;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Models;
using TaskManager.Data.OperationResult;
using TaskManager.API.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITask _task;
        private readonly IConfiguration _config;
        private readonly IHubContext<TaskHub> _hubContext;

        public TaskController(ITask task, IConfiguration config, IHubContext<TaskHub> hubContext)
        {
            _task = task;
            _config = config;
            _hubContext = hubContext;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserModel user)
        {
            if (user.Username == "admin" && user.UserPassword == "password") 
            {
                var token = GenerateJwtToken(user.Username);
                return Ok(new { token });
            }
            return Unauthorized("Credenciales incorrectas");
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.Name, username) };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpGet("GetTask")]
        public async Task<IActionResult> Get()
        {
            OperationResult<List<TaskModel<string>>> result = await _task.GetAll();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("GetTaskById")]
        public async Task<IActionResult> Get(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new { Success = false, Message = "ID inválido." });

            var result = await _task.GetEntityBy(taskId);
            if (!result.Success || result == null)
                return NotFound(new { Success = false, Message = "Tarea no encontrada." });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("SaveTask")]
        public async Task<IActionResult> Post([FromBody] TaskEntity<string> entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (entity.DueDate <= DateTime.Now) return BadRequest("Fecha inválida");
            if (string.IsNullOrEmpty(entity.TaskDescription)) return BadRequest("Descripción vacía");

            var result = await _task.Save(entity);
            if (!result.Success) return BadRequest(result);

            // To send notification
            await _hubContext.Clients.All.SendAsync("ReceiveTaskUpdate", "Nueva tarea creada: " + entity.TaskDescription);
           
            return Ok(result);
        }
    }
}
