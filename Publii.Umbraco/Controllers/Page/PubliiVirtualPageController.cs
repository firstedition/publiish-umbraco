using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Publii.Umbraco.Services.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Publii.Umbraco.Controllers.Page;

public class PubliiVirtualPageController(
	ILogger<UmbracoPageController> logger,
	ICompositeViewEngine compositeViewEngine,
	IUmbracoContextAccessor umbracoContextAccessor,
	IPublicationService publicationService,
	IPubliiMediaService publiiMediaService,
	IPubliiUrlService publiiUrlService)
	: UmbracoPageController(logger, compositeViewEngine), IVirtualPageController
{
	public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext)
	{
		// since we are going to route to media nodes, this method is not used
		// however it is required to return something
		// sets any published content
		umbracoContextAccessor.TryGetUmbracoContext(out var context);
		var root = context?.Content?.GetAtRoot().First();
		return root;
	}
	
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		// get url
		var path = HttpContext.Request.Path.Value;
		if (path == null)
			return NotFound();
        
		// get url segments
		var urlSegments = publiiUrlService.GetUrlSegments(path, out var publicationSegment);
		if (urlSegments == null)
			return NotFound();
        
		// the first url segment must now be a registered url segment, with processed data
		var publicationResult = await publicationService.GetByUrlSegment(publicationSegment, onlyIfProcessed: true);
		if (publicationResult.IsError || publicationResult.Value == null)
			return NotFound();

		// now we have a processed publication
		var publication = publicationResult.Value;

		// get media
		var media = publiiMediaService.GetMedia(urlSegments, publication, out var mimeType);
		if (media == null)
			return NotFound();
        
		// get media stream
		var stream = publiiMediaService.GetMediaStream(media);
		if (stream == null)
			return NotFound();
        
		// return file stream
		return File(stream, mimeType!);
	}
}