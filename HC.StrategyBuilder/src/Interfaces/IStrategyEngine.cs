using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.src.Models.Events;

namespace HC.StrategyBuilder.src.Interfaces
{
    public interface IStrategyEngine
    {
        Task Initialize(string configurationPath);
        Task<bool> EvaluateStrategy(string strategyName, Candle candle, Dictionary<string, double> indicators);
        Task<Dictionary<string, double>> CalculateIndicators(Candle[] candles);
        event EventHandler<StrategySignalEventArgs> OnStrategySignal;
    }
}
