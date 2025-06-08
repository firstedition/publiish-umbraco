namespace Publii.Umbraco.Models;

public static class Tables
{
	public const string Publications = "PubliiPublication";
	public const string Settings = "PubliiSettings";
}

public static class Medias
{
	public const string RootMediaFolderName = "Publii";
	public const string FontsMediaFolderName = "Fonts";
	public const string ContentsMediaFolderName = "Contents";
}

public static class AppSettings
{
	public const string DefaultRootUrlSegment = "publications";
	public const MediaUrlConfig DefaultMediaUrlConfig = MediaUrlConfig.UseVirtualPageAndRouting;
	public const string RootUrlSegmentName = "RootUrlSegment";
	public const string MediaUrlConfigName = "MediaUrlConfiguration";
}