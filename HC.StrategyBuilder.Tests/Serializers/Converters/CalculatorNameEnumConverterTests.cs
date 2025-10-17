using HC.StrategyBuilder.src.Serializers.Converters;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.Tests.Serializers.Converters
{
    public class CalculatorNameEnumConverterTests
    {
        private readonly CalculatorNameEnumConverter _converter;
        private readonly JsonSerializer _serializer;

        public CalculatorNameEnumConverterTests()
        {
            _converter = new CalculatorNameEnumConverter();
            _serializer = new JsonSerializer();
        }

        #region WriteJson Tests

        [Theory]
        [InlineData(CalculatorNameEnum.SMA, "SMA")]
        [InlineData(CalculatorNameEnum.RSI, "RSI")]
        [InlineData(CalculatorNameEnum.MACD, "MACD")]
        [InlineData(CalculatorNameEnum.EMA, "EMA")]
        [InlineData(CalculatorNameEnum.BBANDS, "BBANDS")]
        public void WriteJson_ShouldSerializeEnumToString(CalculatorNameEnum enumValue, string expectedString)
        {
            // Arrange
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);

            // Act
            _converter.WriteJson(jsonWriter, enumValue, _serializer);

            // Assert
            string result = stringWriter.ToString();
            Assert.Equal($"\"{expectedString}\"", result);
        }

        [Fact]
        public void WriteJson_ShouldHandleAllEnumValues()
        {
            // Arrange
            var allEnumValues = Enum.GetValues<CalculatorNameEnum>();

            foreach (var enumValue in allEnumValues)
            {
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);

                // Act
                _converter.WriteJson(jsonWriter, enumValue, _serializer);

                // Assert
                string result = stringWriter.ToString();
                Assert.NotEmpty(result);
                Assert.StartsWith("\"", result);
                Assert.EndsWith("\"", result);
                Assert.Equal($"\"{enumValue}\"", result);
            }
        }

        #endregion

        #region ReadJson Tests

        [Theory]
        [InlineData("SMA", CalculatorNameEnum.SMA)]
        [InlineData("RSI", CalculatorNameEnum.RSI)]
        [InlineData("MACD", CalculatorNameEnum.MACD)]
        [InlineData("EMA", CalculatorNameEnum.EMA)]
        [InlineData("BBANDS", CalculatorNameEnum.BBANDS)]
        public void ReadJson_ShouldDeserializeStringToEnum_ExactMatch(string jsonValue, CalculatorNameEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer);

            // Assert
            Assert.Equal(expectedEnum, result);
        }

        [Theory]
        [InlineData("sma", CalculatorNameEnum.SMA)]
        [InlineData("rsi", CalculatorNameEnum.RSI)]
        [InlineData("macd", CalculatorNameEnum.MACD)]
        [InlineData("ema", CalculatorNameEnum.EMA)]
        [InlineData("bbands", CalculatorNameEnum.BBANDS)]
        [InlineData("Sma", CalculatorNameEnum.SMA)]
        [InlineData("Rsi", CalculatorNameEnum.RSI)]
        [InlineData("SmA", CalculatorNameEnum.SMA)]
        public void ReadJson_ShouldDeserializeStringToEnum_CaseInsensitive(string jsonValue, CalculatorNameEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer);

            // Assert
            Assert.Equal(expectedEnum, result);
        }

        [Fact]
        public void ReadJson_ShouldThrowJsonException_WhenValueIsNull()
        {
            // Arrange
            using var stringReader = new StringReader("null");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the null token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer));

            Assert.Equal("Cannot convert null to CalculatorNameEnum", exception.Message);
        }

        [Theory]
        [InlineData("INVALID_CALCULATOR")]
        [InlineData("NOT_A_CALCULATOR")]
        [InlineData("UNKNOWN")]
        [InlineData("123")]
        [InlineData("!@#")]
        public void ReadJson_ShouldThrowJsonException_WhenValueIsInvalid(string invalidValue)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{invalidValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer));

            Assert.Equal($"Cannot convert '{invalidValue}' to CalculatorNameEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleEmptyString()
        {
            // Arrange
            using var stringReader = new StringReader("\"\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer));

            Assert.Equal("Cannot convert empty string to CalculatorNameEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleWhitespaceString()
        {
            // Arrange
            using var stringReader = new StringReader("\"   \"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '   ' to CalculatorNameEnum", exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Converter_ShouldWorkWithJsonConvertSerializeObject()
        {
            // Arrange
            var testObject = new TestCalculatorConfig
            {
                Name = "TestCalculator",
                CalculatorName = CalculatorNameEnum.SMA
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CalculatorNameEnumConverter());

            // Act
            string json = JsonConvert.SerializeObject(testObject, settings);
            var deserializedObject = JsonConvert.DeserializeObject<TestCalculatorConfig>(json, settings);

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.Equal(testObject.Name, deserializedObject.Name);
            Assert.Equal(testObject.CalculatorName, deserializedObject.CalculatorName);
            Assert.Contains("\"CalculatorName\":\"SMA\"", json);
        }

        [Fact]
        public void Converter_ShouldWorkWithCaseInsensitiveJson()
        {
            // Arrange
            string json = @"{
                ""Name"": ""TestCalculator"",
                ""CalculatorName"": ""sma""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CalculatorNameEnumConverter());

            // Act
            var result = JsonConvert.DeserializeObject<TestCalculatorConfig>(json, settings);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestCalculator", result.Name);
            Assert.Equal(CalculatorNameEnum.SMA, result.CalculatorName);
        }

        [Fact]
        public void Converter_ShouldThrowOnInvalidJsonEnum()
        {
            // Arrange
            string json = @"{
                ""Name"": ""TestCalculator"",
                ""CalculatorName"": ""INVALID_CALCULATOR""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CalculatorNameEnumConverter());

            // Act & Assert
            var exception = Assert.ThrowsAny<JsonException>(() =>
                JsonConvert.DeserializeObject<TestCalculatorConfig>(json, settings));

            Assert.Contains("Cannot convert 'INVALID_CALCULATOR' to CalculatorNameEnum", exception.Message);
        }

        [Fact]
        public void Converter_ShouldRoundTripAllEnumValues()
        {
            // Arrange
            var allEnumValues = Enum.GetValues<CalculatorNameEnum>();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CalculatorNameEnumConverter());

            foreach (var enumValue in allEnumValues)
            {
                var testObject = new TestCalculatorConfig
                {
                    Name = $"Test{enumValue}",
                    CalculatorName = enumValue
                };

                // Act
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestCalculatorConfig>(json, settings);

                // Assert
                Assert.Equal(testObject.CalculatorName, deserializedObject.CalculatorName);
                Assert.Contains($"\"{enumValue}\"", json);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ReadJson_ShouldHandleNumericStringsThatMatchEnumValues()
        {
            // Arrange - Test that numeric strings that might match enum ordinal values are rejected
            using var stringReader = new StringReader("\"0\""); // Assuming 0 might be SMA's ordinal
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '0' to CalculatorNameEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleExistingValueParameter()
        {
            // Arrange
            using var stringReader = new StringReader("\"RSI\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(CalculatorNameEnum), CalculatorNameEnum.SMA, true, _serializer);

            // Assert
            Assert.Equal(CalculatorNameEnum.RSI, result); // Should return the new value, not the existing one
        }

        [Fact]
        public void WriteJson_ShouldHandleDefaultEnumValue()
        {
            // Arrange
            var defaultValue = default(CalculatorNameEnum);
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);

            // Act
            _converter.WriteJson(jsonWriter, defaultValue, _serializer);

            // Assert
            string result = stringWriter.ToString();
            Assert.NotEmpty(result);
            Assert.StartsWith("\"", result);
            Assert.EndsWith("\"", result);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Converter_ShouldPerformWellWithMultipleConversions()
        {
            // Arrange
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CalculatorNameEnumConverter());
            var testObjects = new List<TestCalculatorConfig>();

            for (int i = 0; i < 1000; i++)
            {
                testObjects.Add(new TestCalculatorConfig
                {
                    Name = $"Calculator{i}",
                    CalculatorName = (CalculatorNameEnum)(i % Enum.GetValues<CalculatorNameEnum>().Length)
                });
            }

            // Act & Assert - Should complete without timeout
            var startTime = DateTime.UtcNow;

            foreach (var testObject in testObjects)
            {
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestCalculatorConfig>(json, settings);
                Assert.Equal(testObject.CalculatorName, deserializedObject.CalculatorName);
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Should complete within reasonable time (adjust threshold as needed)
            Assert.True(duration.TotalSeconds < 5, $"Performance test took too long: {duration.TotalSeconds} seconds");
        }

        #endregion

        #region Helper Classes

        private class TestCalculatorConfig
        {
            public string Name { get; set; }

            [JsonConverter(typeof(CalculatorNameEnumConverter))]
            public CalculatorNameEnum CalculatorName { get; set; }
        }

        #endregion
    }
}
