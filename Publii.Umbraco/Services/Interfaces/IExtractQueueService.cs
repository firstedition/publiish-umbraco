using Publii.Umbraco.Models;

namespace Publii.Umbraco.Services.Interfaces;

public interface IExtractQueueService
{
	ValueTask QueueAsync(ExtractQueueMessage input);
	ValueTask<ExtractQueueMessage> DequeueAsync(CancellationToken cancellationToken);
}