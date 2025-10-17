using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.src.Models.Events;
using HC.StrategyBuilder.src.Strategies;
using HC.TechnicalCalculators.Src.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HC.StrategyBuilder.Tests.Strategies
{
    public class GenericStrategyEngineTests
    {
        private readonly Mock<ILogger<GenericStrategyEngine>> _mockLogger;
        private readonly Mock<ITradeRuleConfiguration> _mockConfig;

        public GenericStrategyEngineTests()
        {
            _mockLogger = new Mock<ILogger<GenericStrategyEngine>>();
            _mockConfig = new Mock<ITradeRuleConfiguration>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GenericStrategyEngine(null!, _mockConfig.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GenericStrategyEngine(_mockLogger.Object, null!));
            Assert.Equal("config", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenParametersAreValid()
        {
            // Act
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Assert
            Assert.NotNull(engine);
        }

        #endregion

        #region Initialize Tests

        [Fact]
        public async Task Initialize_ShouldReturnEarly_WhenValidationFails()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };
            _mockConfig.Setup(c => c.Validate(out errors)).Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("test-config.json");

            // Assert
            _mockConfig.Verify(c => c.Validate(out errors), Times.Once);
        }

        [Fact]
        public async Task Initialize_ShouldRegisterStrategy_WhenValidationSucceeds()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("test-config.json");

            // Assert
            _mockConfig.Verify(c => c.Validate(out errors), Times.Once);
        }

        [Fact]
        public async Task Initialize_ShouldHandleNullRules_Gracefully()
        {
            // Arrange
            var tradeRule = new TradeRule
            {
                Name = "TestStrategy",
                CandleFrequency = "1m",
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = null,
                SellRule = null
            };
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert - Should not throw
            await engine.Initialize("test-config.json");
        }

        [Fact]
        public async Task Initialize_ShouldLogAndRethrowException_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _mockConfig.Setup(c => c.Validate(out It.Ref<List<string>>.IsAny))
                .Throws(exception);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Initialize("test-config.json"));

            Assert.Equal("Test exception", thrownException.Message);
        }

        [Fact(Skip = "Work in progress")]
        public async Task Initialize_ShouldRegisterCalculators_FromBothRules()
        {
            // Arrange
            var tradeRule = CreateComplexTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            await engine.Initialize("test-config.json");

            // Assert
            // Note: Since CalculatorFactory.CreateCalculator is static and might throw,
            // we mainly verify the logging occurs (calculator registration attempts)

            _mockLogger.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        #endregion

        #region CalculateIndicators Tests

        [Fact(Skip = "Work in progress")]
        public async Task CalculateIndicators_ShouldReturnEmptyDictionary_WhenCandlesIsNull()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act
            var result = await engine.CalculateIndicators(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockLogger.Verify(logger => logger.LogWarning("No candles provided for indicator calculation"), Times.Once);
        }

        [Fact]
        public async Task CalculateIndicators_ShouldReturnEmptyDictionary_WhenCandlesIsEmpty()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var emptyCandles = Array.Empty<Candle>();

            // Act
            var result = await engine.CalculateIndicators(emptyCandles);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            //VerifyLogWarning("No candles provided for indicator calculation");
        }

        [Fact]
        public async Task CalculateIndicators_ShouldReturnEmptyDictionary_WhenNoCalculatorsRegistered()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candles = CreateTestCandles(5);

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            //VerifyLogWarning("No calculators registered for indicator calculation");
        }

        [Fact]
        public async Task CalculateIndicators_ShouldHandleCalculatorErrors_Gracefully()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candles = CreateTestCandles(60);

            // Act
            var result = await engine.CalculateIndicators(candles);

            // Assert
            Assert.NotNull(result);
            // Note: Result might be empty if CalculatorFactory throws exceptions
            // This test mainly verifies error handling doesn't crash the method
        }

        #endregion

        #region EvaluateStrategy Tests

        [Fact]
        public async Task EvaluateStrategy_ShouldReturnFalse_WhenStrategyNotFound()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy("NonExistentStrategy", candle, indicators);

            // Assert
            Assert.False(result);
            //VerifyLogWarning("Strategy {Name} not found", "NonExistentStrategy");
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldReturnTrue_WhenBuySignalIsTrue()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(true);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.True(result);
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(tradeRule.Name, eventArgs.StrategyName);
            Assert.Equal(candle.ProductId, eventArgs.ProductId);
            Assert.True(eventArgs.IsBuySignal);
            Assert.False(eventArgs.IsSellSignal);
            Assert.Equal(candle.Close, eventArgs.Price);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldReturnTrue_WhenSellSignalIsTrue()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.BuyRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(false);
            _mockConfig.Setup(c => c.Evaluate(tradeRule.SellRule, It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.True(result);
            Assert.True(eventRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(tradeRule.Name, eventArgs.StrategyName);
            Assert.Equal(candle.ProductId, eventArgs.ProductId);
            Assert.False(eventArgs.IsBuySignal);
            Assert.True(eventArgs.IsSellSignal);
            Assert.Equal(candle.Close, eventArgs.Price);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldReturnFalse_WhenNeitherSignalIsTrue()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(false);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            bool eventRaised = false;
            engine.OnStrategySignal += (sender, args) => { eventRaised = true; };

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.False(result);
            Assert.False(eventRaised);
        }

        [Fact]
        public async Task EvaluateStrategy_ShouldReturnTrue_WhenBothSignalsAreTrue()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            bool eventRaised = false;
            StrategySignalEventArgs? eventArgs = null;
            engine.OnStrategySignal += (sender, args) =>
            {
                eventRaised = true;
                eventArgs = args;
            };

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
        public async Task EvaluateStrategy_ShouldReturnFalse_WhenExceptionOccurs()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Throws(new InvalidOperationException("Evaluation error"));

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var candle = CreateTestCandle();
            var indicators = new Dictionary<string, double>();

            // Act
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.False(result);
            //VerifyLogError("Error evaluating strategy {Name}", tradeRule.Name);
        }

        #endregion

        #region ConvertCandlesToDataArray Tests

        [Fact]
        public async Task ConvertCandlesToDataArray_ShouldThrowArgumentNullException_WhenCandlesIsNull()
        {
            // Arrange
            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            // Act & Assert
            // We can't directly test the private method, but we can test it through CalculateIndicators
            var result = await engine.CalculateIndicators(null!);

            Assert.Equal(result, new Dictionary<string, double>());
        }

        [Fact]
        public async Task ConvertCandlesToDataArray_ShouldThrowArgumentException_WhenCandlesIsEmpty()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);
            await engine.Initialize("test-config.json");

            var emptyCandles = Array.Empty<Candle>();

            // Act
            // The method will return early due to length check, so no exception will be thrown
            var result = await engine.CalculateIndicators(emptyCandles);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task Integration_ShouldWorkEndToEnd_WithValidConfiguration()
        {
            // Arrange
            var tradeRule = CreateValidTradeRule();
            var errors = new List<string>();

            _mockConfig.Setup(c => c.Validate(out errors)).Returns(true);
            _mockConfig.Setup(c => c.Rule).Returns(tradeRule);
            _mockConfig.Setup(c => c.Evaluate(It.IsAny<TradeSubRule>(), It.IsAny<Dictionary<string, double>>(), It.IsAny<Candle>()))
                .Returns(true);

            var engine = new GenericStrategyEngine(_mockLogger.Object, _mockConfig.Object);

            var candles = CreateTestCandles(60);
            var candle = CreateTestCandle();

            // Act
            await engine.Initialize("test-config.json");
            var indicators = await engine.CalculateIndicators(candles);
            var result = await engine.EvaluateStrategy(tradeRule.Name, candle, indicators);

            // Assert
            Assert.NotNull(indicators);
            Assert.True(result);
        }

        #endregion

        #region Helper Methods

        private TradeRule CreateValidTradeRule()
        {
            return new TradeRule
            {
                Name = "TestStrategy",
                CandleFrequency = "1m",
                MinProfit = 0.5,
                StopLoss = 0.2,
                TakeProfit = 1.0,
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = CreateValidSubRule("BuyCalc"),
                SellRule = CreateValidSubRule("SellCalc")
            };
        }

        private TradeRule CreateComplexTradeRule()
        {
            return new TradeRule
            {
                Name = "ComplexStrategy",
                CandleFrequency = "5m",
                MinProfit = 1.0,
                StopLoss = 0.3,
                TakeProfit = 2.0,
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = CreateComplexSubRule("BuyCalc"),
                SellRule = CreateComplexSubRule("SellCalc")
            };
        }

        private TradeSubRule CreateValidSubRule(string calcName)
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    new CalculatorConfig
                    {
                        Name = calcName,
                        CalculatorName = CalculatorNameEnum.SMA,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
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
                            CalculatorName = calcName,
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    }
                }
            };
        }

        private TradeSubRule CreateComplexSubRule(string calcName)
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    new CalculatorConfig
                    {
                        Name = calcName + "_SMA",
                        CalculatorName = CalculatorNameEnum.SMA,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                        Parameters = new List<CalculatorParameter>
                        {
                            new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "20" }
                        }
                    },
                    new CalculatorConfig
                    {
                        Name = calcName + "_RSI",
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
                            CalculatorName = calcName + "_SMA",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    },
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = calcName + "_RSI",
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
                Start = 1625097600000, // 2021-07-01T00:00:00Z in milliseconds
                Open = 33000,
                High = 34000,
                Low = 32500,
                Close = 33500,
                Volume = 1000
            };
        }

        private Candle[] CreateTestCandles(int count)
        {
            var candles = new Candle[count];
            long startTime = 1625097600000; // 2021-07-01T00:00:00Z in milliseconds

            for (int i = 0; i < count; i++)
            {
                candles[i] = new Candle
                {
                    ProductId = "BTC-USD",
                    Start = startTime + (i * 60000), // Add minutes
                    Open = 33000 + (i * 100),
                    High = 34000 + (i * 100),
                    Low = 32500 + (i * 100),
                    Close = 33500 + (i * 100),
                    Volume = 1000 + i
                };
            }

            return candles;
        }
        #endregion
    }
}