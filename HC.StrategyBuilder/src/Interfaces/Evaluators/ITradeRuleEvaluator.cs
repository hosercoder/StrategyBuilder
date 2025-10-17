
using HC.StrategyBuilder.src.Models.Common;

namespace HC.StrategyBuilder.src.Interfaces.Evaluators
{
    public interface ITradeRuleEvaluator
    {
        public bool Evaluate(TradeSubRule rule, Dictionary<string, double> indicators, Candle data);
    }
}
