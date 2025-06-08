using Publii.Umbraco.Models;
using Umbraco.Cms.Core.Models;

namespace Publii.Umbraco.Services.Interfaces;

public interface IPubliiMediaService
{
	IMedia? GetMedia(List<string> urlSegments, Publication publication, out string? mimeType);

	Stream? GetMediaStream(IMedia media);
}