using HC.StrategyBuilder.src.Interfaces.Validators;
using HC.TechnicalCalculators.Src.Models;

namespace HC.StrategyBuilder.src.Validators
{
    public class TradeRuleValidator : ITradeRuleValidator
    {
        public bool Validate(TradeRule rule, out List<string> errors)
        {
            errors = new List<string>();
            if (rule == null)
            {
                errors.Add("Rule section is missing.");
                return false;
            }

            // Validate top-level Rule
            if (string.IsNullOrWhiteSpace(rule.Name))
                errors.Add("Rule.Name is required.");
            if (string.IsNullOrWhiteSpace(rule.CandleFrequency))
                errors.Add("Rule.CandleFrequency is required.");
            if (rule.Bankroll == null)
            {
                errors.Add("Bankroll configuration is required");
            }
            if (rule.MinProfit <= 0)
            {
                errors.Add("Rule.MinProfit must be greater than 0.");
            }
            if (rule.StopLoss <= 0)
            {
                errors.Add("Rule.MaxLoss must be greater than or equal to 0.");
            }
            if (rule.TakeProfit <= 0)
            {
                errors.Add("Rule.TakeProfit must be greater than 0.");
            }
            if (rule.Bankroll != null && rule.Bankroll!.MaxRiskPerTrade <= 0)
            {
                errors.Add("Rule.MaxTradesPerCandle must be greater than or equal to 0.");
            }
            if (rule.Bankroll != null && rule.Bankroll!.MinEntryAmount <= 0)
            {
                errors.Add("Rule.MinEntryAmount must be greater than 0.");
            }

            // Validate BuyRule and SellRule
            ValidateSubRule("BuyRule", rule.BuyRule!, errors);
            ValidateSubRule("SellRule", rule.SellRule!, errors);

            return errors.Count == 0;
        }
        private void ValidateSubRule(string ruleName, TradeSubRule subRule, List<string> errors)
        {
            if (subRule == null)
            {
                errors.Add($"{ruleName} is missing.");
                return;
            }

            if (subRule.Calculators == null || subRule.Calculators.Count == 0)
                errors.Add($"{ruleName}.Calculators is required and cannot be empty.");

            if (subRule.Conditions == null || subRule.Conditions.Count == 0)
                errors.Add($"{ruleName}.Conditions is required and cannot be empty.");

            // Validate each calculator
            if (subRule.Calculators != null)
            {
                foreach (var calc in subRule.Calculators)
                {
                    if (string.IsNullOrWhiteSpace(calc.Name))
                        errors.Add($"{ruleName}.Calculator.Name is required.");
                    if (calc.CalculatorName == default(CalculatorNameEnum))
                        errors.Add($"{ruleName}.Calculator.CalculatorName is required.");
                    if (calc.Parameters == null || calc.Parameters.Count == 0)
                        errors.Add($"{ruleName}.Calculator.Parameters is required and cannot be empty.");
                    else
                    {
                        foreach (var param in calc.Parameters)
                        {
                            if (string.IsNullOrWhiteSpace(param.Value))
                                errors.Add($"{ruleName}.Calculator.Parameter.Value is required.");
                        }
                    }
                }
            }

            // Validate each condition
            if (subRule.Conditions != null)
            {
                foreach (var condition in subRule.Conditions)
                {
                    if (condition.Indicator1 == null)
                        errors.Add($"{ruleName}.Condition.Indicator1 is required.");
                    if (string.IsNullOrWhiteSpace(condition.Operator))
                        errors.Add($"{ruleName}.Condition.Operator is required.");
                    if (condition.Indicator2 == null && string.IsNullOrWhiteSpace(condition.Value))
                        errors.Add($"{ruleName}.Condition must have either Indicator2 or Value specified.");
                }
            }
        }
    }
}
