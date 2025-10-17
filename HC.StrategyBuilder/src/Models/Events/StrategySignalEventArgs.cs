namespace HC.StrategyBuilder.src.Models.Events
{
    public class StrategySignalEventArgs : EventArgs
    {
        public string StrategyName { get; }
        public string ProductId { get; }
        public bool IsBuySignal { get; }
        public bool IsSellSignal { get; }
        public double Price { get; }
        public DateTime Timestamp { get; }

        public StrategySignalEventArgs(string strategyName, string productId,
            bool isBuySignal, bool isSellSignal, double price, DateTime timestamp)
        {
            StrategyName = strategyName;
            ProductId = productId;
            IsBuySignal = isBuySignal;
            IsSellSignal = isSellSignal;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
