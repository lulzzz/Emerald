using Emerald.Application;
using Emerald.Persistence;
using Microsoft.AspNetCore.Http;
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
            if (operationResult.IsSuccess) return new OkResult();
            if (operationResult.IsCreated) return new CreatedResult(location, null);
            if (operationResult.IsDeleted) return new NoContentResult();
            if (operationResult.IsNotFound) return new NotFoundResult();
            if (operationResult.IsError) return new BadRequestObjectResult(operationResult.GetError().Code.HasValue ? (object)operationResult.GetError() : new { operationResult.GetError().Message });
            if (operationResult.IsPaymentRequired) return new StatusCodeResult(StatusCodes.Status402PaymentRequired);
            if (operationResult.IsForbidden) return new StatusCodeResult(StatusCodes.Status403Forbidden);
            if (operationResult.IsUnauthorized) return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            throw new NotSupportedException();
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
            if (operationResult.IsSuccess) return new OkObjectResult(viewModelFactory(operationResult.Output));
            if (operationResult.IsCreated) return new CreatedResult(location, viewModelFactory(operationResult.Output));
            if (operationResult.IsDeleted) return operationResult.Output == null ? (IActionResult)new NoContentResult() : new OkObjectResult(viewModelFactory(operationResult.Output));
            if (operationResult.IsNotFound) return new NotFoundResult();
            if (operationResult.IsError) return new BadRequestObjectResult(operationResult.GetError().Code.HasValue ? (object)operationResult.GetError() : new { operationResult.GetError().Message });
            if (operationResult.IsPaymentRequired) return new StatusCodeResult(StatusCodes.Status402PaymentRequired);
            if (operationResult.IsForbidden) return new StatusCodeResult(StatusCodes.Status403Forbidden);
            if (operationResult.IsUnauthorized) return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            throw new NotSupportedException();
        }
        public static IActionResult QueryResult<TResult>(this Controller controller, QueryResult<TResult> queryResult)
        {
            return QueryResult(controller, queryResult, r => r);
        }
        public static IActionResult QueryResult<TResult, TViewModel>(this Controller controller, QueryResult<TResult> queryResult, Func<TResult, TViewModel> viewModelFactory)
        {
            if (queryResult.IsSuccess) return new OkObjectResult(queryResult.Output == null ? (object)null : viewModelFactory(queryResult.Output));
            if (queryResult.IsNotFound) return new NotFoundResult();
            if (queryResult.IsError)
            {
                var error = queryResult.GetError();
                return new BadRequestObjectResult(error.Code.HasValue ? (object)error : new { error.Message });
            }
            if (queryResult.IsFile)
            {
                var file = queryResult.Output as FileQueryResultOutput;
                return new FileContentResult(file?.Content, file?.ContentType) { FileDownloadName = file?.FileName };
            }
            throw new NotSupportedException();
        }
    }
}