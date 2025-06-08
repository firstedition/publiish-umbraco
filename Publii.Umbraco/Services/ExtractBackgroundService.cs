using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;

namespace Publii.Umbraco.Services;

public class ExtractBackgroundService(
	IExtractQueueService queueService, 
	ILoggingService<ExtractBackgroundService> logger,
	IZipFileProcessingProvider zipFileProcessingProvider,
	IPublicationProvider publicationProvider)
	: BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("ExtractBackgroundService started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var input = await queueService.DequeueAsync(stoppingToken);
				logger.LogInformation($"Processing: {input.NewGuid}");

				// Simulate work
				await Task.Delay(1000, stoppingToken);
				
				// processes and unzips the zip file
				// it will then create more files in the media library
				var fileProcessedStatus = zipFileProcessingProvider.ProcessZipFile(
					input.ZipFileMediaNodeId,
					input.PublicationRootMediaFolderId,
					input.PublicationContentsMediaFolderId,
					input.FontsMediaFolderId);
				
				// if error, log this
				// if success, set processed to true
				if (fileProcessedStatus.IsError)
				{
					logger.LogError(fileProcessedStatus.Error, fileProcessedStatus.Error.Message);
				}
				else
				{
					// get newly created publication
					var newPublicationResult = await publicationProvider.Get(input.NewGuid);

					// update it, and set processed = true
					var newPublication = newPublicationResult.Value;
					newPublication!.Processed = true;
					await publicationProvider.UpdateMany(new List<Publication>()
					{
						newPublication
					});
				}
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing background task.");
			}
		}

		logger.LogInformation("ExtractBackgroundService stopped.");
	}
}