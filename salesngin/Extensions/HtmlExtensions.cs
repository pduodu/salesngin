namespace salesngin.Extensions;

public static class HtmlExtensions
{
    //public static IHtmlContent IsActive(this IHtmlHelper htmlHelper,string controller,string action,string cssClass = "")
    public static IHtmlContent IsActive(this IHtmlHelper htmlHelper, string[] controllers, string[] actions, string cssClass = "")
    {
        var currentController = htmlHelper.ViewContext.RouteData.Values["controller"].ToString();
        var currentAction = htmlHelper.ViewContext.RouteData.Values["action"].ToString();

        //if (controller == currentController && (action == currentAction || currentAction == "Details" || currentAction == "Create" || currentAction == "Edit"))
        //{
        //    return new HtmlString(cssClass);
        //}

        if (controllers.Contains(currentController) && actions.Contains(currentAction))
        {
            return new HtmlString(cssClass);
        }

        return HtmlString.Empty;
    }

    public static IHtmlContent FormatRelativeTime(this IHtmlHelper htmlHelper, DateTime dateTime)
    {
        TimeSpan timeAgo = DateTime.Now - dateTime;
        if (timeAgo.TotalSeconds < 60)
        {
            return new HtmlString("just now");
        }
        if (timeAgo.TotalMinutes < 60)
        {
            int minutes = (int)timeAgo.TotalMinutes;
            return new HtmlString($"{minutes} minute{(minutes != 1 ? "s" : "")} ago");
        }
        if (timeAgo.TotalHours < 24)
        {
            int hours = (int)timeAgo.TotalHours;
            return new HtmlString($"{hours} hour{(hours != 1 ? "s" : "")} ago");
        }
        // Add more conditions for days, weeks, months, etc., as needed
        return new HtmlString(dateTime.ToString("MM/dd/yyyy HH:mm:ss"));
    }
}

