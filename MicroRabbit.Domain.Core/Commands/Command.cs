using System;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Commands
{
    //Command is ty0pe of Message
    public abstract class Command : Message
    {
        // only classes inherited from this class can set Timestamp
        public DateTime Timestamp { get; protected set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    }
    
}