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
        public override bool Equals(object obj)
        {
            return obj is Error other && other.Code == Code && other.Message == Message;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (Code.HasValue ? Code.GetHashCode() * 397 : 0) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }
    }
}