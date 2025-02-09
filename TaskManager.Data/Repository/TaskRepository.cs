using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Me.Planner.Plans.Item.Tasks;
using Microsoft.Graph.Models;
using Microsoft.VisualBasic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        // Validate null fields
        public delegate string ValidateNullInputs(TaskEntity<string> entity, List<TaskModel<string>> tasks);

        //API methods
        public async Task<OperationResult<List<TaskModel<string>>>> GetAll()
        {
            OperationResult<List<TaskModel<string>>> operationResult = new OperationResult<List<TaskModel<string>>>();

            try
            {
                var tasks = await _taskManagercontext.Task
                    .OrderByDescending(db => db.TaskId)
                    .Select(db => new TaskModel<string>()
                    {
                        TaskId = db.TaskId,
                        TaskDescription = db.TaskDescription,
                        DueDate = db.DueDate,
                        TaskStatus = db.TaskStatus,
                        AdditionalData = db.AdditionalData
                    }).ToListAsync();

                // Implementando Factory
                return OperationResult<List<TaskModel<string>>>.SuccessResult(tasks);
            }

            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error obteniendo las tareas. {ex.Message}";
                _logger.LogError(errorMessage, ex.ToString());

                return OperationResult<List<TaskModel<string>>>.ErrorResult(errorMessage, ex);
            }
        }

        public async Task<OperationResult<List<TaskModel<string>>>> GetAll(Expression<Func<TaskEntity<string>, bool>> filter)
        {
            OperationResult<List<TaskModel<string>>> operationResult = new OperationResult<List<TaskModel<string>>>();

            try
            {
                var tasks = await _taskManagercontext.Task
                    .Where(filter)
                    .Select(db => new TaskModel<string>()
                    {
                        TaskId = db.TaskId,
                        TaskDescription = db.TaskDescription,
                        DueDate = db.DueDate,
                        TaskStatus = db.TaskStatus,
                        AdditionalData = db.AdditionalData
                    }).ToListAsync();

                // Implementando Factory
                return OperationResult<List<TaskModel<string>>>.SuccessResult(tasks);
            }

            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error obteniendo las tareas. {ex.Message}";
                _logger.LogError(errorMessage, ex.ToString());

                return OperationResult<List<TaskModel<string>>>.ErrorResult(errorMessage, ex);
            }
        }

        public async Task<OperationResult<TaskModel<string>>> GetEntityBy(int taskId)
        {
            OperationResult<TaskModel<string>> operationResult = new OperationResult<TaskModel<string>>();

            try
            {
                // Func para validar el ID
                Func<int, string> validateTaskId = (id) =>
                {
                    if (id <= 0) return "El ID no puede ser menor o igual a 0.";
                    return null;
                };

                // Ejecutar validación del ID
                string validationMessage = validateTaskId(taskId);

                if (validationMessage != null)
                {
                    operationResult.Success = false;
                    operationResult.Message = validationMessage;
                    return operationResult;
                }

                // Buscar la tarea
                var task = await _taskManagercontext.Task.FindAsync(taskId);

                if (task == null)
                {
                    return OperationResult<TaskModel<string>>.ErrorResult("No se encontró una tarea con el ID especificado.");
                }

                // Trae la tarea solicitada
                Action<TaskEntity<string>> TaskRetrieval = (retrievedTask) =>
                {
                    _logger.LogInformation($"Tarea obtenida: {retrievedTask.TaskId} - {retrievedTask.TaskDescription}");
                };

                // Mapear la entidad al modelo
                operationResult.Result = new TaskModel<string>
                {
                    TaskId = task.TaskId,
                    TaskDescription = task.TaskDescription,
                    DueDate = task.DueDate,
                    TaskStatus = task.TaskStatus,
                    AdditionalData = task.AdditionalData
                };

                operationResult.Success = true;
                operationResult.Message = "Tarea obtenida correctamente.";

                // Ejecutar log
                TaskRetrieval(task);
            }

            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error obteniendo la tarea {taskId}. {ex.Message}";
                _logger.LogError(errorMessage, ex);
                return OperationResult<TaskModel<string>>.ErrorResult(errorMessage, ex);
            }
        }


        public async Task<OperationResult<TaskModel<string>>> Remove(TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> operationResult = new OperationResult<TaskModel<string>>();

            try
            {
                // Func para validar si la tarea existe
                Func<TaskEntity<string>, Task<string>> validateTaskExistence = async (taskEntity) =>
                {
                    var task = await _taskManagercontext.Task.FindAsync(taskEntity.TaskId);
                    return task == null ? "La tarea no existe o el ID es inválido." : null;
                };

                // Ejecutar la validación
                string validationMessage = await validateTaskExistence(entity);
                if (validationMessage != null)
                {
                    operationResult.Success = false;
                    operationResult.Message = validationMessage;
                    return operationResult;
                }

                // Buscar la tarea en la base de datos
                var taskToRemove = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                // Action para notificar la eliminación
                Action<TaskEntity<string>> notifyTaskDeletion = (task) =>
                {
                    Console.WriteLine($"La tarea '{task.TaskDescription}' ha sido eliminada.");
                };

                // Eliminar la tarea
                _taskManagercontext.Task.Remove(taskToRemove);
                await _taskManagercontext.SaveChangesAsync();

                // Ejecutar la notificación
                notifyTaskDeletion(taskToRemove);

                // Asignar mensaje de éxito
                operationResult.Success = true;
                operationResult.Message = "La tarea fue eliminada correctamente.";
            }
            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error eliminando la tarea. {ex.Message}";
                _logger.LogError(operationResult.Message, ex);
            }

            return operationResult;
        }

        public async Task<OperationResult<TaskModel<string>>> Save(TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> operationResult = new OperationResult<TaskModel<string>>();

            try
            {
                var tasks = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (entity == null)
                {
                    operationResult.Success = false;
                    operationResult.Message = "La tarea no puede ser nula";
                    return operationResult;
                }

                // Validate if description or due date is null or before actual date
                ValidateNullInputs validateDescriptionOrDueDates = (TaskEntity<string> task, List<TaskModel<string>> tasks) =>
                {
                    // validate if description is null
                    if (string.IsNullOrWhiteSpace(task.TaskDescription))
                    {
                        return "La descripcion no puede ser nula";
                    }

                    if (task.DueDate <= DateTime.Now)
                    {
                        return "La fecha no puede ser antes de la fecha actual";
                    }

                    var taskIdExist = tasks.Any(task => task.TaskId == entity.TaskId);

                    if (taskIdExist)
                    {
                        return "El ID de tarea ya existe, intenta con otro.";
                    }

                    // If there's no any error
                    return null;

                };

                // Agregar la tarea al contexto
                _taskManagercontext.Add(entity);
                await _taskManagercontext.SaveChangesAsync();

                // Func para calcular días restantes
                Func<TaskEntity<string>, int> calculateRemainingDays = task => (task.DueDate - DateTime.Now).Days;

                // Action para notificar la creación de la tarea
                Action<TaskEntity<string>> taskNotification = task =>
                {
                    Console.WriteLine($"Tarea creada: {task.TaskDescription}, se vence en {calculateRemainingDays(task)} días.");
                };

                // Ejecutar la acción
                taskNotification(entity);

                // Asignar mensaje de éxito
                operationResult.Success = true;
                operationResult.Message = $"La tarea fue agregada correctamente y se vence en {calculateRemainingDays(entity)} días.";
            
            }

            catch (Exception ex)
            {
                operationResult.Success = false;
                operationResult.Message = $"Ocurrió un error guardando la tarea. {ex.Message}";
                _logger.LogError(operationResult.Message, ex.ToString());
            }

            return operationResult;
        }

        public async Task<OperationResult<TaskModel<string>>> Update(TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> operationResult = new OperationResult<TaskModel<string>>();

            try
            {
                if (entity == null || entity.TaskId <= 0)
                {
                    operationResult.Message = "La tarea no puede ser nula y debe tener un ID válido.";
                    operationResult.Success = false;
                    return operationResult;
                }

                var task = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (task == null)
                {
                    operationResult.Message = "La tarea no existe.";
                    operationResult.Success = false;
                    return operationResult;
                }

                var tasks = await _taskManagercontext.Task.ToListAsync();

                ValidateNullInputs validateDescriptionOrDueDates = (TaskEntity<string> task, List<TaskModel<string>> tasks) =>
                {
                    if (string.IsNullOrWhiteSpace(task.TaskDescription))
                    {
                        return "La descripción no puede ser nula.";
                    }

                    if (task.DueDate <= DateTime.Now)
                    {
                        return "La fecha no puede ser antes de la fecha actual.";
                    }

                    var taskIdExist = tasks.Any(t => t.TaskId == entity.TaskId && t.TaskId != task.TaskId);

                    if (taskIdExist)
                    {
                        return "El ID de tarea ya existe, intenta con otro.";
                    }

                    return null;
                };

               
                task.TaskDescription = entity.TaskDescription;
                task.DueDate = entity.DueDate;
                task.TaskStatus = entity.TaskStatus;
                task.AdditionalData = entity.AdditionalData;

                _taskManagercontext.Task.Update(task);
                await _taskManagercontext.SaveChangesAsync();

                Func<TaskEntity<string>, int> calculateRemainingDays = task => (task.DueDate - DateTime.Now).Days;

                Action<TaskEntity<string>> taskNotification = task =>
                {
                    Console.WriteLine($"Tarea actualizada: {task.TaskDescription}, se vence en {calculateRemainingDays(task)} días.");
                };

                taskNotification(entity);

                operationResult.Message = $"La tarea ha sido actualizada correctamente y se vence en {calculateRemainingDays(entity)} días.";
                operationResult.Success = true;
                operationResult.Result = new TaskModel<string>
                {
                    TaskId = task.TaskId,
                    TaskDescription = task.TaskDescription,
                    DueDate = task.DueDate,
                    TaskStatus = task.TaskStatus,
                    AdditionalData = task.AdditionalData
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
