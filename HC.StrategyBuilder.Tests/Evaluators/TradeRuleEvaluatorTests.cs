using HC.StrategyBuilder.src.Evaluators;
using HC.StrategyBuilder.src.Models.Common;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.Tests.Evaluators
{
    public class TradeRuleEvaluatorTests
    {
        private readonly TradeRuleEvaluator _evaluator;

        public TradeRuleEvaluatorTests()
        {
            _evaluator = new TradeRuleEvaluator();
        }

        #region Evaluate Method Tests - Basic Functionality

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenRuleIsNull()
        {
            // Arrange
            var indicators = new Dictionary<string, double>();
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(null!, indicators, candle);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenConditionsIsNull()
        {
            // Arrange
            var rule = new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = null!
            };
            var indicators = new Dictionary<string, double>();
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenConditionsIsEmpty()
        {
            // Arrange
            var rule = new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>(),
                Conditions = new List<ConditionConfig>()
            };
            var indicators = new Dictionary<string, double>();
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnTrue_WhenSingleConditionIsTrue()
        {
            // Arrange
            var rule = CreateSimpleRule("50", ">", "30");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenSingleConditionIsFalse()
        {
            // Arrange
            var rule = CreateSimpleRule("30", ">", "50");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 30.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnTrue_WhenAllConditionsAreTrue()
        {
            // Arrange
            var rule = CreateMultipleConditionsRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 },
                { "RSI", 30.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenOneConditionIsFalse()
        {
            // Arrange
            var rule = CreateMultipleConditionsRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }, // This condition will be true (50 > 40)
                { "RSI", 80.0 }  // This condition will be false (80 < 70)
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Operator Tests

        [Theory]
        [InlineData(">", 50.0, 30.0, true)]
        [InlineData(">", 30.0, 50.0, false)]
        [InlineData("<", 30.0, 50.0, true)]
        [InlineData("<", 50.0, 30.0, false)]
        [InlineData(">=", 50.0, 50.0, true)]
        [InlineData(">=", 50.0, 30.0, true)]
        [InlineData(">=", 30.0, 50.0, false)]
        [InlineData("<=", 50.0, 50.0, true)]
        [InlineData("<=", 30.0, 50.0, true)]
        [InlineData("<=", 50.0, 30.0, false)]
        [InlineData("=", 50.0, 50.0, true)]
        [InlineData("=", 50.0, 50.000001, true)]
        public void Evaluate_ShouldHandleAllOperators_Correctly(string operatorSymbol, double value1, double value2, bool expected)
        {
            // Arrange
            var rule = CreateSimpleRule(value1.ToString(), operatorSymbol, value2.ToString());
            var indicators = new Dictionary<string, double>
            {
                { "SMA", value1 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Evaluate_ShouldThrowArgumentException_WhenOperatorIsInvalid()
        {
            // Arrange
            var rule = CreateSimpleRule("50", "!=", "30");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Invalid operator", exception.Message);
        }

        #endregion

        #region Indicator Comparison Tests

        [Fact]
        public void Evaluate_ShouldCompareIndicators_WhenIndicator2IsProvided()
        {
            // Arrange
            var rule = CreateIndicatorComparisonRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 },
                { "EMA", 45.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result); // 50 > 45
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenIndicatorComparisonFails()
        {
            // Arrange
            var rule = CreateIndicatorComparisonRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 40.0 },
                { "EMA", 45.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result); // 40 > 45 is false
        }

        #endregion

        #region Candle Value Tests

        [Theory]
        [InlineData("close", 100.0)]
        [InlineData("open", 95.0)]
        [InlineData("high", 105.0)]
        [InlineData("low", 90.0)]
        [InlineData("volume", 1000.0)]
        public void Evaluate_ShouldGetCandleValues_Correctly(string candleProperty, double expectedValue)
        {
            // Arrange
            var rule = CreateCandleComparisonRule(candleProperty);
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            // The rule compares SMA (50) < candleProperty, so result should be true for all candle values > 50
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldHandleCaseInsensitiveCandleProperties()
        {
            // Arrange
            var rule = CreateCandleComparisonRule("CLOSE");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result); // 50 < 100 (close price)
        }

        #endregion

        #region Calculator Value Tests

        [Fact]
        public void Evaluate_ShouldFindCalculatorValue_WhenAvailable()
        {
            // Arrange
            var rule = CreateCalculatorBasedRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA20", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result); // 50 > 40
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void Evaluate_ShouldThrowArgumentException_WhenIndicator1NotFound()
        {
            // Arrange
            var rule = CreateSimpleRule("50", ">", "30");
            var indicators = new Dictionary<string, double>(); // Empty indicators
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Indicator1", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldThrowArgumentException_WhenIndicator2NotFound()
        {
            // Arrange
            var rule = CreateIndicatorComparisonRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 } // Missing EMA
            };
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Indicator2", exception.Message);
        }

        [Fact(Skip = "Work in Progress")]
        public void Evaluate_ShouldThrowArgumentException_WhenValueNotFound()
        {
            // Arrange
            var rule = CreateSimpleRule("unknown_indicator", ">", "30");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Indicator1", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldThrowArgumentException_WhenNeitherIndicator2NorValueProvided()
        {
            // Arrange
            var rule = new TradeSubRule
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
                        Indicator2 = null,
                        Value = null
                    }
                }
            };
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Condition must have either Indicator2 or Value specified", exception.Message);
        }

        #endregion

        #region Value Resolution Tests

        [Fact]
        public void Evaluate_ShouldResolveValueByTechnicalIndicatorName()
        {
            // Arrange
            var rule = CreateSimpleRule("MOVINGAVERAGE", ">", "30");
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldResolveValueByCalculatorName()
        {
            // Arrange
            var rule = CreateSimpleRule("SMA", ">", "30");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ShouldParseNumericValues_Correctly()
        {
            // Arrange
            var rule = CreateSimpleRule("50.5", ">", "30.2");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.5 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Evaluate_ShouldHandleEqualityWithinTolerance()
        {
            // Arrange
            var rule = CreateSimpleRule("50.0000005", "=", "50.0000001");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0000005 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.True(result); // Difference is within tolerance (0.000001)
        }

        [Fact]
        public void Evaluate_ShouldHandleNullCandle_InTryGetCandleValue()
        {
            // Arrange
            var rule = CreateCandleComparisonRule("close");
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 50.0 }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, null!));
            Assert.Contains("Value", exception.Message);
        }

        [Fact]
        public void Evaluate_ShouldHandleEmptyStringValues()
        {
            // Arrange
            var rule = new TradeSubRule
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
                        Value = ""
                    }
                }
            };
            var indicators = new Dictionary<string, double>
            {
                { "MOVINGAVERAGE", 50.0 }
            };
            var candle = CreateTestCandle();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _evaluator.Evaluate(rule, indicators, candle));
            Assert.Contains("Condition must have either Indicator2 or Value specified", exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Evaluate_ShouldWorkWithComplexRule()
        {
            // Arrange
            var rule = CreateComplexRule();
            var indicators = new Dictionary<string, double>
            {
                { "SMA", 55.0 },      // SMA > close (100) - false
                { "RSI", 25.0 },      // RSI < 30 - true
                { "MACD", 1.5 },      // MACD > signal (1.0) - true
                { "SIGNAL", 1.0 }
            };
            var candle = CreateTestCandle();

            // Act
            var result = _evaluator.Evaluate(rule, indicators, candle);

            // Assert
            Assert.False(result); // One condition is false, so overall result is false
        }

        #endregion

        #region Helper Methods

        private TradeSubRule CreateSimpleRule(string indicator1Value, string operatorSymbol, string value)
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
                        Operator = operatorSymbol,
                        Value = value
                    }
                }
            };
        }

        private TradeSubRule CreateMultipleConditionsRule()
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
                        Value = "40"
                    },
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "RSI",
                            TechnicalIndicatorName = TechnicalNamesEnum.RSI
                        },
                        Operator = "<",
                        Value = "70"
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
                            CalculatorName = "EMA",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        }
                    }
                }
            };
        }

        private TradeSubRule CreateCandleComparisonRule(string candleProperty)
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
                        Operator = "<",
                        Value = candleProperty
                    }
                }
            };
        }

        private TradeSubRule CreateCalculatorBasedRule()
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
                        Value = "40"
                    }
                }
            };
        }

        private TradeSubRule CreateComplexRule()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    new CalculatorConfig
                    {
                        Name = "SMA",
                        CalculatorName = CalculatorNameEnum.SMA,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                        Parameters = new List<CalculatorParameter>()
                    },
                    new CalculatorConfig
                    {
                        Name = "RSI",
                        CalculatorName = CalculatorNameEnum.RSI,
                        TechnicalIndicators = new[] { TechnicalNamesEnum.RSI },
                        Parameters = new List<CalculatorParameter>()
                    }
                },
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
                        Value = "close"
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
                    },
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "MACD",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Indicator2 = new Indicator
                        {
                            CalculatorName = "SIGNAL",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        }
                    }
                }
            };
        }

        private Candle CreateTestCandle()
        {
            return new Candle
            {
                ProductId = "BTC-USD",
                Start = 1625097600000,
                Open = 95.0,
                High = 105.0,
                Low = 90.0,
                Close = 100.0,
                Volume = 1000.0
            };
        }

        #endregion
    }
}