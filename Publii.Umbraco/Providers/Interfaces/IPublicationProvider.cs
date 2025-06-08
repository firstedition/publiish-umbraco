using OperationResult;
using Publii.Umbraco.Models;

namespace Publii.Umbraco.Providers.Interfaces;

public interface IPublicationProvider
{
	Task<Result<IEnumerable<Publication>?, Exception>> GetAll();
	Task<Result<Publication?, Exception>> Get(int id);
	Task<Result<Publication?, Exception>> Get(Guid guid);
	Task<Result<Publication?, Exception>> GetByUrlSegment(string? urlSegment);
	Task<Status<Exception>> AddMany(IEnumerable<Publication>? publications);
	Task<Status<Exception>> UpdateMany(IEnumerable<Publication>? publications);
	Task<Status<Exception>> DeleteMany(IEnumerable<int> publicationIds);
}