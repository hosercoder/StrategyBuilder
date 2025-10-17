using HC.StrategyBuilder.src.Evaluators;
using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Interfaces.Evaluators;
using HC.StrategyBuilder.src.Interfaces.Serializers;
using HC.StrategyBuilder.src.Interfaces.Validators;
using HC.StrategyBuilder.src.Serializers;
using HC.StrategyBuilder.src.Strategies;
using HC.StrategyBuilder.src.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HC.StrategyBuilder.src.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddStrategyConfigurationServices(
                this IServiceCollection services,
                IConfiguration configuration)
        {
            services.AddScoped<IStrategyEngine, GenericStrategyEngine>();
            services.AddScoped<ITradeRuleEvaluator, TradeRuleEvaluator>();
            services.AddScoped<ITradeRuleSerializer, TradeRuleSerializer>();
            services.AddScoped<ITradeRuleValidator, TradeRuleValidator>();
            services.AddScoped<ITradeRuleConfiguration, TradeRuleConfiguration>();

            return services;
        }
    }
}