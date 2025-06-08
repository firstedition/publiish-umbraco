using System.Drawing.Text;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using OperationResult;
using Publii.Umbraco.Extensions;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using static OperationResult.Helpers;

namespace Publii.Umbraco.Providers;

public class ZipFileProcessingProvider(
	IMediaService mediaService,
	MediaFileManager mediaFileManager,
	ILoggingService<ZipFileProcessingProvider> logger,
	IUmbracoProvider umbracoProvider,
	MediaUrlGeneratorCollection mediaUrlGeneratorCollection,
	IShortStringHelper shortStringHelper,
	IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
	: IZipFileProcessingProvider
{
    public Status<Exception> ProcessZipFile(int mediaNodeId, int publicationFolderNodeId, int publicationContentsFolderNodeId,
		int fontsFolderNodeId)
	{
		try
        {
            // get media nodes
            var media = mediaService.GetById(mediaNodeId);
            var publicationFolder = mediaService.GetById(publicationFolderNodeId);
            var publicationContentsFolder = mediaService.GetById(publicationContentsFolderNodeId);
            var fontsFolder = mediaService.GetById(fontsFolderNodeId);
            
            if (media == null || 
                publicationFolder == null || 
                publicationContentsFolder == null || 
                fontsFolder == null)
                throw new Exception($"media nodes not found with id {mediaNodeId}, {publicationFolderNodeId}, {publicationContentsFolderNodeId}, {fontsFolderNodeId}");
            
            // get filepath in media
            mediaFileManager.GetFile(media, out var mediaFilePath);

            if (string.IsNullOrWhiteSpace(mediaFilePath))
                throw new Exception($"Unable to get media filepath for media {mediaNodeId}");
            
            using var zipFileStream = mediaService.GetMediaFileContentStream(mediaFilePath);
            using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Read);

            // find font files
            var fontFileEntries = archive.Entries.Where(x =>
                x.FullName.StartsWith(Medias.FontsMediaFolderName + "/", StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            
            // unzip font files
            foreach (var fontFileEntry in fontFileEntries)
            {
                // if font file exists, skip it
                var fontFileExistsResult = umbracoProvider.MediaExists(fontFileEntry.Name, fontsFolderNodeId);
                if (fontFileExistsResult.IsError)
                    throw new Exception(
                        $"Error occurred trying to locate media file, {fontFileEntry.Name} in parent {fontsFolderNodeId}");
                if (fontFileExistsResult.Value)
                    continue;
                    
                // upload font file
                var uploadedFontFile = mediaService.CreateMediaWithIdentity(fontFileEntry.Name, fontsFolderNodeId, "File");
                uploadedFontFile.SetValue(mediaFileManager,
                    mediaUrlGeneratorCollection,
                    shortStringHelper,
                    contentTypeBaseServiceProvider,
                    Constants.Conventions.Media.File,
                    fontFileEntry.Name,
                    fontFileEntry.Open());
                
                // persist font file
                var fontSaveResult = mediaService.Save(uploadedFontFile);
                if (!fontSaveResult.Success)
                    throw new Exception($"Unable to save uploaded font '{fontFileEntry.FullName}'");
            }

            // get all the files that are in a directory
            var otherFilesWithinDirectories = archive.Entries
                .Except(fontFileEntries)
                .Where(x => x.FullName.Contains('/'))
                .OrderBy(x => x.FullName)
                .ToList();

            // get a distinct list of the folders within
            var otherFolderNames = otherFilesWithinDirectories
                .Select(x => x.FullName.GetSubstringBeforeSlash())
                .Distinct();

            // a dictionary to hold folder name and the id of the created folder
            var foldersDict = new Dictionary<string, int>();
            foreach (var folderName in otherFolderNames)
            {
                if (string.IsNullOrWhiteSpace(folderName))
                    continue;

                var folderExistsResult = umbracoProvider.MediaExists(folderName, publicationContentsFolderNodeId);
                if (folderExistsResult.IsError)
                    throw new Exception(
                        $"Unable to determine if folder '{folderName}' exists in parent {publicationContentsFolderNodeId}");
                if (folderExistsResult.Value)
                    continue;
                
                var createdMediaFolderIdResult = umbracoProvider.CreateMediaFolder(folderName, publicationContentsFolderNodeId);
                if (createdMediaFolderIdResult.IsError)
                    throw new Exception(
                        $"Unable to create media folder '{folderName}' in parent {publicationContentsFolderNodeId}");
                
                foldersDict.Add(folderName, createdMediaFolderIdResult.Value);
            }

            var nonFontFilesEntries = archive.Entries.Except(fontFileEntries);

            foreach (var fileEntry in nonFontFilesEntries)
            {
                var isRootFileEntry = !fileEntry.FullName.Contains('/');
                var isImageFile = fileEntry.FullName.StartsWith("img/", StringComparison.InvariantCultureIgnoreCase);
                var folderId = -1;
                if (!isRootFileEntry)
                {
                    var folderName = fileEntry.FullName.GetSubstringBeforeSlash();
                    foldersDict.TryGetValue(folderName!, out folderId);
                }
                
                // upload file
                var uploadedFile = mediaService.CreateMediaWithIdentity(
                    fileEntry.Name,
                    folderId == -1 ? publicationContentsFolderNodeId : folderId, 
                    isImageFile ? "Image" : "File");
                uploadedFile.SetValue(mediaFileManager,
                    mediaUrlGeneratorCollection,
                    shortStringHelper,
                    contentTypeBaseServiceProvider,
                    Constants.Conventions.Media.File,
                    fileEntry.Name,
                    fileEntry.Open());
                
                // persist file
                var fileSaveResult = mediaService.Save(uploadedFile);
                if (!fileSaveResult.Success)
                    throw new Exception($"Unable to save uploaded file '{fileEntry.FullName}'");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
	}
}