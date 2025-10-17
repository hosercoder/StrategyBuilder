# SMT.StrategyBuilder

A comprehensive .NET 8 library for building, validating, and executing trading strategies with technical indicators.

## Overview

SMT.StrategyBuilder provides a flexible framework for creating algorithmic trading strategies using technical analysis indicators. The library follows SOLID principles and implements a clean architecture with dependency injection, making it highly extensible and testable.

## Features

- **Strategy Engine**: Generic strategy execution engine with event-driven architecture
- **Rule-Based Configuration**: JSON-based trading rule definitions with validation
- **Technical Indicators**: Integration with SMT.TechnicalCalculators for comprehensive indicator support
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
dotnet add package SMT.StrategyBuilder
```

### Dependency Injection Setup

```csharp
using SMT.StrategyBuilder.src.Extensions;

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
- PowerShell (for build script)

### Build Commands

```powershell
# Basic build and test
.\build.ps1

# Build with code coverage
.\build.ps1 -CodeCoverage

# Create NuGet package
.\build.ps1 -CreatePackage

# Full build with coverage and package
.\build.ps1 -CodeCoverage -CreatePackage

# Debug build with coverage
.\build.ps1 -Configuration Debug -CodeCoverage
```

### Test Coverage

The project maintains high test coverage with 110+ comprehensive test methods covering:
- Unit tests for all core components
- Integration tests for end-to-end workflows
- Serialization/deserialization scenarios
- Error handling and edge cases
- Performance testing for large datasets

Current coverage targets:
- Line Coverage: >90%
- Branch Coverage: >85%
- Method Coverage: >95%

## Dependencies

- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **Microsoft.Extensions.Logging** - Logging abstraction
- **Newtonsoft.Json** - JSON serialization
- **SMT.TechnicalCalculators** - Technical indicator calculations
- **SMT.Common** - Shared models and utilities

## Project Structure

```
SMT.StrategyBuilder/
├── src/
│   ├── Evaluators/          # Strategy evaluation logic
│   ├── Extensions/          # Dependency injection extensions
│   ├── Interfaces/          # Core interfaces
│   ├── Models/              # Data models and events
│   ├── Serializers/         # JSON serialization with converters
│   ├── Strategies/          # Strategy engine implementations
│   └── Validators/          # Rule validation logic
├── Tests/                   # Comprehensive test suite
├── build.ps1               # Build and test script
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

- **1.0.6.3** - Current version with comprehensive test coverage and SOLID architecture
- **1.0.5.x** - Previous iterations with core functionality
- **1.0.0** - Initial release

## Support

For issues and questions, please use the project's issue tracker.