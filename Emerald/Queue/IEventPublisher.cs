using System.Threading.Tasks;

namespace Emerald.Queue
{
    public interface IEventPublisher
    {
        Task Publish(object @event);
    }
}