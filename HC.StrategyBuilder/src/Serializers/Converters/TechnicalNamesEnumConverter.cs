using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.src.Serializers.Converters
{
    public class TechnicalNamesEnumConverter : JsonConverter<TechnicalNamesEnum>
    {
        public override void WriteJson(JsonWriter writer, TechnicalNamesEnum value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override TechnicalNamesEnum ReadJson(JsonReader reader, Type objectType, TechnicalNamesEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                throw new JsonException("Cannot convert null to TechnicalNamesEnum");
            }

            string? enumString = reader.Value.ToString();
            if (string.IsNullOrEmpty(enumString))
            {
                throw new JsonException("Cannot convert empty string to TechnicalNamesEnum");
            }

            // Reject numeric strings to prevent parsing enum ordinal values
            if (int.TryParse(enumString, out _))
            {
                throw new JsonException($"Cannot convert '{enumString}' to TechnicalNamesEnum");
            }

            // Try exact match first
            if (Enum.TryParse<TechnicalNamesEnum>(enumString, out var result) && Enum.IsDefined(typeof(TechnicalNamesEnum), result))
            {
                return result;
            }

            // Try case-insensitive match
            if (Enum.TryParse<TechnicalNamesEnum>(enumString, true, out result) && Enum.IsDefined(typeof(TechnicalNamesEnum), result))
            {
                return result;
            }

            throw new JsonException($"Cannot convert '{enumString}' to TechnicalNamesEnum");
        }
    }
}