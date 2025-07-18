# EOM System Verification Script
Write-Host "=== EOM System Verification ===" -ForegroundColor Green

# 1. System Requirements
Write-Host "`n1. System Requirements:" -ForegroundColor Yellow
$os = Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion
Write-Host "OS: $($os.WindowsProductName) $($os.WindowsVersion)"

$memory = Get-WmiObject -Class Win32_ComputerSystem
$ramGB = [math]::Round($memory.TotalPhysicalMemory/1GB,2)
Write-Host "RAM: $ramGB GB $(if($ramGB -ge 4){'✓'}else{'✗ Need 4GB+'})"

$disk = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
$freeGB = [math]::Round($disk.FreeSpace/1GB,2)
Write-Host "Free Disk Space: $freeGB GB $(if($freeGB -ge 10){'✓'}else{'✗ Need 10GB+'})"

# 2. IIS Verification
Write-Host "`n2. IIS Verification:" -ForegroundColor Yellow
try {
    $iisVersion = Get-ItemProperty "HKLM:SOFTWARE\Microsoft\InetStp\"
    Write-Host "IIS Version: $($iisVersion.MajorVersion).$($iisVersion.MinorVersion) $(if($iisVersion.MajorVersion -ge 10){'✓'}else{'✗'})"
    
    $w3svc = Get-Service W3SVC
    Write-Host "W3SVC Status: $($w3svc.Status) $(if($w3svc.Status -eq 'Running'){'✓'}else{'✗'})"
} catch {
    Write-Host "IIS not installed ✗" -ForegroundColor Red
}

# 3. .NET Verification
Write-Host "`n3. .NET Verification:" -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET Version: $dotnetVersion $(if($dotnetVersion -like '8.*'){'✓'}else{'✗'})"
    
    $aspNetCore = Get-ChildItem "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App" | Where-Object {$_.Name -like '8.*'}
    if($aspNetCore) {
        Write-Host "ASP.NET Core 8.0: ✓"
    } else {
        Write-Host "ASP.NET Core 8.0: ✗ Not found"
    }
} catch {
    Write-Host ".NET not installed ✗" -ForegroundColor Red
}

# 4. Oracle Client Verification
Write-Host "`n4. Oracle Client Verification:" -ForegroundColor Yellow
$oraclePath = $env:PATH -split ';' | Where-Object {$_ -like "*Oracle*"}
if($oraclePath) {
    Write-Host "Oracle in PATH: ✓"
    
    try {
        $sqlplusVersion = sqlplus -v 2>&1
        if($sqlplusVersion -match "Release") {
            Write-Host "SQL*Plus: ✓"
        } else {
            Write-Host "SQL*Plus: ✗"
        }
    } catch {
        Write-Host "SQL*Plus: ✗ Not found"
    }
} else {
    Write-Host "Oracle Client: ✗ Not found in PATH"
}

# 5. Network Connectivity
Write-Host "`n5. Network Connectivity:" -ForegroundColor Yellow
Write-Host "Replace 'YOUR_ORACLE_SERVER' with actual server name/IP"
# Test-NetConnection -ComputerName "YOUR_ORACLE_SERVER" -Port 1521

# 6. Required Windows Features
Write-Host "`n6. Windows Features:" -ForegroundColor Yellow
$requiredFeatures = @(
    "IIS-WebServerRole",
    "IIS-WebServer", 
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-HttpLogging",
    "IIS-RequestFiltering",
    "IIS-StaticContent"
)

foreach($feature in $requiredFeatures) {
    try {
        $featureState = Get-WindowsOptionalFeature -Online -FeatureName $feature
        $status = if($featureState.State -eq "Enabled") {"✓"} else {"✗"}
        Write-Host "$feature : $status"
    } catch {
        Write-Host "$feature : ✗ (Error checking)"
    }
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
Write-Host "Review any ✗ marks and resolve issues before proceeding with deployment." -ForegroundColor Yellow