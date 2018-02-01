using System.Threading.Tasks;

namespace Emerald.Jobs
{
    public interface IJob
    {
        Task Execute();
    }
}