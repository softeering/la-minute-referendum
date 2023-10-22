using Azure.Data.Tables;
using MonReferendum.contracts;
using MonReferendum.Models;

namespace MonReferendum.Services;

public class AzureTableStorageSurveyRepository : ISurveyRepository
{
	private readonly ILogger _logger;
	private readonly TableServiceClient _client;

	public AzureTableStorageSurveyRepository(ILogger<AzureTableStorageSurveyRepository> logger, TableServiceClient client)
	{
		this._logger = logger;
		this._client = client;
	}

    public Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
