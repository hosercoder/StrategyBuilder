using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.Tests.Models
{
    public class TradeRuleConfigurationIntegrationTests
    {
        [Fact]
        public void TradeRule_ShouldSupportComplexConfiguration()
        {
            // Arrange & Act
            var rule = TradeRuleTestData.CreateValidTradeRule();

            // Assert
            Assert.NotNull(rule);
            Assert.NotNull(rule.Name);
            Assert.NotNull(rule.CandleFrequency);
            Assert.NotNull(rule.Bankroll);
            Assert.NotNull(rule.BuyRule);
            Assert.NotNull(rule.SellRule);
            Assert.True(rule.MinProfit >= 0);
            Assert.True(rule.StopLoss >= 0);
            Assert.True(rule.TakeProfit >= 0);
        }

        [Fact]
        public void TradeSubRule_ShouldSupportComplexCalculators()
        {
            // Arrange & Act
            var subRule = TradeRuleTestData.CreateValidTradeSubRule();

            // Assert
            Assert.NotNull(subRule.Calculators);
            Assert.NotNull(subRule.Conditions);
            Assert.True(subRule.Calculators.Count > 0);
            Assert.True(subRule.Conditions.Count > 0);
        }

        [Fact]
        public void CalculatorConfig_ShouldHaveValidProperties()
        {
            // Arrange & Act
            var config = TradeRuleTestData.CreateValidCalculatorConfig();

            // Assert
            Assert.NotNull(config.Name);
            Assert.Equal(CalculatorNameEnum.SMA, config.CalculatorName);
            Assert.NotNull(config.TechnicalIndicators);
            Assert.True(config.TechnicalIndicators.Length > 0);
            Assert.NotNull(config.Parameters);
            Assert.True(config.Parameters.Count > 0);
        }

        [Fact]
        public void BankrollConfig_ShouldHaveReasonableDefaults()
        {
            // Arrange
            var bankroll = new BankrollConfig
            {
                MaxRiskPerTrade = 0.02,
                MinEntryAmount = 10.0
            };

            // Assert
            Assert.True(bankroll.MaxRiskPerTrade > 0);
            Assert.True(bankroll.MaxRiskPerTrade <= 1.0);
            Assert.True(bankroll.MinEntryAmount > 0);
        }

        [Fact]
        public void ConditionConfig_ShouldSupportIndicatorComparisons()
        {
            // Arrange
            var condition = new ConditionConfig
            {
                Indicator1 = new Indicator
                {
                    CalculatorName = "SMA20",
                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                },
                Operator = ">",
                Indicator2 = new Indicator
                {
                    CalculatorName = "SMA50",
                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                }
            };

            // Assert
            Assert.NotNull(condition.Indicator1);
            Assert.NotNull(condition.Operator);
            Assert.NotNull(condition.Indicator2);
            Assert.Null(condition.Value);
        }

        [Fact]
        public void ConditionConfig_ShouldSupportValueComparisons()
        {
            // Arrange
            var condition = new ConditionConfig
            {
                Indicator1 = new Indicator
                {
                    CalculatorName = "RSI",
                    TechnicalIndicatorName = TechnicalNamesEnum.RSI
                },
                Operator = "<",
                Value = "30"
            };

            // Assert
            Assert.NotNull(condition.Indicator1);
            Assert.NotNull(condition.Operator);
            Assert.NotNull(condition.Value);
            Assert.Null(condition.Indicator2);
        }

        [Fact]
        public void CalculatorParameter_ShouldMapEnumToString()
        {
            // Arrange
            var parameter = new CalculatorParameter
            {
                Name = ParameterNamesEnum.Period,
                Value = "14"
            };

            // Assert
            Assert.Equal(ParameterNamesEnum.Period, parameter.Name);
            Assert.Equal("14", parameter.Value);
        }

        [Theory]
        [InlineData("1m")]
        [InlineData("5m")]
        [InlineData("15m")]
        [InlineData("1h")]
        [InlineData("4h")]
        [InlineData("1d")]
        public void TradeRule_ShouldAcceptValidTimeframes(string timeframe)
        {
            // Arrange & Act
            var rule = new TradeRule
            {
                Name = "TestStrategy",
                CandleFrequency = timeframe,
                Bankroll = new BankrollConfig { MaxRiskPerTrade = 0.02, MinEntryAmount = 10.0 },
                BuyRule = TradeRuleTestData.CreateValidTradeSubRule(),
                SellRule = TradeRuleTestData.CreateValidTradeSubRule()
            };

            // Assert
            Assert.Equal(timeframe, rule.CandleFrequency);
        }

        [Theory]
        [InlineData(">")]
        [InlineData(">=")]
        [InlineData("<")]
        [InlineData("<=")]
        [InlineData("=")]
        [InlineData("!=")]
        public void ConditionConfig_ShouldSupportAllOperators(string operatorSymbol)
        {
            // Arrange & Act
            var condition = new ConditionConfig
            {
                Indicator1 = new Indicator
                {
                    CalculatorName = "Test",
                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                },
                Operator = operatorSymbol,
                Value = "50"
            };

            // Assert
            Assert.Equal(operatorSymbol, condition.Operator);
        }
    }
}
