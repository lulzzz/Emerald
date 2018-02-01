﻿namespace Emerald.AspNetCore.Application
{
    public sealed class OperationResult
    {
        private OperationResult(OperationResultType type, string errorMessage)
        {
            Type = type;
            ErrorMessage = errorMessage;
        }

        public OperationResultType Type { get; }
        public string ErrorMessage { get; }

        public static OperationResult Success() => new OperationResult(OperationResultType.Success, null);
        public static OperationResult NotFound() => new OperationResult(OperationResultType.NotFound, null);
        public static OperationResult Error(string errorMessage) => new OperationResult(OperationResultType.Error, errorMessage);
    }

    public sealed class OperationResult<TOutput>
    {
        private OperationResult(OperationResultType type, string errorMessage, TOutput output)
        {
            Type = type;
            ErrorMessage = errorMessage;
            Output = output;
        }

        public OperationResultType Type { get; }
        public string ErrorMessage { get; }
        public TOutput Output { get; }

        public static OperationResult<TOutput> Success(TOutput output) => new OperationResult<TOutput>(OperationResultType.Success, null, output);
        public static OperationResult<TOutput> NotFound() => new OperationResult<TOutput>(OperationResultType.NotFound, null, default(TOutput));
        public static OperationResult<TOutput> Error(string errorMessage) => new OperationResult<TOutput>(OperationResultType.Error, errorMessage, default(TOutput));
    }
}