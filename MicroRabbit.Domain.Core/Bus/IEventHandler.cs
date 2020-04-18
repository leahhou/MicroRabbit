using System.Threading.Tasks;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    // IEventHandler takes in any type of event TEvent,
    // where TEvent implement Event Type, IEventHandler implement IEventHandler
    public interface IEventHandler<in TEvent> : IEventHandler
            where TEvent : Event
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler
    {}
}