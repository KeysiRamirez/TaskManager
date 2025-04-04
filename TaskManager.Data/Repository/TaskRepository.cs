﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Me.Planner.Plans.Item.Tasks;
using Microsoft.Graph.Models;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Subjects;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TaskManager.Data.Context;
using TaskManager.Data.Entities;
using TaskManager.Data.Interfaces;
using TaskManager.Data.Models;
using TaskManager.Data.OperationResult;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace TaskManager.Data.Repository
{
    public sealed class TaskRepository : ITask
    {
        private readonly TaskManagerContext _taskManagercontext;
        private readonly ILogger<TaskRepository> _logger;
        private readonly Subject<Func<Task>> _taskQueue = new Subject<Func<Task>>();
        private readonly ConcurrentQueue<Func<Task>> _pendingTasks = new ConcurrentQueue<Func<Task>>();
        // To memorize
        private static readonly Dictionary<string, int> _daysRemainingCache = new();

        public TaskRepository(TaskManagerContext taskManagerContext, ILogger<TaskRepository> logger)
        {
            _taskManagercontext = taskManagerContext;
            _logger = logger;

            _taskQueue
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(async task => await task());
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

        public void EnqueueTask(Func<Task> task)
        {
            _pendingTasks.Enqueue(task);
            _taskQueue.OnNext(async () =>
            {
                if (_pendingTasks.TryDequeue(out var nextTask))
                {
                    await nextTask();
                }
            });
        }

        public async Task ProcessTasks(List<TaskModel<string>> tasks)
        {
            foreach (var task in tasks)
            {
                _logger.LogInformation($"Procesando tarea: {task.TaskId}");
                await Task.Delay(500);
            }
        }

        public async Task<OperationResult<List<TaskModel<string>>>> GetAll(Expression<Func<TaskEntity<string>, bool>> filter)
        {
            try
            {
                var tasks = await _taskManagercontext.Task
                    .AsNoTracking()
                    .Where(filter)
                    .Select(db => new TaskModel<string>
                    {
                        TaskId = db.TaskId,
                        TaskDescription = db.TaskDescription,
                        DueDate = db.DueDate,
                        TaskStatus = db.TaskStatus,
                        AdditionalData = db.AdditionalData
                    })
                    .ToListAsync();

                // Retorna un resultado exitoso
                return OperationResult<List<TaskModel<string>>>.SuccessResult(tasks);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error obteniendo las tareas: {ex.Message}";

                // Mejor forma de loggear
                _logger.LogError(ex, errorMessage);

                // Retorna el error correctamente
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

                // Mapea la entidad al modelo
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

            return operationResult;
        }


        public async Task<OperationResult<TaskModel<string>>> Remove(TaskEntity<string> entity)
        {
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
                    return OperationResult<TaskModel<string>>.ErrorResult(validationMessage);
                }

                // Buscar la tarea en la base de datos
                var taskToRemove = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                // Eliminar la tarea
                _taskManagercontext.Task.Remove(taskToRemove);
                await _taskManagercontext.SaveChangesAsync();

                // Log de eliminación
                _logger.LogInformation($"La tarea '{taskToRemove.TaskDescription}' ha sido eliminada.");

                var result = OperationResult<TaskModel<string>>.SuccessResult(new TaskModel<string>());
                result.Message = "La tarea fue eliminada correcta";
                return result;
            }

            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error eliminando la tarea. {ex.Message}";
                _logger.LogError(errorMessage, ex);
                return OperationResult<TaskModel<string>>.ErrorResult(errorMessage, ex);
            }
        }


        

        public async Task<OperationResult<TaskModel<string>>> Save(TaskEntity<string> entity)
        {
            OperationResult<TaskModel<string>> operationResult = new();

            try
            {
                var tasks = await _taskManagercontext.Task.FindAsync(entity.TaskId);

                if (entity == null)
                {
                    operationResult.Success = false;
                    operationResult.Message = "La tarea no puede ser nula";
                    return operationResult;
                }

                ValidateNullInputs validateDescriptionOrDueDates = (TaskEntity<string> task, List<TaskModel<string>> tasks) =>
                {
                    if (string.IsNullOrWhiteSpace(task.TaskDescription))
                    {
                        return "La descripcion no puede ser nula";
                    }

                    if (task.DueDate <= DateTime.Now)
                    {
                        return "La fecha no puede ser antes de la fecha actual";
                    }

                    var taskIdExist = tasks.Any(t => t.TaskId == entity.TaskId);
                    if (taskIdExist)
                    {
                        return "El ID de tarea ya existe, intenta con otro.";
                    }

                    return null;
                };

                _taskManagercontext.Add(entity);
                await _taskManagercontext.SaveChangesAsync();

                Func<TaskEntity<string>, int> calculateRemainingDays = task =>
                {
                    string cacheKey = task.TaskId.ToString();
                    if (_daysRemainingCache.TryGetValue(cacheKey, out int cachedDays))
                    {
                        return cachedDays;
                    }
                    int days = (task.DueDate - DateTime.Now).Days;
                    _daysRemainingCache[cacheKey] = days;
                    return days;
                };

                Action<TaskEntity<string>> taskNotification = task =>
                {
                    Console.WriteLine($"Tarea creada: {task.TaskDescription}, se vence en {calculateRemainingDays(task)} días.");
                };

                taskNotification(entity);

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
            OperationResult<TaskModel<string>> operationResult = new();

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

                Func<TaskEntity<string>, int> calculateRemainingDays = task =>
                {
                    string cacheKey = task.TaskId.ToString();
                    if (_daysRemainingCache.TryGetValue(cacheKey, out int cachedDays))
                    {
                        return cachedDays;
                    }
                    int days = (task.DueDate - DateTime.Now).Days;
                    _daysRemainingCache[cacheKey] = days;
                    return days;
                };

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
