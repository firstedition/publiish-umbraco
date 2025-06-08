using Microsoft.AspNetCore.Http;

namespace Publii.Umbraco.Models.ViewModels;

public class PublicationUploadViewModel
{
	public string? Name { get; set; }
	public string? UrlSegment { get; set; }
	public string? Description { get; set; }
	public string? SelectedFileName { get; set; }
	public IFormFile? FileData { get; set; }
}