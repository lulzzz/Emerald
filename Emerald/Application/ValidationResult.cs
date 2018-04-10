namespace Emerald.Application
{
    public sealed class ValidationResult
    {
        private ValidationResult(ValidationResultType type, string errorMessage)
        {
            Type = type;
            ErrorMessage = errorMessage;
        }

        public ValidationResultType Type { get; }
        public string ErrorMessage { get; }
        public bool IsSuccess => Type == ValidationResultType.Success;
        public bool IsError => Type == ValidationResultType.Error;

        public static ValidationResult Success() => new ValidationResult(ValidationResultType.Success, null);
        public static ValidationResult Error(string errorMessage) => new ValidationResult(ValidationResultType.Error, errorMessage);
    }

    public sealed class ValidationResult<TOutput> where TOutput : class
    {
        private ValidationResult(ValidationResultType type, string errorMessage, TOutput output)
        {
            Type = type;
            ErrorMessage = errorMessage;
            Output = output;
        }

        public ValidationResultType Type { get; }
        public string ErrorMessage { get; }
        public TOutput Output { get; }
        public bool IsSuccess => Type == ValidationResultType.Success;
        public bool IsError => Type == ValidationResultType.Error;

        public static ValidationResult<TOutput> Success(TOutput output) => new ValidationResult<TOutput>(ValidationResultType.Success, null, output);
        public static ValidationResult<TOutput> Error(string errorMessage) => new ValidationResult<TOutput>(ValidationResultType.Error, errorMessage, null);
    }

    public enum ValidationResultType
    {
        Success,
        Error
    }
}