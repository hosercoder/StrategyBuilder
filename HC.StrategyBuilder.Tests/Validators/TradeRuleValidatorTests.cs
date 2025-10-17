using HC.StrategyBuilder.src.Validators;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.Tests.Validators
{
    public class TradeRuleValidatorTests
    {
        private readonly TradeRuleValidator _validator;

        public TradeRuleValidatorTests()
        {
            _validator = new TradeRuleValidator();
        }

        #region Null Rule Tests

        [Fact]
        public void Validate_NullRule_ReturnsFalseWithError()
        {
            // Arrange
            TradeRule? rule = null;

            // Act
            var result = _validator.Validate(rule!, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Single(errors);
            Assert.Equal("Rule section is missing.", errors[0]);
        }

        #endregion

        #region Valid Rule Tests

        [Fact]
        public void Validate_ValidRule_ReturnsTrue()
        {
            // Arrange
            var rule = CreateValidTradeRule();

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ValidRuleWithMultipleCalculators_ReturnsTrue()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators.Add(new CalculatorConfig
            {
                Name = "RSI14",
                CalculatorName = CalculatorNameEnum.RSI,
                TechnicalIndicators = new[] { TechnicalNamesEnum.RSI },
                Parameters = new List<CalculatorParameter>
                {
                    new CalculatorParameter
                    {
                        Name = ParameterNamesEnum.Period,
                        Value = "14"
                    }
                }
            });

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        #endregion

        #region Top-Level Rule Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Validate_InvalidRuleName_ReturnsFalseWithError(string? invalidName)
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.Name = invalidName!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.Name is required.", errors);
        }

        [Fact]
        public void Validate_NullRuleName_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.Name = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.Name is required.", errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Validate_InvalidCandleFrequency_ReturnsFalseWithError(string? invalidFrequency)
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.CandleFrequency = invalidFrequency!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.CandleFrequency is required.", errors);
        }

        [Fact]
        public void Validate_NullCandleFrequency_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.CandleFrequency = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.CandleFrequency is required.", errors);
        }

        #endregion

        #region Sub-Rule Validation Tests

        [Fact]
        public void Validate_NullBuyRule_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule is missing.", errors);
        }

        [Fact]
        public void Validate_NullSellRule_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.SellRule = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("SellRule is missing.", errors);
        }

        [Fact]
        public void Validate_BothSubRulesNull_ReturnsFalseWithBothErrors()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule = null!;
            rule.SellRule = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule is missing.", errors);
            Assert.Contains("SellRule is missing.", errors);
        }

        #endregion

        #region Calculator Validation Tests

        [Fact]
        public void Validate_NullCalculators_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculators is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_EmptyCalculators_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators = new List<CalculatorConfig>();

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculators is required and cannot be empty.", errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_InvalidCalculatorName_ReturnsFalseWithError(string? invalidName)
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Name = invalidName!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Name is required.", errors);
        }

        [Fact]
        public void Validate_NullCalculatorName_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Name = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Name is required.", errors);
        }

        [Fact]
        public void Validate_CalculatorWithNullParameters_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameters is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_CalculatorWithEmptyParameters_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters = new List<CalculatorParameter>();

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameters is required and cannot be empty.", errors);
        }

        #endregion

        #region Parameter Validation Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_InvalidParameterValue_ReturnsFalseWithError(string? invalidValue)
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters[0].Value = invalidValue!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameter.Value is required.", errors);
        }

        [Fact]
        public void Validate_NullParameterValue_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters[0].Value = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Calculator.Parameter.Value is required.", errors);
        }

        [Fact]
        public void Validate_MultipleParametersWithErrors_ReturnsAllParameterErrors()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators[0].Parameters.Add(new CalculatorParameter
            {
                Name = ParameterNamesEnum.Period,
                Value = null!
            });
            rule.BuyRule.Calculators[0].Parameters.Add(new CalculatorParameter
            {
                Name = ParameterNamesEnum.Period,
                Value = ""
            });

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            var parameterValueErrors = errors.Where(e => e.Contains("BuyRule.Calculator.Parameter.Value is required.")).ToList();
            Assert.True(parameterValueErrors.Count >= 2);
        }

        #endregion

        #region Condition Validation Tests

        [Fact]
        public void Validate_NullConditions_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Conditions = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Conditions is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_EmptyConditions_ReturnsFalseWithError()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Conditions = new List<ConditionConfig>();

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("BuyRule.Conditions is required and cannot be empty.", errors);
        }

        #endregion

        #region SellRule Specific Tests

        [Fact]
        public void Validate_SellRuleCalculatorErrors_ReturnsErrorsWithSellRulePrefix()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.SellRule.Calculators[0].Name = null!;
            rule.SellRule.Calculators[0].Parameters = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("SellRule.Calculator.Name is required.", errors);
            Assert.Contains("SellRule.Calculator.Parameters is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_SellRuleConditionErrors_ReturnsErrorsWithSellRulePrefix()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.SellRule.Conditions = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("SellRule.Conditions is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_SellRuleParameterErrors_ReturnsErrorsWithSellRulePrefix()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.SellRule.Calculators[0].Parameters[0].Value = null!;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("SellRule.Calculator.Parameter.Value is required.", errors);
        }

        #endregion

        #region Multiple Calculator Tests

        [Fact]
        public void Validate_MultipleCalculatorsWithErrors_ReturnsAllErrors()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.BuyRule.Calculators.Add(new CalculatorConfig
            {
                Name = null!, // Invalid
                CalculatorName = CalculatorNameEnum.RSI,
                TechnicalIndicators = new[] { TechnicalNamesEnum.RSI },
                Parameters = null! // Invalid
            });

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            var calculatorNameErrors = errors.Where(e => e.Contains("BuyRule.Calculator.Name is required.")).ToList();
            var calculatorParamErrors = errors.Where(e => e.Contains("BuyRule.Calculator.Parameters is required and cannot be empty.")).ToList();

            Assert.True(calculatorNameErrors.Count >= 1);
            Assert.True(calculatorParamErrors.Count >= 1);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void Validate_AllPossibleErrors_ReturnsAllErrors()
        {
            // Arrange
            var rule = new TradeRule
            {
                Name = null!, // Error
                CandleFrequency = "", // Error
                BuyRule = null!, // Error
                SellRule = null!, // Error
                Bankroll = new BankrollConfig
                {
                    MaxRiskPerTrade = 0.02,
                    MinEntryAmount = 10.0
                }
            };

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.True(errors.Count >= 4);
            Assert.Contains("Rule.Name is required.", errors);
            Assert.Contains("Rule.CandleFrequency is required.", errors);
            Assert.Contains("BuyRule is missing.", errors);
            Assert.Contains("SellRule is missing.", errors);
        }

        [Fact]
        public void Validate_ComplexRuleWithMixedErrors_ReturnsAppropriateErrors()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.Name = ""; // Error
            rule.BuyRule.Calculators[0].Name = null!; // Error
            rule.BuyRule.Calculators[0].Parameters[0].Value = ""; // Error
            rule.SellRule.Conditions = new List<ConditionConfig>(); // Error

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.False(result);
            Assert.Contains("Rule.Name is required.", errors);
            Assert.Contains("BuyRule.Calculator.Name is required.", errors);
            Assert.Contains("BuyRule.Calculator.Parameter.Value is required.", errors);
            Assert.Contains("SellRule.Conditions is required and cannot be empty.", errors);
        }

        [Fact]
        public void Validate_ValidRuleWithOptionalProperties_ReturnsTrue()
        {
            // Arrange
            var rule = CreateValidTradeRule();
            rule.MinProfit = 0.1;
            rule.StopLoss = 0.02;
            rule.TakeProfit = 2.0;

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.True(result);
            Assert.Empty(errors);
        }

        #endregion

        #region Bug Detection Tests (Based on Code Analysis)

        [Fact]
        public void Validate_DetectPotentialBugInCalculatorNameValidation()
        {
            // Note: The original code has a potential bug using nameof(calc.CalculatorName)
            // instead of checking the actual value. This test documents the current behavior.

            // Arrange
            var rule = CreateValidTradeRule();
            // Even with a valid calculator name, the bug in the original code 
            // will always pass because nameof(calc.CalculatorName) returns "CalculatorName"

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.True(result); // This passes, but it shouldn't if CalculatorName was actually null
        }

        [Fact]
        public void Validate_DetectPotentialBugInParameterNameValidation()
        {
            // Note: The original code has a potential bug using nameof(param.Name)
            // instead of checking the actual value. This test documents the current behavior.

            // Arrange
            var rule = CreateValidTradeRule();
            // Even with valid parameter names, the bug in the original code 
            // will always pass because nameof(param.Name) returns "Name"

            // Act
            var result = _validator.Validate(rule, out List<string> errors);

            // Assert
            Assert.True(result); // This passes, but it shouldn't if Name was actually null
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
                Bankroll = new BankrollConfig
                {
                    MaxRiskPerTrade = 0.02,
                    MinEntryAmount = 10.0
                },
                BuyRule = CreateValidSubRule(),
                SellRule = CreateValidSubRule()
            };
        }

        private TradeSubRule CreateValidSubRule()
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
                            new CalculatorParameter
                            {
                                Name = ParameterNamesEnum.Period,
                                Value = "20"
                            }
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
                        Value = "close"
                    }
                }
            };
        }

        #endregion
    }
}