using HC.StrategyBuilder.src.Models.Common;

namespace HC.StrategyBuilder.src.Interfaces
{
    public interface ITradeRuleConfiguration
    {
        TradeRule Rule { get; set; }
        void Initialize(string filePath);

        bool Validate(out List<string> errors);

        bool Evaluate(TradeSubRule rule, Dictionary<string, double> indicators, Candle data);
    }
}