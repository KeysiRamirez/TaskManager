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

        [HttpPut("UpdateTask")]
        public async Task<IActionResult> Put([FromBody] TaskEntity<string> entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (entity.TaskId <= 0)
            {
                return BadRequest("El ID de la tarea es inválido.");
            }

            if (entity.DueDate <= DateTime.Now)
            {
                return BadRequest("La fecha de vencimiento no puede ser antes del día actual.");
            }

            if (string.IsNullOrEmpty(entity.TaskDescription))
            {
                return BadRequest("La descripción no puede estar vacía.");
            }

            TaskEntity<string> newTask = new TaskEntity<string>()
            {
                TaskId = entity.TaskId,  
                TaskDescription = entity.TaskDescription,
                DueDate = entity.DueDate,
                TaskStatus = entity.TaskStatus,
                AdditionalData = entity.AdditionalData
            };

            var result = await _task.Update(newTask);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("DeleteTask")]
        public async Task<IActionResult> Delete([FromBody] TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> result = null;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TaskEntity<string> newTask = new TaskEntity<string>()
            {
                TaskId = entity.TaskId,
                TaskDescription = entity.TaskDescription,
                DueDate = entity.DueDate,
                TaskStatus = entity.TaskStatus,
                AdditionalData = entity.AdditionalData
            };

            result = await _task.Remove(newTask);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

    }

}
