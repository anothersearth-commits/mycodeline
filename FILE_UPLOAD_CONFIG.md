# File Upload Configuration

## Issue Fixed
- "Request entity is too large" error when uploading PDF files
- Default ASP.NET Core limits are too small for larger files

## Changes Made

### 1. Program.cs Configuration
Added upload limits configuration:
- **IIS**: 50 MB request body size
- **Kestrel**: 50 MB request body size  
- **Forms**: 50 MB multipart body length

### 2. web.config for IIS
Created web.config with:
- `maxAllowedContentLength="52428800"` (50 MB in bytes)
- Handles IIS-level request filtering

### 3. Controller Attributes
Added `[RequestSizeLimit(50 * 1024 * 1024)]` to:
- `NominationsController.Create` POST action
- `NominationsController.Score` POST action

### 4. PDF Display in Browser
Modified `FilesController.Supporting()` to:
- Set `Content-Disposition: inline` for PDF files (display in browser)
- Set `Content-Disposition: attachment` for other files (download)

## File Size Limits

| Component | Limit | Purpose |
|-----------|-------|---------|
| IIS Server | 50 MB | Server-level request filtering |
| Kestrel | 50 MB | ASP.NET Core server limits |
| Forms | 50 MB | Multipart form data limits |
| Actions | 50 MB | Per-action request limits |

## Deployment Notes

### For IIS Production:
1. Ensure `web.config` is deployed with the application
2. IIS will automatically use the `maxAllowedContentLength` setting
3. No additional IIS configuration needed

### For Kestrel (Self-hosted):
- Configuration in `Program.cs` handles all limits
- No additional server configuration needed

### Testing Upload Limits:
1. Try uploading a PDF file < 10 MB (should work)
2. Try uploading a PDF file 20-30 MB (should work)
3. Try uploading a file > 50 MB (should show appropriate error)

## Error Messages

| Scenario | Error | Solution |
|----------|-------|----------|
| File > 50 MB | "Request entity is too large" | Increase limits or ask user to compress file |
| IIS not configured | 404.13 error | Deploy web.config with maxAllowedContentLength |
| Timeout on large files | Request timeout | Consider increasing timeout limits |

## Browser Behavior
- **PDF files**: Display inline in browser (can still be downloaded via browser controls)
- **Other files**: Prompt for download
- **Large files**: Show upload progress (handled by browser)