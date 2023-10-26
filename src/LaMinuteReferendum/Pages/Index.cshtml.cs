using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LaMinuteReferendum.contracts;
using LaMinuteReferendum.Models;
using LaMinuteReferendum.Utils;

namespace LaMinuteReferendum.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger _logger;
	private readonly ISurveyRepository _surveyRepository;
	private readonly IWebHostEnvironment _environment;

	public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, ISurveyRepository surveyRepository)
	{
		this._logger = logger;
		this._environment = environment;
		this._surveyRepository = surveyRepository;
	}

	[BindProperty] internal Survey TodaySurvey { get; set; }

	[BindProperty] internal bool Answer { get; set; }

	public async Task OnGetAsync(CancellationToken cancellationToken = default)
	{
		this.EnsureCookie();
		this.TodaySurvey = await this._surveyRepository.GetTodaysSurveyAsync(cancellationToken);
	}

	private void EnsureCookie()
	{
		if (!this.Request.Cookies.ContainsKey(Constants.COOKIE_NAME))
		{
			this.Response.Cookies.Append(
				Constants.COOKIE_NAME,
				Guid.NewGuid().ToString(),
				new()
				{
					Expires = this._environment.IsDevelopment() ? DateTime.Now.AddMinutes(5) : DateTime.Now.AddDays(30).Date,
					Secure = true,
					HttpOnly = true
				}
			);
		}
	}
}
