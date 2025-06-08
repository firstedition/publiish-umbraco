using Publii.Umbraco.Models;

namespace Publii.Umbraco.Settings;

public class PubliiAppSettings
{
	private readonly string? _rootUrlSegment;
	public string RootUrlSegment
	{
		get => string.IsNullOrWhiteSpace(_rootUrlSegment) ? AppSettings.DefaultRootUrlSegment : _rootUrlSegment;
		init => _rootUrlSegment = value;
	}

	private readonly MediaUrlConfig? _mediaUrlConfig;
	public MediaUrlConfig? MediaUrlConfiguration
	{
		get => _mediaUrlConfig ?? AppSettings.DefaultMediaUrlConfig;
		init => _mediaUrlConfig = value;
	}
}