using OperationResult;

namespace Publii.Umbraco.Providers.Interfaces;

public interface IZipFileProcessingProvider
{
	Status<Exception> ProcessZipFile(int mediaNodeId, int publicationFolderNodeId, int publicationContentsFolderNodeId, int fontsFolderNodeId);
}