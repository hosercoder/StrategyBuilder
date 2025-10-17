using HC.StrategyBuilder.src.Interfaces.Serializers;
using HC.StrategyBuilder.src.Serializers.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HC.StrategyBuilder.src.Serializers
{
    public class TradeRuleSerializer : ITradeRuleSerializer
    {
        public TradeRules LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The specified file is empty.");
            }

            return LoadFromJson(json);
        }

        public async Task<TradeRules> LoadFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            string json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The specified file is empty.");
            }

            return LoadFromJson(json);
        }

        public TradeRules LoadFromJObject(JObject jsonObject)
        {
            if (jsonObject == null)
            {
                throw new ArgumentNullException(nameof(jsonObject), "JSON object cannot be null.");
            }

            try
            {
                var config = jsonObject.ToObject<TradeRules>(JsonSerializer.Create(GetSerializerSettings()));
                if (config == null || config.Rule == null)
                {
                    throw new InvalidOperationException("Failed to convert JObject to TradeRuleConfiguration.");
                }

                return config;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error converting JObject to TradeRuleConfiguration: {ex.Message}", ex);
            }
        }

        public TradeRules LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));
            }

            try
            {
                var config = JsonConvert.DeserializeObject<TradeRules>(json, GetSerializerSettings());
                if (config == null)
                {
                    throw new InvalidOperationException("Failed to deserialize TradeRuleConfiguration from JSON.");
                }

                return config;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error parsing JSON: {ex.Message}", ex);
            }
        }

        public string ToJson(TradeRules config)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters =
        {
            new CalculatorNameEnumConverter(),
            new TechnicalNamesEnumConverter(),
            new ParameterNamesConverter()
        }
            };

            return JsonConvert.SerializeObject(config, settings);
        }

        public void SaveToFile(TradeRules config, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            string json = ToJson(config);
            File.WriteAllText(filePath, json);
        }

        public async Task SaveToFileAsync(TradeRules config, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            string json = ToJson(config);
            await File.WriteAllTextAsync(filePath, json);
        }

        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Converters =
                {
                    new CalculatorNameEnumConverter(),
                    new TechnicalNamesEnumConverter(),
                    new ParameterNamesConverter()
                }
            };
        }
    }
}
