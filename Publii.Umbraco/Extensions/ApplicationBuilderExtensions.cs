using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Publii.Rules;
using Publii.Umbraco.Models;

namespace Publii.Umbraco.Extensions;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UsePubliiRouting(this IApplicationBuilder app, IConfiguration configuration)
	{
		// Retrieve the root URL segment from configuration or use the default
		var rootUrlSegment =
			configuration.GetSection("Publii").GetValue<string>(AppSettings.RootUrlSegmentName) ??
			AppSettings.DefaultRootUrlSegment;

		// Add middleware for handling missing trailing slash in specific URLs
		app.Use(AddTrailingSlashMiddleware(rootUrlSegment));

		// Configure URL rewrite rules
		var rewriteOptions = ConfigureRewriteOptions(rootUrlSegment);
		app.UseRewriter(rewriteOptions);

		return app;
	}
	
	private static Func<HttpContext, Func<Task>, Task> AddTrailingSlashMiddleware(string rootUrlSegment)
	{
		return async (context, next) =>
		{
			var requestPath = context.Request.Path;

			// Redirect if the path starts with the root segment and has exactly two segments
			if (requestPath.StartsWithSegments($"/{rootUrlSegment}") &&
			    requestPath.Value!.Count(c => c == '/') == 2)
			{
				context.Response.Redirect($"{requestPath}/", permanent: true);
				return;
			}

			await next();
		};
	}
	
	private static RewriteOptions ConfigureRewriteOptions(string rootUrlSegment)
	{
		return new RewriteOptions()
			// Allow extensionless URLs
			.AddRewrite(
				$"(?i)^{rootUrlSegment}(.*)(\\..+)$", 
				$"{rootUrlSegment}$1", 
				skipRemainingRules: true)
			// Redirect uppercase URLs to lowercase
			.Add(new RedirectLowerCaseRule(StatusCodes.Status301MovedPermanently));
	}
}