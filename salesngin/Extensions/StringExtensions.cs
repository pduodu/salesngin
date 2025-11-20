namespace salesngin.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }

    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    // Extension method for string type
    public static bool IsValidEmail(this string email)
    {
        return email.Contains("@") && email.Contains(".");
    }

    public static string Truncate(this string value, int maxLength)
    {
        // if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        //     return value;
        // return value.Substring(0, maxLength) + "...";

        return value?.Length > maxLength ? value.Substring(0, maxLength) + "..." : value;
    }
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Convert to lower case and replace spaces with hyphens
        return value.ToLowerInvariant().Replace(" ", "-");
    }
    public static string ToSafeFileName(this string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        // Remove invalid characters and replace spaces with underscores
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
    }
    public static string ToSafePath(this string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Remove invalid characters and replace spaces with underscores
        var invalidChars = System.IO.Path.GetInvalidPathChars();
        return string.Concat(path.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
    }
    public static string ToSafeUrl(this string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        // Remove invalid characters and replace spaces with hyphens
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        return string.Concat(url.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "-");
    }
    public static string ToSafeHtml(this string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        // Remove script tags and other unsafe content
        return System.Text.RegularExpressions.Regex.Replace(html, "<script.*?>.*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
    public static string ToSafeJson(this string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        // Remove any unsafe characters or patterns
        return System.Text.RegularExpressions.Regex.Replace(json, @"[^\w\s\{\}\[\],:]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
    public static string ToSafeXml(this string xml)
    {
        if (string.IsNullOrEmpty(xml))
            return xml;

        // Remove any unsafe characters or patterns
        return System.Text.RegularExpressions.Regex.Replace(xml, @"[^\w\s\{\}\[\]<>\/:]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
    public static string ToSafeCsv(this string csv)
    {
        if (string.IsNullOrEmpty(csv))
            return csv;

        // Remove any unsafe characters or patterns
        return System.Text.RegularExpressions.Regex.Replace(csv, @"[^\w\s,]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
    // public static string ToSafeMarkdown(this string markdown)
    // {
    //     if (string.IsNullOrEmpty(markdown))
    //         return markdown;

    //     // Remove any unsafe characters or patterns
    //     return System.Text.RegularExpressions.Regex.Replace(markdown, @"[^\w\s\*\-\_\[\]\(\)\\`]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    // }
    // public static string ToSafeYaml(this string yaml)
    // {
    //     if (string.IsNullOrEmpty(yaml))
    //         return yaml;

    //     // Remove any unsafe characters or patterns
    //     return System.Text.RegularExpressions.Regex.Replace(yaml, @"[^\w\s\:\-\_\[\]\{\}]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    // }
    public static string ToSafeXmlAttribute(this string attribute)
    {
        if (string.IsNullOrEmpty(attribute))
            return attribute;

        // Remove any unsafe characters or patterns
        return System.Text.RegularExpressions.Regex.Replace(attribute, @"[^\w\s\-]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

}

