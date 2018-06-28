using Emerald.Common;

namespace Emerald.Persistence
{
    public sealed class QueryResult<TOutput>
    {
        private readonly Error _error;
        private readonly QueryResultType _type;

        private QueryResult(Error error, TOutput output, QueryResultType type)
        {
            _error = error;
            Output = output;
            _type = type;
        }

        public bool IsSuccess => _type == QueryResultType.Success;
        public bool IsNotFound => _type == QueryResultType.NotFound;
        public bool IsError => _type == QueryResultType.Error;
        public bool IsFile => _type == QueryResultType.File;

        public object GetError() => _error;
        public TOutput Output { get; }

        public static QueryResult<TOutput> Success(TOutput output) => new QueryResult<TOutput>(null, output, QueryResultType.Success);
        public static QueryResult<TOutput> NotFound() => new QueryResult<TOutput>(null, default(TOutput), QueryResultType.NotFound);
        public static QueryResult<TOutput> Error(string errorMessage) => new QueryResult<TOutput>(new Error(null, errorMessage), default(TOutput), QueryResultType.Error);
        public static QueryResult<TOutput> Error(int errorCode, string errorMessage) => new QueryResult<TOutput>(new Error(errorCode, errorMessage), default(TOutput), QueryResultType.Error);
        public static QueryResult<TOutput> Error(Error error) => new QueryResult<TOutput>(error, default(TOutput), QueryResultType.Error);
        public static QueryResult<FileQueryResultOutput> File(byte[] content, string contentType, string fileName) => new QueryResult<FileQueryResultOutput>(null, new FileQueryResultOutput(content, contentType, fileName), QueryResultType.File);
    }

    public sealed class FileQueryResultOutput
    {
        public FileQueryResultOutput(byte[] content, string contentType, string fileName)
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
        Success = 0,
        NotFound = 1,
        Error = 2,
        File = 3
    }
}