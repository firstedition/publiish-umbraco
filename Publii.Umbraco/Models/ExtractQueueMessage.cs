namespace Publii.Umbraco.Models;

public class ExtractQueueMessage
{
	public Guid NewGuid { get; init; }
	public int ZipFileMediaNodeId { get; init; }
	public int PublicationRootMediaFolderId { get; init; }
	public int PublicationContentsMediaFolderId { get; init; }
	public int FontsMediaFolderId { get; init; }
}