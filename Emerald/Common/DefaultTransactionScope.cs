using Emerald.Abstractions;

namespace Emerald.Common
{
    public sealed class DefaultTransactionScope : ITransactionScope
    {
        public void Dispose()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }
    }
}