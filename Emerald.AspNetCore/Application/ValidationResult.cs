namespace Emerald.AspNetCore.Application
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

        public static ValidationResult Success() => new ValidationResult(ValidationResultType.Success, null);
        public static ValidationResult Error(string errorMessage) => new ValidationResult(ValidationResultType.Error, errorMessage);
    }
}