using HC.StrategyBuilder.src.Validators;
using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.Tests.Validators
{
    public class TradeRuleValidatorExtendedTests
    {
        private readonly TradeRuleValidator _validator;

        public TradeRuleValidatorExtendedTests()
        {
            _validator = new TradeRuleValidator();
        }

        #region Comprehensive Validation Tests

        [Fact]
        public void Validate_ShouldPassWithValidRule()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenRuleIsNull()
        {
            // Act
            var result = _validator.Validate(null!, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule section is missing.", errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldFailWithInvalidName(string? name)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Name = name!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.Name is required.", errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldFailWithInvalidCandleFrequency(string? frequency)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.CandleFrequency = frequency!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.CandleFrequency is required.", errors);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        public void Validate_ShouldFailWithNegativeMinProfit(double minProfit)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.MinProfit = minProfit;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.MinProfit must be greater than 0.", errors);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        public void Validate_ShouldFailWithNegativeStopLoss(double stopLoss)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.StopLoss = stopLoss;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.MaxLoss must be greater than or equal to 0.", errors);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        public void Validate_ShouldFailWithNegativeTakeProfit(double takeProfit)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.TakeProfit = takeProfit;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.TakeProfit must be greater than 0.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenBankrollIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Bankroll = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Bankroll configuration is required", errors);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.01)]
        public void Validate_ShouldFailWithInvalidMaxRiskPerTrade(double maxRisk)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Bankroll.MaxRiskPerTrade = maxRisk;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.MaxTradesPerCandle must be greater than or equal to 0.", errors);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.1)]
        [InlineData(0.0)]
        public void Validate_ShouldFailWithInvalidMinEntryAmount(double minEntry)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Bankroll.MinEntryAmount = minEntry;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.MinEntryAmount must be greater than 0.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenBuyRuleIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule is missing.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenSellRuleIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.SellRule = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("SellRule is missing.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenSubRuleCalculatorsIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculators is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenSubRuleConditionsIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Conditions is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenCalculatorNameIsNullOrEmpty()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Name = "";

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Name is required.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenCalculatorNameEnumIsDefault()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators[0].CalculatorName = default(CalculatorNameEnum);

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.CalculatorName is required.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenParametersIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameters is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenParameterValueIsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters[0].Value = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameter.Value is required.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenConditionIndicator1IsNull()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions[0].Indicator1 = null!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Condition.Indicator1 is required.", errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldFailWhenConditionOperatorIsInvalid(string? operatorValue)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions[0].Operator = operatorValue!;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Condition.Operator is required.", errors);
        }

        [Fact]
        public void Validate_ShouldFailWhenConditionHasNeitherIndicator2NorValue()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions[0].Indicator2 = null;
            rule.BuyRule.Conditions[0].Value = null;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Condition must have either Indicator2 or Value specified.", errors);
        }

        [Fact]
        public void Validate_ShouldCollectMultipleErrors()
        {
            // Arrange
            var rule = new TradeRule
            {
                Name = "", // Error 1
                CandleFrequency = "invalid", // Error 2
                MinProfit = -1.0, // Error 3
                StopLoss = -1.0, // Error 4
                TakeProfit = -1.0, // Error 5
                Bankroll = null!, // Error 6
                BuyRule = null!, // Error 7
                SellRule = null! // Error 8
            };

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.True(errors.Count >= 1); // Should collect multiple errors
        }

        [Theory]
        [InlineData("1m")]
        [InlineData("3m")]
        [InlineData("5m")]
        [InlineData("15m")]
        [InlineData("30m")]
        [InlineData("1h")]
        [InlineData("2h")]
        [InlineData("4h")]
        [InlineData("6h")]
        [InlineData("8h")]
        [InlineData("12h")]
        [InlineData("1d")]
        [InlineData("3d")]
        [InlineData("1w")]
        [InlineData("1M")]
        public void Validate_ShouldPassWithValidCandleFrequencies(string frequency)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.CandleFrequency = frequency;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldPassWithEdgeValueBankrollSettings()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Bankroll.MaxRiskPerTrade = 0.1;  // Edge case: 0 risk
            rule.Bankroll.MinEntryAmount = 0.01;  // Edge case: minimal entry

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldPassWithComplexConditions()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions.Add(new ConditionConfig
            {
                Indicator1 = new Indicator
                {
                    CalculatorName = "RSI",
                    TechnicalIndicatorName = TechnicalNamesEnum.RSI
                },
                Operator = "<",
                Value = "30"
            });

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldPassWithIndicatorToIndicatorComparison()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Conditions[0].Value = null;
            rule.BuyRule.Conditions[0].Indicator2 = new Indicator
            {
                CalculatorName = "SMA50",
                TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
            };

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ShouldPassWithEmptyParametersList()
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters = new List<CalculatorParameter>();

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.False(result);
            Assert.True(errors.Count > 0);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void Validate_ShouldPassWithValidMaxRiskPerTrade(double maxRisk)
        {
            // Arrange
            var rule = TradeRuleTestData.CreateValidTradeRule();
            rule.Bankroll.MaxRiskPerTrade = maxRisk;

            // Act
            var result = _validator.Validate(rule, out var errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        #endregion
    }
}
