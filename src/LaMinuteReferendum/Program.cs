using Azure.Data.Tables;
using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Services;
using Microsoft.Extensions.Caching.Memory;

bool useCachedRepository = true;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddMemoryCache(options =>
{
	// options.SizeLimit = 1024 * 1024 * 100; // 10 MB
	options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // 5 minutes
});

// Get connection string from Aspire service discovery or fallback to configuration
var tableStorageConnectionString = builder.Configuration.GetConnectionString("AzureTableStorage") ??
	throw new InvalidOperationException("AzureTableStorage connection string not found in configuration or service discovery");

var surveyTableName = builder.Configuration["AppConfiguration:SurveyTableName"] ?? "surveys";
var surveyAnswerTableName = builder.Configuration["AppConfiguration:SurveyAnswerTableName"] ?? "surveyanswers";

if (useCachedRepository)
{
	builder.Services.AddSingleton<ISurveyRepository, CachedSurveyRepository>(provider =>
	{
		var logger = provider.GetRequiredService<ILogger<CachedSurveyRepository>>();
		var coreRepoLogger = provider.GetRequiredService<ILogger<AzureTableSurveyRepository>>();

		// Get the survey and answer table clients
		var surveyTableClient = new TableClient(tableStorageConnectionString, surveyTableName);
		var answerTableClient = new TableClient(tableStorageConnectionString, surveyAnswerTableName);

		var coreRepository = new AzureTableSurveyRepository(
			coreRepoLogger,
			surveyTableClient,
			answerTableClient
		);

		return new CachedSurveyRepository(
			logger,
			provider.GetRequiredService<IMemoryCache>(),
			coreRepository
		);
	});
}
else
{
	builder.Services.AddSingleton<ISurveyRepository>(provider =>
	{
		var logger = provider.GetRequiredService<ILogger<AzureTableSurveyRepository>>();
		var surveyTableClient = new TableClient(tableStorageConnectionString, surveyTableName);
		var answerTableClient = new TableClient(tableStorageConnectionString, surveyAnswerTableName);

		return new AzureTableSurveyRepository(
			logger,
			surveyTableClient,
			answerTableClient
		);
	});
}

builder.Services.AddRazorPages();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
// app.MapStaticAssets();
app.MapRazorPages()
	.WithStaticAssets();

await app.Services.GetRequiredService<ISurveyRepository>().InitializeAsync();

await app.RunAsync();
