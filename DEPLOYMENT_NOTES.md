# Deployment Notes - File Upload Security Fix

## Issue Fixed
- File upload was failing in production with `UnauthorizedAccessException` 
- Files were being stored in `wwwroot\uploads` which has security restrictions

## Changes Made

### 1. Updated Upload Path
- **Old**: `wwwroot\uploads\nominations\` 
- **New**: `C:\EOM\uploads\`

### 2. New FilesController
- Created `Controllers/FilesController.cs` to serve secured files
- Route: `/Files/Supporting/{fileName}`
- Includes authorization checks and file validation

### 3. Updated Views
- All views now use `@Url.Action("Supporting", "Files", new { fileName = ... })` instead of direct file paths
- Files: Score.cshtml, Edit.cshtml, Create.cshtml, Details.cshtml, CycleDetails.cshtml

### 4. Database Storage
- `SupportingDocPath` now stores just the filename (not full path)
- Example: `"abc123-guid.pdf"` instead of `"/uploads/nominations/abc123-guid.pdf"`

## Production Deployment Steps

### 1. Create Upload Directory
```cmd
mkdir C:\EOM\uploads
```

### 2. Set Permissions
Grant **Modify** permissions to the IIS Application Pool identity:
```cmd
icacls "C:\EOM\uploads" /grant "IIS AppPool\YourAppPoolName:(OI)(CI)M"
```

### 3. Migrate Existing Files (if any)
If there are existing files in `wwwroot\uploads\nominations\`:
1. Copy files to `C:\EOM\uploads\`
2. Update database records to store just filenames instead of full paths
3. Run this SQL to update existing records:
```sql
UPDATE NOMINATIONS 
SET SUPPORTINGDOCPATH = SUBSTR(SUPPORTINGDOCPATH, INSTR(SUPPORTINGDOCPATH, '/', -1) + 1)
WHERE SUPPORTINGDOCPATH IS NOT NULL 
AND SUPPORTINGDOCPATH LIKE '/uploads/nominations/%';
```

### 4. Deploy Application
- Deploy the updated application code
- Test file upload and download functionality

## Security Benefits
- Files are stored outside web root (not publicly accessible)
- All file access goes through FilesController with authorization checks
- File existence is validated against database records
- Only authenticated users can access files

## Testing
1. Login as a manager
2. Create a nomination and upload a PDF file
3. Verify file appears in `C:\EOM\uploads\`
4. Verify file can be downloaded from the nominations page
5. Verify unauthorized users cannot access files directly