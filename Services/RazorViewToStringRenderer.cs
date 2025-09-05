using Faysal.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Faysal.Services
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(string viewPath, object model, ViewDataDictionary viewData)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new ActionDescriptor(),
                viewData.ModelState
            );

            await using var sw = new StringWriter();
            var getResult = _viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: true);
            if (!getResult.Success)
                throw new FileNotFoundException($"Could not find view at {viewPath}");

            var viewContext = new ViewContext(
                actionContext,
                getResult.View,
                viewData,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await getResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
