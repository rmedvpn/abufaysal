using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace Faysal.Services
{
    /// <summary>
    /// Renders a Razor view to HTML string.
    /// </summary>
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync(string viewPath, object model, ViewDataDictionary viewData);
    }
}
