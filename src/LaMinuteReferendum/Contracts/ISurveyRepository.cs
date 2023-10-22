using MonReferendum.Models;

namespace MonReferendum.contracts;

public interface ISurveyRepository
{
    Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default);
}