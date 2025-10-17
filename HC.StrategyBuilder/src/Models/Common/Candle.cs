namespace HC.StrategyBuilder.src.Models.Common
{
    public class Candle
    {
        public long Start { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public string ProductId { get; set; }
    }
}
