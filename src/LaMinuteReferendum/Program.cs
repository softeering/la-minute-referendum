using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using LaMinuteReferendum.contracts;
using LaMinuteReferendum.Models;
using LaMinuteReferendum.Services;

bool useCachedRepository = true;

var builder = WebApplication.CreateBuilder(args);

// map configs and options
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
	SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
}));

if (useCachedRepository)
{
	builder.Services.AddSingleton<ISurveyRepository, CachedSurveyRepository>(provider =>
	{
		var logger = provider.GetRequiredService<ILogger<CachedSurveyRepository>>();
		var coreRepoLogger = provider.GetRequiredService<ILogger<CosmosDBSurveyRepository>>();
		var coreRepository = new CosmosDBSurveyRepository(
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
	builder.Services.AddSingleton<ISurveyRepository, CosmosDBSurveyRepository>();
}

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 5 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

await app.Services.GetRequiredService<ISurveyRepository>().InitializeAsync();

app.Run();
