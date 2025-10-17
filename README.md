# HC.StrategyBuilder

A comprehensive .NET 8 library for building, validating, and executing trading strategies with technical indicators.

## Overview

HC.StrategyBuilder provides a flexible framework for creating algorithmic trading strategies using technical analysis indicators. The library follows SOLID principles and implements a clean architecture with dependency injection, making it highly extensible and testable.

## Features

- **Strategy Engine**: Generic strategy execution engine with event-driven architecture
- **Rule-Based Configuration**: JSON-based trading rule definitions with validation
- **Technical Indicators**: Integration with HC.TechnicalCalculators for comprehensive indicator support
- **Serialization**: Robust JSON serialization with custom enum converters
- **Validation**: Comprehensive rule validation with detailed error reporting
- **Dependency Injection**: Full Microsoft.Extensions.DependencyInjection support
- **Extensible Design**: Interface-based architecture following SOLID principles

## Architecture

The library is organized into several key components:

### Core Interfaces
- `IStrategyEngine` - Strategy execution and management
- `ITradeRuleConfiguration` - Configuration management
- `ITradeRuleEvaluator` - Business logic evaluation
- `ITradeRuleValidator` - Rule validation
- `ITradeRuleSerializer` - JSON serialization/deserialization

### Key Classes
- `GenericStrategyEngine` - Main strategy execution engine
- `TradeRuleConfiguration` - Configuration container with validation
- `TradeRuleEvaluator` - Strategy evaluation logic
- `TradeRuleValidator` - Rule validation implementation
- `TradeRuleSerializer` - JSON handling with custom converters

## Getting Started

### Installation

Add the package to your project:

```bash
dotnet add package HC.StrategyBuilder
```

### Dependency Injection Setup

```csharp
using HC.StrategyBuilder.src.Extensions;

// In your Program.cs or Startup.cs
services.AddStrategyConfigurationServices(configuration);
```

### Basic Usage

```csharp
// Inject the strategy engine
public class TradingService
{
    private readonly IStrategyEngine _strategyEngine;
    
    public TradingService(IStrategyEngine strategyEngine)
    {
        _strategyEngine = strategyEngine;
    }
    
    public async Task InitializeStrategy(string configPath)
    {
        await _strategyEngine.Initialize(configPath);
        
        // Subscribe to strategy signals
        _strategyEngine.OnStrategySignal += (sender, args) =>
        {
            Console.WriteLine($"Strategy signal: {args.Signal}");
        };
    }
    
    public async Task ProcessCandles(Candle[] candles)
    {
        var indicators = await _strategyEngine.CalculateIndicators(candles);
        await _strategyEngine.EvaluateStrategy("MyStrategy", candles.Last(), indicators);
    }
}
```

### Configuration Example

```json
{
  "Rule": {
    "Name": "SMA Crossover Strategy",
    "CandleFrequency": "15m",
    "MinProfit": 0.8,
    "StopLoss": 0.3,
    "TakeProfit": 1.5,
    "Bankroll": {
      "MaxRiskPerTrade": 0.025,
      "MinEntryAmount": 50.0
    },
    "BuyRule": {
      "Calculators": [
        {
          "Name": "FastSMA",
          "CalculatorName": "SMA",
          "TechnicalIndicators": ["MOVINGAVERAGE"],
          "Parameters": [
            { "Name": "Period", "Value": "10" }
          ]
        },
        {
          "Name": "SlowSMA",
          "CalculatorName": "SMA",
          "TechnicalIndicators": ["MOVINGAVERAGE"],
          "Parameters": [
            { "Name": "Period", "Value": "30" }
          ]
        }
      ],
      "Conditions": [
        {
          "Indicator1": {
            "CalculatorName": "FastSMA",
            "TechnicalIndicatorName": "MOVINGAVERAGE"
          },
          "Operator": ">",
          "Indicator2": {
            "CalculatorName": "SlowSMA",
            "TechnicalIndicatorName": "MOVINGAVERAGE"
          }
        }
      ]
    },
    "SellRule": {
      "Calculators": [
        {
          "Name": "RSI14",
          "CalculatorName": "RSI",
          "TechnicalIndicators": ["RSI"],
          "Parameters": [
            { "Name": "Period", "Value": "14" }
          ]
        }
      ],
      "Conditions": [
        {
          "Indicator1": {
            "CalculatorName": "RSI14",
            "TechnicalIndicatorName": "RSI"
          },
          "Operator": ">",
          "Value": "70"
        }
      ]
    }
  }
}
```

## Building and Testing

### Prerequisites
- .NET 8.0 SDK

### Build Commands

The project can be built using standard .NET CLI commands:

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack

# Run tests with coverage (requires coverlet.collector)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

The project maintains comprehensive test coverage with 21 test files covering:
- Unit tests for all core components
- Integration tests for end-to-end workflows
- Serialization/deserialization scenarios
- Error handling and edge cases
- Converter functionality for enum serialization

Test files include:
- Strategy engine functionality
- Trade rule evaluation logic
- Validation components
- Serialization with custom converters
- Service extension registration
- Configuration models

## Dependencies

- **Microsoft.Extensions.Configuration.Abstractions** - Configuration abstractions
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Dependency injection abstractions
- **Microsoft.Extensions.Logging.Abstractions** - Logging abstractions
- **Newtonsoft.Json** - JSON serialization
- **HC.TechnicalCalculators** - Technical indicator calculations

## Project Structure

```
HC.StrategyBuilder/
├── src/
│   ├── Evaluators/          # Strategy evaluation logic
│   ├── Extensions/          # Dependency injection extensions
│   ├── Interfaces/          # Core interfaces
│   │   ├── Evaluators/      # Evaluator interfaces
│   │   ├── Serializers/     # Serializer interfaces
│   │   └── Validators/      # Validator interfaces
│   ├── Models/              # Data models and events
│   │   ├── Common/          # Common models (Candle)
│   │   └── Events/          # Event argument models
│   ├── Serializers/         # JSON serialization with converters
│   │   └── Converters/      # Custom JSON converters
│   ├── Strategies/          # Strategy engine implementations
│   └── Validators/          # Rule validation logic
├── Tests/                   # Comprehensive test suite (21 test files)
│   ├── Evaluators/          # Evaluator tests
│   ├── Extensions/          # Extension tests
│   ├── Helpers/             # Test helpers and data
│   ├── Models/              # Model tests
│   ├── Serializers/         # Serializer and converter tests
│   ├── Strategies/          # Strategy engine tests
│   └── Validators/          # Validator tests
└── README.md               # This file
```

## SOLID Principles Compliance

This library demonstrates excellent adherence to SOLID principles:

- **Single Responsibility**: Each class has a focused, single purpose
- **Open/Closed**: Extensible through interfaces without modification
- **Liskov Substitution**: All implementations properly substitute their interfaces
- **Interface Segregation**: Well-segregated, focused interfaces
- **Dependency Inversion**: Consistent dependency injection throughout

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass and coverage remains high
5. Submit a pull request

## License

This project is part of the Silver Mustang Trading Libraries suite.

## Version History

- **1.0.6.4** - Current version with comprehensive test coverage and SOLID architecture
- **1.0.6.3** - Previous version with enhanced functionality
- **1.0.5.x** - Previous iterations with core functionality
- **1.0.0** - Initial release

## Support

For issues and questions, please use the project's issue tracker.