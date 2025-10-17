using Newtonsoft.Json.Linq;

namespace HC.StrategyBuilder.src.Interfaces.Serializers
{
    public interface ITradeRuleSerializer
    {
        TradeRules LoadFromJson(string json);
        TradeRules LoadFromFile(string filePath);
        Task<TradeRules> LoadFromFileAsync(string filePath);
        TradeRules LoadFromJObject(JObject jsonObject);
        string ToJson(TradeRules config);
        void SaveToFile(TradeRules config, string filePath);
        Task SaveToFileAsync(TradeRules config, string filePath);
    }
}
