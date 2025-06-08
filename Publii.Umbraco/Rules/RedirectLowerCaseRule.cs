using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Publii.Rules;

public class RedirectLowerCaseRule(int statusCode) : IRule
{
	private readonly string[] _reservedPaths =
	{
		"/umbraco", "/install", "/api", "/media", "/App_Plugins", "/css", "/scripts", "/static", "/lib/"
	};
	
	private int StatusCode { get; } = statusCode;

	public void ApplyRule(RewriteContext context)
	{
		var request = context.HttpContext.Request;
		var path = context.HttpContext.Request.Path;
		var host = context.HttpContext.Request.Host;

		if (path.HasValue
		    && !Path.HasExtension(path.Value) // Check if the path is not a file
		    && !_reservedPaths.Any(x => path.Value.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
		    && path.Value!.Any(char.IsUpper)
		    || host.HasValue && host.Value.Any(char.IsUpper))
		{
			var response = context.HttpContext.Response;
			response.StatusCode = StatusCode;
			response.Headers[HeaderNames.Location] =
				(request.Scheme + Uri.SchemeDelimiter + host.Value + request.PathBase.Value + request.Path.Value)
				.ToLower() + request.QueryString;
			context.Result = RuleResult.EndResponse;
		}
		else
		{
			context.Result = RuleResult.ContinueRules;
		}
	}
}