using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;

namespace EOM.Web.Controllers;

[Authorize]
public class FilesController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string UPLOADS_PATH = @"C:\EOM\uploads";

    public FilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Files/Supporting/filename.pdf
    [HttpGet("Files/Supporting/{fileName}")]
    public async Task<IActionResult> Supporting(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }

        // Verify the file belongs to a valid nomination
        var nomination = await _context.Nominations
            .FirstOrDefaultAsync(n => n.SupportingDocPath == fileName);

        if (nomination == null)
        {
            return NotFound("File not found or not authorized");
        }

        var filePath = Path.Combine(UPLOADS_PATH, fileName);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Physical file not found");
        }

        // Determine content type
        var contentType = "application/pdf";
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        switch (extension)
        {
            case ".pdf":
                contentType = "application/pdf";
                break;
            case ".doc":
                contentType = "application/msword";
                break;
            case ".docx":
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                break;
            default:
                contentType = "application/octet-stream";
                break;
        }

        try
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            
            // Set Content-Disposition to inline for PDFs to display in browser
            if (extension == ".pdf")
            {
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                return File(fileBytes, contentType);
            }
            else
            {
                // For non-PDF files, force download
                return File(fileBytes, contentType, fileName);
            }
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error serving file {fileName}: {ex.Message}");
            return StatusCode(500, "Error reading file");
        }
    }
}