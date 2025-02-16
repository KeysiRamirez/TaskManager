using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Data.Entities;
using TaskManager.Data.Models;
using TaskManager.Data.OperationResult;

namespace TaskManager.Data.Interfaces
{
    public interface ITask : IBase<TaskEntity<string>, int, TaskModel<string>>
    {
        public Task<OperationResult<List<TaskModel<string>>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<List<TaskModel<string>>>> GetAll(Expression<Func<TaskEntity<string>, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel<string>>> GetEntityBy(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel<string>>> Remove(TaskEntity<string> entity)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel<string>>> Save(TaskEntity<string> entity)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel<string>>> Update(TaskEntity<string> entity)
        {
            throw new NotImplementedException();
        }

        // Métodos para manejar la cola de tareas
        void EnqueueTask(Func<Task> task);
        Task ProcessTasks(List<TaskModel<string>> tasks);
    }
}
