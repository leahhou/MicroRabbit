using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Events;
using MicroRabbit.Domain.Core.Bus;

namespace MicroRabbit.Banking.Domain.CommandHandlers
{
    public class TransferCommandHandler : IRequestHandler<CreateTransferCommand, bool>
    {
        private readonly IEventBus _bus;

        public TransferCommandHandler(IEventBus bus)
        {
            _bus = bus;
        }
        
        public Task<bool> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
        {
            //MediatR knows which handler is handling which command
            //Once bus send the command via mediaR, the right handler will respond to handle the command
            
            //Then the bus will public event to RabbitMQ
            // Event will get information from the request, i.e. the Command
            _bus.Publish(new TransferCreateEvent(request.From, request.To, request.Amount));
            
            return Task.FromResult(true);
        }
    }
}