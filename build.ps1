# Build and Test Script for SMT.StrategyBuilder
# This script mimics the pipeline steps for local testing

param(
    [Parameter(HelpMessage="Build configuration (Debug/Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(HelpMessage="Skip tests")]
    [switch]$SkipTests,
    
    [Parameter(HelpMessage="Show code coverage analysis")]
    [switch]$ShowCoverage,
    
    [Parameter(HelpMessage="Create NuGet package")]
    [switch]$CreatePackage,
    
    [Parameter(HelpMessage="Generate code coverage report")]
    [switch]$CodeCoverage,
    
    [Parameter(HelpMessage="Output directory for packages")]
    [string]$OutputDir = ".\packages",
    
    [Parameter(HelpMessage="Output directory for coverage reports")]
    [string]$CoverageDir = ".\coverage"
)

# Function to parse Cobertura XML coverage file
function Parse-CoverageFile {
    param([string]$FilePath)
    
    try {
        [xml]$xml = Get-Content $FilePath
        $coverage = $xml.coverage
        
        if ($coverage) {
            # Calculate percentages from Cobertura format
            $lineRate = [double]$coverage.'line-rate'
            $branchRate = [double]$coverage.'branch-rate'
            
            # Get method and class coverage from packages
            $totalMethods = 0
            $coveredMethods = 0
            $totalClasses = 0
            $coveredClasses = 0
            
            foreach ($package in $coverage.packages.package) {
                foreach ($class in $package.classes.class) {
                    $totalClasses++
                    $classLineCoverage = [double]$class.'line-rate'
                    if ($classLineCoverage -gt 0) { $coveredClasses++ }
                    
                    foreach ($method in $class.methods.method) {
                        $totalMethods++
                        $methodLineCoverage = [double]$method.'line-rate'
                        if ($methodLineCoverage -gt 0) { $coveredMethods++ }
                    }
                }
            }
            
            $methodCoverage = if ($totalMethods -gt 0) { [math]::Round(($coveredMethods / $totalMethods) * 100, 2) } else { 0 }
            $classCoverage = if ($totalClasses -gt 0) { [math]::Round(($coveredClasses / $totalClasses) * 100, 2) } else { 0 }
            
            return @{
                LineCoverage = [math]::Round($lineRate * 100, 2)
                BranchCoverage = [math]::Round($branchRate * 100, 2)
                MethodCoverage = $methodCoverage
                ClassCoverage = $classCoverage
            }
        }
    }
    catch {
        Write-Warning "Failed to parse coverage file: $($_.Exception.Message)"
        return $null
    }
    
    return $null
}

# Script variables
$SolutionPath = "SMT.StrategyBuilder.sln"
$ProjectPath = "SMT.StrategyBuilder\SMT.StrategyBuilder.csproj"
$TestProjectPath = "SMT.StrategyBuilder.Tests\SMT.StrategyBuilder.Tests.csproj"

Write-Host "SMT.StrategyBuilder Build Script" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Show usage examples if no parameters provided
if (-not $SkipTests -and -not $CreatePackage -and -not $CodeCoverage) {
    Write-Host "`nUsage Examples:" -ForegroundColor Cyan
    Write-Host "  .\build.ps1                                    # Basic build and test" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -SkipTests                         # Build only, skip tests" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -CreatePackage                     # Build, test, and create package" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -CodeCoverage                      # Run with code coverage" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -CodeCoverage -CreatePackage       # Full build with coverage" -ForegroundColor Gray
    Write-Host "  .\build.ps1 -Configuration Debug -CodeCoverage # Debug build with coverage" -ForegroundColor Gray
    Write-Host ""
}

# Check if solution file exists
if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution file not found: $SolutionPath"
    exit 1
}

try {
    # Step 1: Restore NuGet packages
    Write-Host "`nRestoring NuGet packages..." -ForegroundColor Cyan
    dotnet restore $SolutionPath
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }

    # Step 2: Build solution
    Write-Host "`nBuilding solution..." -ForegroundColor Cyan
    if ($SkipTests) {
        # Build only the main project if tests are skipped
        dotnet build $ProjectPath --configuration $Configuration --no-restore
    } else {
        # Build the entire solution including tests
        dotnet build $SolutionPath --configuration $Configuration --no-restore
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Step 3: Run tests (if not skipped)
    if (-not $SkipTests) {
        Write-Host "`nRunning tests..." -ForegroundColor Cyan
        
        # Check if test project exists
        if (Test-Path $TestProjectPath) {
            # Prepare coverage arguments
            $testArgs = @(
                $TestProjectPath
                "--configuration", $Configuration
                "--no-build"
                "--logger", "console;verbosity=normal"
            )
            
            if ($CodeCoverage) {
                # Create coverage directory if it doesn't exist
                if (-not (Test-Path $CoverageDir)) {
                    New-Item -ItemType Directory -Path $CoverageDir -Force | Out-Null
                }
                
                # Add coverage collection arguments
                $testArgs += @(
                    "--collect", "XPlat Code Coverage"
                    "--results-directory", $CoverageDir
                    "--", "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura"
                )
                
                Write-Host "Code coverage collection enabled..." -ForegroundColor Gray
            }
            
            dotnet test @testArgs
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Some tests failed, but continuing..."
            }
            
            # Process coverage results if collected
            if ($CodeCoverage) {
                Write-Host "`nProcessing code coverage..." -ForegroundColor Cyan
                $coverageFiles = Get-ChildItem -Path $CoverageDir -Filter "coverage.cobertura.xml" -Recurse
                
                if ($coverageFiles.Count -gt 0) {
                    foreach ($coverageFile in $coverageFiles) {
                        $coverageData = Parse-CoverageFile -FilePath $coverageFile.FullName
                        if ($coverageData) {
                            Write-Host "`nCode Coverage Summary:" -ForegroundColor Green
                            Write-Host "  Line Coverage: $($coverageData.LineCoverage)%" -ForegroundColor White
                            Write-Host "  Branch Coverage: $($coverageData.BranchCoverage)%" -ForegroundColor White
                            Write-Host "  Method Coverage: $($coverageData.MethodCoverage)%" -ForegroundColor White
                            Write-Host "  Class Coverage: $($coverageData.ClassCoverage)%" -ForegroundColor White
                            
                            # Store coverage for summary
                            $script:CoverageResults = $coverageData
                        }
                    }
                } else {
                    Write-Warning "No coverage files found in $CoverageDir"
                }
            }
        } else {
            Write-Warning "Test project not found: $TestProjectPath"
        }
    } else {
        Write-Host "`nSkipping tests..." -ForegroundColor Yellow
    }

    # Step 4: Create NuGet package (if requested)
    if ($CreatePackage) {
        Write-Host "`nCreating NuGet package..." -ForegroundColor Cyan
        
        # Create output directory if it doesn't exist
        if (-not (Test-Path $OutputDir)) {
            New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
        }
        
        dotnet pack $ProjectPath --configuration $Configuration --output $OutputDir --no-build
        if ($LASTEXITCODE -ne 0) {
            throw "Package creation failed"
        }
        
        # List created packages
        $packages = Get-ChildItem -Path $OutputDir -Filter "*.nupkg" | Sort-Object CreationTime -Descending
        if ($packages.Count -gt 0) {
            Write-Host "`nPackages created:" -ForegroundColor Green
            foreach ($package in $packages) {
                Write-Host "  - $($package.Name)" -ForegroundColor White
            }
        }
    }

    # Step 5: Security checks
    Write-Host "`nRunning security checks..." -ForegroundColor Cyan
    
    Write-Host "Checking for vulnerable packages..." -ForegroundColor Gray
    dotnet list $SolutionPath package --vulnerable --include-transitive
    
    Write-Host "`nChecking for outdated packages..." -ForegroundColor Gray
    dotnet list $SolutionPath package --outdated

    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "`nBuild failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Display summary
Write-Host "`nBuild Summary:" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor White
Write-Host "  Tests: $(if ($SkipTests) { 'Skipped' } else { 'Executed' })" -ForegroundColor White
Write-Host "  Package: $(if ($CreatePackage) { 'Created' } else { 'Not Created' })" -ForegroundColor White
Write-Host "  Code Coverage: $(if ($CodeCoverage -and -not $SkipTests) { 'Collected' } else { 'Not Collected' })" -ForegroundColor White

if ($CreatePackage -and (Test-Path $OutputDir)) {
    $packageCount = (Get-ChildItem -Path $OutputDir -Filter "*.nupkg").Count
    Write-Host "  Package Count: $packageCount" -ForegroundColor White
}

# Display coverage summary if available
if ($script:CoverageResults) {
    Write-Host "`nFinal Coverage Summary:" -ForegroundColor Magenta
    Write-Host "  Line Coverage: $($script:CoverageResults.LineCoverage)%" -ForegroundColor $(if ($script:CoverageResults.LineCoverage -ge 80) { 'Green' } elseif ($script:CoverageResults.LineCoverage -ge 60) { 'Yellow' } else { 'Red' })
    Write-Host "  Branch Coverage: $($script:CoverageResults.BranchCoverage)%" -ForegroundColor $(if ($script:CoverageResults.BranchCoverage -ge 80) { 'Green' } elseif ($script:CoverageResults.BranchCoverage -ge 60) { 'Yellow' } else { 'Red' })
    Write-Host "  Method Coverage: $($script:CoverageResults.MethodCoverage)%" -ForegroundColor $(if ($script:CoverageResults.MethodCoverage -ge 80) { 'Green' } elseif ($script:CoverageResults.MethodCoverage -ge 60) { 'Yellow' } else { 'Red' })
    Write-Host "  Class Coverage: $($script:CoverageResults.ClassCoverage)%" -ForegroundColor $(if ($script:CoverageResults.ClassCoverage -ge 80) { 'Green' } elseif ($script:CoverageResults.ClassCoverage -ge 60) { 'Yellow' } else { 'Red' })
    
    # Coverage quality assessment
    $overallCoverage = ($script:CoverageResults.LineCoverage + $script:CoverageResults.BranchCoverage) / 2
    $coverageGrade = if ($overallCoverage -ge 90) { "Excellent" } 
                    elseif ($overallCoverage -ge 80) { "Good" }
                    elseif ($overallCoverage -ge 70) { "Fair" }
                    elseif ($overallCoverage -ge 60) { "Poor" }
                    else { "Very Poor" }
    
    Write-Host "  Overall Grade: $coverageGrade ($([math]::Round($overallCoverage, 1))%)" -ForegroundColor $(if ($overallCoverage -ge 80) { 'Green' } elseif ($overallCoverage -ge 60) { 'Yellow' } else { 'Red' })
}

# Show coverage analysis if requested
if ($ShowCoverage) {
    Write-Host "`n$('='*60)" -ForegroundColor Cyan
    Write-Host "RUNNING COVERAGE ANALYSIS" -ForegroundColor Cyan
    Write-Host "$('='*60)" -ForegroundColor Cyan
    
    & ".\coverage.ps1"
}

Write-Host "`nAll done!" -ForegroundColor Green
