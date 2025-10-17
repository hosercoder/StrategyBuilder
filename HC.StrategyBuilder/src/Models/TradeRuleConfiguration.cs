using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Interfaces.Evaluators;
using HC.StrategyBuilder.src.Interfaces.Serializers;
using HC.StrategyBuilder.src.Interfaces.Validators;
using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.src.Serializers.Converters;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

public class TradeRuleConfiguration(ITradeRuleEvaluator evaluator, ITradeRuleSerializer serializer, ITradeRuleValidator validator) : ITradeRuleConfiguration
{
    public required TradeRule Rule { get; set; }

    public void Initialize(string filePath)
    {
        Rule = serializer.LoadFromFile(filePath).Rule;
    }

    public bool Validate(out List<string> errors) => validator.Validate(Rule, out errors);

    public bool Evaluate(TradeSubRule rule, Dictionary<string, double> indicators, Candle data) =>
        evaluator.Evaluate(rule, indicators, data);



}
public class TradeRules
{
    public required TradeRule Rule { get; set; }
}
public class TradeRule
{
    public required string Name { get; set; }
    public required string CandleFrequency { get; set; }
    public double MinProfit { get; set; }
    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }
    public required BankrollConfig Bankroll { get; set; }
    public required TradeSubRule BuyRule { get; set; }
    public required TradeSubRule SellRule { get; set; }
}

public class TradeSubRule
{
    public required List<CalculatorConfig> Calculators { get; set; }
    public required List<ConditionConfig> Conditions { get; set; }
}

public class CalculatorConfig
{
    public required string Name { get; set; }

    [JsonConverter(typeof(CalculatorNameEnumConverter))]
    public CalculatorNameEnum CalculatorName { get; set; }

    public required TechnicalNamesEnum[] TechnicalIndicators { get; set; }
    public required List<CalculatorParameter> Parameters { get; set; }
}

public class CalculatorParameter
{
    [JsonConverter(typeof(ParameterNamesConverter))]
    public ParameterNamesEnum Name { get; set; }
    public required string Value { get; set; }
}

public class ConditionConfig
{
    public required Indicator Indicator1 { get; set; }
    public required string Operator { get; set; }
    public Indicator? Indicator2 { get; set; }
    public string? Value { get; set; }
}
public class Indicator
{
    public required string CalculatorName { get; set; }
    [JsonConverter(typeof(TechnicalNamesEnumConverter))]
    public TechnicalNamesEnum TechnicalIndicatorName { get; set; }
}
public class BankrollConfig
{
    public double MaxRiskPerTrade { get; set; }
    public double MinEntryAmount { get; set; }
}