namespace Emerald.Application
{
    public sealed class OperationResult : IOperationResult
    {
        private OperationResult(OperationResultType type, string errorMessage)
        {
            Type = type;
            ErrorMessage = errorMessage;
        }

        public OperationResultType Type { get; }
        public string ErrorMessage { get; }
        public bool IsSuccess => Type != OperationResultType.Error;
        public bool IsError => Type == OperationResultType.Error;

        public static OperationResult Success() => new OperationResult(OperationResultType.Success, null);
        public static OperationResult NotFound() => new OperationResult(OperationResultType.NotFound, null);
        public static OperationResult Created() => new OperationResult(OperationResultType.Created, null);
        public static OperationResult Deleted() => new OperationResult(OperationResultType.Deleted, null);
        public static OperationResult Error(string errorMessage) => new OperationResult(OperationResultType.Error, errorMessage);
        public static OperationResult PaymentRequired() => new OperationResult(OperationResultType.PaymentRequired, null);
    }

    public sealed class OperationResult<TOutput> : IOperationResult
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
        public bool IsSuccess => Type != OperationResultType.Error;
        public bool IsError => Type == OperationResultType.Error;

        public static OperationResult<TOutput> Success(TOutput output) => new OperationResult<TOutput>(OperationResultType.Success, null, output);
        public static OperationResult<TOutput> Created(TOutput output) => new OperationResult<TOutput>(OperationResultType.Created, null, output);
        public static OperationResult<TOutput> Deleted(TOutput output) => new OperationResult<TOutput>(OperationResultType.Deleted, null, output);
        public static OperationResult<TOutput> NotFound() => new OperationResult<TOutput>(OperationResultType.NotFound, null, default(TOutput));
        public static OperationResult<TOutput> Error(string errorMessage) => new OperationResult<TOutput>(OperationResultType.Error, errorMessage, default(TOutput));
        public static OperationResult<TOutput> PaymentRequired(TOutput output) => new OperationResult<TOutput>(OperationResultType.PaymentRequired, null, output);
    }

    public enum OperationResultType
    {
        Success = 0,
        Created = 1,
        Deleted = 2,
        NotFound = 3,
        Error = 4,
        PaymentRequired = 5
    }

    public interface IOperationResult
    {
        bool IsSuccess { get; }
        bool IsError { get; }
    }
}