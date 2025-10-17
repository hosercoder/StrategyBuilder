namespace HC.StrategyBuilder.src.Interfaces.Validators
{
    public interface ITradeRuleValidator
    {
        bool Validate(TradeRule rule, out List<string> errors);
    }
}
