namespace Publii.Umbraco.Services.Interfaces;

public interface IPubliiUrlService
{
	/// <summary>
	/// Given a path like this /publications/mypub/img/pict will return an array containing 'img' and 'pict'.
	///  Will also return the publicationSegment, 'mypub' in this example.
	/// </summary>
	/// <param name="requestUrl"></param>
	/// <param name="publicationSegment"></param>
	/// <returns></returns>
	List<string>? GetUrlSegments(string requestUrl, out string? publicationSegment);
}