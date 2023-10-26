using LaMinuteReferendum.Models;

namespace LaMinuteReferendum.contracts;

public interface ISurveyRepository
{
	Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default);
}
