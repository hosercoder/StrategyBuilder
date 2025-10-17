using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.src.Models.Events;
using HC.StrategyBuilder.src.Strategies;
using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HC.StrategyBuilder.Tests.Strategies
{
    public class GenericStrategyEngineExtendedTests
    {
        private readonly Mock<ILogger<GenericStrategyEngine>> _mockLogger;
        private readonly Mock<ITradeRuleConfiguration> _mockConfig;

        public GenericStrategyEngineExtendedTests()
        {
            _mockLogger = new Mock<ILogger<GenericStrategyEngine>>();
            _mockConfig = new Mock<ITradeRuleConfiguration>();
        }

        #region Constructor and Edge Cases

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new GenericStrategyEngine(null!, _mockConfig.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new GenericStrategyEngine(_mockLogger.Object, null!));
        }

        [Fact]
        public void Constructor_ShouldCreateValidInstance()
        {
            // Act
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Assert
            Assert.NotNull(engine);
        }

        [Fact]
        public void Constructor_ShouldAcceptValidParameters()
        {
            // Arrange & Act
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Assert
            Assert.NotNull(engine);
            // Verify the event is available
            Assert.NotNull(engine.GetType().GetEvent("OnStrategySignal"));
        }

        #endregion

        #region Initialize Tests - Extended Coverage

        [Fact]
        public async Task Initialize_ShouldHandleValidationFailure()
        {
            // Arrange
            var errors = new List<string> { "Validation Error 1", "Validation Error 2" };
            _mockConfig.Setup(c => c.Validate(out errors)).Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("invalid-config.json");

            // Assert
            _mockConfig.Verify(c => c.Initialize("invalid-config.json"), Times.Once);
            _mockConfig.Verify(c => c.Validate(out errors), Times.Once);
        }

        [Fact]
        public async Task Initialize_ShouldHandleConfigInitializationException()
        {
            // Arrange
            _mockConfig.Setup(c => c.Initialize(It.IsAny<string>()))
                      .Throws(new FileNotFoundException("Config file not found"));

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                engine.Initialize("missing-config.json"));
        }

        [Fact]
        public async Task Initialize_ShouldRegisterStrategyWithComplexRules()
        {
            // Arrange
            var tradeRule = CreateComplexTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("complex-config.json");

            // Assert
            _mockConfig.Verify(c => c.Initialize("complex-config.json"), Times.Once);
            _mockConfig.Verify(c => c.Validate(out errors), Times.Once);
        }

        [Fact]
        public async Task Initialize_ShouldHandleNullSubRules()
        {
            // Arrange
            var tradeRule = new TradeRule
            {
                Name = "TestStrategy",
                CandleFrequency = "1m",
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = null!,
                SellRule = null!
            };
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert - Should not throw
            await engine.Initialize("null-rules-config.json");
        }

        [Fact]
        public async Task Initialize_ShouldHandleSuccessfulConfiguration()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("valid-config.json");

            // Assert
            _mockConfig.Verify(c => c.Initialize("valid-config.json"), Times.Once);
            _mockConfig.Verify(c => c.Validate(out errors), Times.Once);
        }

        [Fact]
        public async Task Initialize_ShouldHandleEmptyCalculatorsList()
        {
            // Arrange
            var tradeRule = new TradeRule
            {
                Name = "EmptyCalculatorsStrategy",
                CandleFrequency = "1m",
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = new TradeSubRule
                {
                    Calculators = new List<CalculatorConfig>(),
                    Conditions = new List<ConditionConfig>()
                },
                SellRule = new TradeSubRule
                {
                    Calculators = new List<CalculatorConfig>(),
                    Conditions = new List<ConditionConfig>()
                }
            };
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert - Should not throw
            await engine.Initialize("empty-calculators-config.json");
        }

        #endregion

        #region CalculateIndicators Tests - Extended Coverage

        [Fact]
        public async Task CalculateIndicators_ShouldHandleEmptyCandleArray()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = Array.Empty<Candle>();

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldHandleNullCandleArray()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            var result = await engine.CalculateIndicators(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldHandleSingleCandle()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = new[] { CreateTestCandle() };

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldHandleLargeCandleArray()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = CreateTestCandles(1000);

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldReturnEmptyWhenNoCalculatorsRegistered()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = CreateTestCandles(10);

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldHandleValidCandleSequence()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = CreateSequentialTestCandles(20);

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
            // With no calculators registered, should return empty dictionary
            Assert.Empty(result);
        }

        #endregion

        #region EvaluateStrategy Tests - Extended Coverage

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleNullStrategyName()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => engine.EvaluateStrategy(null!, candle, indicators));
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleEmptyStrategyName()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy("", candle, indicators);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleNullCandle()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, null!, indicators);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleNullIndicators()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldRaiseEventOnBuySignal()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.True(result);
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.True(eventArgs.IsBuySignal);
            Assert.False(eventArgs.IsSellSignal);
            Assert.Equal(tradeRule.Name, eventArgs.StrategyName);
            Assert.Equal(candle.ProductId, eventArgs.ProductId);
            Assert.Equal(candle.Close, eventArgs.Price);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldRaiseEventOnSellSignal()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.True(result);
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.False(eventArgs.IsBuySignal);
            Assert.True(eventArgs.IsSellSignal);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldRaiseEventOnBothSignals()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.True(result);
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.True(eventArgs.IsBuySignal);
            Assert.True(eventArgs.IsSellSignal);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldNotRaiseEventWhenNoSignals()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            bool eventRaised = false;
            engine.OnStrategySignal += (sender, args) => eventRaised = true;

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.False(result);
            Assert.False(eventRaised);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleEvaluationException()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Throws(new InvalidOperationException("Evaluation failed"));

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldHandleNonExistentStrategy()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy("NonExistentStrategy", candle, indicators);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task Integration_ShouldHandleCompleteWorkflow()
        {
            // Arrange
            var tradeRule = CreateComplexTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            var candles = CreateTestCandles(100);
            var candle = CreateTestCandle();

            bool eventRaised = false;
            engine.OnStrategySignal += (sender, args) => eventRaised = true;

            // Act
            await engine.Initialize("test-config.json");
            var indicators = await engine.CalculateIndicators(candles);
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.NotNull(indicators);
            Assert.True(result);
            Assert.True(eventRaised);
        }

        [Fact]
        public async Task Integration_ShouldHandleMultipleStrategies()
        {
            // Arrange - This test simulates having multiple strategies, though our current implementation
            // only supports one strategy per engine instance
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act - Try to evaluate multiple strategy names
            var result1 = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);
            var result2 = await engine.EvaluateStrategy("NonExistentStrategy", candle, indicators);

            // Assert
            Assert.False(result2); // Non-existent strategy should return false
        }

        [Fact]
        public async Task Integration_ShouldHandleRealTimeDataFlow()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candles = CreateSequentialTestCandles(100);
            var eventCount = 0;
            engine.OnStrategySignal += (sender, args) => eventCount++;

            // Act - Simulate real-time processing
            var indicators = await engine.CalculateIndicators(candles);
            var results = new List<bool>();

            for (int i = 0; i < 10; i++)
            {
                var candle = CreateTestCandleAtTime(DateTime.UtcNow.AddMinutes(i));
                var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);
                results.Add(result);
            }

            // Assert
            Assert.NotNull(indicators);
            Assert.True(results.All(r => r)); // All should be true based on our mock setup
            Assert.Equal(10, eventCount); // Should have raised 10 events
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task CalculateIndicators_ShouldHandleLargeDatasets()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = CreateTestCandles(10000); // Large dataset

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await engine.CalculateIndicators(candles);
            stopwatch.Stop();

            // Assert
            Assert.NotNull(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Should complete within 5 seconds
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldBePerformant()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act - Run multiple evaluations
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Should complete 1000 evaluations within 1 second
        }

        [Fact]
        public async Task Performance_ShouldHandleConcurrentEvaluations()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act - Run concurrent evaluations
            var tasks = new List<Task<bool>>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(engine.EvaluateStrategy(tradeRule.Name, candle, indicators));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.True(results.All(r => r));
            Assert.Equal(100, results.Length);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task EdgeCase_ShouldHandleZeroVolumeCandles()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = new[]
            {
                new Candle
                {
                    ProductId = "BTC-USD",
                    Open = 50000.0,
                    High = 50000.0,
                    Low = 50000.0,
                    Close = 50000.0,
                    Volume = 0.0, // Zero volume
                    Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EdgeCase_ShouldHandleIdenticalPriceCandles()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = Enumerable.Range(0, 10)
                .Select(i => new Candle
                {
                    ProductId = "BTC-USD",
                    Open = 50000.0,
                    High = 50000.0,
                    Low = 50000.0,
                    Close = 50000.0,
                    Volume = 1000.0,
                    Start = DateTimeOffset.UtcNow.AddMinutes(i).ToUnixTimeMilliseconds()
                })
                .ToArray();

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task EdgeCase_ShouldHandleExtremelyHighPrices()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = new[]
            {
                new Candle
                {
                    ProductId = "BTC-USD",
                    Open = double.MaxValue / 2,
                    High = double.MaxValue / 2,
                    Low = double.MaxValue / 2,
                    Close = double.MaxValue / 2,
                    Volume = 1000.0,
                    Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region Event Handling Tests

        [Fact]
        public async Task EventHandling_ShouldSupportMultipleSubscribers()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            int subscriber1Count = 0;
            int subscriber2Count = 0;
            StrategySignalEventArgs? lastEventArgs1 = null;
            StrategySignalEventArgs? lastEventArgs2 = null;

            engine.OnStrategySignal += (sender, args) =>
            {
                subscriber1Count++;
                lastEventArgs1 = args;
            };

            engine.OnStrategySignal += (sender, args) =>
            {
                subscriber2Count++;
                lastEventArgs2 = args;
            };

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.Equal(1, subscriber1Count);
            Assert.Equal(1, subscriber2Count);
            Assert.NotNull(lastEventArgs1);
            Assert.NotNull(lastEventArgs2);
            Assert.Equal(lastEventArgs1.StrategyName, lastEventArgs2.StrategyName);
        }

        [Fact]
        public async Task EventHandling_ShouldIncludeCorrectTimestamp()
        {
            // Arrange
            var tradeRule = TradeRuleTestData.CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                      .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            StrategySignalEventArgs? eventArgs = null;
            var beforeTime = DateTime.UtcNow;

            engine.OnStrategySignal += (sender, args) => eventArgs = args;

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.NotNull(eventArgs);
            Assert.True(eventArgs.Timestamp >= beforeTime);
            Assert.True(eventArgs.Timestamp <= afterTime);
        }

        #endregion

        #region Helper Methods

        private TradeRule CreateComplexTradeRule()
        {
            return new TradeRule
            {
                Name = "ComplexStrategy",
                CandleFrequency = "5m",
                MinProfit = 1.0,
                StopLoss = 0.3,
                TakeProfit = 2.0,
                Bankroll = new BankrollConfig
                {
                    MaxRiskPerTrade = 0.03,
                    MinEntryAmount = 50.0
                },
                BuyRule = CreateComplexSubRule("Buy"),
                SellRule = CreateComplexSubRule("Sell")
            };
        }

        private TradeSubRule CreateComplexSubRule(string prefix)
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    new CalculatorConfig
                    {
                        Name = $"{prefix}_SMA",
                        CalculatorName = CalculatorNameEnum.SMA,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                        Parameters = new List<CalculatorParameter>
                        {
                            new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "20" }
                        }
                    },
                    new CalculatorConfig
                    {
                        Name = $"{prefix}_RSI",
                        CalculatorName = CalculatorNameEnum.RSI,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.RSI },
                        Parameters = new List<CalculatorParameter>
                        {
                            new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "14" }
                        }
                    }
                },
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = $"{prefix}_SMA",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    },
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = $"{prefix}_RSI",
                            TechnicalIndicatorName = TechnicalNamesEnum.RSI
                        },
                        Operator = "<",
                        Value = "70"
                    }
                }
            };
        }

        private Candle CreateTestCandle()
        {
            return new Candle
            {
                ProductId = "BTC-USD",
                Open = 50000.0,
                High = 51000.0,
                Low = 49000.0,
                Close = 50500.0,
                Volume = 1000.0,
                Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        private Candle CreateTestCandleAtTime(DateTime time)
        {
            return new Candle
            {
                ProductId = "BTC-USD",
                Open = 50000.0,
                High = 51000.0,
                Low = 49000.0,
                Close = 50500.0,
                Volume = 1000.0,
                Start = new DateTimeOffset(time).ToUnixTimeMilliseconds()
            };
        }

        private Candle[] CreateTestCandles(int count)
        {
            var candles = new Candle[count];
            var baseTime = DateTimeOffset.UtcNow;

            for (int i = 0; i < count; i++)
            {
                var price = 50000.0 + (i * 0.1); // Gradual price increase
                candles[i] = new Candle
                {
                    ProductId = "BTC-USD",
                    Open = price,
                    High = price + 100,
                    Low = price - 100,
                    Close = price + 50,
                    Volume = 1000.0,
                    Start = baseTime.AddSeconds(i).ToUnixTimeMilliseconds()
                };
            }

            return candles;
        }

        private Candle[] CreateSequentialTestCandles(int count)
        {
            var candles = new Candle[count];
            var baseTime = DateTimeOffset.UtcNow.AddDays(-count);

            for (int i = 0; i < count; i++)
            {
                var price = 50000.0 + (i * 10); // More significant price movement
                candles[i] = new Candle
                {
                    ProductId = "BTC-USD",
                    Open = price,
                    High = price + (i % 5 == 0 ? 500 : 100), // Occasional spikes
                    Low = price - (i % 7 == 0 ? 300 : 50),   // Occasional dips
                    Close = price + (i % 2 == 0 ? 25 : -25), // Alternating closes
                    Volume = 1000.0 + (i * 10),
                    Start = baseTime.AddMinutes(i).ToUnixTimeMilliseconds()
                };
            }

            return candles;
        }

        #endregion
    }
}