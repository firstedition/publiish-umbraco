using Microsoft.Extensions.Logging;
using Publii.Umbraco.Services.Interfaces;

namespace Publii.Umbraco.Services;

public class LoggingService<T>(ILogger<T> logger) : ILoggingService<T>
	where T : class
{
	private const string PubliiLoggingPrefix = "Publii: ";
	
	public void LogInformation(string message)
	{
		logger.LogInformation($"{PubliiLoggingPrefix}{message}");
	}

	public void LogWarning(string message)
	{
		logger.LogWarning($"{PubliiLoggingPrefix}{message}");
	}

	public void LogError(Exception ex, string message)
	{
		logger.LogError(ex, $"{PubliiLoggingPrefix}{message}");
	}

	public void LogError(string message)
	{
		logger.LogError($"{PubliiLoggingPrefix}{message}");
	}
}