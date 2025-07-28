# EOM Deployment Script
# Usage: powershell -ExecutionPolicy Bypass -File .\deploy.ps1 -Environment [Production|Staging] -OutputPath "C:\Publish\EOM"
# Or: .\deploy.ps1 -Environment [Production|Staging] -OutputPath "C:\Publish\EOM"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Production", "Staging")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "C:\Publish\EOM"
)

Write-Host "Deploying EOM to $Environment environment..." -ForegroundColor Green

try {
    if ($Environment -eq "Staging") {
        # Deploy for Staging (uses EOM_DIV database)
        Write-Host "Building for Staging environment..." -ForegroundColor Yellow
        dotnet publish --configuration Release --output $OutputPath -p:DeployEnvironment=Staging
        Write-Host "Staging deployment completed successfully!" -ForegroundColor Green
        Write-Host "Database: EOM_DIV" -ForegroundColor Cyan
    }
    else {
        # Deploy for Production (uses EOM database)
        Write-Host "Building for Production environment..." -ForegroundColor Yellow
        dotnet publish --configuration Release --output $OutputPath
        Write-Host "Production deployment completed successfully!" -ForegroundColor Green
        Write-Host "Database: EOM" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "Files published to: $OutputPath" -ForegroundColor Cyan
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Copy contents of $OutputPath to your IIS website folder" -ForegroundColor White
    Write-Host "2. The correct web.config has been automatically applied" -ForegroundColor White
    Write-Host "3. No manual configuration changes needed!" -ForegroundColor White
}
catch {
    Write-Host "Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}