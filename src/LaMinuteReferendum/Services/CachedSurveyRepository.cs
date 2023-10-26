using Microsoft.Extensions.Caching.Memory;
using LaMinuteReferendum.contracts;
using LaMinuteReferendum.Models;

namespace LaMinuteReferendum.Services;

public class CachedSurveyRepository : ISurveyRepository
{
	private readonly ILogger<CachedSurveyRepository> _logger;
	private readonly IMemoryCache _memoryCache;
	private readonly ISurveyRepository _coreRepository;

	public CachedSurveyRepository(
		ILogger<CachedSurveyRepository> logger,
		IMemoryCache memoryCache,
		ISurveyRepository coreRpository)
	{
		this._logger = logger;
		this._memoryCache = memoryCache;
		this._coreRepository = coreRpository;
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		return this._coreRepository.InitializeAsync(cancellationToken);
	}

	public async Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default)
	{
		var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Survey.CET).Date;

		var result = await this._memoryCache.GetOrCreateAsync<Survey>(
			$"TodaySurvey-{today.ToString("yyyy-MM-dd")}",
			_ => this._coreRepository.GetTodaysSurveyAsync(cancellationToken));

		// result cannot be null since we generate a default one if today's one was not created already
		return result!;
	}
}
