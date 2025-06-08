using Microsoft.Extensions.Options;
using Publii.Umbraco.Services.Interfaces;
using Publii.Umbraco.Settings;

namespace Publii.Umbraco.Services;

public class PubliiUrlService(IOptions<PubliiAppSettings> publiiAppSettings) : IPubliiUrlService
{
	private readonly PubliiAppSettings _publiiAppSettings = publiiAppSettings.Value;

	public List<string>? GetUrlSegments(string requestUrl, out string? publicationSegment)
	{
		publicationSegment = null;
        
		if (string.IsNullOrWhiteSpace(requestUrl))
			return null;
        
		// ensure we have at least 2 segments
		var urlSegments = requestUrl.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
		if (urlSegments.Count < 2)
			return null;

		// ensure first segment matches RootUrlSegment
		if (!urlSegments.First().Equals(_publiiAppSettings.RootUrlSegment, StringComparison.InvariantCultureIgnoreCase))
			return null;

		// remove RootUrlSegment
		urlSegments.RemoveAt(0);
        
		// first segment is now the publication part 
		publicationSegment = urlSegments.First();

		// if the url segment count is 1, we are trying to hit the index.html page  under /publications/{name}/index.html
		// so we add the segment
		if (urlSegments.Count == 1)
			urlSegments.Add("index.html");
        
		// now remove the unnecessary first segment
		urlSegments.RemoveAt(0);

		return urlSegments;
	}
}