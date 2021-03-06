using System.Threading.Tasks;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;

namespace MicroRabbit.Transfer.Domain.EventHandlers
{
    public class TransferCreateEventHandler :  IEventHandler<TransferCreateEvent>
    {
        private readonly ITransferRepository _transferRepository;
        public TransferCreateEventHandler(ITransferRepository  transferRepository)
        {
            _transferRepository = transferRepository;
        }

        public Task Handle(TransferCreateEvent @event)
        {
            _transferRepository.Add(new TransferLog()
            {
                FromAccount = @event.From,
                ToAccount = @event.To,
                TransferAmount = @event.Amount
            });
            
            return Task.CompletedTask;
        }
    }
}