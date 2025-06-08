using Microsoft.Extensions.Logging;
using NPoco;
using OperationResult;
using Publii.Umbraco.Extensions;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers.Interfaces;
using Publii.Umbraco.Services.Interfaces;
using Umbraco.Cms.Infrastructure.Scoping;
using static OperationResult.Helpers;

namespace Publii.Umbraco.Providers;

public class PublicationProvider(IScopeProvider scopeProvider, ILoggingService<PublicationProvider> logger)
    : IPublicationProvider
{
    public async Task<Result<IEnumerable<Publication>?, Exception>> GetAll()
    {
        try
        {
            using var scope = scopeProvider.CreateScope();

            var sql = Sql.Builder.From(Tables.Publications);
            var queryResults = await scope.Database.FetchAsync<Publication>(sql);
            scope.Complete();

            return queryResults;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Result<Publication?, Exception>> Get(int id)
    {
        try
        {
            using var scope = scopeProvider.CreateScope();

            var sql = Sql.Builder.From(Tables.Publications)
                .Where("Id = @0", id);
            var queryResult = await scope.Database.FirstOrDefaultAsync<Publication>(sql);
            scope.Complete();

            return queryResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Result<Publication?, Exception>> Get(Guid guid)
    {
        try
        {
            using var scope = scopeProvider.CreateScope();

            var sql = Sql.Builder.From(Tables.Publications)
                .Where("Guid = @0", guid);
            var queryResult = await scope.Database.FirstOrDefaultAsync<Publication>(sql);
            scope.Complete();

            return queryResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Result<Publication?, Exception>> GetByUrlSegment(string? urlSegment)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(urlSegment))
                return null;
            
            using var scope = scopeProvider.CreateScope();

            var sql = Sql.Builder.From(Tables.Publications)
                .Where("UrlSegment = @0", urlSegment);
            var queryResult = await scope.Database.FirstOrDefaultAsync<Publication>(sql);
            scope.Complete();

            return queryResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Status<Exception>> AddMany(IEnumerable<Publication>? publications)
    {
        try
        {
            if (publications == null)
                throw new Exception("publications is null");

            using var scope = scopeProvider.CreateScope();

            var list = publications.ToList();
            var result = await scope.Database.InsertBatchAsync(list, new BatchOptions()
            {
                BatchSize = 1000
            });
            
            if (result != list.Count)
                logger.LogWarning($"Attempted to insert {list.Count} publications. Only {result} where inserted.");

            scope.Complete();

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Status<Exception>> UpdateMany(IEnumerable<Publication>? publications)
    {
        try
        {
            if (publications == null)
                throw new Exception("publications is null");

            using var scope = scopeProvider.CreateScope();

            var updateBatchList = new List<UpdateBatch<Publication>>();
            var list = publications.ToList();
            foreach (var publication in list)
            {
                var updateBatch = new UpdateBatch<Publication>
                {
                    Poco = publication
                };

                updateBatchList.Add(updateBatch);
            }

            var result = await scope.Database.UpdateBatchAsync(updateBatchList, new BatchOptions()
            {
                BatchSize = 1000
            });

            if (result != list.Count)
                logger.LogWarning($"Attempted to update {list.Count} publications. Only {result} where updated.");

            scope.Complete();

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }

    public async Task<Status<Exception>> DeleteMany(IEnumerable<int> publicationIds)
    {
        try
        {
            var groupOfPublicationIds = publicationIds.ToList().SplitIntoChunks(1000);

            using var scope = scopeProvider.CreateScope();

            foreach (var ids in groupOfPublicationIds)
            {
                var deleteQuery = scope.Database.DeleteManyAsync<Publication>();
                deleteQuery.Where(x => ids.Contains(x.Id));

                var result = await deleteQuery.Execute();

                var list = ids.ToList();
                if (result != list.Count())
                    logger.LogWarning(
                        $"Attempted to delete {list.Count()} publications. Only {result} where deleted.");
            }

            scope.Complete();

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error(ex);
        }
    }
}