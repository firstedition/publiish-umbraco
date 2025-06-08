using Microsoft.Extensions.Options;
using Publii.Umbraco.Settings;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;

namespace Publii.Umbraco.Providers;

public class PubliiMediaUrlProvider(
	MediaUrlGeneratorCollection mediaPathGenerators,
	UriUtility uriUtility,
	IOptions<PubliiAppSettings> publiiAppSettings)
	: DefaultMediaUrlProvider(mediaPathGenerators, uriUtility)
{
	private readonly PubliiAppSettings _publiiAppSettings = publiiAppSettings.Value;

	//TODO: make this work
	public override UrlInfo? GetMediaUrl(IPublishedContent content, string propertyAlias, UrlMode mode, string? culture, Uri current)
	{
		return base.GetMediaUrl(content, propertyAlias, mode, culture, current);
	}
}