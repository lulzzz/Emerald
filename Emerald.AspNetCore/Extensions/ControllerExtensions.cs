using Emerald.AspNetCore.Application;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Emerald.AspNetCore.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult OperationResult(this Controller controller, OperationResult operationResult)
        {
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkResult();
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }

        public static IActionResult OperationResult<T>(this Controller controller, OperationResult<T> operationResult)
        {
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkObjectResult(operationResult.Output);
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }

        public static IActionResult OperationResult<TResult, TViewModel>(this Controller controller, OperationResult<TResult> operationResult, Func<TResult, TViewModel> viewModelFactory)
        {
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkObjectResult(viewModelFactory(operationResult.Output));
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }

        public static IActionResult QueryResult(this Controller controller, object result)
        {
            return result == null ? (IActionResult)new NotFoundResult() : new OkObjectResult(result);
        }

        public static IActionResult QueryResult<TResult, TViewModel>(this Controller controller, TResult result, Func<TResult, TViewModel> viewModelFactory)
        {
            return result == null ? (IActionResult)new NotFoundResult() : new OkObjectResult(viewModelFactory(result));
        }
    }
}