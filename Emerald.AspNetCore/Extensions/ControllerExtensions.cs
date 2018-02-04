using Emerald.AspNetCore.Application;
using Emerald.AspNetCore.Persistence;
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
            return OperationResult(controller, operationResult, r => r);
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

        public static IActionResult QueryResult<TResult>(this Controller controller, QueryResult<TResult> queryResult)
        {
            return QueryResult(controller, queryResult, r => r);
        }
        public static IActionResult QueryResult<TResult, TViewModel>(this Controller controller, QueryResult<TResult> queryResult, Func<TResult, TViewModel> viewModelFactory)
        {
            switch (queryResult.Type)
            {
                case QueryResultType.Success: return new OkObjectResult(viewModelFactory(queryResult.Output));
                case QueryResultType.NotFound: return new NotFoundResult();
                case QueryResultType.Error: return new BadRequestObjectResult(queryResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }
    }
}