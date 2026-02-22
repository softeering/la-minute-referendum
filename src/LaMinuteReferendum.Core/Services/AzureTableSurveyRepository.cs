using System.Net;
using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Extensions;
using LaMinuteReferendum.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LaMinuteReferendum.Core.Services;

public class AzureTableSurveyRepository(ILogger<AzureTableSurveyRepository> logger, CosmosClient client, IOptions<AppConfiguration> configuration) : ISurveyRepository
{
	private readonly AppConfiguration _configuration = configuration.Value;
	private Container? _surveyContainer;
	private Container? _surveyAnswerContainer;

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(this._configuration.DatabaseName, cancellationToken: cancellationToken);

		var surveyContainerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
			id: this._configuration.SurveyContainerName,
			partitionKeyPath: "/day",
			cancellationToken: cancellationToken
		);

		this._surveyContainer = surveyContainerResponse.Container;

		var surveyAnswerContainerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
			id: this._configuration.SurveyAnswerContainerName,
			partitionKeyPath: "/day",
			cancellationToken: cancellationToken
		);

		this._surveyAnswerContainer = surveyAnswerContainerResponse.Container;
	}

	public async Task HandleSurveyAnswer(string surveyId, string answerId, (string, string) metadata, CancellationToken cancellationToken = default)
	{
	}

	public async Task<Survey> GetTodaysSurveyAsync(CancellationToken cancellationToken = default)
	{
		var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Survey.CET).Date);

		var todaySurvey = await this._surveyContainer.FindOneAsync<Survey>(s => s.Day == today && s.Deprecated == false);

		return todaySurvey ?? await this.CreateTodayDefaultSurvey(today, cancellationToken);
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

		var response = await this._surveyContainer.CreateItemAsync(survey, cancellationToken: cancellationToken);
		if (response.StatusCode != HttpStatusCode.OK)
			throw new Exception($"Failed to insert default survey for date={today}). Status:" + response.StatusCode);

		return response.Resource;
	}
}
