using LaMinuteReferendum.Core.Models;

namespace LaMinuteReferendum.Core.Contracts;

public interface ISurveyRepository
{
	Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default);
}
