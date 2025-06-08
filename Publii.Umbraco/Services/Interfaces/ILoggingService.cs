namespace Publii.Umbraco.Services.Interfaces;

public interface ILoggingService<T>
{
	void LogInformation(string message);
	void LogWarning(string message);
	void LogError(Exception ex, string message);
	void LogError(string message);
}