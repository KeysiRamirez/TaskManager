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
    public interface ITask : IBase<TaskEntity, int, TaskModel>
    {
        public Task<OperationResult<List<TaskModel>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<List<TaskModel>>> GetAll(Expression<Func<TaskEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel>> GetEntityBy(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel>> Remove(TaskEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel>> Save(TaskEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<TaskModel>> Update(TaskEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
