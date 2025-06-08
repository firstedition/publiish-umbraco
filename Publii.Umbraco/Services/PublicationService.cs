using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OperationResult;
using Publii.Umbraco.Extensions;
using Publii.Umbraco.Models;
using Publii.Umbraco.Models.ViewModels;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;
using static OperationResult.Helpers;
using Exception = System.Exception;

namespace Publii.Umbraco.Services;

public class PublicationService(
    IPublicationProvider publicationProvider,
    ILoggingService<PublicationService> logger,
    IUmbracoProvider umbracoProvider,
    IExtractQueueService queueService)
    : IPublicationService
{
    public async Task<Result<IEnumerable<Publication>?, Exception>> GetAll(bool onlyIfProcessed)
    {
        var result = await publicationProvider.GetAll();
        if (result.IsError)
            return result;
        if (onlyIfProcessed)
            return result.Value?.Where(x => x.Processed).ToList() ?? result;
        return result;
    }

    public async Task<Result<Publication?, Exception>> Get(int id, bool onlyIfProcessed)
    {
        var result = await publicationProvider.Get(id);
        if (result.IsError)
            return result;
        if (result.Value == null)
            return result;
        if (onlyIfProcessed)
            return result.Value.Processed ? result : null;
        return result;
    }

    public async Task<Result<Publication?, Exception>> GetByUrlSegment(string? urlSegment, bool onlyIfProcessed)
    {
        var result = await publicationProvider.GetByUrlSegment(urlSegment);
        if (result.IsError)
            return result;
        if (result.Value == null)
            return result;
        if (onlyIfProcessed)
            return result.Value.Processed ? result : null;
        return result;
    }

    public async Task<Status<Exception>> Add(PublicationUploadViewModel? uploadModel)
    {
        try
        {
            if (uploadModel?.FileData == null)
                throw new Exception("No file provided.");
            
            if (uploadModel.FileData.Length <= 0)
                throw new Exception("No file provided.");

            if (!(uploadModel.SelectedFileName ?? "").EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("File must be a .zip file.");
            
            // ensuring valid values
            uploadModel.Name = uploadModel.Name.MakeValidName();
            uploadModel.UrlSegment = uploadModel.UrlSegment.MakeValidUrlSegment();
            uploadModel.Description = uploadModel.Description.MakeValidDescription();
            
            // ensure the Publii folder is in the media library root
            var rootMediaFolderIdResult = umbracoProvider.EnsureRootMediaFolder(Medias.RootMediaFolderName);
            if (rootMediaFolderIdResult.IsError)
                throw new Exception("Unable to ensure root media folder.");
            var rootMediaFolderId = rootMediaFolderIdResult.Value;
            
            // ensure the Fonts media folder under the Publii folder
            var fontsMediaFolderIdResult = umbracoProvider.EnsureFontsMediaFolder(rootMediaFolderId);
            if (fontsMediaFolderIdResult.IsError)
                throw new Exception("Unable to ensure fonts media folder.");
            var fontsMediaFolderId = fontsMediaFolderIdResult.Value;
            
            // ensure a folder in the media library for this file
            var publicationRootMediaFolderIdResult = umbracoProvider.EnsurePublicationRootMediaFolder(uploadModel.Name, rootMediaFolderId);
            if (publicationRootMediaFolderIdResult.IsError)
                throw new Exception("Unable to ensure publication root media folder.");
            var publicationRootMediaFolderId = publicationRootMediaFolderIdResult.Value;

            // ensure the contents folder in the publication folder
            var publicationContentsMediaFolderIdResult =
                umbracoProvider.EnsurePublicationContentsMediaFolder(publicationRootMediaFolderId);
            if (publicationContentsMediaFolderIdResult.IsError)
                throw new Exception("Unable to ensure publication contents media folder.");
            var publicationContentsMediaFolderId = publicationContentsMediaFolderIdResult.Value;
            
            // upload the zip file to this folder
            var zipFileMediaNodeIdResult = umbracoProvider.UploadFile(publicationRootMediaFolderId,
                uploadModel.FileData, uploadModel.SelectedFileName);
            if (zipFileMediaNodeIdResult.IsError)
                throw new Exception("Unable to upload zip file to media node.");
            var zipFileMediaNodeId = zipFileMediaNodeIdResult.Value;

            // now create the entry in the database for this publication
            // set the flag as not processed yet
            var now = DateTime.UtcNow;
            var newGuid = Guid.NewGuid();
            var addResult = await publicationProvider.AddMany(new[]
            {
                new Publication()
                {
                    Guid = newGuid,
                    Created = now,
                    Name = uploadModel.Name,
                    UrlSegment = uploadModel.UrlSegment,
                    Description = uploadModel.Description,
                    Updated = now,
                    MediaRootId = publicationRootMediaFolderId,
                    Processed = false
                }
            });
            if (addResult.IsError)
                throw new Exception("Unable to create publication in database table.");
            
            // enqueue extract message
            await queueService.QueueAsync(new ExtractQueueMessage()
            {
                NewGuid = newGuid,
                FontsMediaFolderId = fontsMediaFolderId,
                PublicationContentsMediaFolderId = publicationContentsMediaFolderId,
                PublicationRootMediaFolderId = publicationRootMediaFolderId,
                ZipFileMediaNodeId = zipFileMediaNodeId
            });
            
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public Task<Status<Exception>> Update(Publication? publication)
    {
        return publicationProvider.UpdateMany(publication == null 
            ? null
            : new[] { publication });
    }

    public Task<Status<Exception>> Delete(int id)
    {
        return publicationProvider.DeleteMany(new[] { id });
    }
}