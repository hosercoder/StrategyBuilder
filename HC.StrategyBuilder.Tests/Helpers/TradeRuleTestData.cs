namespace HC.StrategyBuilder.Tests.Helpers
{
    using HC.TechnicalCalculators.Src.Models;

    public static class TradeRuleTestData
    {
        /// <summary>
        /// Creates a valid TradeRule with all required properties set
        /// </summary>
        public static TradeRule CreateValidTradeRule()
        {
            return new TradeRule
            {
                Name = "TestStrategy",
                CandleFrequency = "1m",
                MinProfit = 0.5,
                StopLoss = 0.2,
                TakeProfit = 1.0,
                Bankroll = new BankrollConfig
                {
                    MaxRiskPerTrade = 0.02,
                    MinEntryAmount = 10.0
                },
                BuyRule = CreateValidTradeSubRule(),
                SellRule = CreateValidTradeSubRule()
            };
        }

        /// <summary>
        /// Creates a valid TradeSubRule with all required properties set
        /// </summary>
        public static TradeSubRule CreateValidTradeSubRule()
        {
            return new TradeSubRule
            {
                Calculators = new List<CalculatorConfig>
                {
                    CreateValidCalculatorConfig()
                },
                Conditions = new List<ConditionConfig>
                {
                    new ConditionConfig
                    {
                        Indicator1 = new Indicator
                        {
                            CalculatorName = "SMA20",
                            TechnicalIndicatorName = TechnicalNamesEnum.MOVINGAVERAGE
                        },
                        Operator = ">",
                        Value = "close"
                    }
                }
            };
        }

        /// <summary>
        /// Creates a valid CalculatorConfig with all required properties set
        /// </summary>
        public static CalculatorConfig CreateValidCalculatorConfig()
        {
            return new CalculatorConfig
            {
                Name = "SMA20",
                CalculatorName = CalculatorNameEnum.SMA,
                TechnicalIndicators = new TechnicalNamesEnum[] { TechnicalNamesEnum.MOVINGAVERAGE },
                Parameters = new List<CalculatorParameter>
                {
                    new CalculatorParameter
                    {
                        Name = ParameterNamesEnum.Period,
                        Value = "20"
                    }
                }
            };
        }

        public static string ValidBasicTradeRuleJson = @"
            {
                ""Rule"": {
                ""Name"": ""BasicStrategy"",
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
                        ""Name"": ""SMA20"",
                        ""CalculatorName"": ""SMA"",
                        ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                        ],
                        ""Parameters"": [
                        {
                            ""Name"": ""period"",
                            ""Value"": ""20""
                        }
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
                        ""Name"": ""SMA20"",
                        ""CalculatorName"": ""SMA"",
                        ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                        ],
                        ""Parameters"": [
                        {
                            ""Name"": ""period"",
                            ""Value"": ""20""
                        }
                        ]
                    }
                    ],
                    ""Conditions"": [
                    {
                        ""Indicator1"": {
                        ""CalculatorName"": ""SMA20"",
                        ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                        },
                        ""Operator"": ""<"",
                        ""Value"": ""close""
                    }
                    ]
                }
            }
        }";
        public static string ValidTradeRuleWithMultipleIndicatorsJson = @"
           {
              ""Rule"": {
                ""Name"": ""AdvancedRSI_SMA_Strategy"",
                ""CandleFrequency"": ""5m"",
                ""MinProfit"": 1.0,
                ""StopLoss"": 0.5,
                ""TakeProfit"": 2.0,
                ""Bankroll"": {
                  ""MaxRiskPerTrade"": 0.03,
                  ""MinEntryAmount"": 25.0
                },
                ""BuyRule"": {
                  ""Calculators"": [
                    {
                      ""Name"": ""RSI14"",
                      ""CalculatorName"": ""RSI"",
                      ""TechnicalIndicators"": [
                        ""RSI""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""14""
                        }
                      ]
                    },
                    {
                      ""Name"": ""SMA50"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""50""
                        }
                      ]
                    }
                  ],
                  ""Conditions"": [
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""RSI14"",
                        ""TechnicalIndicatorName"": ""RSI""
                      },
                      ""Operator"": ""<"",
                      ""Value"": ""30""
                    },
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""SMA50"",
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
                      ""TechnicalIndicators"": [
                        ""RSI""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""14""
                        }
                      ]
                    },
                    {
                      ""Name"": ""SMA50"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""50""
                        }
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
                    },
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
        public static string ValidTradeRuleWithIndicatorComparisons = @"
            {
              ""Rule"": {
                ""Name"": ""ComplexCrossoverStrategy"",
                ""CandleFrequency"": ""15m"",
                ""MinProfit"": 0.8,
                ""StopLoss"": 0.3,
                ""TakeProfit"": 1.5,
                ""Bankroll"": {
                  ""MaxRiskPerTrade"": 0.025,
                  ""MinEntryAmount"": 50.0
                },
                ""BuyRule"": {
                  ""Calculators"": [
                    {
                      ""Name"": ""SMA_Fast"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""10""
                        }
                      ]
                    },
                    {
                      ""Name"": ""SMA_Slow"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""30""
                        }
                      ]
                    },
                    {
                      ""Name"": ""MACD"",
                      ""CalculatorName"": ""MACD"",
                      ""TechnicalIndicators"": [
                        ""MACD"",
                        ""MACDSIGNAL"",
                        ""MACDHIST""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""fastPeriod"",
                          ""Value"": ""12""
                        },
                        {
                          ""Name"": ""slowPeriod"",
                          ""Value"": ""26""
                        },
                        {
                          ""Name"": ""signalPeriod"",
                          ""Value"": ""9""
                        }
                      ]
                    }
                  ],
                  ""Conditions"": [
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""SMA_Fast"",
                        ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                      },
                      ""Operator"": "">"",
                      ""Indicator2"": {
                        ""CalculatorName"": ""SMA_Slow"",
                        ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                      }
                    },
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""MACD"",
                        ""TechnicalIndicatorName"": ""MACD""
                      },
                      ""Operator"": "">"",
                      ""Indicator2"": {
                        ""CalculatorName"": ""MACD"",
                        ""TechnicalIndicatorName"": ""MACDSIGNAL""
                      }
                    },
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""MACD"",
                        ""TechnicalIndicatorName"": ""MACDHIST""
                      },
                      ""Operator"": "">"",
                      ""Value"": ""0""
                    }
                  ]
                },
                ""SellRule"": {
                  ""Calculators"": [
                    {
                      ""Name"": ""SMA_Fast"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""10""
                        }
                      ]
                    },
                    {
                      ""Name"": ""SMA_Slow"",
                      ""CalculatorName"": ""SMA"",
                      ""TechnicalIndicators"": [
                        ""MOVINGAVERAGE""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""period"",
                          ""Value"": ""30""
                        }
                      ]
                    },
                    {
                      ""Name"": ""MACD"",
                      ""CalculatorName"": ""MACD"",
                      ""TechnicalIndicators"": [
                        ""MACD"",
                        ""MACDSIGNAL"",
                        ""MACDHIST""
                      ],
                      ""Parameters"": [
                        {
                          ""Name"": ""fastPeriod"",
                          ""Value"": ""12""
                        },
                        {
                          ""Name"": ""slowPeriod"",
                          ""Value"": ""26""
                        },
                        {
                          ""Name"": ""signalPeriod"",
                          ""Value"": ""9""
                        }
                      ]
                    }
                  ],
                  ""Conditions"": [
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""SMA_Fast"",
                        ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                      },
                      ""Operator"": ""<"",
                      ""Indicator2"": {
                        ""CalculatorName"": ""SMA_Slow"",
                        ""TechnicalIndicatorName"": ""MOVINGAVERAGE""
                      }
                    },
                    {
                      ""Indicator1"": {
                        ""CalculatorName"": ""MACD"",
                        ""TechnicalIndicatorName"": ""MACD""
                      },
                      ""Operator"": ""<"",
                      ""Indicator2"": {
                        ""CalculatorName"": ""MACD"",
                        ""TechnicalIndicatorName"": ""MACDSIGNAL""
                      }
                    }
                  ]
                }
            }
        }";
    }
}
