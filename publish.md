# EOM Deployment Commands

## For Staging Environment
```bash
dotnet publish --configuration Release --output "C:\Publish\Staging" -p:StagingBuild=true
```
- Uses EOM_DIV database
- Sets ASPNETCORE_ENVIRONMENT=Staging

## For Production Environment
```bash
dotnet publish --configuration Release --output "C:\Publish\EOM"
```
- Uses EOM database
- Sets ASPNETCORE_ENVIRONMENT=Production

## Next Steps After Publish

1. **For Staging**: Copy contents of `C:\Publish\Staging` to your IIS staging website folder
2. **For Production**: Copy contents of `C:\Publish\EOM` to your IIS production website folder
3. The correct web.config has been automatically applied
4. No manual configuration changes needed!