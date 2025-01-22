using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Me.Planner.Plans.Item.Tasks;
using Microsoft.Graph.Models;
using System.Linq.Expressions;
using TaskManager.Data.Context;
using TaskManager.Data.Entities;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Models;
using TaskManager.Data.OperationResult;

namespace TaskManager.Data.Repository
{
    public sealed class TaskRepository : ITask
    {
        private readonly TaskManagerContext _taskManagercontext;
        private readonly ILogger<TaskRepository> _logger;

        public TaskRepository(TaskManagerContext taskManagerContext, ILogger<TaskRepository> logger)
        {
            _taskManagercontext = taskManagerContext;
            _logger = logger;
        }
        public async Task<OperationResult<List<TaskModel>>> GetAll()
        {
            OperationResult<List<TaskModel>> operationResult = new OperationResult<List<TaskModel>>();

            try
            {
                var tasks = await _taskManagercontext.Task
                    /*.Where(db => db.TaskStatus == false)*/
                    .OrderByDescending(db => db.TaskId)
                    .Select(db => new TaskModel()
                    {
                        TaskId = db.TaskId,
                        TaskDescription = db.TaskDescription,
                        DueDate = db.DueDate,
                        TaskStatus = db.TaskStatus
                    }).ToListAsync();

                operationResult.Result = tasks;
            }

            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error obteniendo las tareas. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;
        }

        public async Task<OperationResult<List<TaskModel>>> GetAll(Expression<Func<TaskEntity, bool>> filter)
        {
            OperationResult<List<TaskModel>> operationResult = new OperationResult<List<TaskModel>>();

            try
            {
                var tasks = await _taskManagercontext.Task
                    .Where(filter)
                    .Select(db => new TaskModel()
                    {
                        TaskId = db.TaskId,
                        TaskDescription = db.TaskDescription,
                        DueDate = db.DueDate,
                        TaskStatus = db.TaskStatus
                    }).ToListAsync();

                operationResult.Result = tasks;
            }

            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error obteniendo las tareas. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;
        }

        public async Task<OperationResult<TaskModel>> GetEntityBy(int TaskId)
        {
            OperationResult<TaskModel> operationResult = new OperationResult<TaskModel>();
            try
            {
                var tasks = await _taskManagercontext.Task.FindAsync(TaskId);

                if (tasks == null)
                {
                    operationResult.Success = false;
                    operationResult.Message = "El ID no puede estar vacio.";
                    return operationResult;
                }

                if (tasks.TaskId <= 0)
                {
                    operationResult.Success = false;
                    operationResult.Message = "El ID no puede ser  menor o igual a 0";
                    return operationResult;
                }

                operationResult.Result = new TaskModel()
                {
                    TaskId = tasks.TaskId,
                    TaskDescription = tasks.TaskDescription,
                    DueDate = tasks.DueDate,
                    TaskStatus = tasks.TaskStatus
                };

            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error obteniendo la tarea {TaskId}. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;

        }

        public async Task<OperationResult<TaskModel>> Remove(TaskEntity entity)
        {
            OperationResult<TaskModel> operationResult = new OperationResult<TaskModel>();
            try
            {
                var tasks = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (entity == null)
                {
                    operationResult.Success = true;
                    operationResult.Message = "El ID no puede estar vacio.";
                    return operationResult;
                }

                _taskManagercontext.Remove(tasks);
                await _taskManagercontext.SaveChangesAsync();

                operationResult.Message = "La tarea fue eliminada correctamente";
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error obteniendo los buses. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;
        }

        public async Task<OperationResult<TaskModel>> Save(TaskEntity entity)
        {
            OperationResult<TaskModel> operationResult = new OperationResult<TaskModel>();
            try
            {
                var tasks = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (entity == null)
                {
                    operationResult.Success = false;
                    operationResult.Message = "La tarea no puede ser nula";
                    return operationResult;
                }

                if (string.IsNullOrEmpty(entity.TaskDescription))
                {
                    operationResult.Success = false;
                    operationResult.Message = "Los campos de la tarea no pueden ser nulos";
                    return operationResult;
                }

                // Evitar duplicar el ID
                var taskIdExist = await _taskManagercontext.Task.AnyAsync(task => task.TaskId == entity.TaskId);

                if (taskIdExist)
                {
                    operationResult.Success = false;
                    operationResult.Message = "El ID de tarea ya existe, intenta con otro.";
                    return operationResult;
                }

                _taskManagercontext.Add(entity);
                await _taskManagercontext.SaveChangesAsync();

                operationResult.Message = "La tarea fue agregada correctamente";
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error guardando la tarea. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;
        }

        public async Task<OperationResult<TaskModel>> Update(TaskEntity entity)
        {
            OperationResult<TaskModel> operationResult = new OperationResult<TaskModel>();

            try
            {
                // Validar si la entidad es nula o el ID es inválido antes de hacer cualquier operación
                if (entity == null || entity.TaskId <= 0)
                {
                    operationResult.Message = "La tarea no puede ser nula y debe tener un ID válido.";
                    operationResult.Success = false;
                    return operationResult;
                }

                // Buscar la tarea en la base de datos
                var task = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (task == null)
                {
                    operationResult.Message = "La tarea no existe.";
                    operationResult.Success = false;
                    return operationResult;
                }

                // Validar si alguno de los campos necesarios es nulo
                if (string.IsNullOrEmpty(entity.TaskDescription))
                {
                    operationResult.Message = "Algunos campos de la tarea no pueden ser nulos o vacíos.";
                    operationResult.Success = false;
                    return operationResult;
                }

                // Actualizar los campos de la tarea existente
                task.TaskDescription = entity.TaskDescription;
                task.DueDate = entity.DueDate;
                task.TaskStatus = entity.TaskStatus;

                // Guardar los cambios en la base de datos
                _taskManagercontext.Task.Update(task);
                await _taskManagercontext.SaveChangesAsync();

                // Asignar los resultados de la actualización
                operationResult.Message = "La tarea ha sido actualizada correctamente.";
                operationResult.Success = true;
                operationResult.Result = new TaskModel
                {
                    TaskId = task.TaskId,
                    TaskDescription = task.TaskDescription,
                    DueDate = task.DueDate,
                    TaskStatus = task.TaskStatus
                };
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error actualizando la tarea. {ex.Message}";
                _logger.LogError($"Error al actualizar la tarea: {ex}");
            }

            return operationResult;
        }

    }
}
