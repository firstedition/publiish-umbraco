using Publii.Umbraco.Extensions;
using Publii.Umbraco.Models;
using Publii.Umbraco.Services.Interfaces;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Publii.Umbraco.Services;

public class PubliiMediaService(IMediaService mediaService, MediaFileManager mediaFileManager)
    : IPubliiMediaService
{
    public IMedia? GetMedia(List<string> urlSegments, Publication publication, out string mimeType)
    {
        mimeType = string.Empty;
        
        var contentsFolder = GetContentsFolder(publication);
        
        if (contentsFolder == null)
            return null;
        
        var contentsFolderChildren = GetChildren(contentsFolder);

        // look at url segments
        switch (urlSegments.Count)
        {
            // if in root folder
            case 1:
                return GetMediaAtRoot(contentsFolderChildren, urlSegments, out mimeType);
            
            case 2:
            {
                return urlSegments.First().Equals(Medias.FontsMediaFolderName, StringComparison.InvariantCultureIgnoreCase)
                    // if a font file
                    ? GetMediaFont(contentsFolder, urlSegments, out mimeType)
                    // else look at second level file
                    : GetMediaAtSecondLevel(contentsFolderChildren, urlSegments, out mimeType);
            }
            
            default:
                return null;
        }
    }

    private IMedia? GetMediaAtRoot(List<IMedia> contentsFolderChildren, List<string> urlSegments, out string mimeType)
    {
        mimeType = string.Empty;
        
        var rootMedia = contentsFolderChildren.FirstOrDefault(x =>
            (x.Name ?? string.Empty).StartsWith(urlSegments.Last(), StringComparison.InvariantCultureIgnoreCase) &&
            (x.Name ?? string.Empty).IndexOf('.') > 0);
        
        if (rootMedia == null)
            return null;
        
        mimeType = rootMedia.Name.GetUrlFileExtension().GetMimeType();
        
        return rootMedia;
    }

    private IMedia? GetMediaFont(IMedia contentsFolder, List<string> urlSegments, out string mimeType)
    {
        mimeType = string.Empty;
        
        var parentContentFolder = mediaService.GetAncestors(contentsFolder).FirstOrDefault(x =>
            (x.Name ?? string.Empty).Equals(Medias.RootMediaFolderName,
                StringComparison.InvariantCultureIgnoreCase));
        
        if (parentContentFolder == null)
            return null;
        
        var parentContentFolderItems = GetChildren(parentContentFolder);
        
        var fontsFolder = parentContentFolderItems.FirstOrDefault(x =>
            (x.Name ?? string.Empty).Equals(Medias.FontsMediaFolderName,
                StringComparison.InvariantCultureIgnoreCase));
        
        if (fontsFolder == null)
            return null;
        
        var fontsFolderChildren = GetChildren(fontsFolder);
        
        var fontMedia = fontsFolderChildren.FirstOrDefault(x =>
            (x.Name ?? string.Empty).StartsWith(urlSegments.Last(),
                StringComparison.InvariantCultureIgnoreCase) &&
            (x.Name ?? string.Empty).IndexOf('.') > 0);
        
        if (fontMedia == null)
            return null;
        
        mimeType = fontMedia.Name.GetUrlFileExtension().GetMimeType();
        
        return fontMedia;
    }

    private IMedia? GetMediaAtSecondLevel(List<IMedia> contentsFolderChildren, List<string> urlSegments,
        out string mimeType)
    {
        mimeType = string.Empty;
        
        var segmentContentFolder = contentsFolderChildren.FirstOrDefault(x =>
            (x.Name ?? string.Empty).Equals(urlSegments.First(),
                StringComparison.InvariantCultureIgnoreCase));
        
        if (segmentContentFolder == null)
            return null;
        
        var childContentFolder = GetChildren(segmentContentFolder);
        
        var childMedia = childContentFolder.FirstOrDefault(x =>
            (x.Name ?? string.Empty).StartsWith(urlSegments.Last(),
                StringComparison.InvariantCultureIgnoreCase) &&
            (x.Name ?? string.Empty).IndexOf('.') > 0);
        
        if (childMedia == null)
            return null;
        
        mimeType = childMedia.Name.GetUrlFileExtension().GetMimeType();
        
        return childMedia;
    }

    public Stream? GetMediaStream(IMedia media)
    {
        // get media file path
        mediaFileManager.GetFile(media, out var mediaFilePath);
        
        // get media file stream
        return mediaFilePath == null
            ? null :
            mediaService.GetMediaFileContentStream(mediaFilePath);
    }

    private List<IMedia> GetChildren(IMedia contentFolder)
    {
        return mediaService
            .GetPagedChildren(contentFolder.Id, 0, int.MaxValue, out _)
            .ToList();
    }
    
    private IMedia? GetContentsFolder(Publication publication)
    {
        if (publication.MediaRootId == null)
            return null;
        
        var rootFolder = mediaService.GetById(publication.MediaRootId.Value);
        
        var rootChildren = mediaService
            .GetPagedChildren(publication.MediaRootId.Value, 0, int.MaxValue, out _)
            .ToList();
        
        if (rootFolder == null)
            return null;
        
        if (!rootChildren.Any())
            return null;
        
        return rootChildren.FirstOrDefault(x =>
            (x.Name ?? string.Empty).Equals(Medias.ContentsMediaFolderName,
                StringComparison.InvariantCultureIgnoreCase));
    }
}