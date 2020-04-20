using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.Infrastructure.Bus
{
    //sealed prevent inherit or extend from this bus
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;

        //eventHandlers to handle our subscription events and event types'
        private readonly Dictionary<string, List<Type>>
            _handlers; // eventHandler: key: eventName, value: List of subscriptions related to the eventName

        private readonly List<Type> _eventTypes; // hold all types of event
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // like a subscription handler that handles and knows about which subscription are tired to which handlers and events
        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        //publish the event to the queue in RabbitMQ Server
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.QueueDeclare(eventName, false, false, false, null);

                // message is the body of an event
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("", eventName, null, body);
            }
        }

        //takes in event + eventHandler, when subscribe to an event, it will used the required eventHandler
        public void Subscribe<T, THandler>() where T : Event where THandler : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(THandler);

            //add new eventType if not exist prior
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            //add the eventHandler when a new eventType added.
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            //Validate that the Type of eventHandler does not exist in the _handlers.
            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} is already registered for ' {eventName}'. ");
            }

            //add new eventHandler in handlers for the event
            _handlers[eventName].Add(handlerType);

            //Once subscribe, consume the messages
            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true // Async
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            //event consumer
            var consumer = new AsyncEventingBasicConsumer(channel); //RabbitMQBus Type

            //it is a Delegate: a method pointer, a placeholder for events
            // += is syntax for creating assigning a method pointer
            //it is listening to message coming  the queue
            consumer.Received += Consumer_Received;

            channel.BasicConsume(eventName, true, consumer);
        }

        // message already in the queue, someone subscribe to the event,
        // we need a way to pick up that message and convert to an actual object so that 
        // it can be send to the event bus 
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey; //eventArgs contains all info about the message delivered
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span); // message = Body of eventArgs

            try
            {
                //know which handler is subscribed to this event and will process the event
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
            }
        }

        //the subscriber who subscribe the event will process the event
        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName)
            ) // look through dictionary of handlers and check if we have existing handler for the event
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    //if handler existed for this event
                    // multiple subscriptions for an event handler
                    var subscriptions = _handlers[eventName]; //subscription is List<T> that subscribe to this eventName
                    foreach (var subscription in subscriptions)
                    {
                        //dynamic approach to our generics 
                        var handler = scope.ServiceProvider.GetService(subscription); //dependency injection to get the service
                        if (handler == null) continue;
                        var eventType = _eventTypes.SingleOrDefault((t => t.Name == eventName));

                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        // this will use generics to kick off the handle method inside our handler and passing the event
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                        // invoke the right handler in micro services
                        await (Task) concreteType.GetMethod("Handle").Invoke(handler, new object[] {@event});
                    }
                }
            }
        }
    }
}