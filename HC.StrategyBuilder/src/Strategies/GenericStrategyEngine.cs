using HC.StrategyBuilder.src.Interfaces;
using HC.StrategyBuilder.src.Models.Common;
using HC.StrategyBuilder.src.Models.Events;
using HC.TechnicalCalculators.Src.Factory;
using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace HC.StrategyBuilder.src.Strategies
{
    public class GenericStrategyEngine : IStrategyEngine
    {
        private readonly ILogger<GenericStrategyEngine> _logger;
        private readonly ITradeRuleConfiguration _config;
        private ConcurrentDictionary<string, ITradeRuleConfiguration> _strategies = new();
        private ConcurrentDictionary<string, ITechnicalCalculator> _calculators = new();

        public event EventHandler<StrategySignalEventArgs>? OnStrategySignal;

        public GenericStrategyEngine(ILogger<GenericStrategyEngine> logger, ITradeRuleConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

        }

        public Task Initialize(string configurationPath)
        {
            try
            {
                // Initialize the configuration from the provided path
                _config.Initialize(configurationPath);

                if (!_config.Validate(out var errors))
                {
                    foreach (var error in errors)
                        _logger.LogError("Configuration validation error: {Error}", error);
                    return Task.CompletedTask;
                }

                // Register strategy
                _strategies[_config.Rule.Name] = _config;

                // Register all calculators from buy and sell rules
                RegisterCalculators(_config.Rule.BuyRule);
                RegisterCalculators(_config.Rule.SellRule);

                _logger.LogInformation("Strategy {Name} initialized successfully", _config.Rule.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing strategy from {Path}", configurationPath);
                throw;
            }

            return Task.CompletedTask;
        }

        private void RegisterCalculators(TradeSubRule rule)
        {
            if (rule?.Calculators == null) return;

            foreach (var calcConfig in rule.Calculators)
            {
                try
                {
                    var calParameters = CalculatorFactory.GetParameterConstraints(calcConfig.CalculatorName);
                    // Create parameters dictionary for the calculator
                    var parameters = new Dictionary<string, string>();
                    foreach (var param in calParameters)
                    {
                        var configCal = calcConfig.Parameters.Where(p => p.Name.ToString() == param.Key)
                            .FirstOrDefault();

                        if (configCal != null)
                        {
                            var valueType = param.Value.valueType;
                            switch (valueType)
                            {
                                case ParameterValueTypeEnum.INT:
                                    if (int.TryParse(configCal.Value, out int intValue))
                                    {
                                        parameters[param.Key] = intValue.ToString();
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"Invalid integer value for parameter {param.Key} in calculator {calcConfig.Name}");
                                    }
                                    break;
                                case ParameterValueTypeEnum.DOUBLE:
                                    if (double.TryParse(configCal.Value, out double doubleValue))
                                    {
                                        parameters[param.Key] = doubleValue.ToString();
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"Invalid double value for parameter {param.Key} in calculator {calcConfig.Name}");
                                    }
                                    break;
                            }

                        }
                    }

                    // Create calculator instance
                    var calculator = CalculatorFactory.CreateCalculator(calcConfig.CalculatorName, parameters,
                        calcConfig.Name, null);

                    _calculators[calcConfig.Name] = calculator;

                    _logger.LogDebug("Registered calculator {Name} of type {Type}",
                        calcConfig.Name, calcConfig.CalculatorName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating calculator {Name} of type {Type}",
                        calcConfig.Name, calcConfig.CalculatorName);
                }
            }
        }

        public async Task<Dictionary<string, double>> CalculateIndicators(Candle[] candles)
        {
            if (candles == null || candles.Length == 0)
            {
                _logger.LogWarning("No candles provided for indicator calculation");
                return new Dictionary<string, double>();
            }
            if (_calculators.Count == 0)
            {
                _logger.LogWarning("No calculators registered for indicator calculation");
                return new Dictionary<string, double>();
            }
            var results = new Dictionary<string, double>();

            // Convert candles to the format expected by calculators (assuming OHLCV format)
            var data = ConvertCandlesToDataArray(candles);

            foreach (var entry in _calculators)
            {
                // Calculate the indicator value using the calculator
                var calculatorResults = await Task.Run(() => entry.Value.Calculate(data));

                // Fix Problem 3: Handle CalculatorResults properly
                // Extract the main value from CalculatorResults
                if (calculatorResults != null && calculatorResults.Results.Values.Count > 0)
                {
                    // Since Values is an array, access the first element properly
                    var firstResult = calculatorResults.Results.Values.FirstOrDefault();

                    if (firstResult != null)
                    {
                        foreach (var fr in firstResult)
                        {
                            results[entry.Key] = fr.Value;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Calculator {Name} returned null or empty results", entry.Key);
                }
            }

            return results;
        }

        public Task<bool> EvaluateStrategy(string strategyName, Candle candle, Dictionary<string, double> indicators)
        {
            if (!_strategies.TryGetValue(strategyName, out var strategy))
            {
                _logger.LogWarning("Strategy {Name} not found", strategyName);
                return Task.FromResult(false);
            }

            try
            {
                // Evaluate buy rule
                bool buySignal = strategy.Evaluate(strategy.Rule.BuyRule, indicators, candle);

                // Evaluate sell rule
                bool sellSignal = strategy.Evaluate(strategy.Rule.SellRule, indicators, candle);

                // Raise event if there's a signal
                if (buySignal || sellSignal)
                {
                    OnStrategySignal?.Invoke(this, new StrategySignalEventArgs(
                        strategyName,
                        candle.ProductId,
                        buySignal,
                        sellSignal,
                        candle.Close,
                        DateTime.UtcNow));
                }

                return Task.FromResult(buySignal || sellSignal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating strategy {Name}", strategyName);
                return Task.FromResult(false);
            }
        }

        // Helper method to convert Candles to the double[,] format used by calculators
        private double[,] ConvertCandlesToDataArray(Candle[] candles)
        {
            if (candles == null)
                throw new ArgumentNullException(nameof(candles));

            if (candles.Length == 0)
                throw new ArgumentException("Candles array cannot be empty", nameof(candles));

            double[,] data = new double[candles.Length, 6];

            for (int i = 0; i < candles.Length; i++)
            {
                data[i, 0] = DateTimeOffset.FromUnixTimeMilliseconds(candles[i].Start).ToUnixTimeSeconds();
                data[i, 1] = candles[i].Open;
                data[i, 2] = candles[i].High;
                data[i, 3] = candles[i].Low;
                data[i, 4] = candles[i].Close;
                data[i, 5] = candles[i].Volume;
            }

            return data;
        }
    }
}
