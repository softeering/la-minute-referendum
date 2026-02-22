using Microsoft.Extensions.Caching.Memory;
using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Models;
using Microsoft.Extensions.Logging;

namespace LaMinuteReferendum.Core.Services;

public class CachedSurveyRepository(ILogger<CachedSurveyRepository> logger, IMemoryCache memoryCache, ISurveyRepository coreRepository) : ISurveyRepository
{
	private readonly ILogger<CachedSurveyRepository> _logger = logger;
	private readonly IMemoryCache _memoryCache = memoryCache;

	public Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		return coreRepository.InitializeAsync(cancellationToken);
	}

	public async Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default)
	{
		var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Survey.CET).Date;

		var result = await this._memoryCache.GetOrCreateAsync<Survey>(
			$"TodaySurvey-{today.ToString("yyyy-MM-dd")}",
			_ => coreRepository.GetTodaysSurveyAsync(cancellationToken));

		// result cannot be null since we generate a default one if today's one was not created already
		return result!;
	}
}
