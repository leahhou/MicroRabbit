using System.Threading.Tasks;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventBus // hold the logic for publishers & subscribers
    {
        //Take generic type T where T must be Type Command
        Task SendCommand<T>(T command) where T : Command;

        //publish Event, event is reserved keyword:event, thus add @ in front of the event
        void Publish<T>(T @event) where T : Event;

        void Subscribe<T, THandler>()
            where T : Event
            where THandler : IEventHandler<T>;
    }
}