using MediatR;

namespace MicroRabbit.Domain.Core.Events
{
    //Message implement IRequest which is from MediatR package
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
    
}