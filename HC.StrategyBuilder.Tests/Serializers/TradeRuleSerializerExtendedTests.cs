using HC.StrategyBuilder.src.Serializers;
using HC.StrategyBuilder.Tests.Helpers;
using HC.TechnicalCalculators.Src.Models;
using Newtonsoft.Json.Linq;

namespace HC.StrategyBuilder.Tests.Serializers
{
    public class TradeRuleSerializerExtendedTests : IDisposable
    {
        private readonly TradeRuleSerializer _serializer;
        private readonly List<string> _tempFiles;

        public TradeRuleSerializerExtendedTests()
        {
            _serializer = new TradeRuleSerializer();
            _tempFiles = new List<string>();
        }

        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        #region Error Handling Tests

        [Fact]
        public void LoadFromJson_ShouldThrowArgumentException_WhenJsonIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.LoadFromJson(null!));
        }

        [Fact]
        public void LoadFromJson_ShouldThrowArgumentException_WhenJsonIsEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.LoadFromJson(""));
        }

        [Fact]
        public void LoadFromJson_ShouldThrowArgumentException_WhenJsonIsWhitespace()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.LoadFromJson("   "));
        }

        [Fact]
        public void LoadFromJson_ShouldThrowException_WhenJsonIsInvalid()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act & Assert
            Assert.Throws<Newtonsoft.Json.JsonException>(() => _serializer.LoadFromJson(invalidJson));
        }

        [Fact]
        public void LoadFromFile_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.LoadFromFile(null!));
        }

        [Fact]
        public void LoadFromFile_ShouldThrowArgumentException_WhenFilePathIsEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.LoadFromFile(""));
        }

        [Fact]
        public void LoadFromFile_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => _serializer.LoadFromFile("nonexistent.json"));
        }

        [Fact]
        public void SaveToFile_ShouldThrowArgumentException_WhenFilePathIsNull()
        {
            // Arrange
            var config = CreateTestConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.SaveToFile(config, null!));
        }

        [Fact]
        public void SaveToFile_ShouldThrowArgumentException_WhenFilePathIsEmpty()
        {
            // Arrange
            var config = CreateTestConfiguration();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serializer.SaveToFile(config, ""));
        }

        [Fact]
        public void LoadFromJObject_ShouldThrowArgumentNullException_WhenJObjectIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _serializer.LoadFromJObject(null!));
        }

        #endregion

        #region Comprehensive Serialization Tests

        [Fact]
        public void ToJson_ShouldSerializeCompleteConfiguration()
        {
            // Arrange
            var config = CreateCompleteTestConfiguration();

            // Act
            var json = _serializer.ToJson(config);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"Rule\":", json);
            Assert.Contains("\"Name\":", json);
            Assert.Contains("\"CandleFrequency\":", json);
            Assert.Contains("\"BuyRule\":", json);
            Assert.Contains("\"SellRule\":", json);
            Assert.Contains("\"Calculators\":", json);
            Assert.Contains("\"Conditions\":", json);
        }

        [Fact]
        public void ToJson_ShouldIncludeAllEnumValues()
        {
            // Arrange
            var config = CreateCompleteTestConfiguration();

            // Act
            var json = _serializer.ToJson(config);

            // Assert
            Assert.Contains("\"CalculatorName\":", json);
            Assert.Contains("\"TechnicalIndicators\":", json);
            Assert.Contains("\"TechnicalIndicatorName\":", json);
        }

        [Fact]
        public void SaveToFile_AndLoadFromFile_ShouldRoundTripCorrectly()
        {
            // Arrange
            var originalConfig = CreateCompleteTestConfiguration();
            var filePath = CreateTempFile();

            // Act
            _serializer.SaveToFile(originalConfig, filePath);
            var loadedConfig = _serializer.LoadFromFile(filePath);

            // Assert
            Assert.NotNull(loadedConfig);
            Assert.Equal(originalConfig.Rule.Name, loadedConfig.Rule.Name);
            Assert.Equal(originalConfig.Rule.CandleFrequency, loadedConfig.Rule.CandleFrequency);
            Assert.Equal(originalConfig.Rule.MinProfit, loadedConfig.Rule.MinProfit);
            Assert.Equal(originalConfig.Rule.BuyRule.Calculators.Count, loadedConfig.Rule.BuyRule.Calculators.Count);
            Assert.Equal(originalConfig.Rule.BuyRule.Conditions.Count, loadedConfig.Rule.BuyRule.Conditions.Count);
        }

        [Fact]
        public void LoadFromJson_ShouldHandleComplexNestedStructure()
        {
            // Arrange
            var json = CreateComplexJsonConfiguration();

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Rule);
            Assert.NotNull(result.Rule.BuyRule);
            Assert.NotNull(result.Rule.SellRule);
            Assert.True(result.Rule.BuyRule.Calculators.Count > 0);
            Assert.True(result.Rule.BuyRule.Conditions.Count > 0);
            Assert.True(result.Rule.SellRule.Calculators.Count > 0);
            Assert.True(result.Rule.SellRule.Conditions.Count > 0);
        }

        [Fact]
        public void LoadFromJObject_ShouldHandleJObjectInput()
        {
            // Arrange
            var config = CreateTestConfiguration();
            var json = _serializer.ToJson(config);
            var jObject = JObject.Parse(json);

            // Act
            var result = _serializer.LoadFromJObject(jObject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(config.Rule.Name, result.Rule.Name);
        }

        [Fact]
        public void Serialization_ShouldPreservePrecision()
        {
            // Arrange
            var config = CreateTestConfiguration();
            config.Rule.MinProfit = 0.123456789;
            config.Rule.StopLoss = 0.987654321;
            config.Rule.TakeProfit = 1.555555555;

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(config.Rule.MinProfit, loaded.Rule.MinProfit, 6);
            Assert.Equal(config.Rule.StopLoss, loaded.Rule.StopLoss, 6);
            Assert.Equal(config.Rule.TakeProfit, loaded.Rule.TakeProfit, 6);
        }

        [Fact]
        public void Serialization_ShouldPreserveParameterEnums()
        {
            // Arrange
            var config = CreateTestConfiguration();
            var originalParameterName = config.Rule.BuyRule.Calculators[0].Parameters[0].Name;

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(originalParameterName, loaded.Rule.BuyRule.Calculators[0].Parameters[0].Name);
        }

        [Fact]
        public void Serialization_ShouldPreserveTechnicalIndicatorEnums()
        {
            // Arrange
            var config = CreateTestConfiguration();
            var originalIndicators = config.Rule.BuyRule.Calculators[0].TechnicalIndicators;

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(originalIndicators.Length, loaded.Rule.BuyRule.Calculators[0].TechnicalIndicators.Length);
            Assert.Equal(originalIndicators[0], loaded.Rule.BuyRule.Calculators[0].TechnicalIndicators[0]);
        }

        [Fact]
        public void Serialization_ShouldPreserveCalculatorNameEnum()
        {
            // Arrange
            var config = CreateTestConfiguration();
            var originalCalculatorName = config.Rule.BuyRule.Calculators[0].CalculatorName;

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.Equal(originalCalculatorName, loaded.Rule.BuyRule.Calculators[0].CalculatorName);
        }

        [Fact]
        public void Serialization_ShouldHandleNullOptionalFields()
        {
            // Arrange
            var config = CreateTestConfiguration();
            config.Rule.BuyRule.Conditions[0].Indicator2 = null;
            config.Rule.BuyRule.Conditions[0].Value = "50";

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.Null(loaded.Rule.BuyRule.Conditions[0].Indicator2);
            Assert.Equal("50", loaded.Rule.BuyRule.Conditions[0].Value);
        }

        [Fact]
        public void Serialization_ShouldHandleEmptyCollections()
        {
            // Arrange
            var config = CreateTestConfiguration();
            config.Rule.BuyRule.Calculators[0].Parameters = new List<CalculatorParameter>();

            // Act
            var json = _serializer.ToJson(config);
            var loaded = _serializer.LoadFromJson(json);

            // Assert
            Assert.NotNull(loaded.Rule.BuyRule.Calculators[0].Parameters);
            Assert.Empty(loaded.Rule.BuyRule.Calculators[0].Parameters);
        }

        [Fact]
        public void LoadFromJson_ShouldHandleMissingOptionalFields()
        {
            // Arrange
            var json = @"{
                ""Rule"": {
                    ""Name"": ""MinimalStrategy"",
                    ""CandleFrequency"": ""1m"",
                    ""MinProfit"": 0.5,
                    ""StopLoss"": 0.2,
                    ""TakeProfit"": 1.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.02,
                        ""MinEntryAmount"": 10.0
                    },
                    ""BuyRule"": {
                        ""Calculators"": [],
                        ""Conditions"": []
                    },
                    ""SellRule"": {
                        ""Calculators"": [],
                        ""Conditions"": []
                    }
                }
            }";

            // Act
            var result = _serializer.LoadFromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MinimalStrategy", result.Rule.Name);
            Assert.Empty(result.Rule.BuyRule.Calculators);
            Assert.Empty(result.Rule.BuyRule.Conditions);
        }

        [Fact]
        public void SaveToFile_ShouldOverwriteExistingFile()
        {
            // Arrange
            var config1 = CreateTestConfiguration();
            config1.Rule.Name = "FirstConfig";

            var config2 = CreateTestConfiguration();
            config2.Rule.Name = "SecondConfig";

            var filePath = CreateTempFile();

            // Act
            _serializer.SaveToFile(config1, filePath);
            _serializer.SaveToFile(config2, filePath);
            var loaded = _serializer.LoadFromFile(filePath);

            // Assert
            Assert.Equal("SecondConfig", loaded.Rule.Name);
        }

        #endregion

        #region Helper Methods

        private string CreateTempFile(string content = "")
        {
            var filePath = Path.GetTempFileName();
            _tempFiles.Add(filePath);
            if (!string.IsNullOrEmpty(content))
            {
                File.WriteAllText(filePath, content);
            }
            return filePath;
        }

        private TradeRules CreateTestConfiguration()
        {
            return new TradeRules
            {
                Rule = TradeRuleTestData.CreateValidTradeRule()
            };
        }

        private TradeRules CreateCompleteTestConfiguration()
        {
            return new TradeRules
            {
                Rule = new TradeRule
                {
                    Name = "CompleteTestStrategy",
                    CandleFrequency = "5m",
                    MinProfit = 0.75,
                    StopLoss = 0.25,
                    TakeProfit = 1.5,
                    Bankroll = new BankrollConfig
                    {
                        MaxRiskPerTrade = 0.03,
                        MinEntryAmount = 25.0
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
                        }
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

        private string CreateComplexJsonConfiguration()
        {
            return @"{
                ""Rule"": {
                    ""Name"": ""ComplexStrategy"",
                    ""CandleFrequency"": ""15m"",
                    ""MinProfit"": 1.0,
                    ""StopLoss"": 0.5,
                    ""TakeProfit"": 2.0,
                    ""Bankroll"": {
                        ""MaxRiskPerTrade"": 0.04,
                        ""MinEntryAmount"": 100.0
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
                            },
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
                                    ""CalculatorName"": ""SMA20"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": "">"",
                                ""Value"": ""close""
                            },
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""RSI14"",
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
                                ""Name"": ""SMA50"",
                                ""CalculatorName"": ""SMA"",
                                ""TechnicalIndicators"": [""MOVINGAVERAGE""],
                                ""Parameters"": [
                                    { ""Name"": ""Period"", ""Value"": ""50"" }
                                ]
                            }
                        ],
                        ""Conditions"": [
                            {
                                ""Indicator1"": {
                                    ""CalculatorName"": ""SMA50"",
                                    ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                                },
                                ""Operator"": ""<"",
                                ""Value"": ""close""
                            }
                        ]
                    }
                }
            }";
        }

        #endregion
    }
}
