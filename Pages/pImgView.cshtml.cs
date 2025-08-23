using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using Dapper;
using Faysal.Helpers;   // your Database.Open shim

public class pImgViewModel : PageModel
{
    public IActionResult OnGet(int? id, int? strain_id, string size)
    {
        using var db = Database.Open("faysal");
        dynamic file = null;
        bool tn = string.Equals(size, "tn", StringComparison.OrdinalIgnoreCase);

        // 1) Lookup by File_Id
        if ((id ?? 0) > 0)
        {
            var sql = tn
                ? "SELECT MimeType, ImageThumb AS fileContent, filename FROM StrainImages WHERE File_Id=@0 AND is_deleted=0"
                : "SELECT MimeType, fileContent AS fileContent, filename FROM StrainImages WHERE File_Id=@0 AND is_deleted=0";
            file = db.QuerySingleOrDefault(sql, id);
        }

        // 2) Lookup by strain_id (prefer primary; else latest)
        if (file == null && (strain_id ?? 0) > 0)
        {
            var sql = tn
                ? @"SELECT TOP 1 MimeType, ImageThumb AS fileContent, filename
                    FROM StrainImages
                    WHERE strain_id=@0 AND is_deleted=0
                    ORDER BY is_primary DESC, [timestamp] DESC"
                : @"SELECT TOP 1 MimeType, fileContent AS fileContent, filename
                    FROM StrainImages
                    WHERE strain_id=@0 AND is_deleted=0
                    ORDER BY is_primary DESC, [timestamp] DESC";
            file = db.QuerySingleOrDefault(sql, strain_id);
        }

        // 3) Serve DB image if found
        if (file != null && file.fileContent != null)
        {
            byte[] bytes = (byte[])file.fileContent;
            string mimeType = file.MimeType ?? "image/jpeg";
            string fileName = file.filename ?? "image.jpg";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"";
            return File(bytes, mimeType);
        }

        // 4) Fallback to /wwwroot/images/nopic.jpg
        string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "nopic.jpg");
        if (System.IO.File.Exists(fallbackPath))
        {
            var fallbackBytes = System.IO.File.ReadAllBytes(fallbackPath);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fallbackPath, out var fallbackMime))
                fallbackMime = "image/jpeg";

            Response.Headers["Content-Disposition"] = "inline; filename=\"nopic.jpg\"";
            return File(fallbackBytes, fallbackMime);
        }

        // If even the fallback is missing
        return NotFound();
    }
}
