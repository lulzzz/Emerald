using Emerald.Common;

namespace Emerald.Application
{
    public sealed class ValidationResult
    {
        private readonly Error _error;
        private readonly ValidationResultType _type;

        private ValidationResult(Error error, ValidationResultType type)
        {
            _error = error;
            _type = type;
        }

        public bool IsSuccess => _type == ValidationResultType.Success;
        public bool IsError => _type == ValidationResultType.Error;

        public Error GetError() => _error;

        public static ValidationResult Success() => new ValidationResult(null, ValidationResultType.Success);
        public static ValidationResult Error(string errorMessage) => new ValidationResult(new Error(null, errorMessage), ValidationResultType.Error);
        public static ValidationResult Error(int errorCode, string errorMessage) => new ValidationResult(new Error(errorCode, errorMessage), ValidationResultType.Error);
        public static ValidationResult Error(Error error) => new ValidationResult(error, ValidationResultType.Error);
    }

    public sealed class ValidationResult<TOutput> where TOutput : class
    {
        private readonly Error _error;
        private readonly ValidationResultType _type;

        private ValidationResult(Error error, TOutput output, ValidationResultType type)
        {
            _error = error;
            Output = output;
            _type = type;
        }

        public bool IsSuccess => _type == ValidationResultType.Success;
        public bool IsError => _type == ValidationResultType.Error;

        public TOutput Output { get; }

        public Error GetError() => _error;

        public static ValidationResult<TOutput> Success(TOutput output) => new ValidationResult<TOutput>(null, output, ValidationResultType.Success);
        public static ValidationResult<TOutput> Error(string errorMessage) => new ValidationResult<TOutput>(new Error(null, errorMessage), null, ValidationResultType.Error);
        public static ValidationResult<TOutput> Error(int errorCode, string errorMessage) => new ValidationResult<TOutput>(new Error(errorCode, errorMessage), null, ValidationResultType.Error);
        public static ValidationResult<TOutput> Error(Error error) => new ValidationResult<TOutput>(error, null, ValidationResultType.Error);
    }

    public enum ValidationResultType
    {
        Success = 0,
        Error = 1
    }
}