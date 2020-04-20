using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Infrastructure.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace MicroRbbit.Infrastructure.IoC
{
    public class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            //Domain Buss
            services.AddTransient<IEventBus, RabbitMQBus>();

            //Application Services Layer
            services.AddTransient<IAccountService, AccountService>();
            
            //Data
            services.AddTransient<IAccountRepository, IAccountRepository>();
            services.AddTransient<BankingDbContext>();
        }
    }
}