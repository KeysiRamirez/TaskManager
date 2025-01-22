using System.Linq.Expressions;
using TaskManager.Data.OperationResult;

namespace TaskManager.Data.Interfaces
{
    public interface IBase<TEntity, TType, TModel> where TEntity : class
    {
        Task<OperationResult<TModel>> Save(TEntity entity);
        Task<OperationResult<TModel>> Update(TEntity entity);
        Task<OperationResult<TModel>> Remove(TEntity entity);
        Task<OperationResult<List<TModel>>> GetAll();
        Task<OperationResult<List<TModel>>> GetAll(Expression<Func<TEntity, bool>> filter);
        Task<OperationResult<TModel>> GetEntityBy(TType Id);
    }
}
