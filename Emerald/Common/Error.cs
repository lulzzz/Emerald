namespace Emerald.Common
{
    public sealed class Error
    {
        public Error(int? code, string message)
        {
            Code = code;
            Message = message;
        }

        public int? Code { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"{(Code.HasValue ? $"{Code}: " : string.Empty)}{Message}";
        }
    }
}