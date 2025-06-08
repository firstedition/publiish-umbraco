using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OperationResult;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using static OperationResult.Helpers;
using Exception = System.Exception;

namespace Publii.Umbraco.Providers;

public class UmbracoProvider(
    IMediaService mediaService,
    ILoggingService<UmbracoProvider> logger,
    MediaFileManager mediaFileManager,
    IShortStringHelper shortStringHelper,
    IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
    MediaUrlGeneratorCollection mediaUrlGeneratorCollection)
    : IUmbracoProvider
{
    public Result<int, Exception> EnsureRootMediaFolder(string rootMediaFolderName)
    {
        try
        {
            // get medias at root
            var rootMedias = mediaService.GetRootMedia();

            // get Publii root folder
            var rootFolder = rootMedias.FirstOrDefault(x =>
                (x.Name ?? string.Empty).Equals(Medias.RootMediaFolderName,
                    StringComparison.InvariantCultureIgnoreCase) &&
                x is { Trashed: false, ContentType.Alias: "Folder" });

            // if exists, return its id
            if (rootFolder != null)
                return rootFolder.Id;

            // if not, then create the folder
            var folder = mediaService.CreateMediaWithIdentity(Medias.RootMediaFolderName, -1, "Folder");
            
            // persist folder
            var saveResult = mediaService.Save(folder);
            if (!saveResult.Success)
                throw new Exception($"Unable to save root folder. {saveResult.Exception?.Message}");
            
            // and return the id
            return folder.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<int, Exception> EnsureFontsMediaFolder(int rootMediaFolderId)
    {
        try
        {
            // get Publii root folder
            var rootFolder = mediaService.GetById(rootMediaFolderId);
            if (rootFolder == null)
                throw new Exception($"root folder not found with id {rootMediaFolderId}");

            // get all children of Publii folder
            var children = mediaService.GetPagedChildren(rootMediaFolderId, 0, int.MaxValue, out _);

            // get the folder for the fonts
            var folder = children.FirstOrDefault(x =>
                (x.Name ?? string.Empty).Equals(Medias.FontsMediaFolderName,
                    StringComparison.InvariantCultureIgnoreCase) &&
                x is { Trashed: false, ContentType.Alias: "Folder" });

            // if exists, return its id
            if (folder != null)
                return folder.Id;

            // create fonts folder
            var fontsFolder = mediaService.CreateMediaWithIdentity(Medias.FontsMediaFolderName, rootFolder.Id, "Folder");

            // persists fonts folder
            var saveResult = mediaService.Save(fontsFolder);
            if (!saveResult.Success)
                throw new Exception($"Unable to save fonts folder. {saveResult.Exception?.Message}");
            
            // and return its id
            return fontsFolder.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<int, Exception> EnsurePublicationRootMediaFolder(string? folderName, int rootMediaFolderId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderName))
                throw new Exception("folderName is null");
            
            // get Publii root folder
            var rootFolder = mediaService.GetById(rootMediaFolderId);
            if (rootFolder == null)
                throw new Exception($"root folder not found with id {rootMediaFolderId}");

            // get all children of Publii folder
            var children = mediaService.GetPagedChildren(rootMediaFolderId, 0, int.MaxValue, out _);

            // get the folder for this publication
            var folder = children.FirstOrDefault(x =>
                (x.Name ?? string.Empty).Equals(folderName,
                    StringComparison.InvariantCultureIgnoreCase) &&
                x is { Trashed: false, ContentType.Alias: "Folder" });

            // if exists, return its id
            if (folder != null)
                return folder.Id;

            // create publication folder
            var publicationFolder = mediaService.CreateMediaWithIdentity(folderName, rootFolder.Id, "Folder");

            // persists publication folder
            var saveResult = mediaService.Save(publicationFolder);
            if (!saveResult.Success)
                throw new Exception(
                    $"Unable to save publication folder '{folderName}'. {saveResult.Exception?.Message}");
                
            // and return its id
            return publicationFolder.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<int, Exception> EnsurePublicationContentsMediaFolder(int publicationRootMediaFolderId)
    {
        try
        {
            // get publication root folder
            var rootFolder = mediaService.GetById(publicationRootMediaFolderId);
            if (rootFolder == null)
                throw new Exception($"publication root folder not found with id {publicationRootMediaFolderId}");

            // get all children of publication root folder
            var children = mediaService.GetPagedChildren(publicationRootMediaFolderId, 0, int.MaxValue, out _);

            // get the folder for the contents
            var folder = children.FirstOrDefault(x =>
                (x.Name ?? string.Empty).Equals(Medias.ContentsMediaFolderName,
                    StringComparison.InvariantCultureIgnoreCase) &&
                x is { Trashed: false, ContentType.Alias: "Folder" });

            // if exists, return its id
            if (folder != null)
                return folder.Id;

            // create contents folder
            var contentsFolder = mediaService.CreateMediaWithIdentity(Medias.ContentsMediaFolderName, rootFolder.Id, "Folder");

            // persists contents folder
            var saveResult = mediaService.Save(contentsFolder);
            if (!saveResult.Success)
                throw new Exception($"Unable to save contents folder. {saveResult.Exception?.Message}");
            
            // and return its id
            return contentsFolder.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<int, Exception> UploadFile(int parentMediaNodeId, IFormFile fileData, string? fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new Exception("fileName is null");
            
            // upload file
            var uploadedFile = mediaService.CreateMediaWithIdentity(fileName, parentMediaNodeId, "File");
            uploadedFile.SetValue(mediaFileManager,
                mediaUrlGeneratorCollection,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                Constants.Conventions.Media.File,
                fileName, fileData.OpenReadStream());

            // persists file
            var saveResult = mediaService.Save(uploadedFile);
            if (!saveResult.Success)
                throw new Exception($"Unable to upload file '{fileName}'. {saveResult.Exception?.Message}");

            // return id of uploaded file
            return uploadedFile.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<int, Exception> CreateMediaFolder(string folderName, int parentMediaId)
    {
        try
        {
            // create media folder
            var mediaFolder = mediaService.CreateMediaWithIdentity(folderName, parentMediaId, "Folder");

            // persists media folder
            var saveResult = mediaService.Save(mediaFolder);
            if (!saveResult.Success)
                throw new Exception($"Failed to save media folder '{folderName}' on parent {parentMediaId}");

            return mediaFolder.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<bool, Exception> MediaExists(string nodeName, int parentMediaId)
    {
        try
        {
            var children = mediaService
                .GetPagedChildren(parentMediaId, 0, int.MaxValue, out _)
                .ToList();

            if (!children.Any())
                return false;

            var node = children
                .FirstOrDefault(x => (x.Name ?? string.Empty).Equals(nodeName, StringComparison.InvariantCultureIgnoreCase));

            return node != null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Result<bool, Exception> DeleteMediaFolder(int mediaRootId)
    {
        try
        {
            var rootMedia = mediaService.GetById(mediaRootId);
            if (rootMedia == null)
                throw new Exception($"Media root {mediaRootId} not found");
            var deleteAttempt = mediaService.Delete(rootMedia);
            return deleteAttempt.Success; 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }
}