using HC.StrategyBuilder.src.Serializers;
using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HC.StrategyBuilder.Tests.Serializers
{
    public class TradeRuleSerializerTests : IDisposable
    {
        private readonly TradeRuleSerializer _serializer;
        private readonly List<string> _tempFiles;

        public TradeRuleSerializerTests()
        {
            _serializer = new TradeRuleSerializer();
            _tempFiles = new List<string>();
        }

        public void Dispose()
        {
            // Cleanup all temporary files created during tests
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private string CreateTempFile(string content = "")
        {
            var tempFile = Path.GetTempFileName();
            _tempFiles.Add(tempFile);
            if (!string.IsNullOrEmpty(content))
            {
                File.WriteAllText(tempFile, content);
            }
            return tempFile;
        }

        #region LoadFromJson Tests

        [Fact]
        public void LoadFromJson_ShouldDeserializeCompleteTradeRuleWithEnums()
        {
            // Arrange
            string json = @"{
                ""Rule"": {
                    ""Name"": ""CompleteEnumTestStrategy"",
                    ""CandleFrequency"": ""5m"",
                    ""MinProfit"": 0.5,
                    ""StopLoss"": 0.2,
                    ""TakeProfit"": 1.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.02,
                        ""MinEntryAmount"": 10.0
                    },
                    ""BuyRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""SMA20"",
                                ""CalculatorName"": ""SMA"",
                                ""TechnicalIndicators"": [""MOVINGAVERAGE""],
                                ""Parameters"": [
                                    { ""Name"": ""Period"", ""Value"": ""20"" }
                                ]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""SMA20"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""close""
                            }
                        ]
                    },
                    ""SellRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""RSI14"",
                                ""CalculatorName"": ""RSI"",
                                ""TechnicalIndicators"": [""RSI""],
                                ""Parameters"": [
                                    { ""Name"": ""Period"", ""Value"": ""14"" }
                                ]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""RSI14"",
                                    ""TechnicalIndicatorName"": ""RSI""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""70""
                            }
                        ]
                    }
                }
            }";

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Rule);
            Assert.Equal("CompleteEnumTestStrategy", result.Rule.Name);
            Assert.Equal("5m", result.Rule.CandleFrequency);
            Assert.Equal(0.5, result.Rule.MinProfit);
            Assert.Equal(0.2, result.Rule.StopLoss);
            Assert.Equal(1.0, result.Rule.TakeProfit);

            // Verify Bankroll
            Assert.NotNull(result.Rule.Bankroll);
            Assert.Equal(0.02, result.Rule.Bankroll.MaxRiskPerTrade);
            Assert.Equal(10.0, result.Rule.Bankroll.MinEntryAmount);

            // Verify BuyRule with Enums
            Assert.NotNull(result.Rule.BuyRule);
            Assert.Single(result.Rule.BuyRule.Calculators);
            var buyCalc = result.Rule.BuyRule.Calculators[0];
            Assert.Equal("SMA20", buyCalc.Name);
            Assert.Equal(CalculatorNameEnum.SMA, buyCalc.CalculatorName);
            Assert.Single(buyCalc.TechnicalIndicators);
            Assert.Equal(TechnicalNamesEnum.MOVINGAVERAGE, buyCalc.TechnicalIndicators[0]);
            Assert.Single(buyCalc.Parameters);
            Assert.Equal(ParameterNamesEnum.Period, buyCalc.Parameters[0].Name);
            Assert.Equal("20", buyCalc.Parameters[0].Value);

            // Verify BuyRule Conditions
            Assert.Single(result.Rule.BuyRule.Conditions);
            var buyCondition = result.Rule.BuyRule.Conditions[0];
            Assert.Equal("SMA20", buyCondition.Indicator1.CalculatorName);
            Assert.Equal(TechnicalNamesEnum.MOVINGAVERAGE, buyCondition.Indicator1.TechnicalIndicatorName);
            Assert.Equal(">", buyCondition.Operator);
            Assert.Equal("close", buyCondition.Value);

            // Verify SellRule with Enums
            Assert.NotNull(result.Rule.SellRule);
            Assert.Single(result.Rule.SellRule.Calculators);
            var sellCalc = result.Rule.SellRule.Calculators[0];
            Assert.Equal("RSI14", sellCalc.Name);
            Assert.Equal(CalculatorNameEnum.RSI, sellCalc.CalculatorName);
            Assert.Single(sellCalc.TechnicalIndicators);
            Assert.Equal(TechnicalNamesEnum.RSI, sellCalc.TechnicalIndicators[0]);
        }

        [Fact]
        public void LoadFromJson_ShouldHandleCaseInsensitiveEnumValues()
        {
            // Arrange
            string json = @"{
                ""Rule"": {
                    ""Name"": ""CaseInsensitiveTest"",
                    ""CandleFrequency"": ""1m"",
                    ""MinProfit"": 0.5,
                    ""StopLoss"": 0.2,
                    ""TakeProfit"": 1.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.02,
                        ""MinEntryAmount"": 10.0
                    },
                    ""BuyRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""TestCalc"",
                                ""CalculatorName"": ""sma"",
                                ""TechnicalIndicators"": [""movingaverage""],
                                ""Parameters"": [
                                    { ""Name"": ""period"", ""Value"": ""14"" }
                                ]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""TestCalc"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""50""
                            }
                        ]
                    },
                    ""SellRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""TestCalc2"",
                                ""CalculatorName"": ""Rsi"",
                                ""TechnicalIndicators"": [""rsi""],
                                ""Parameters"": [
                                    { ""Name"": ""Period"", ""Value"": ""14"" }
                                ]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""TestCalc2"",
                                    ""TechnicalIndicatorName"": ""RSI""
                                },
                                ""Operator"": ""<"",
                                ""Value"": ""50""
                            }
                        ]
                    }
                }
            }";

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(CalculatorNameEnum.SMA, result.Rule.BuyRule.Calculators[0].CalculatorName);
            Assert.Equal(TechnicalNamesEnum.MOVINGAVERAGE, result.Rule.BuyRule.Calculators[0].TechnicalIndicators[0]);
            Assert.Equal(ParameterNamesEnum.Period, result.Rule.BuyRule.Calculators[0].Parameters[0].Name);

            Assert.Equal(CalculatorNameEnum.RSI, result.Rule.SellRule.Calculators[0].CalculatorName);
            Assert.Equal(TechnicalNamesEnum.RSI, result.Rule.SellRule.Calculators[0].TechnicalIndicators[0]);
            Assert.Equal("TestCalc2", result.Rule.SellRule.Conditions[0].Indicator1.CalculatorName);
        }

        [Fact]
        public void LoadFromJson_ShouldHandleComplexConditionsWithIndicatorComparisons()
        {
            // Arrange
            string json = TradeRuleTestData.ValidTradeRuleWithIndicatorComparisons;

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            Assert.NotNull(result.Rule.BuyRule.Conditions[0].Indicator2);
            Assert.Equal("SMA_Slow", result.Rule.BuyRule.Conditions[0].Indicator2!.CalculatorName);
            Assert.Equal(TechnicalNamesEnum.MOVINGAVERAGE, result.Rule.BuyRule.Conditions[0].Indicator2!.TechnicalIndicatorName);
        }

        [Fact]
        public void LoadFromJson_ShouldHandleMultipleTechnicalIndicators()
        {
            // Arrange
            string json = TradeRuleTestData.ValidTradeRuleWithMultipleIndicatorsJson;

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            var buyCalculators = result.Rule.BuyRule.Calculators;
            // Get total count of technical indicators across all calculators
            int totalTechnicalIndicators = buyCalculators
                .SelectMany(calc => calc.TechnicalIndicators)
                .Count();

            // Get all technical indicators as a flat list
            var allTechnicalIndicators = buyCalculators
                .SelectMany(calc => calc.TechnicalIndicators)
                .ToList();

            int totalParameters = buyCalculators
                .SelectMany(calc => calc.TechnicalIndicators)
                .Count();

            int allParameteres = buyCalculators
                .SelectMany(calc => calc.Parameters)
                .Distinct()
                .Count();

            Assert.Equal(2, totalTechnicalIndicators);
            Assert.Contains(TechnicalNamesEnum.RSI, allTechnicalIndicators);
            Assert.Contains(TechnicalNamesEnum.MOVINGAVERAGE, allTechnicalIndicators);

            Assert.Equal(2, totalParameters);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void LoadFromJson_ShouldThrowArgumentException_WhenJsonIsEmptyOrWhitespace(string json)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.LoadFromJson(json));
            Assert.Equal("JSON string cannot be null or empty. (Parameter 'json')", exception.Message);
        }

        [Fact]
        public void LoadFromJson_ShouldThrowArgumentException_WhenJsonIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.LoadFromJson(null!));
            Assert.Equal("JSON string cannot be null or empty. (Parameter 'json')", exception.Message);
        }

        [Fact]
        public void LoadFromJson_ShouldThrowJsonException_WhenJsonIsInvalid()
        {
            // Arrange
            string invalidJson = "{ invalid json }";

            // Act & Assert
            Assert.ThrowsAny<JsonException>(() => _serializer.LoadFromJson(invalidJson));
        }

        [Fact]
        public void LoadFromJson_ShouldThrowInvalidOperationException_WhenJsonDeserializesToNull()
        {
            // Arrange
            string nullJson = "null";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _serializer.LoadFromJson(nullJson));
            Assert.Equal("Failed to deserialize TradeRuleConfiguration from JSON.", exception.Message);
        }

        [Fact]
        public void LoadFromJson_ShouldThrowJsonException_WhenEnumValueIsInvalid()
        {
            // Arrange
            string json = @"{
                ""Rule"": {
                    ""Name"": ""InvalidEnumTest"",
                    ""CandleFrequency"": ""1m"",
                    ""MinProfit"": 0.5,
                    ""StopLoss"": 0.2,
                    ""TakeProfit"": 1.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.02,
                        ""MinEntryAmount"": 10.0
                    },
                    ""BuyRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""TestCalc"",
                                ""CalculatorName"": ""INVALID_CALCULATOR"",
                                ""TechnicalIndicators"": [""MOVINGAVERAGE""],
                                ""Parameters"": [{ ""Name"": ""Period"", ""Value"": ""14"" }]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""SMA"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""50""
                            }
                        ]
                    },
                    ""SellRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""TestCalc"",
                                ""CalculatorName"": ""SMA"",
                                ""TechnicalIndicators"": [""MOVINGAVERAGE""],
                                ""Parameters"": [{ ""Name"": ""Period"", ""Value"": ""14"" }]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""SMA"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": ""<"",
                                ""Value"": ""50""
                            }
                        ]
                    }
                }
            }";

            // Act & Assert
            Assert.ThrowsAny<JsonException>(() => _serializer.LoadFromJson(json));
        }

        #endregion

        #region LoadFromJObject Tests

        [Fact]
        public void LoadFromJObject_ShouldDeserializeCorrectly()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{
                ""Rule"": {
                    ""Name"": ""JObjectTestStrategy"",
                    ""CandleFrequency"": ""1h"",
                    ""MinProfit"": 1.0,
                    ""StopLoss"": 0.5,
                    ""TakeProfit"": 2.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.02,
                        ""MinEntryAmount"": 10.0
                    },
                    ""BuyRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""RSI14"",
                                ""CalculatorName"": ""RSI"",
                                ""TechnicalIndicators"": [""RSI""],
                                ""Parameters"": [{ ""Name"": ""Period"", ""Value"": ""14"" }]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""RSI"",
                                    ""TechnicalIndicatorName"": ""RSI""
                                },
                                ""Operator"": ""<"",
                                ""Value"": ""30""
                            }
                        ]
                    },
                    ""SellRule"": {
                        ""Calculators"": [
                            {
                                ""Name"": ""RSI14"",
                                ""CalculatorName"": ""RSI"",
                                ""TechnicalIndicators"": [""RSI""],
                                ""Parameters"": [{ ""Name"": ""Period"", ""Value"": ""14"" }]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""RSI"",
                                    ""TechnicalIndicatorName"": ""RSI""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""70""
                            }
                        ]
                    }
                }
            }");

            // Act
            var result = _serializer.LoadFromJObject(jsonObject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JObjectTestStrategy", result.Rule.Name);
            Assert.Equal("1h", result.Rule.CandleFrequency);
            Assert.Equal(CalculatorNameEnum.RSI, result.Rule.BuyRule.Calculators[0].CalculatorName);
            Assert.Equal(TechnicalNamesEnum.RSI, result.Rule.BuyRule.Calculators[0].TechnicalIndicators[0]);
        }

        [Fact]
        public void LoadFromJObject_ShouldThrowArgumentNullException_WhenJObjectIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _serializer.LoadFromJObject(null!));
            Assert.Equal("jsonObject", exception.ParamName);
        }

        [Fact]
        public void LoadFromJObject_ShouldThrowInvalidOperationException_WhenConversionFails()
        {
            // Arrange
            var invalidJObject = JObject.Parse(@"{ ""InvalidProperty"": ""InvalidValue"" }");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _serializer.LoadFromJObject(invalidJObject));
            Assert.Equal("Failed to convert JObject to TradeRuleConfiguration.", exception.Message);
        }

        #endregion

        #region LoadFromFile Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void LoadFromFile_ShouldThrowArgumentException_WhenFilePathIsEmptyOrWhitespace(string filePath)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.LoadFromFile(filePath));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public void LoadFromFile_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.LoadFromFile(null!));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public void LoadFromFile_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _serializer.LoadFromFile(nonExistentFile));
        }

        [Fact]
        public void LoadFromFile_ShouldThrowInvalidDataException_WhenFileIsEmpty()
        {
            // Arrange
            string tempFile = CreateTempFile("");

            // Act & Assert
            Assert.Throws<InvalidDataException>(() => _serializer.LoadFromFile(tempFile));
        }

        [Fact]
        public void LoadFromFile_ShouldThrowInvalidDataException_WhenFileContainsOnlyWhitespace()
        {
            // Arrange
            string tempFile = CreateTempFile("   \n\t  ");

            // Act & Assert
            Assert.Throws<InvalidDataException>(() => _serializer.LoadFromFile(tempFile));
        }

        [Fact]
        public void LoadFromFile_ShouldLoadValidConfiguration()
        {
            // Arrange
            var testConfig = CreateCompleteTestConfiguration();
            var json = _serializer.ToJson(testConfig);
            string tempFile = CreateTempFile(json);

            // Act
            var result = _serializer.LoadFromFile(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testConfig.Rule.Name, result.Rule.Name);
            Assert.Equal(testConfig.Rule.CandleFrequency, result.Rule.CandleFrequency);
        }

        #endregion

        #region LoadFromFileAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadFromFileAsync_ShouldThrowArgumentException_WhenFilePathIsEmptyOrWhitespace(string filePath)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _serializer.LoadFromFileAsync(filePath));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public async Task LoadFromFileAsync_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _serializer.LoadFromFileAsync(null!));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public async Task LoadFromFileAsync_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _serializer.LoadFromFileAsync(nonExistentFile));
        }

        [Fact]
        public async Task LoadFromFileAsync_ShouldThrowInvalidDataException_WhenFileIsEmpty()
        {
            // Arrange
            string tempFile = CreateTempFile("");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => _serializer.LoadFromFileAsync(tempFile));
        }

        [Fact]
        public async Task LoadFromFileAsync_ShouldLoadValidConfiguration()
        {
            // Arrange
            var testConfig = CreateCompleteTestConfiguration();
            var json = _serializer.ToJson(testConfig);
            string tempFile = CreateTempFile(json);

            // Act
            var result = await _serializer.LoadFromFileAsync(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testConfig.Rule.Name, result.Rule.Name);
            Assert.Equal(testConfig.Rule.CandleFrequency, result.Rule.CandleFrequency);
        }

        #endregion

        #region ToJson Tests

        [Fact]
        public void ToJson_ShouldSerializeCompleteConfigurationWithEnumsCorrectly()
        {
            // Arrange
            var config = CreateCompleteTestConfiguration();

            // Act
            string json = _serializer.ToJson(config);

            // Assert
            Assert.NotEmpty(json);
            Assert.Contains("CompleteTestRule", json);
            Assert.Contains("15m", json);
            Assert.Contains("SMA", json);
            Assert.Contains("RSI", json);
            Assert.Contains("Bankroll", json);
            Assert.Contains("MaxRiskPerTrade", json);
            Assert.Contains("MOVINGAVERAGE", json);
            Assert.Contains("Period", json);

            // Verify it's valid JSON by parsing it back
            var parsedConfig = _serializer.LoadFromJson(json);
            Assert.Equal(config.Rule.Name, parsedConfig.Rule.Name);
        }

        [Fact]
        public void ToJson_ShouldHandleNullConfig()
        {
            // Act
            string json = _serializer.ToJson(null!);

            // Assert
            Assert.Equal("null", json);
        }

        [Fact]
        public void ToJson_ShouldProduceFormattedJson()
        {
            // Arrange
            var config = CreateSimpleTestConfiguration();

            // Act
            string json = _serializer.ToJson(config);

            // Assert
            Assert.Contains("\n", json); // Should contain newlines for formatting
            Assert.Contains("  ", json); // Should contain indentation
        }

        [Fact]
        public void ToJson_ShouldUseCustomConvertersForEnums()
        {
            // Arrange
            var config = CreateCompleteTestConfiguration();

            // Act
            string json = _serializer.ToJson(config);

            // Assert - Check that enums are serialized as strings, not numbers
            Assert.DoesNotContain("\"CalculatorName\": 0", json); // Should not be numeric
            Assert.DoesNotContain("\"CalculatorName\": 1", json); // Should not be numeric
            Assert.Contains("\"CalculatorName\": \"SMA\"", json); // Should be string
            Assert.Contains("\"CalculatorName\": \"RSI\"", json); // Should be string
        }

        #endregion

        #region SaveToFile Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void SaveToFile_ShouldThrowArgumentException_WhenFilePathIsEmptyOrWhitespace(string filePath)
        {
            // Arrange
            var config = CreateSimpleTestConfiguration();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.SaveToFile(config, filePath));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public void SaveToFile_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Arrange
            var config = CreateSimpleTestConfiguration();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _serializer.SaveToFile(config, null!));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public void SaveToFile_ShouldSaveAndReloadCorrectly()
        {
            // Arrange
            string tempFile = CreateTempFile();
            var originalConfig = CreateCompleteTestConfiguration();

            // Act
            _serializer.SaveToFile(originalConfig, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));

            var reloadedConfig = _serializer.LoadFromFile(tempFile);
            Assert.Equal(originalConfig.Rule.Name, reloadedConfig.Rule.Name);
            Assert.Equal(originalConfig.Rule.MinProfit, reloadedConfig.Rule.MinProfit);
            Assert.Equal(originalConfig.Rule.Bankroll.MaxRiskPerTrade, reloadedConfig.Rule.Bankroll.MaxRiskPerTrade);
        }

        [Fact]
        public void SaveToFile_ShouldOverwriteExistingFile()
        {
            // Arrange
            string tempFile = CreateTempFile();
            var config1 = CreateSimpleTestConfiguration();
            var config2 = CreateCompleteTestConfiguration();

            _serializer.SaveToFile(config1, tempFile);
            var originalSize = new FileInfo(tempFile).Length;

            // Act
            _serializer.SaveToFile(config2, tempFile);

            // Assert
            var newSize = new FileInfo(tempFile).Length;
            Assert.NotEqual(originalSize, newSize);

            var reloadedConfig = _serializer.LoadFromFile(tempFile);
            Assert.Equal(config2.Rule.Name, reloadedConfig.Rule.Name);
        }

        [Fact]
        public void SaveToFile_ShouldHandleNullConfig()
        {
            // Arrange
            string tempFile = CreateTempFile();

            // Act
            _serializer.SaveToFile(null!, tempFile);

            // Assert
            string content = File.ReadAllText(tempFile);
            Assert.Equal("null", content);
        }

        #endregion

        #region SaveToFileAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SaveToFileAsync_ShouldThrowArgumentException_WhenFilePathIsEmptyOrWhitespace(string filePath)
        {
            // Arrange
            var config = CreateSimpleTestConfiguration();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _serializer.SaveToFileAsync(config, filePath));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public async Task SaveToFileAsync_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Arrange
            var config = CreateSimpleTestConfiguration();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _serializer.SaveToFileAsync(config, null!));
            Assert.Equal("File path cannot be null or empty. (Parameter 'filePath')", exception.Message);
        }

        [Fact]
        public async Task SaveToFileAsync_ShouldSaveAndReloadCorrectly()
        {
            // Arrange
            string tempFile = CreateTempFile();
            var originalConfig = CreateCompleteTestConfiguration();

            // Act
            await _serializer.SaveToFileAsync(originalConfig, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));

            var reloadedConfig = await _serializer.LoadFromFileAsync(tempFile);
            Assert.Equal(originalConfig.Rule.Name, reloadedConfig.Rule.Name);
            Assert.Equal(originalConfig.Rule.MinProfit, reloadedConfig.Rule.MinProfit);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void RoundTrip_ShouldPreserveAllDataIncludingEnums()
        {
            // Arrange
            var originalConfig = CreateCompleteTestConfiguration();

            // Act - Convert to JSON and back
            string json = _serializer.ToJson(originalConfig);
            var roundTripConfig = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(originalConfig.Rule.Name, roundTripConfig.Rule.Name);
            Assert.Equal(originalConfig.Rule.CandleFrequency, roundTripConfig.Rule.CandleFrequency);
            Assert.Equal(originalConfig.Rule.MinProfit, roundTripConfig.Rule.MinProfit);
            Assert.Equal(originalConfig.Rule.StopLoss, roundTripConfig.Rule.StopLoss);
            Assert.Equal(originalConfig.Rule.TakeProfit, roundTripConfig.Rule.TakeProfit);

            Assert.Equal(originalConfig.Rule.Bankroll.MaxRiskPerTrade, roundTripConfig.Rule.Bankroll.MaxRiskPerTrade);
            Assert.Equal(originalConfig.Rule.Bankroll.MinEntryAmount, roundTripConfig.Rule.Bankroll.MinEntryAmount);

            // Verify enum preservation
            Assert.Equal(originalConfig.Rule.BuyRule.Calculators[0].CalculatorName,
                        roundTripConfig.Rule.BuyRule.Calculators[0].CalculatorName);
            Assert.Equal(originalConfig.Rule.BuyRule.Calculators[0].TechnicalIndicators[0],
                        roundTripConfig.Rule.BuyRule.Calculators[0].TechnicalIndicators[0]);
            Assert.Equal(originalConfig.Rule.BuyRule.Calculators[0].Parameters[0].Name,
                        roundTripConfig.Rule.BuyRule.Calculators[0].Parameters[0].Name);
        }

        [Fact]
        public async Task AsyncRoundTrip_ShouldPreserveAllData()
        {
            // Arrange
            string tempFile = CreateTempFile();
            var originalConfig = CreateCompleteTestConfiguration();

            // Act
            await _serializer.SaveToFileAsync(originalConfig, tempFile);
            var roundTripConfig = await _serializer.LoadFromFileAsync(tempFile);

            // Assert
            Assert.Equal(originalConfig.Rule.Name, roundTripConfig.Rule.Name);
            Assert.Equal(originalConfig.Rule.Bankroll.MaxRiskPerTrade, roundTripConfig.Rule.Bankroll.MaxRiskPerTrade);
            Assert.Equal(originalConfig.Rule.BuyRule.Calculators[0].CalculatorName,
                        roundTripConfig.Rule.BuyRule.Calculators[0].CalculatorName);
        }

        #endregion

        #region Test Helpers

        private TradeRules CreateSimpleTestConfiguration()
        {
            return new TradeRules
            {
                Rule = new TradeRule
                {
                    Name = "SimpleTestRule",
                    CandleFrequency = "1m",
                    MinProfit = 0.5,
                    StopLoss = 0.2,
                    TakeProfit = 1.0,
                    Bankroll = new BankrollConfig
                    {
                        MaxRiskPerTrade = 0.02,
                        MinEntryAmount = 10.0
                    },
                    BuyRule = new TradeSubRule
                    {
                        Calculators = new List<CalculatorConfig>
                        {
                            new CalculatorConfig
                            {
                                Name = "SimpleCalc",
                                CalculatorName = CalculatorNameEnum.SMA,
                                TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                                Parameters = new List<CalculatorParameter>
                                {
                                    new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "14" }
                                }
                            }
                        },
                        Conditions = new List<ConditionConfig>
                        {
                            new ConditionConfig
                            {
                                Indicator1 = new Indicator
                                {
                                    CalculatorName = nameof(CalculatorNameEnum.SMA),
                                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                                },
                                Operator = ">",
                                Value = "50"
                            }
                        }
                    },
                    SellRule = new TradeSubRule
                    {
                        Calculators = new List<CalculatorConfig>
                        {
                            new CalculatorConfig
                            {
                                Name = "SimpleCalc",
                                CalculatorName = CalculatorNameEnum.SMA,
                                TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                                Parameters = new List<CalculatorParameter>
                                {
                                    new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "14" }
                                }
                            }
                        },
                        Conditions = new List<ConditionConfig>
                        {
                            new ConditionConfig
                            {
                                Indicator1 = new Indicator
                                {
                                    CalculatorName = nameof(CalculatorNameEnum.SMA),
                                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                                },
                                Operator = "<",
                                Value = "50"
                            }
                        }
                    }
                }
            };
        }

        private TradeRules CreateCompleteTestConfiguration()
        {
            return new TradeRules
            {
                Rule = new TradeRule
                {
                    Name = "CompleteTestRule",
                    CandleFrequency = "15m",
                    MinProfit = 0.8,
                    StopLoss = 0.3,
                    TakeProfit = 1.5,
                    Bankroll = new BankrollConfig
                    {
                        MaxRiskPerTrade = 0.025,
                        MinEntryAmount = 50.0
                    },
                    BuyRule = new TradeSubRule
                    {
                        Calculators = new List<CalculatorConfig>
                        {
                            new CalculatorConfig
                            {
                                Name = "FastSMA",
                                CalculatorName = CalculatorNameEnum.SMA,
                                TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                                Parameters = new List<CalculatorParameter>
                                {
                                    new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "10" }
                                }
                            },
                            new CalculatorConfig
                            {
                                Name = "SlowSMA",
                                CalculatorName = CalculatorNameEnum.SMA,
                                TechnicalIndicators = new[] { TechnicalNamesEnum.MOVINGAVERAGE },
                                Parameters = new List<CalculatorParameter>
                                {
                                    new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "30" }
                                }
                            }
                        },
                        Conditions = new List<ConditionConfig>
                        {
                            new ConditionConfig
                            {
                                Indicator1 = new Indicator
                                {
                                    CalculatorName = "FastSMA",
                                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                                },
                                Operator = ">",
                                Indicator2 = new Indicator
                                {
                                    CalculatorName = "SlowSMA",
                                    TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                                }
                            }
                        },
                    },
                    SellRule = new TradeSubRule
                    {
                        Calculators = new List<CalculatorConfig>
                        {
                            new CalculatorConfig
                            {
                                Name = "RSI14",
                                CalculatorName = CalculatorNameEnum.RSI,
                                TechnicalIndicators = new[] { TechnicalNamesEnum.RSI },
                                Parameters = new List<CalculatorParameter>
                                {
                                    new CalculatorParameter { Name = ParameterNamesEnum.Period, Value = "14" }
                                }
                            }
                        },
                        Conditions = new List<ConditionConfig>
                        {
                            new ConditionConfig
                            {
                                Indicator1 = new Indicator
                                {
                                    CalculatorName = "RSI14",
                                    TechnicalIndicatorName = TechnicalNamesEnum.RSI
                                },
                                Operator = ">",
                                Value = "70"
                            }
                        }
                    }
                }
            };
        }

        #endregion
    }
}