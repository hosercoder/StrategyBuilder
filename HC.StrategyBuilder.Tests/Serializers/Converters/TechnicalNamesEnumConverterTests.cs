using HC.StrategyBuilder.src.Serializers.Converters;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.Tests.Serializers.Converters
{
    public class TechnicalNamesEnumConverterTests
    {
        private readonly TechnicalNamesEnumConverter _converter;
        private readonly JsonSerializer _serializer;

        public TechnicalNamesEnumConverterTests()
        {
            _converter = new TechnicalNamesEnumConverter();
            _serializer = new JsonSerializer();
        }

        #region WriteJson Tests

        [Theory]
        [InlineData(TechnicalNamesEnum.MOVINGAVERAGE, "MOVINGAVERAGE")]
        [InlineData(TechnicalNamesEnum.RSI, "RSI")]
        public void WriteJson_ShouldSerializeEnumToString(TechnicalNamesEnum enumValue, string expectedString)
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
            var allEnumValues = Enum.GetValues<TechnicalNamesEnum>();

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
        [InlineData("MOVINGAVERAGE", TechnicalNamesEnum.MOVINGAVERAGE)]
        [InlineData("RSI", TechnicalNamesEnum.RSI)]
        public void ReadJson_ShouldDeserializeStringToEnum_ExactMatch(string jsonValue, TechnicalNamesEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer);

            // Assert
            Assert.Equal(expectedEnum, result);
        }

        [Theory]
        [InlineData("movingaverage", TechnicalNamesEnum.MOVINGAVERAGE)]
        [InlineData("rsi", TechnicalNamesEnum.RSI)]
        [InlineData("MOVINGAVERAGE", TechnicalNamesEnum.MOVINGAVERAGE)]
        [InlineData("Rsi", TechnicalNamesEnum.RSI)]
        [InlineData("MovingAverage", TechnicalNamesEnum.MOVINGAVERAGE)]
        public void ReadJson_ShouldDeserializeStringToEnum_CaseInsensitive(string jsonValue, TechnicalNamesEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer);

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
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert null to TechnicalNamesEnum", exception.Message);
        }

        [Theory]
        [InlineData("INVALID_TECHNICAL")]
        [InlineData("NOT_A_TECHNICAL")]
        [InlineData("UNKNOWN")]
        [InlineData("123")]
        [InlineData("!@#")]
        [InlineData("BOLLINGER")]
        [InlineData("STOCHASTIC")]
        [InlineData("MACD_HISTOGRAM")]
        public void ReadJson_ShouldThrowJsonException_WhenValueIsInvalid(string invalidValue)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{invalidValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal($"Cannot convert '{invalidValue}' to TechnicalNamesEnum", exception.Message);
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
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert empty string to TechnicalNamesEnum", exception.Message);
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
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '   ' to TechnicalNamesEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleNumericStringsThatMatchEnumValues()
        {
            // Arrange - Test that numeric strings that might match enum ordinal values are rejected
            using var stringReader = new StringReader("\"0\""); // Assuming 0 might be MOVINGAVERAGE's ordinal
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '0' to TechnicalNamesEnum", exception.Message);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("10")]
        [InlineData("-1")]
        [InlineData("999")]
        public void ReadJson_ShouldRejectNumericStrings(string numericValue)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{numericValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer));

            Assert.Equal($"Cannot convert '{numericValue}' to TechnicalNamesEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleExistingValueParameter()
        {
            // Arrange
            using var stringReader = new StringReader("\"RSI\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), TechnicalNamesEnum.MOVINGAVERAGE, true, _serializer);

            // Assert
            Assert.Equal(TechnicalNamesEnum.RSI, result); // Should return the new value, not necessarily the existing one
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Converter_ShouldWorkWithJsonConvertSerializeObject()
        {
            // Arrange
            var testObject = new TestTechnicalConfig
            {
                Description = "TestTechnical",
                TechnicalName = TechnicalNamesEnum.MOVINGAVERAGE
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TechnicalNamesEnumConverter());

            // Act
            string json = JsonConvert.SerializeObject(testObject, settings);
            var deserializedObject = JsonConvert.DeserializeObject<TestTechnicalConfig>(json, settings);

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.Equal(testObject.Description, deserializedObject.Description);
            Assert.Equal(testObject.TechnicalName, deserializedObject.TechnicalName);
            Assert.Contains("\"TechnicalName\":\"MOVINGAVERAGE\"", json);
        }

        [Fact]
        public void Converter_ShouldWorkWithCaseInsensitiveJson()
        {
            // Arrange
            string json = @"{
                ""Description"": ""TestTechnical"",
                ""TechnicalName"": ""rsi""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TechnicalNamesEnumConverter());

            // Act
            var result = JsonConvert.DeserializeObject<TestTechnicalConfig>(json, settings);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestTechnical", result.Description);
            Assert.Equal(TechnicalNamesEnum.RSI, result.TechnicalName);
        }

        [Fact]
        public void Converter_ShouldThrowOnInvalidJsonEnum()
        {
            // Arrange
            string json = @"{
                ""Description"": ""TestTechnical"",
                ""TechnicalName"": ""INVALID_TECHNICAL""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TechnicalNamesEnumConverter());

            // Act & Assert
            var exception = Assert.ThrowsAny<JsonException>(() =>
                JsonConvert.DeserializeObject<TestTechnicalConfig>(json, settings));

            Assert.Contains("Cannot convert 'INVALID_TECHNICAL' to TechnicalNamesEnum", exception.Message);
        }

        [Fact]
        public void Converter_ShouldRoundTripAllEnumValues()
        {
            // Arrange
            var allEnumValues = Enum.GetValues<TechnicalNamesEnum>();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TechnicalNamesEnumConverter());

            foreach (var enumValue in allEnumValues)
            {
                var testObject = new TestTechnicalConfig
                {
                    Description = $"Test{enumValue}",
                    TechnicalName = enumValue
                };

                // Act
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestTechnicalConfig>(json, settings);

                // Assert
                Assert.NotNull(deserializedObject);
                Assert.Equal(testObject.TechnicalName, deserializedObject.TechnicalName);
                Assert.Contains($"\"{enumValue}\"", json);
            }
        }

        [Fact]
        public void Converter_ShouldWorkWithTechnicalIndicatorsArray()
        {
            // Arrange
            var testObject = new TestCalculatorConfigWithTechnicals
            {
                Name = "TestCalculator",
                TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE, TechnicalNamesEnum.RSI }
            };

            var settings = new JsonSerializerSettings
            {
                Converters =
                {
                    new CalculatorNameEnumConverter(),
                    new TechnicalNamesEnumConverter(),
                    new ParameterNamesConverter()
                }
            };

            // Act
            string json = JsonConvert.SerializeObject(testObject, settings);
            var deserializedObject = JsonConvert.DeserializeObject<TestCalculatorConfigWithTechnicals>(json, settings);

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.Equal(testObject.Name, deserializedObject.Name);
            Assert.Equal(2, deserializedObject.TechnicalIndicators.Length);
            Assert.Contains(TechnicalNamesEnum.MOVINGAVERAGE, deserializedObject.TechnicalIndicators);
            Assert.Contains(TechnicalNamesEnum.RSI, deserializedObject.TechnicalIndicators);
            Assert.Contains("\"MOVINGAVERAGE\"", json);
            Assert.Contains("\"RSI\"", json);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void WriteJson_ShouldHandleDefaultEnumValue()
        {
            // Arrange
            var defaultValue = default(TechnicalNamesEnum);
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

        [Fact]
        public void ReadJson_ShouldHandleAllValidEnumValues()
        {
            // Arrange
            var allEnumValues = Enum.GetValues<TechnicalNamesEnum>();

            foreach (var enumValue in allEnumValues)
            {
                using var stringReader = new StringReader($"\"{enumValue}\"");
                using var jsonReader = new JsonTextReader(stringReader);
                jsonReader.Read(); // Move to the string token

                // Act
                var result = _converter.ReadJson(jsonReader, typeof(TechnicalNamesEnum), default, false, _serializer);

                // Assert
                Assert.Equal(enumValue, result);
            }
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Converter_ShouldPerformWellWithMultipleConversions()
        {
            // Arrange
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TechnicalNamesEnumConverter());
            var testObjects = new List<TestTechnicalConfig>();

            for (int i = 0; i < 1000; i++)
            {
                testObjects.Add(new TestTechnicalConfig
                {
                    Description = $"Technical{i}",
                    TechnicalName = (TechnicalNamesEnum)(i % Enum.GetValues<TechnicalNamesEnum>().Length)
                });
            }

            // Act & Assert - Should complete without timeout
            var startTime = DateTime.UtcNow;

            foreach (var testObject in testObjects)
            {
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestTechnicalConfig>(json, settings);
                Assert.NotNull(deserializedObject);
                Assert.Equal(testObject.TechnicalName, deserializedObject.TechnicalName);
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Should complete within reasonable time (adjust threshold as needed)
            Assert.True(duration.TotalSeconds < 5, $"Performance test took too long: {duration.TotalSeconds} seconds");
        }

        #endregion

        #region Helper Classes

        private class TestTechnicalConfig
        {
            public required string Description { get; set; }

            [JsonConverter(typeof(TechnicalNamesEnumConverter))]
            public TechnicalNamesEnum TechnicalName { get; set; }
        }

        private class TestCalculatorConfigWithTechnicals
        {
            public required string Name { get; set; }

            public TechnicalNamesEnum[] TechnicalIndicators { get; set; } = Array.Empty<TechnicalNamesEnum>();
        }

        #endregion
    }
}
