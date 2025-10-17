namespace HC.StrategyBuilder.src.Models
{
    public class StrategyEventArgs : EventArgs
    {
        public string Product { get; }
        public string StrategySignal { get; set; }
        public double Price { get; }
        public DateTime Timestamp { get; }
        public double TickTimestamp { get; }

        public StrategyEventArgs(double price, string strategySignal, DateTime timestamp, double tickTimestamp, string product)
        {
            Price = price;
            StrategySignal = strategySignal;
            Timestamp = timestamp;
            TickTimestamp = tickTimestamp;
            Product = product;
        }
    }
}

