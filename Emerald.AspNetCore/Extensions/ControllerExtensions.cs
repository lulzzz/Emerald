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
            return OperationResult(controller, operationResult, string.Empty);
        }
        public static IActionResult OperationResult(this Controller controller, OperationResult operationResult, string location)
        {
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkResult();
                case OperationResultType.Created: return new CreatedResult(location, null);
                case OperationResultType.Deleted: return new NoContentResult();
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }
        public static IActionResult OperationResult<T>(this Controller controller, OperationResult<T> operationResult)
        {
            return OperationResult(controller, operationResult, r => r, string.Empty);
        }
        public static IActionResult OperationResult<T>(this Controller controller, OperationResult<T> operationResult, string location)
        {
            return OperationResult(controller, operationResult, r => r, location);
        }
        public static IActionResult OperationResult<TResult, TViewModel>(this Controller controller, OperationResult<TResult> operationResult, Func<TResult, TViewModel> viewModelFactory)
        {
            return OperationResult(controller, operationResult, viewModelFactory, string.Empty);
        }
        public static IActionResult OperationResult<TResult, TViewModel>(this Controller controller, OperationResult<TResult> operationResult, Func<TResult, TViewModel> viewModelFactory, string location)
        {
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkObjectResult(viewModelFactory(operationResult.Output));
                case OperationResultType.Created: return new CreatedResult(location, viewModelFactory(operationResult.Output));
                case OperationResultType.Deleted: return new NoContentResult();
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
                case QueryResultType.Success: return new OkObjectResult(queryResult.Output == null ? (object)null : viewModelFactory(queryResult.Output));
                case QueryResultType.NotFound: return new NotFoundResult();
                case QueryResultType.Error: return new BadRequestObjectResult(queryResult.ErrorMessage);
                default: throw new NotSupportedException();
            }
        }
    }
}