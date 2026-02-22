using System.Text.Json;
using Azure;
using Azure.Data.Tables;

namespace LaMinuteReferendum.Core.Models;

public class SurveyTableEntity : ITableEntity
{
	public string PartitionKey { get; set; } = string.Empty;
	public string RowKey { get; set; } = string.Empty;
	public DateTimeOffset? Timestamp { get; set; }
	public ETag ETag { get; set; }

	public string Language { get; set; } = string.Empty;
	public string Question { get; set; } = string.Empty;
	public string AnswersJson { get; set; } = string.Empty;
	public bool Deprecated { get; set; }

	public static SurveyTableEntity FromSurvey(Survey survey)
	{
		return new SurveyTableEntity
		{
			PartitionKey = survey.Id,
			RowKey = survey.Id,
			Language = survey.Language,
			Question = survey.Question,
			AnswersJson = JsonSerializer.Serialize(survey.Answers),
			Deprecated = survey.Deprecated
		};
	}

	public Survey ToSurvey()
	{
		var answers = JsonSerializer.Deserialize<List<Answer>>(AnswersJson) ?? new List<Answer>();
		var dayFromId = DateOnly.ParseExact(RowKey, "yyyy-MM-dd");

		return new Survey(
			Id: RowKey,
			Day: dayFromId,
			Language: Language,
			Question: Question,
			Answers: answers,
			Deprecated: Deprecated
		);
	}
}

