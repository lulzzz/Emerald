using Emerald.Common;

namespace Emerald.Application
{
    public sealed class OperationResult : IOperationResult
    {
        private readonly Error _error;
        private readonly string _errorMessage;
        private readonly OperationResultType _type;

        private OperationResult(Error error, string errorMessage, OperationResultType type)
        {
            _error = error;
            _errorMessage = errorMessage;
            _type = type;
        }

        public bool IsSuccess => _type == OperationResultType.Success;
        public bool IsNotFound => _type == OperationResultType.NotFound;
        public bool IsCreated => _type == OperationResultType.Created;
        public bool IsDeleted => _type == OperationResultType.Deleted;
        public bool IsError => _type == OperationResultType.Error;
        public bool IsPaymentRequired => _type == OperationResultType.PaymentRequired;
        public bool IsForbidden => _type == OperationResultType.Forbidden;
        public bool IsUnauthorized => _type == OperationResultType.Unauthorized;

        public string ErrorMessage => _errorMessage ?? _error?.Message;
        public object GetError() => _error ?? _errorMessage as object;

        public static OperationResult Success() => new OperationResult(null, null, OperationResultType.Success);
        public static OperationResult NotFound() => new OperationResult(null, null, OperationResultType.NotFound);
        public static OperationResult Created() => new OperationResult(null, null, OperationResultType.Created);
        public static OperationResult Deleted() => new OperationResult(null, null, OperationResultType.Deleted);
        public static OperationResult Error(string errorMessage) => new OperationResult(null, errorMessage, OperationResultType.Error);
        public static OperationResult Error(Error error) => new OperationResult(error, null, OperationResultType.Error);
        public static OperationResult PaymentRequired() => new OperationResult(null, null, OperationResultType.PaymentRequired);
        public static OperationResult Forbidden() => new OperationResult(null, null, OperationResultType.Forbidden);
        public static OperationResult Unauthorized() => new OperationResult(null, null, OperationResultType.Unauthorized);
    }

    public sealed class OperationResult<TOutput> : IOperationResult
    {
        private readonly Error _error;
        private readonly string _errorMessage;
        private readonly TOutput _output;
        private readonly OperationResultType _type;

        private OperationResult(Error error, string errorMessage, TOutput output, OperationResultType type)
        {
            _error = error;
            _errorMessage = errorMessage;
            _output = output;
            _type = type;
        }

        public bool IsSuccess => _type == OperationResultType.Success;
        public bool IsNotFound => _type == OperationResultType.NotFound;
        public bool IsCreated => _type == OperationResultType.Created;
        public bool IsDeleted => _type == OperationResultType.Deleted;
        public bool IsError => _type == OperationResultType.Error;
        public bool IsPaymentRequired => _type == OperationResultType.PaymentRequired;
        public bool IsForbidden => _type == OperationResultType.Forbidden;
        public bool IsUnauthorized => _type == OperationResultType.Unauthorized;

        public string ErrorMessage => _errorMessage ?? _error?.Message;
        public object GetError() => _error ?? _errorMessage as object;
        public TOutput GetOutput() => _output;

        public static OperationResult<TOutput> Success(TOutput output) => new OperationResult<TOutput>(null, null, output, OperationResultType.Success);
        public static OperationResult<TOutput> NotFound() => new OperationResult<TOutput>(null, null, default(TOutput), OperationResultType.NotFound);
        public static OperationResult<TOutput> Created(TOutput output) => new OperationResult<TOutput>(null, null, output, OperationResultType.Created);
        public static OperationResult<TOutput> Deleted(TOutput output) => new OperationResult<TOutput>(null, null, output, OperationResultType.Deleted);
        public static OperationResult<TOutput> Error(string errorMessage) => new OperationResult<TOutput>(null, errorMessage, default(TOutput), OperationResultType.Error);
        public static OperationResult<TOutput> Error(Error error) => new OperationResult<TOutput>(error, null, default(TOutput), OperationResultType.Error);
        public static OperationResult<TOutput> PaymentRequired() => new OperationResult<TOutput>(null, null, default(TOutput), OperationResultType.PaymentRequired);
        public static OperationResult<TOutput> Forbidden() => new OperationResult<TOutput>(null, null, default(TOutput), OperationResultType.Forbidden);
        public static OperationResult<TOutput> Unauthorized() => new OperationResult<TOutput>(null, null, default(TOutput), OperationResultType.Unauthorized);
    }

    public enum OperationResultType
    {
        Success = 0,
        Created = 1,
        Deleted = 2,
        NotFound = 3,
        Error = 4,
        PaymentRequired = 5,
        Forbidden = 6,
        Unauthorized = 7
    }

    public interface IOperationResult
    {
        bool IsSuccess { get; }
        bool IsError { get; }
    }
}