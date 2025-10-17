using HC.StrategyBuilder.src.Serializers.Converters;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;

namespace HC.StrategyBuilder.Tests.Serializers.Converters
{
    public class ParameterNamesConverterTests
    {
        private readonly ParameterNamesConverter _converter;
        private readonly JsonSerializer _serializer;

        public ParameterNamesConverterTests()
        {
            _converter = new ParameterNamesConverter();
            _serializer = new JsonSerializer();
        }

        #region WriteJson Tests

        [Theory]
        [InlineData(ParameterNamesEnum.Period, "Period")]
        public void WriteJson_ShouldSerializeEnumToString(ParameterNamesEnum enumValue, string expectedString)
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
            var allEnumValues = Enum.GetValues<ParameterNamesEnum>();

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
        [InlineData("Period", ParameterNamesEnum.Period)]
        public void ReadJson_ShouldDeserializeStringToEnum_ExactMatch(string jsonValue, ParameterNamesEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer);

            // Assert
            Assert.Equal(expectedEnum, result);
        }

        [Theory]
        [InlineData("period", ParameterNamesEnum.Period)]
        [InlineData("PERIOD", ParameterNamesEnum.Period)]
        [InlineData("Period", ParameterNamesEnum.Period)]
        [InlineData("PeRiOd", ParameterNamesEnum.Period)]
        public void ReadJson_ShouldDeserializeStringToEnum_CaseInsensitive(string jsonValue, ParameterNamesEnum expectedEnum)
        {
            // Arrange
            using var stringReader = new StringReader($"\"{jsonValue}\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer);

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
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert null to ParameterNamesEnum", exception.Message);
        }

        [Theory]
        [InlineData("INVALID_PARAMETER")]
        [InlineData("NOT_A_PARAMETER")]
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
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal($"Cannot convert '{invalidValue}' to ParameterNamesEnum", exception.Message);
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
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert empty string to ParameterNamesEnum", exception.Message);
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
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '   ' to ParameterNamesEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleNumericStringsThatMatchEnumValues()
        {
            // Arrange - Test that numeric strings that might match enum ordinal values are rejected
            using var stringReader = new StringReader("\"0\""); // Assuming 0 might be Period's ordinal
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act & Assert
            var exception = Assert.Throws<JsonException>(() =>
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal("Cannot convert '0' to ParameterNamesEnum", exception.Message);
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
                _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer));

            Assert.Equal($"Cannot convert '{numericValue}' to ParameterNamesEnum", exception.Message);
        }

        [Fact]
        public void ReadJson_ShouldHandleExistingValueParameter()
        {
            // Arrange
            using var stringReader = new StringReader("\"Period\"");
            using var jsonReader = new JsonTextReader(stringReader);
            jsonReader.Read(); // Move to the string token

            // Act
            var result = _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), ParameterNamesEnum.Period, true, _serializer);

            // Assert
            Assert.Equal(ParameterNamesEnum.Period, result); // Should return the new value, not necessarily the existing one
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Converter_ShouldWorkWithJsonConvertSerializeObject()
        {
            // Arrange
            var testObject = new TestParameterConfig
            {
                Description = "TestParameter",
                ParameterName = ParameterNamesEnum.Period
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ParameterNamesConverter());

            // Act
            string json = JsonConvert.SerializeObject(testObject, settings);
            var deserializedObject = JsonConvert.DeserializeObject<TestParameterConfig>(json, settings);

            // Assert
            Assert.NotNull(deserializedObject);
            Assert.Equal(testObject.Description, deserializedObject.Description);
            Assert.Equal(testObject.ParameterName, deserializedObject.ParameterName);
            Assert.Contains("\"ParameterName\":\"Period\"", json);
        }

        [Fact]
        public void Converter_ShouldWorkWithCaseInsensitiveJson()
        {
            // Arrange
            string json = @"{
                ""Description"": ""TestParameter"",
                ""ParameterName"": ""period""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ParameterNamesConverter());

            // Act
            var result = JsonConvert.DeserializeObject<TestParameterConfig>(json, settings);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestParameter", result.Description);
            Assert.Equal(ParameterNamesEnum.Period, result.ParameterName);
        }

        [Fact]
        public void Converter_ShouldThrowOnInvalidJsonEnum()
        {
            // Arrange
            string json = @"{
                ""Description"": ""TestParameter"",
                ""ParameterName"": ""INVALID_PARAMETER""
            }";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ParameterNamesConverter());

            // Act & Assert
            var exception = Assert.ThrowsAny<JsonException>(() =>
                JsonConvert.DeserializeObject<TestParameterConfig>(json, settings));

            Assert.Contains("Cannot convert 'INVALID_PARAMETER' to ParameterNamesEnum", exception.Message);
        }

        [Fact]
        public void Converter_ShouldRoundTripAllEnumValues()
        {
            // Arrange
            var allEnumValues = Enum.GetValues<ParameterNamesEnum>();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ParameterNamesConverter());

            foreach (var enumValue in allEnumValues)
            {
                var testObject = new TestParameterConfig
                {
                    Description = $"Test{enumValue}",
                    ParameterName = enumValue
                };

                // Act
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestParameterConfig>(json, settings);

                // Assert
                Assert.NotNull(deserializedObject);
                Assert.Equal(testObject.ParameterName, deserializedObject.ParameterName);
                Assert.Contains($"\"{enumValue}\"", json);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void WriteJson_ShouldHandleDefaultEnumValue()
        {
            // Arrange
            var defaultValue = default(ParameterNamesEnum);
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
            var allEnumValues = Enum.GetValues<ParameterNamesEnum>();

            foreach (var enumValue in allEnumValues)
            {
                using var stringReader = new StringReader($"\"{enumValue}\"");
                using var jsonReader = new JsonTextReader(stringReader);
                jsonReader.Read(); // Move to the string token

                // Act
                var result = _converter.ReadJson(jsonReader, typeof(ParameterNamesEnum), default, false, _serializer);

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
            settings.Converters.Add(new ParameterNamesConverter());
            var testObjects = new List<TestParameterConfig>();

            for (int i = 0; i < 1000; i++)
            {
                testObjects.Add(new TestParameterConfig
                {
                    Description = $"Parameter{i}",
                    ParameterName = (ParameterNamesEnum)(i % Enum.GetValues<ParameterNamesEnum>().Length)
                });
            }

            // Act & Assert - Should complete without timeout
            var startTime = DateTime.UtcNow;

            foreach (var testObject in testObjects)
            {
                string json = JsonConvert.SerializeObject(testObject, settings);
                var deserializedObject = JsonConvert.DeserializeObject<TestParameterConfig>(json, settings);
                Assert.NotNull(deserializedObject);
                Assert.Equal(testObject.ParameterName, deserializedObject.ParameterName);
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Should complete within reasonable time (adjust threshold as needed)
            Assert.True(duration.TotalSeconds < 5, $"Performance test took too long: {duration.TotalSeconds} seconds");
        }

        #endregion

        #region Helper Classes

        private class TestParameterConfig
        {
            public required string Description { get; set; }

            [JsonConverter(typeof(ParameterNamesConverter))]
            public ParameterNamesEnum ParameterName { get; set; }
        }

        #endregion
    }
}
