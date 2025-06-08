using System.Text.RegularExpressions;

namespace Publii.Umbraco.Extensions;

public static partial class StringExtensions
{
	public static string MakeValidName(this string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "-";

        return name.Length > 200 
            ? name.Substring(0, 200)
            : name;
    }

    private static readonly Regex InvalidCharactersRegex = MyRegex();
    
    public static string MakeValidUrlSegment(this string? urlSegment)
    {
        if (string.IsNullOrWhiteSpace(urlSegment))
            throw new Exception("Unable to make valid url segment.");

        var cleanedUrlSegment = urlSegment.Replace(" ", "-");
        cleanedUrlSegment = InvalidCharactersRegex.Replace(cleanedUrlSegment, "");
        cleanedUrlSegment = cleanedUrlSegment.Trim('-');
        return cleanedUrlSegment;
    }

    public static string MakeValidDescription(this string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;
        return description.Length > 2000 
            ? description[..2000] 
            : description;
    }
    
    public static string GetUrlFileExtension(this string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;
        var fakePath = Path.Combine(Path.GetTempPath(), "fakeDirectory", url.TrimStart('/'));
        return Path.GetExtension(fakePath).TrimStart('.');
    }

    public static string GetMimeType(this string extension)
    {
        string mimeType;
        
        switch (extension.ToLower())
        {
            case "html":
                mimeType = "text/html";
                break;
            case "htm":
                mimeType = "text/html";
                break;
            case "txt":
                mimeType = "text/plain";
                break;
            case "css":
                mimeType = "text/css";
                break;
            case "js":
                mimeType = "application/javascript";
                break;
            case "json":
                mimeType = "application/json";
                break;
            case "xml":
                mimeType = "application/xml";
                break;
            case "jpg":
            case "jpeg":
                mimeType = "image/jpeg";
                break;
            case "png":
                mimeType = "image/png";
                break;
            case "gif":
                mimeType = "image/gif";
                break;
            case "bmp":
                mimeType = "image/bmp";
                break;
            case "svg":
                mimeType = "image/svg+xml";
                break;
            case "pdf":
                mimeType = "application/pdf";
                break;
            case "doc":
                mimeType = "application/msword";
                break;
            case "docx":
                mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                break;
            case "xls":
                mimeType = "application/vnd.ms-excel";
                break;
            case "xlsx":
                mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                break;
            case "ppt":
                mimeType = "application/vnd.ms-powerpoint";
                break;
            case "pptx":
                mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                break;
            case "csv":
                mimeType = "text/csv";
                break;
            case "woff":
                mimeType = "font/woff";
                break;
            case "woff2":
                mimeType = "font/woff2";
                break;
            case "ttf":
                mimeType = "font/ttf";
                break;
            case "otf":
                mimeType = "font/otf";
                break;
            default:
                mimeType = "application/octet-stream";
                break;
        }
        
        return mimeType;
    }

    public static string? GetSubstringBeforeSlash(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;
        
        var index = str.IndexOf('/');
        
        return index != -1 
            ? str.Substring(0, index) 
            : str;
    }

    [GeneratedRegex("[^a-z0-9-]", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex();
}