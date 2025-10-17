using HC.StrategyBuilder.src.Extensions;
using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Interfaces.Evaluators;
using HC.StrategyBuilder.src.Interfaces.Serializers;
using HC.StrategyBuilder.src.Interfaces.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HC.StrategyBuilder.Tests.Extensions
{
    public class ServiceExtensionsTests
    {
        private IConfiguration CreateTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "TestKey", "TestValue" }
                })
                .Build();
            return config;
        }

        #region Service Registration Tests

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterAllServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();

            services.AddLogging(); // Ensure logging is available for strategy engine
            // Act
            services.AddStrategyConfigurationServices(configuration);

            // Assert
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<ITradeRuleEvaluator>());
            Assert.NotNull(provider.GetService<ITradeRuleSerializer>());
            Assert.NotNull(provider.GetService<ITradeRuleValidator>());
            Assert.NotNull(provider.GetService<ITradeRuleConfiguration>());
            Assert.NotNull(provider.GetService<IStrategyEngine>());
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterAllServicesAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);

            // Act & Assert
            var serviceDescriptors = services.Where(s =>
                s.ServiceType == typeof(ITradeRuleEvaluator) ||
                s.ServiceType == typeof(ITradeRuleSerializer) ||
                s.ServiceType == typeof(ITradeRuleValidator) ||
                s.ServiceType == typeof(ITradeRuleConfiguration) ||
                s.ServiceType == typeof(IStrategyEngine)).ToList();

            Assert.Equal(5, serviceDescriptors.Count);
            Assert.All(serviceDescriptors, descriptor =>
                Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime));
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterServicesWithCorrectImplementations()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);
            services.AddLogging(); // Ensure logging is available for strategy engine
            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.IsType<HC.StrategyBuilder.src.Evaluators.TradeRuleEvaluator>(
                provider.GetService<ITradeRuleEvaluator>());
            Assert.IsType<HC.StrategyBuilder.src.Serializers.TradeRuleSerializer>(
                provider.GetService<ITradeRuleSerializer>());
            Assert.IsType<HC.StrategyBuilder.src.Validators.TradeRuleValidator>(
                provider.GetService<ITradeRuleValidator>());
            Assert.IsType<TradeRuleConfiguration>(
                provider.GetService<ITradeRuleConfiguration>());
            Assert.IsType<HC.StrategyBuilder.src.Strategies.GenericStrategyEngine>(
                provider.GetService<IStrategyEngine>());
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterServicesWithCorrectLifetime()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);

            // Act
            using var provider = services.BuildServiceProvider();
            using var scope1 = provider.CreateScope();
            using var scope2 = provider.CreateScope();

            var config1 = scope1.ServiceProvider.GetService<ITradeRuleConfiguration>();
            var config2 = scope1.ServiceProvider.GetService<ITradeRuleConfiguration>();
            var config3 = scope2.ServiceProvider.GetService<ITradeRuleConfiguration>();

            // Assert - Within same scope should be same instance (Scoped)
            Assert.Same(config1, config2);
            // Assert - Different scopes should have different instances
            Assert.NotSame(config1, config3);
        }

        #endregion

        #region Dependency Resolution Tests

        [Fact]
        public void AddStrategyConfigurationServices_ShouldResolveDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);
            services.AddLogging(); // Ensure logging is available for strategy engine
            // Act
            var provider = services.BuildServiceProvider();
            var strategyEngine = provider.GetService<IStrategyEngine>();
            var tradeRuleConfig = provider.GetService<ITradeRuleConfiguration>();

            // Assert
            Assert.NotNull(strategyEngine);
            Assert.NotNull(tradeRuleConfig);

            // Verify that dependencies are properly resolved
            // TradeRuleConfiguration should have its dependencies injected
            Assert.NotNull(tradeRuleConfig);
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldResolveComplexDependencyGraph()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);
            services.AddLogging(); // Ensure logging is available for strategy engine
            // Act & Assert - Should not throw when building provider with complex dependencies
            var provider = services.BuildServiceProvider();

            // All services should be resolvable
            Assert.NotNull(provider.GetRequiredService<ITradeRuleEvaluator>());
            Assert.NotNull(provider.GetRequiredService<ITradeRuleSerializer>());
            Assert.NotNull(provider.GetRequiredService<ITradeRuleValidator>());
            Assert.NotNull(provider.GetRequiredService<ITradeRuleConfiguration>());
            Assert.NotNull(provider.GetRequiredService<IStrategyEngine>());
        }

        #endregion

        #region Method Behavior Tests

        [Fact]
        public void AddStrategyConfigurationServices_ShouldReturnServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();

            // Act
            var result = services.AddStrategyConfigurationServices(configuration);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldAllowMultipleCalls()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();

            // Act & Assert - Should not throw
            services.AddStrategyConfigurationServices(configuration);
            services.AddStrategyConfigurationServices(configuration);

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<ITradeRuleConfiguration>());

            // Verify that calling multiple times doesn't break anything
            // The last registration should win for each service type
            var descriptors = services.Where(s => s.ServiceType == typeof(ITradeRuleConfiguration));
            Assert.Equal(2, descriptors.Count()); // Should have 2 registrations
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldThrowWhenServiceCollectionIsNull()
        {
            // Arrange
            IServiceCollection? services = null;
            var configuration = CreateTestConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                services!.AddStrategyConfigurationServices(configuration));
        }

        #endregion

        #region Service Count and Registration Details Tests

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterExactlyFiveServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            var initialCount = services.Count;

            // Act
            services.AddStrategyConfigurationServices(configuration);

            // Assert
            Assert.Equal(initialCount + 5, services.Count);
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldRegisterServicesWithCorrectServiceTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);

            // Act
            var serviceTypes = services.Select(s => s.ServiceType).ToList();

            // Assert
            Assert.Contains(typeof(ITradeRuleEvaluator), serviceTypes);
            Assert.Contains(typeof(ITradeRuleSerializer), serviceTypes);
            Assert.Contains(typeof(ITradeRuleValidator), serviceTypes);
            Assert.Contains(typeof(ITradeRuleConfiguration), serviceTypes);
            Assert.Contains(typeof(IStrategyEngine), serviceTypes);
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldNotRegisterConcreteTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);

            // Act
            var serviceTypes = services.Select(s => s.ServiceType).ToList();

            // Assert - Should only register interfaces, not concrete implementations
            Assert.DoesNotContain(typeof(HC.StrategyBuilder.src.Evaluators.TradeRuleEvaluator), serviceTypes);
            Assert.DoesNotContain(typeof(HC.StrategyBuilder.src.Serializers.TradeRuleSerializer), serviceTypes);
            Assert.DoesNotContain(typeof(HC.StrategyBuilder.src.Validators.TradeRuleValidator), serviceTypes);
            Assert.DoesNotContain(typeof(TradeRuleConfiguration), serviceTypes);
            Assert.DoesNotContain(typeof(HC.StrategyBuilder.src.Strategies.GenericStrategyEngine), serviceTypes);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public void AddStrategyConfigurationServices_ShouldHandleEmptyConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            var emptyConfig = new ConfigurationBuilder().Build();

            // Act & Assert - Should not throw
            services.AddStrategyConfigurationServices(emptyConfig);

            var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<ITradeRuleConfiguration>());
        }

        [Fact]
        public void AddStrategyConfigurationServices_ShouldWorkWithExistingServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<string>("existing service");
            var configuration = CreateTestConfiguration();

            // Act
            services.AddStrategyConfigurationServices(configuration);

            // Assert
            var provider = services.BuildServiceProvider();
            Assert.Equal("existing service", provider.GetService<string>());
            Assert.NotNull(provider.GetService<ITradeRuleConfiguration>());
        }

        #endregion

        #region Performance and Resource Tests

        [Fact]
        public void AddStrategyConfigurationServices_ShouldDisposeResourcesProperly()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = CreateTestConfiguration();
            services.AddStrategyConfigurationServices(configuration);

            // Act & Assert - Should not throw during disposal
            using (var provider = services.BuildServiceProvider())
            {
                var service = provider.GetService<ITradeRuleConfiguration>();
                Assert.NotNull(service);
            } // Disposal happens here
        }

        #endregion
    }
}