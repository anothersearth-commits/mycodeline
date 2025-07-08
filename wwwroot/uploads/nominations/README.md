# Nominations Supporting Documents Storage

This directory contains supporting documents uploaded during manager scoring for nominations.

## File Storage Structure

```
/wwwroot/uploads/nominations/
├── [GUID].pdf         # Supporting document files
├── [GUID].pdf         # Each file is renamed with a unique GUID
└── README.md          # This documentation file
```

## File Naming Convention

- Files are renamed with a unique GUID to prevent naming conflicts
- Original file extension is preserved (.pdf)
- Example: `a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf`

## Storage Location

- **Physical Path**: `/Users/majid/Documents/EOM/wwwroot/uploads/nominations/`
- **Web Path**: `/uploads/nominations/`
- **Database Field**: `Nominations.SupportingDocPath` stores the web path

## File Upload Process

1. User uploads PDF file during manager scoring
2. File is validated (PDF only)
3. File is renamed with GUID
4. File is stored in this directory
5. Path is saved to database as `/uploads/nominations/[GUID].pdf`

## File Access

Files can be accessed via web URL:
`http://localhost:5200/uploads/nominations/[GUID].pdf`

## File Management

- Old files are automatically deleted when new files are uploaded
- Files are accessible via the web interface
- Directory is created automatically if it doesn't exist