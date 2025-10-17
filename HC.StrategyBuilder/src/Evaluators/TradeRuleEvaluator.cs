using HC.StrategyBuilder.src.Interfaces.Evaluators;
using HC.StrategyBuilder.src.Models.Common;

namespace HC.StrategyBuilder.src.Evaluators
{
    public class TradeRuleEvaluator : ITradeRuleEvaluator
    {
        public bool Evaluate(TradeSubRule rule, Dictionary<string, double> indicators, Candle data)
        {
            if (rule == null || rule.Conditions == null || rule.Conditions.Count == 0)
                return false;

            foreach (var condition in rule.Conditions)
            {
                double indicatorValue1;
                double indicatorValue2;

                // Get the first value (Indicator1)
                if (!TryGetValue(condition.Indicator1, rule.Calculators, indicators, data, out indicatorValue1))
                {
                    throw new ArgumentException($"Indicator1 '{condition.Indicator1?.CalculatorName}' with technical indicator '{condition.Indicator1?.TechnicalIndicatorName}' not found.");
                }

                // Get the second value (either Indicator2 or Value)
                if (condition.Indicator2 != null)
                {
                    // Compare with another indicator
                    if (!TryGetValue(condition.Indicator2, rule.Calculators, indicators, data, out indicatorValue2))
                    {
                        throw new ArgumentException($"Indicator2 '{condition.Indicator2?.CalculatorName}' with technical indicator '{condition.Indicator2?.TechnicalIndicatorName}' not found.");
                    }
                }
                else if (!string.IsNullOrEmpty(condition.Value))
                {
                    // Compare with a value
                    if (!TryParseValue(condition.Value, rule.Calculators, indicators, data, out indicatorValue2))
                    {
                        throw new ArgumentException($"Value '{condition.Value}' not found or could not be parsed.");
                    }
                }
                else
                {
                    throw new ArgumentException("Condition must have either Indicator2 or Value specified.");
                }

                // Evaluate the condition
                bool result = condition.Operator switch
                {
                    ">" => indicatorValue1 > indicatorValue2,
                    "<" => indicatorValue1 < indicatorValue2,
                    "=" => Math.Abs(indicatorValue1 - indicatorValue2) < 0.000001,
                    ">=" => indicatorValue1 >= indicatorValue2,
                    "<=" => indicatorValue1 <= indicatorValue2,
                    _ => throw new ArgumentException($"Invalid operator '{condition.Operator}'.")
                };

                if (!result)
                    return false; // All conditions must be true
            }

            return true;
        }

        private bool TryGetValue(Indicator? indicator, List<CalculatorConfig>? calculators, Dictionary<string, double> indicators, Candle data, out double value)
        {
            value = 0;

            if (indicator == null)
                return false;

            // First try to get by technical indicator name from indicators dictionary
            string technicalIndicatorKey = indicator.TechnicalIndicatorName.ToString();
            if (indicators.TryGetValue(technicalIndicatorKey, out value))
            {
                return true;
            }

            // Try to get by calculator name from indicators dictionary
            string calculatorKey = indicator.CalculatorName.ToString();
            if (indicators.TryGetValue(calculatorKey, out value))
            {
                return true;
            }

            // Try to find calculator by name and get its value from indicators
            if (FindCalculatorValue(calculators, calculatorKey, indicators, out value))
            {
                return true;
            }

            // Try to get candle value if calculator name matches candle properties
            if (TryGetCandleValue(calculatorKey, data, out value))
            {
                return true;
            }

            return false;
        }

        private bool TryParseValue(string? valueString, List<CalculatorConfig>? calculators, Dictionary<string, double> indicators, Candle data, out double value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(valueString))
                return false;

            // Try to parse as double
            if (double.TryParse(valueString, out value))
            {
                return true;
            }

            // Try to get from indicators dictionary
            if (indicators.TryGetValue(valueString, out value))
            {
                return true;
            }

            // Try to get candle value
            if (TryGetCandleValue(valueString, data, out value))
            {
                return true;
            }

            // Try to find calculator value
            if (FindCalculatorValue(calculators, valueString, indicators, out value))
            {
                return true;
            }

            return false;
        }

        private bool FindCalculatorValue(List<CalculatorConfig>? calculators, string name, Dictionary<string, double> indicators, out double value)
        {
            value = 0;
            if (calculators == null || string.IsNullOrWhiteSpace(name))
                return false;

            var calculator = calculators.FirstOrDefault(c => c.Name == name);
            if (calculator == null)
                return false;

            return indicators.TryGetValue(calculator.Name, out value);
        }

        private bool TryGetCandleValue(string name, Candle candle, out double value)
        {
            value = 0;
            if (candle == null || string.IsNullOrWhiteSpace(name))
                return false;

            switch (name.ToLowerInvariant())
            {
                case "close":
                    value = candle.Close;
                    return true;
                case "open":
                    value = candle.Open;
                    return true;
                case "high":
                    value = candle.High;
                    return true;
                case "low":
                    value = candle.Low;
                    return true;
                case "volume":
                    value = candle.Volume;
                    return true;
                default:
                    return false;
            }
        }
    }
}