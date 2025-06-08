using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Publii.Umbraco.Models;
using Publii.Umbraco.Models.ViewModels;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;
using Publii.Umbraco.Settings;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Publii.Umbraco.Controllers.Api;

[PluginController("publii")]
public class PublicationsController(
	IPublicationService publicationService,
	IUmbracoProvider umbracoProvider, 
	IOptions<PubliiAppSettings> publiiAppSettings) : UmbracoAuthorizedApiController
{
	public async Task<Publication?> Get([FromQuery, Required] int id)
	{
		var getResult = await publicationService.Get(id, onlyIfProcessed: false);
		if (getResult.IsError)
			return null;

		var result = getResult.Value;
		if (result == null)
			return null;
		
		result.BaseUrl = publiiAppSettings.Value.RootUrlSegment;
		
		return result; 
	}
    
	public async Task<IEnumerable<Publication>?> GetAll()
	{
		var getResult = await publicationService.GetAll(onlyIfProcessed: false);
		if (getResult.IsError)
			return new List<Publication>();
		
		var result = getResult.Value;
		if (result == null)
			return new List<Publication>();

		var publications = result.ToList();
		
		foreach (var publication in publications)
			publication.BaseUrl = publiiAppSettings.Value.RootUrlSegment;
		
		return publications;
	}

	public async Task<bool> Delete([FromQuery, Required] int id)
	{
		var deleteResult = await publicationService.Delete(id);
		return deleteResult.IsSuccess;
	}

	[HttpPost, DisableRequestSizeLimit]
	public async Task<IActionResult> UploadPublication([FromForm] PublicationUploadViewModel uploadModel)
	{
		var uploadResult = await publicationService.Add(uploadModel);
		if (uploadResult.IsError)
			return BadRequest(new { message = uploadResult.Error });
		return Ok(new { message = "Upload completed." });
	}

	[HttpPost]
	public async Task<IActionResult> Edit([FromBody] PublicationEditViewModel editModel)
	{
		var getResult = await publicationService.Get(editModel.Id, onlyIfProcessed: true);
		if (getResult.IsError)
			return BadRequest(new { message = getResult.Error });
        
		var publication = getResult.Value;
		if (publication == null)
			return BadRequest(new { message = $"Publication not found with id {editModel.Id}" });
        
		publication.Name = editModel.Name;
		publication.UrlSegment = editModel.UrlSegment;
		publication.Description = editModel.Description;

		var updateResult = await publicationService.Update(publication);
		if (updateResult.IsError)
			return BadRequest(new { message = $"Unable to update publication." });

		return Ok(new { message = "Updated." });
	}

	[HttpDelete]
	public async Task<IActionResult> Delete([FromBody] PublicationDeleteViewModel deleteModel)
	{
		var getResult = await publicationService.Get(deleteModel.Id, onlyIfProcessed: false);
		if (getResult.IsError)
			return BadRequest(new { message = getResult.Error });
		
		var publication = getResult.Value;
		if (publication == null)
			return BadRequest(new { message = $"Publication not found with id {deleteModel.Id}" });
		if (publication.MediaRootId == null)
			return BadRequest(new { message = $"Publication (mediaRootId) not found with id {deleteModel.Id}" });

		var deleteResult = await publicationService.Delete(deleteModel.Id);
		if (deleteResult.IsError)
			return BadRequest(new { message = deleteResult.Error });
		
		var mediaDeleteResult = umbracoProvider.DeleteMediaFolder(publication.MediaRootId.Value);
		if (mediaDeleteResult.IsError)
			return BadRequest(new { message = mediaDeleteResult.Error });
		
		return Ok(new { message = "Delete completed." });
	}
}