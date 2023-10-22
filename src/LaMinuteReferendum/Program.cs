using Azure.Data.Tables;
using MonReferendum.contracts;
using MonReferendum.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<TableServiceClient>(provider => new TableServiceClient(builder.Configuration.GetConnectionString("AzureTableStorage")));
builder.Services.AddSingleton<ISurveyRepository, AzureTableStorageSurveyRepository>();

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

app.Run();
