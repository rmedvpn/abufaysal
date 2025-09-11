using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Faysal.Helpers;

public class MixOptImgView : PageModel
{
    public IActionResult OnGet(int? id, int? p_id, int? sale_id, string size)
    {
        using var db = Database.Open("faysal");
        dynamic file = null;

 
        if ((id ?? 0) > 0)
        {
            string sql = size != "tn"
                ? "SELECT MimeType, fileContent,fileName FROM mixtureOptions WHERE id = @0"
                : "SELECT MimeType, ImageThumb,fileName AS fileContent FROM mixtureOptions WHERE id = @0";

            file = db.QuerySingleOrDefault(sql, id);
        }

        if (file != null && file.fileContent != null)
        {
            // Image exists in DB
            byte[] bytes = (byte[])file.fileContent;
            string mimeType = file.MimeType ?? "application/octet-stream";
            string fileName = file.filename ?? "image";

            if (mimeType.StartsWith("image/"))
            {
                Response.Headers["Content-Disposition"] = $"inline; filename={fileName}";
            }

            return File(bytes, mimeType);
        }
        else
        {
            // Fallback to /wwwroot/nopic.jpg
            string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images/nopic.jpg");
            if (System.IO.File.Exists(fallbackPath))
            {
                var fallbackBytes = System.IO.File.ReadAllBytes(fallbackPath);
                var contentTypeProvider = new FileExtensionContentTypeProvider();
                string fallbackMime = "image/jpeg";
                contentTypeProvider.TryGetContentType(fallbackPath, out fallbackMime);

                return File(fallbackBytes, fallbackMime);
            }

            return NotFound();
        }
    }
}
