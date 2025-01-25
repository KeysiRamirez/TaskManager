using Microsoft.AspNetCore.Mvc;
using TaskManager.Data.Entities;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Models;
using TaskManager.Data.OperationResult;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITask _task;
        public TaskController(ITask task)
        {
            _task = task;
        }

        // GET: api/<TaskController>
        [HttpGet("GetTask")]
        public async Task<IActionResult> Get()
        {
            OperationResult<List<TaskModel<string>>> result = await _task.GetAll();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // GET api/<TaskController>/5
        [HttpGet("GetTaskById")]
        public async Task<IActionResult> Get(int taskId)
        {
            
            var result = new OperationResult<TaskModel<string>>();

            if (taskId <= 0)
            {
                result.Success = false;
                result.Message = "La tarea no existe, id invalido. Debe de ser un numero mayor a 0";
                return BadRequest(result);
            }

            result = await _task.GetEntityBy(taskId);

            if (!result.Success || result == null)
            {
                result.Success = false;
                result.Message = "La tarea no fue encontrada.";
                return NotFound(result);
            }

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST api/<TaskController>
        [HttpPost("SaveTask")]
        public async Task<IActionResult> Post([FromBody] TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> result = null;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TaskEntity<string> newTask = new TaskEntity<string>()
            {
                TaskDescription = entity.TaskDescription,
                DueDate = entity.DueDate,
                TaskStatus = entity.TaskStatus,
                AdditionalData = entity.AdditionalData
            };

            if (entity.DueDate <= DateTime.Now)
            {
                return BadRequest("La fecha de vencimiento no puede ser antes del dia actual");
            }

            if (string.IsNullOrEmpty(entity.TaskDescription))
            {
                return BadRequest("La descripcion no puede estar vacia");
            }

            result = await _task.Save(newTask);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT api/<TaskController>/5
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
                TaskId = entity.TaskId,  // Asegurando que el ID se pase correctamente
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


        // DELETE api/<TaskController>/5
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
