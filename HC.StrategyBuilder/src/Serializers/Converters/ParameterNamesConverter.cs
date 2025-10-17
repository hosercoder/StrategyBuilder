using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.src.Serializers.Converters
{
    public class ParameterNamesConverter : JsonConverter<ParameterNamesEnum>
    {
        public override void WriteJson(JsonWriter writer, ParameterNamesEnum value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ParameterNamesEnum ReadJson(JsonReader reader, Type objectType, ParameterNamesEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                throw new JsonException("Cannot convert null to ParameterNamesEnum");
            }

            string? enumString = reader.Value.ToString();
            if (string.IsNullOrEmpty(enumString))
            {
                throw new JsonException("Cannot convert empty string to ParameterNamesEnum");
            }

            // Reject numeric strings to prevent parsing enum ordinal values
            if (int.TryParse(enumString, out _))
            {
                throw new JsonException($"Cannot convert '{enumString}' to ParameterNamesEnum");
            }

            // Try exact match first
            if (Enum.TryParse<ParameterNamesEnum>(enumString, out var result) && Enum.IsDefined(typeof(ParameterNamesEnum), result))
            {
                return result;
            }

            // Try case-insensitive match
            if (Enum.TryParse<ParameterNamesEnum>(enumString, true, out result) && Enum.IsDefined(typeof(ParameterNamesEnum), result))
            {
                return result;
            }

            throw new JsonException($"Cannot convert '{enumString}' to ParameterNamesEnum");
        }
    }
}