using System.Text.Json;
using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Models;
using LaMinuteReferendum.Core.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

bool useCachedRepository = true;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions();
var appConfigurationSection = builder.Configuration.GetSection("AppConfiguration");
builder.Services.Configure<AppConfiguration>(appConfigurationSection);
// var appConfiguration = appConfigurationSection.Get<AppConfiguration>();

// Add services to the container.
builder.Services.AddMemoryCache(options =>
{
	// options.SizeLimit = 1024 * 1024 * 100; // 10 MB
	options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // 5 minutes
});

builder.Services.AddSingleton<CosmosClient>(_ => new CosmosClient(builder.Configuration.GetConnectionString("CosmosDB"), new CosmosClientOptions()
{
	UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	}
}));

if (useCachedRepository)
{
	builder.Services.AddSingleton<ISurveyRepository, CachedSurveyRepository>(provider =>
	{
		var logger = provider.GetRequiredService<ILogger<CachedSurveyRepository>>();
		var coreRepoLogger = provider.GetRequiredService<ILogger<CosmosDbSurveyRepository>>();
		var coreRepository = new CosmosDbSurveyRepository(
			coreRepoLogger,
			provider.GetRequiredService<CosmosClient>(),
			provider.GetRequiredService<IOptions<AppConfiguration>>()
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
	builder.Services.AddSingleton<ISurveyRepository, CosmosDbSurveyRepository>();
}

builder.Services.AddRazorPages();

var app = builder.Build();

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
