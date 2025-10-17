using HC.StrategyBuilder.src.Evaluators;
using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.Tests.Evaluators
{
    public class TradeRuleEvaluatorExtendedTests
    {
        private readonly TradeRuleEvaluator _evaluator;

        public TradeRuleEvaluatorExtendedTests()
        {
            _evaluator = new TradeRuleEvaluator();
        }

        #region Edge Cases and Error Handling

        [Fact]
        public void Evaluate_ShouldHandleNullIndicatorsDictionary()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeSubRule();
            var candle = CreateTestCandle();

            // Act
            var exception = Assert.Throws<NullReferenceException>(() => _evaluator.Evaluate(rule, null!, candle));

            // Assert
            Assert.Contains("Object reference not set to an instance", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldHandleNullCandle()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeSubRule();
            var indicators = new Dictionary<string, double>();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, null!));

            // Assert
            Assert.Contains("Indicator1", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldHandleEmptyIndicatorsDictionary()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeSubRule();
            var indicators = new Dictionary<string, double>();
            var candle = CreateTestCandle();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));

            // Assert
            Assert.Contains("Indicator1", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldHandleMissingIndicatorInDictionary()
        {
            // Arrange
            var rule = CreateRuleWithMissingIndicator();
            var indicators = new Dictionary<string, double>
            {
                { "DIFFERENTINDICATOR", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));

            // Assert
            Assert.Contains("Indicator1", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldHandleInvalidOperator()
        {
            // Arrange
            var rule = CreateRuleWithInvalidOperator();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
        }

        [Fact]
        public void Evaluate_ShouldHandleInvalidValueFormat()
        {
            // Arrange
            var rule = CreateRuleWithInvalidValue();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));

            // Assert
            Assert.Contains("Value 'not_a_number' not found or could not be parsed.", exception.Message);
        }

        [Theory]
        [InlineData("open", 100.0)]
        [InlineData("high", 105.0)]
        [InlineData("low", 95.0)]
        [InlineData("close", 102.0)]
        [InlineData("volume", 1000.0)]
        public void Evaluate_ShouldResolveCandleValues(string candleProperty, double expectedValue)
        {
            // Arrange
            var rule = CreateRuleWithCandleComparison(candleProperty);
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", expectedValue - 1 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleCalculatorNameResolution()
        {
            // Arrange
            var rule = CreateRuleWithCalculatorReference();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 60.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleFloatingPointComparisons()
        {
            // Arrange
            var rule = CreateRuleWithFloatingPointValues();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0000001 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result); // Should handle floating point precision
        }

        [Fact]
        public void Evaluate_ShouldHandleMultipleConditionsWithMixedResults()
        {
            // Arrange
            var rule = CreateComplexMixedRule();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 60.0 },
                { "RSI", 70.0 } // This will fail the < 30 condition
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result); // Should be false because not ALL conditions are true
        }

        [Fact]
        public void Evaluate_ShouldHandleIndicatorToIndicatorComparison()
        {
            // Arrange
            var rule = CreateIndicatorComparisonRule();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 },
                { "RSI", 40.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleZeroValues()
        {
            // Arrange
            var rule = CreateRuleWithZeroComparison();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 0.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleNegativeValues()
        {
            // Arrange
            var rule = CreateRuleWithNegativeComparison();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", -10.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleLargeNumbers()
        {
            // Arrange
            var rule = CreateRuleWithLargeNumberComparison();
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 1000000.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Helper Methods

        private TradeSubRule CreateRuleWithMissingIndicator()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "MissingIndicator",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithInvalidOperator()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = "??", // Invalid operator
                        Value = "50"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithInvalidValue()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "not_a_number"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithCandleComparison(string candleProperty)
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = "<",
                        Value = candleProperty
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithCalculatorReference()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    new CalculatorConfig
                    {
                        Name = "SMA20",
                        CalculatorName = CalculatorNameEnum.SMA,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                        Parameters = new List<CalculatorParameter>
                        {
                            new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "20" }
                        }
                    }
                },
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "SMA20",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithFloatingPointValues()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = "=",
                        Value = "50.0"
                    }
                }
            };
        }

        private TradeSubRule CreateComplexMixedRule()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "SMA",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "50"
                    },
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "RSI",
                            TechnicalIndicatorName = TechnicalNamesEnum.RSI
                        },
                        Operator = "<",
                        Value = "30"
                    }
                }
            };
        }

        private TradeSubRule CreateIndicatorComparisonRule()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "SMA",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Indicator2 = new Indicator
                        {
                            CalculatorName = "RSI",
                            TechnicalIndicatorName = TechnicalNamesEnum.RSI
                        }
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithZeroComparison()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = "=",
                        Value = "0"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithNegativeComparison()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = "<",
                        Value = "0"
                    }
                }
            };
        }

        private TradeSubRule CreateRuleWithLargeNumberComparison()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "Test",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "999999"
                    }
                }
            };
        }

        private Candle CreateTestCandle()
        {
            return new Candle
            {
                ProductId = "BTC-USD",
                Open = 100.0,
                High = 105.0,
                Low = 95.0,
                Close = 102.0,
                Volume = 1000.0,
                Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        #endregion
    }
}
