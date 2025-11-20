using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace salesngin.Extensions;

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(
        this Controller controller,
        string viewName,
        TModel model,
        bool partial = false)
    {
        if (string.IsNullOrEmpty(viewName))
            viewName = controller.ControllerContext.ActionDescriptor.ActionName;

        controller.ViewData.Model = model;

        using var writer = new StringWriter();
        var serviceProvider = controller.HttpContext.RequestServices;
        var engine = serviceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
        var viewResult = engine.FindView(controller.ControllerContext, viewName, !partial);

        if (!viewResult.Success)
        {
            throw new FileNotFoundException($"View {viewName} not found.");
        }

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return writer.GetStringBuilder().ToString();
    }

}