namespace Emerald.Common
{
    public sealed class OperationResult : IOperationResult
    {
        private readonly Error _error;
        private readonly OperationResultType _type;

        private OperationResult(Error error, OperationResultType type)
        {
            _error = error;
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

        public Error GetError() => _error;

        public static OperationResult Success() => new OperationResult(null, OperationResultType.Success);
        public static OperationResult NotFound() => new OperationResult(null, OperationResultType.NotFound);
        public static OperationResult Created() => new OperationResult(null, OperationResultType.Created);
        public static OperationResult Deleted() => new OperationResult(null, OperationResultType.Deleted);
        public static OperationResult Error(string errorMessage) => new OperationResult(new Error(null, errorMessage), OperationResultType.Error);
        public static OperationResult Error(int errorCode, string errorMessage) => new OperationResult(new Error(errorCode, errorMessage), OperationResultType.Error);
        public static OperationResult Error(Error error) => new OperationResult(error, OperationResultType.Error);
        public static OperationResult PaymentRequired() => new OperationResult(null, OperationResultType.PaymentRequired);
        public static OperationResult Forbidden() => new OperationResult(null, OperationResultType.Forbidden);
        public static OperationResult Unauthorized() => new OperationResult(null, OperationResultType.Unauthorized);
    }

    public sealed class OperationResult<TOutput> : IOperationResult
    {
        private readonly Error _error;
        private readonly OperationResultType _type;

        private OperationResult(Error error, TOutput output, OperationResultType type)
        {
            _error = error;
            Output = output;
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

        public TOutput Output { get; }

        public Error GetError() => _error;

        public static OperationResult<TOutput> Success(TOutput output) => new OperationResult<TOutput>(null, output, OperationResultType.Success);
        public static OperationResult<TOutput> NotFound() => new OperationResult<TOutput>(null, default(TOutput), OperationResultType.NotFound);
        public static OperationResult<TOutput> Created(TOutput output) => new OperationResult<TOutput>(null, output, OperationResultType.Created);
        public static OperationResult<TOutput> Deleted(TOutput output) => new OperationResult<TOutput>(null, output, OperationResultType.Deleted);
        public static OperationResult<TOutput> Error(string errorMessage) => new OperationResult<TOutput>(new Error(null, errorMessage), default(TOutput), OperationResultType.Error);
        public static OperationResult<TOutput> Error(int errorCode, string errorMessage) => new OperationResult<TOutput>(new Error(errorCode, errorMessage), default(TOutput), OperationResultType.Error);
        public static OperationResult<TOutput> Error(Error error) => new OperationResult<TOutput>(error, default(TOutput), OperationResultType.Error);
        public static OperationResult<TOutput> PaymentRequired() => new OperationResult<TOutput>(null, default(TOutput), OperationResultType.PaymentRequired);
        public static OperationResult<TOutput> Forbidden() => new OperationResult<TOutput>(null, default(TOutput), OperationResultType.Forbidden);
        public static OperationResult<TOutput> Unauthorized() => new OperationResult<TOutput>(null, default(TOutput), OperationResultType.Unauthorized);
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
        bool IsNotFound { get; }
        bool IsCreated { get; }
        bool IsDeleted { get; }
        bool IsError { get; }
        bool IsPaymentRequired { get; }
        bool IsForbidden { get; }
        bool IsUnauthorized { get; }
    }
}