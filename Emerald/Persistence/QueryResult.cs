namespace Emerald.Persistence
{
    public sealed class QueryResult<TOutput>
    {
        private QueryResult(QueryResultType type, string errorMessage, TOutput output)
        {
            Type = type;
            ErrorMessage = errorMessage;
            Output = output;
        }

        public bool IsSuccess => Type == QueryResultType.Success;
        public bool IsNotFound => Type == QueryResultType.NotFound;
        public bool IsError => Type == QueryResultType.Error;

        public QueryResultType Type { get; }
        public string ErrorMessage { get; }
        public TOutput Output { get; }

        public static QueryResult<TOutput> Success(TOutput output) => new QueryResult<TOutput>(QueryResultType.Success, null, output);
        public static QueryResult<TOutput> NotFound() => new QueryResult<TOutput>(QueryResultType.NotFound, null, default(TOutput));
        public static QueryResult<TOutput> Error(string errorMessage) => new QueryResult<TOutput>(QueryResultType.Error, errorMessage, default(TOutput));
        public static QueryResult<QueryResultFileOutput> File(QueryResultFileOutput output) => new QueryResult<QueryResultFileOutput>(QueryResultType.File, null, output);
    }

    public sealed class QueryResultFileOutput
    {
        public QueryResultFileOutput(byte[] content, string contentType, string fileName)
        {
            Content = content;
            ContentType = contentType;
            FileName = fileName;
        }

        public byte[] Content { get; }
        public string ContentType { get; }
        public string FileName { get; }
    }
}