using OperationResult;
using Publii.Umbraco.Models;
using Publii.Umbraco.Models.ViewModels;

namespace Publii.Umbraco.Services.Interfaces;

public interface IPublicationService
{
	Task<Result<IEnumerable<Publication>?, Exception>> GetAll(bool onlyIfProcessed);
	Task<Result<Publication?, Exception>> Get(int id, bool onlyIfProcessed);
	Task<Result<Publication?, Exception>> GetByUrlSegment(string? urlSegment, bool onlyIfProcessed);
	Task<Status<Exception>> Add(PublicationUploadViewModel? uploadModel);
	Task<Status<Exception>> Update(Publication? publication);
	Task<Status<Exception>> Delete(int id);
}