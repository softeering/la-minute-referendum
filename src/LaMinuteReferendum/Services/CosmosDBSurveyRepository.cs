using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using LaMinuteReferendum.contracts;
using LaMinuteReferendum.Models;
using Microsoft.Azure.Cosmos.Linq;
using MonReferendum.Extensions;

namespace LaMinuteReferendum.Services;

public class CosmosDBSurveyRepository : ISurveyRepository
{
	private readonly ILogger _logger;
	private readonly CosmosClient _client;
	private readonly AppConfiguration _configuration;
	private Container _surveyContainer;
	private Container _surveyAnswerContainer;

	public CosmosDBSurveyRepository(
		ILogger<CosmosDBSurveyRepository> logger,
		CosmosClient client,
		IOptions<AppConfiguration> configuration)
	{
		this._logger = logger;
		this._configuration = configuration.Value;
		this._client = client;
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var databaseResponse = await this._client.CreateDatabaseIfNotExistsAsync(this._configuration.DatabaseName, cancellationToken: cancellationToken);

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
