dotnet publish --configuration Release --output "C:\Publish\EOM"

# EOM System - Windows Server 2019 IIS Deployment Guide

## Overview
This guide covers the deployment of the Employee of the Month (EOM) ASP.NET Core application on Windows Server 2019 with IIS.

## System Requirements

### Server Environment
- **Operating System**: Windows Server 2019
- **Web Server**: IIS 10.0 or higher
- **Database**: Oracle 19c
- **Framework**: .NET 8.0 Runtime

### Prerequisites
1. Windows Server 2019 with IIS role installed
2. ASP.NET Core Hosting Bundle for .NET 8.0
3. Oracle 19c Database Server
4. Oracle Data Access Components (ODAC) 19c
5. Visual C++ Redistributable (latest)

## Pre-Deployment Checklist

### 1. System Verification
- [ ] Windows Server 2019 installed and updatepd
- [ ] Minimum 4GB RAM available
- [ ] At least 10GB free disk space
- [ ] IIS 10.0 or higher installed
- [ ] .NET 8.0 Hosting Bundle installed
- [ ] Oracle Client 19c installed and configured
- [ ] Network connectivity to Oracle database server verified

### 2. Application Configuration
- [ ] Update `appsettings.json` for production environment
- [ ] Configure connection strings for production database
- [ ] Set up proper logging configuration
- [ ] Configure authentication settings
- [ ] Update file upload paths and permissions

### 3. Database Setup
- [ ] Verify existing Oracle database connectivity from server
- [ ] Test current Oracle connection string from production server
- [ ] Ensure Oracle database server allows connections from new server
- [ ] Backup existing Oracle database before deployment
- [ ] No migrations needed - using existing Oracle database and schema

### 4. Security Configuration
- [ ] Configure HTTPS certificates
- [ ] Set up proper file permissions
- [ ] Configure firewall rules
- [ ] Set up application pool identity
- [ ] Configure authentication providers

## Deployment Steps

### Step 1: Prepare the Application

#### 1.1 Update Production Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ORACLE_SERVER:1521/ORCL;User Id=EOM_USER;Password=SECURE_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Oracle.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Note**: Update the connection string with your actual Oracle server details:
- `ORACLE_SERVER`: Your Oracle server hostname/IP
- `ORCL`: Your Oracle SID or Service Name
- `EOM_USER`: Oracle database user
- `SECURE_PASSWORD`: Oracle user password

#### 1.2 Build and Publish Application
```bash
# Clean and build
dotnet clean
dotnet build --configuration Release

# Publish application
dotnet publish --configuration Release --output "C:\Publish\EOM"
```

### Step 2: Server Setup and Verification

#### 2.1 Verify System Requirements
```powershell
# Check Windows version
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion, WindowsBuildLabEx

# Check available memory
Get-WmiObject -Class Win32_ComputerSystem | Select-Object @{Name="RAM (GB)";Expression={[math]::Round($_.TotalPhysicalMemory/1GB,2)}}

# Check disk space
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, @{Name="Size (GB)";Expression={[math]::Round($_.Size/1GB,2)}}, @{Name="Free (GB)";Expression={[math]::Round($_.FreeSpace/1GB,2)}}
```

#### 2.2 Install and Verify IIS
```powershell
# Install IIS with required features
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, IIS-StaticContent, IIS-DefaultDocument, IIS-DirectoryBrowsing, IIS-NetFxExtensibility45, IIS-ISAPIExtensions, IIS-ISAPIFilter, IIS-NetFxExtensibility45, IIS-AspNet45

# Verify IIS installation
Get-WindowsFeature -Name IIS-* | Where-Object {$_.InstallState -eq 'Installed'}

# Check IIS version
Get-ItemProperty "HKLM:SOFTWARE\Microsoft\InetStp\" | Select-Object MajorVersion, MinorVersion

# Test IIS is running
Get-Service W3SVC | Select-Object Name, Status
```

#### 2.3 Install and Verify .NET 8.0 Hosting Bundle
```powershell
# Download and install .NET 8.0 Hosting Bundle
# Go to: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

# After installation, restart IIS
iisreset

# Verify .NET installation
dotnet --list-runtimes
dotnet --list-sdks
dotnet --version

# Check ASP.NET Core Module
Get-WebGlobalModule | Where-Object {$_.Name -like "*AspNetCore*"}

# Verify hosting bundle is installed
Get-ChildItem "C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App" | Select-Object Name
```

#### 2.4 Install and Verify Oracle Client
```powershell
# Download Oracle Data Access Components (ODAC) 19c from Oracle website
# Install ODAC 19c

# Verify Oracle client installation
$env:PATH -split ';' | Where-Object {$_ -like "*Oracle*"}

# Check Oracle installation directory
Get-ChildItem "C:\app\*\product\*\client_*\bin" -ErrorAction SilentlyContinue | Select-Object FullName

# Test Oracle connectivity tools
sqlplus -v
tnsping -v

# Check Oracle environment variables
Get-ChildItem Env: | Where-Object {$_.Name -like "*ORACLE*"}
```

#### 2.5 Verify Network Connectivity
```powershell
# Test Oracle database connectivity (replace with your Oracle server details)
Test-NetConnection -ComputerName "YOUR_ORACLE_SERVER" -Port 1521

# Test general network connectivity
Test-NetConnection -ComputerName "google.com" -Port 80

# Check firewall rules
Get-NetFirewallRule -DisplayName "*Oracle*" -ErrorAction SilentlyContinue
```

#### 2.6 Performance and Resource Verification
```powershell
# Check CPU information
Get-WmiObject -Class Win32_Processor | Select-Object Name, NumberOfCores, NumberOfLogicalProcessors

# Check available ports
netstat -an | findstr :80
netstat -an | findstr :443

# Check system performance
Get-Counter -Counter "\Memory\Available MBytes" -SampleInterval 1 -MaxSamples 1
Get-Counter -Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1
```

### Step 3: Configure IIS

#### 3.1 Create Application Pool
```powershell
# Create new application pool
New-WebAppPool -Name "EOM_AppPool"

# Configure application pool
Set-ItemProperty -Path "IIS:\AppPools\EOM_AppPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\EOM_AppPool" -Name "enable32BitAppOnWin64" -Value $false
Set-ItemProperty -Path "IIS:\AppPools\EOM_AppPool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
```

#### 3.2 Create Website
```powershell
# Create website
New-Website -Name "EOM" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\EOM" -ApplicationPool "EOM_AppPool"
```

#### 3.3 Configure Website Settings
- Set physical path to published application folder
- Configure default document: `index.html`
- Set up request filtering if needed
- Configure compression

### Step 4: Oracle Database Configuration

#### 4.1 Verify Existing Oracle Database Access
```bash
# Test connection to existing Oracle database from new server
# Use SQL*Plus or Oracle SQL Developer
sqlplus CURRENT_USER/CURRENT_PASSWORD@ORACLE_SERVER:1521/ORCL

# Example connection test
sqlplus EOM_USER/password@your-oracle-server:1521/ORCL
```

**Important Notes:**
- Use your current Oracle database connection details
- Test connectivity from the Windows Server 2019 machine to your Oracle server
- Ensure Oracle port 1521 is open in firewalls

#### 4.2 Configure Oracle Provider in Application
Ensure your application has the correct Oracle Entity Framework provider:

```xml
<!-- In EOM.Web.csproj -->
<PackageReference Include="Oracle.EntityFrameworkCore" Version="8.21.121" />
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="3.21.121" />
```

Update your `Program.cs` to use Oracle provider:
```csharp
// Configure Oracle connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
```

#### 4.3 Test Application Connection
```bash
# Test connection using your current connection string
# Update appsettings.json with correct Oracle connection details

# Test from application directory
cd C:\inetpub\wwwroot\EOM
dotnet ef database update --connection "Data Source=YOUR_ORACLE_SERVER:1521/ORCL;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
```

#### 4.4 Configure Oracle Client Environment
```bash
# Ensure Oracle client can connect to your Oracle server
# Configure TNS_ADMIN environment variable if using tnsnames.ora
# Update Windows PATH to include Oracle client

# Test TNS connectivity
tnsping YOUR_ORACLE_SERVICE_NAME

# If using tnsnames.ora, create entry like:
# ORCL =
#   (DESCRIPTION =
#     (ADDRESS = (PROTOCOL = TCP)(HOST = your-oracle-server)(PORT = 1521))
#     (CONNECT_DATA =
#       (SERVER = DEDICATED)
#       (SERVICE_NAME = ORCL)
#     )
#   )
```

#### 4.5 Backup Current Oracle Database
```bash
# Create backup before deployment
# Use Oracle Data Pump or traditional export

# Example using expdp (Data Pump) - Recommended
expdp EOM_USER/password@ORCL schemas=EOM_USER directory=DATA_PUMP_DIR dumpfile=EOM_backup_%date%.dmp logfile=EOM_backup_%date%.log

# Or using traditional export
exp EOM_USER/password@ORCL file=EOM_backup_%date%.dmp log=EOM_backup_%date%.log

# For full database backup (if you have DBA privileges)
expdp system/password@ORCL full=y directory=DATA_PUMP_DIR dumpfile=EOM_full_backup_%date%.dmp logfile=EOM_full_backup_%date%.log
```

### Step 5: Security Configuration

#### 5.1 File Permissions
```powershell
# Set permissions for application pool identity
icacls "C:\inetpub\wwwroot\EOM" /grant "IIS AppPool\EOM_AppPool:(OI)(CI)F"

# Set permissions for upload directories
icacls "C:\inetpub\wwwroot\EOM\wwwroot\uploads" /grant "IIS AppPool\EOM_AppPool:(OI)(CI)F"
```

#### 5.2 Configure HTTPS
1. Obtain SSL certificate
2. Install certificate in IIS
3. Configure HTTPS binding
4. Set up HTTP to HTTPS redirect

### Step 6: Testing and Validation

#### 6.1 Application Testing
- [ ] Test application startup
- [ ] Verify database connectivity
- [ ] Test user authentication
- [ ] Check file upload functionality
- [ ] Validate all major features

#### 6.2 Performance Testing
- [ ] Test application under load
- [ ] Verify memory usage
- [ ] Check response times
- [ ] Monitor database performance

## Post-Deployment Configuration

### 1. Monitoring Setup
- Configure application logging
- Set up performance monitoring
- Configure error tracking
- Set up health checks

### 2. Backup Strategy
- Database backup schedule
- Application files backup
- Configuration backup
- Recovery procedures

### 3. Maintenance Tasks
- Regular security updates
- Database maintenance
- Log file cleanup
- Performance optimization

## Troubleshooting Guide

### Common Issues

#### 1. Application Won't Start
```bash
# Check application logs
Get-EventLog -LogName Application -Source "IIS AspNetCore Module V2"

# Check application pool status
Get-IISAppPool -Name "EOM_AppPool"
```

#### 2. Oracle Database Connection Issues
- Verify Oracle connection string format
- Check Oracle listener status: `lsnrctl status`
- Validate Oracle user privileges
- Test TNS connectivity: `tnsping ORCL`
- Check Oracle client installation
- Verify firewall rules for port 1521

#### 3. File Permission Issues
- Check application pool identity
- Verify folder permissions
- Review security logs

### Diagnostic Commands
```powershell
# Check .NET runtime installation
dotnet --info

# Test application pool
Test-WebAppPool -Name "EOM_AppPool"

# Check website status
Get-Website -Name "EOM"

# View application logs
Get-EventLog -LogName Application -Newest 50
```

## Complete System Verification Script

Create a PowerShell script to verify all requirements:

### verification-script.ps1
```powershell
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
    $featureState = Get-WindowsOptionalFeature -Online -FeatureName $feature
    $status = if($featureState.State -eq "Enabled") {"✓"} else {"✗"}
    Write-Host "$feature : $status"
}

Write-Host "`n=== Verification Complete ===" -ForegroundColor Green
```

### How to use the verification script:
1. Save the script as `verification-script.ps1`
2. Run PowerShell as Administrator
3. Execute: `.\verification-script.ps1`
4. Review the output for any ✗ marks indicating missing requirements

## Performance Optimization

### 1. IIS Configuration
- Enable compression
- Configure caching
- Set up output caching
- Configure connection limits

### 2. Application Settings
- Configure connection pooling
- Set up response caching
- Optimize database queries
- Configure memory limits

### 3. Database Optimization
- Index optimization
- Query performance tuning
- Connection string optimization
- Database maintenance

## Security Considerations

### 1. Application Security
- Regular security updates
- Input validation
- Authentication hardening
- Authorization checks

### 2. Server Security
- Windows updates
- Firewall configuration
- Access controls
- Audit logging

### 3. Database Security
- User permissions
- Connection encryption
- Backup encryption
- Access monitoring

## Maintenance Schedule

### Daily Tasks
- Monitor application logs
- Check system performance
- Verify backup completion

### Weekly Tasks
- Review security logs
- Check database performance
- Update documentation

### Monthly Tasks
- Security updates
- Performance review
- Backup testing
- System maintenance

## Support and Documentation

### Contact Information
- **Developer**: Majed Omar Al-Sheizawi
- **Department**: IT Department
- **Email**: [Contact Information]

### Documentation Links
- Application user manual
- Database schema documentation
- API documentation
- Troubleshooting guide

---

**Note**: This guide assumes familiarity with Windows Server administration and IIS configuration. Always test deployment procedures in a staging environment before production deployment.