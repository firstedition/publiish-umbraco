using System.Threading.Channels;
using Publii.Umbraco.Models;
using Publii.Umbraco.Services.Interfaces;

namespace Publii.Umbraco.Services;

public class ExtractQueueServiceService : IExtractQueueService
{
	private readonly Channel<ExtractQueueMessage> _channel = Channel.CreateUnbounded<ExtractQueueMessage>();

	public ValueTask QueueAsync(ExtractQueueMessage input)
	{
		return _channel.Writer.WriteAsync(input);
	}


	public ValueTask<ExtractQueueMessage> DequeueAsync(CancellationToken cancellationToken)
	{
		return _channel.Reader.ReadAsync(cancellationToken);
	}
	
	
}