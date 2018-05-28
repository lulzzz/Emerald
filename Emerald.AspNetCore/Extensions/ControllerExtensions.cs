﻿using Emerald.Application;
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
            switch (operationResult.Type)
            {
                case OperationResultType.Success: return new OkResult();
                case OperationResultType.Created: return new CreatedResult(location, null);
                case OperationResultType.Deleted: return new NoContentResult();
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                case OperationResultType.PaymentRequired: return new StatusCodeResult(StatusCodes.Status402PaymentRequired);
                case OperationResultType.Forbidden: return new StatusCodeResult(StatusCodes.Status403Forbidden);
                case OperationResultType.Unauthorized: return new StatusCodeResult(StatusCodes.Status401Unauthorized);
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
                case OperationResultType.Deleted: return operationResult.Output == null ? (IActionResult)new NoContentResult() : new OkObjectResult(viewModelFactory(operationResult.Output));
                case OperationResultType.NotFound: return new NotFoundResult();
                case OperationResultType.Error: return new BadRequestObjectResult(operationResult.ErrorMessage);
                case OperationResultType.PaymentRequired: return new StatusCodeResult(StatusCodes.Status402PaymentRequired);
                case OperationResultType.Forbidden: return new StatusCodeResult(StatusCodes.Status403Forbidden);
                case OperationResultType.Unauthorized: return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                default: throw new NotSupportedException();
            }
        }

        public static IActionResult QueryResult<TResult>(this Controller controller, QueryResult<TResult> queryResult)
        {
            return QueryResult(controller, queryResult, r => r);
        }
        public static IActionResult QueryResult<TResult, TViewModel>(this Controller controller, QueryResult<TResult> queryResult, Func<TResult, TViewModel> viewModelFactory)
        {
            if (queryResult.Type == QueryResultType.Success)
            {
                return new OkObjectResult(queryResult.Output == null ? (object)null : viewModelFactory(queryResult.Output));
            }

            if (queryResult.Type == QueryResultType.NotFound)
            {
                return new NotFoundResult();
            }

            if (queryResult.Type == QueryResultType.Error)
            {
                return new BadRequestObjectResult(queryResult.ErrorMessage);
            }

            var fileOutput = queryResult.Output as QueryResultFileOutput;

            if (queryResult.Type == QueryResultType.File && fileOutput != null)
            {
                return new FileContentResult(fileOutput.Content, fileOutput.ContentType) { FileDownloadName = fileOutput.FileName };
            }

            throw new NotSupportedException();
        }
    }
}