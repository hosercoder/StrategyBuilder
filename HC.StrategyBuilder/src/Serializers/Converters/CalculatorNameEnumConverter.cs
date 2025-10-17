using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.src.Serializers.Converters
{
    public class CalculatorNameEnumConverter : JsonConverter<CalculatorNameEnum>
    {
        public override void WriteJson(JsonWriter writer, CalculatorNameEnum value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override CalculatorNameEnum ReadJson(JsonReader reader, Type objectType, CalculatorNameEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                throw new JsonException("Cannot convert null to CalculatorNameEnum");
            }

            string? enumString = reader.Value.ToString();
            if (string.IsNullOrEmpty(enumString))
            {
                throw new JsonException("Cannot convert empty string to CalculatorNameEnum");
            }

            // Reject numeric strings to prevent parsing enum ordinal values
            if (int.TryParse(enumString, out _))
            {
                throw new JsonException($"Cannot convert '{enumString}' to CalculatorNameEnum");
            }

            // Try exact match first
            if (Enum.TryParse<CalculatorNameEnum>(enumString, out var result) && Enum.IsDefined(typeof(CalculatorNameEnum), result))
            {
                return result;
            }

            // Try case-insensitive match
            if (Enum.TryParse<CalculatorNameEnum>(enumString, true, out result) && Enum.IsDefined(typeof(CalculatorNameEnum), result))
            {
                return result;
            }

            throw new JsonException($"Cannot convert '{enumString}' to CalculatorNameEnum");
        }
    }
}
