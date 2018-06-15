using Emerald.Common;

namespace Emerald.Persistence
{
    public sealed class QueryResult<TOutput>
    {
        private readonly Error _error;
        private readonly string _errorMessage;
        private readonly QueryResultType _type;

        private QueryResult(Error error, string errorMessage, TOutput output, QueryResultType type)
        {
            _error = error;
            _errorMessage = errorMessage;
            Output = output;
            _type = type;
        }

        public bool IsSuccess => _type == QueryResultType.Success;
        public bool IsNotFound => _type == QueryResultType.NotFound;
        public bool IsError => _type == QueryResultType.Error;
        public bool IsFile => _type == QueryResultType.File;

        public string ErrorMessage => _errorMessage ?? _error?.Message;
        public object GetError() => _error ?? _errorMessage as object;
        public TOutput Output { get; }

        public static QueryResult<TOutput> Success(TOutput output) => new QueryResult<TOutput>(null, null, output, QueryResultType.Success);
        public static QueryResult<TOutput> NotFound() => new QueryResult<TOutput>(null, null, default(TOutput), QueryResultType.NotFound);
        public static QueryResult<TOutput> Error(string errorMessage) => new QueryResult<TOutput>(null, errorMessage, default(TOutput), QueryResultType.Error);
        public static QueryResult<TOutput> Error(Error error) => new QueryResult<TOutput>(error, null, default(TOutput), QueryResultType.Error);
        public static QueryResult<File> File(File file) => new QueryResult<File>(null, null, file, QueryResultType.File);
    }

    public sealed class File
    {
        public File(byte[] content, string contentType, string fileName)
        {
            Content = content;
            ContentType = contentType;
            FileName = fileName;
        }

        public byte[] Content { get; }
        public string ContentType { get; }
        public string FileName { get; }
    }

    public enum QueryResultType
    {
        Success,
        NotFound,
        Error,
        File
    }
}