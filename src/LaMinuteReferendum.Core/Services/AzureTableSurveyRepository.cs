using Azure.Data.Tables;
using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Models;
using Microsoft.Extensions.Logging;

namespace LaMinuteReferendum.Core.Services;

public class AzureTableSurveyRepository(ILogger<AzureTableSurveyRepository> logger, TableClient surveyTableClient, TableClient surveyAnswerTableClient) : ISurveyRepository
{
	private readonly TableClient _surveyTableClient = surveyTableClient;
	private readonly TableClient _surveyAnswerTableClient = surveyAnswerTableClient;

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		// Azure Table Storage tables are created automatically when accessed
		// This method ensures the tables exist before use
		try
		{
			await _surveyTableClient.CreateAsync(cancellationToken: cancellationToken);
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 409)
		{
			// Table already exists
			logger.LogInformation("Survey table already exists");
		}

		try
		{
			await _surveyAnswerTableClient.CreateAsync(cancellationToken: cancellationToken);
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 409)
		{
			// Table already exists
			logger.LogInformation("SurveyAnswer table already exists");
		}
	}

	public async Task HandleSurveyAnswer(string surveyId, string answerId, (string, string) metadata, CancellationToken cancellationToken = default)
	{
		// TODO: Implement survey answer handling for table storage
	}

	public async Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default)
	{
		var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Survey.CET).Date);
		var partitionKey = today.ToString("yyyy-MM-dd");

		try
		{
			var entity = await _surveyTableClient.GetEntityAsync<SurveyTableEntity>(partitionKey, partitionKey, cancellationToken: cancellationToken);

			if (entity.Value.Deprecated)
			{
				return await CreateTodayDefaultSurvey(today, cancellationToken);
			}

			return entity.Value.ToSurvey();
		}
		catch (Azure.RequestFailedException ex) when (ex.Status == 404)
		{
			// Survey not found, create default
			return await CreateTodayDefaultSurvey(today, cancellationToken);
		}
	}

	private async Task<Survey> CreateTodayDefaultSurvey(DateOnly today, CancellationToken cancellationToken = default)
	{
		var survey = new Survey(
			Id: today.ToString("yyyy-MM-dd"),
			Day: today,
			Language: "FR",
			Question: "Aimez-vous 'La Minute Referendum' ?",
			Answers: new List<Answer>
			{
				Answer.CreateAnswer("Oui"),
				Answer.CreateAnswer("Non")
			}
		);

		var surveyTableEntity = SurveyTableEntity.FromSurvey(survey);
		await _surveyTableClient.AddEntityAsync(surveyTableEntity, cancellationToken: cancellationToken);

		return survey;
	}
}
