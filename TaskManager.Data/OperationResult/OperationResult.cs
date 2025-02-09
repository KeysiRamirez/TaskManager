using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Data.OperationResult
{
    public class OperationResult<TData>
    {
        public OperationResult()
        {
            this.Success = true;
        }
        public bool Success { get; set; }
        public string? Message { get; set; }

        public TData? Result { get; set; }

        // Factory Method para éxito
        public static OperationResult<TData> SuccessResult(TData result) =>
            new OperationResult<TData> { Success = true, Result = result };

        // Factory Method para error
        public static OperationResult<TData> ErrorResult(string message, Exception? ex = null)
        {
            var operationResult = new OperationResult<TData>
            {
                Success = false,
                Message = message
            };

            // Si hay una excepción, loguearla aquí o agregar más detalles
            if (ex != null)
            {
                operationResult.Message += $" Error: {ex.Message}";
            }

            return operationResult;

        }
    }
}
