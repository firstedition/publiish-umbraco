using Microsoft.AspNetCore.Http;
using OperationResult;

namespace Publii.Umbraco.Providers.Interfaces;

public interface IUmbracoProvider
{
	Result<int, Exception> EnsureRootMediaFolder(string rootMediaFolderName);

	Result<int, Exception> EnsureFontsMediaFolder(int rootMediaFolderId);
    
	Result<int, Exception> EnsurePublicationRootMediaFolder(string? folderName, int rootMediaFolderId);

	Result<int, Exception> EnsurePublicationContentsMediaFolder(int publicationRootMediaFolderId);
    
	Result<int, Exception> UploadFile(int parentMediaNodeId, IFormFile fileData, string? fileName);

	Result<int, Exception> CreateMediaFolder(string folderName, int parentMediaId);

	Result<bool, Exception> MediaExists(string nodeName, int parentMediaId);
	
	Result<bool, Exception> DeleteMediaFolder(int mediaRootId);
}