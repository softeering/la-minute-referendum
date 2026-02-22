using LaMinuteReferendum.Core.Contracts;
using LaMinuteReferendum.Core.Models;
using LaMinuteReferendum.Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMinuteReferendum.Pages;

public class IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment environment, ISurveyRepository surveyRepository) : PageModel
{
	[BindProperty] internal Survey TodaySurvey { get; set; }

	[BindProperty] internal bool Answer { get; set; }

	public async Task OnGetAsync(CancellationToken cancellationToken = default)
	{
		this.EnsureCookie();
		this.TodaySurvey = await surveyRepository.GetTodaysSurveyAsync(cancellationToken);
	}

	private void EnsureCookie()
	{
		if (!this.Request.Cookies.ContainsKey(Constants.CookieName))
		{
			this.Response.Cookies.Append(
				Constants.CookieName,
				Guid.NewGuid().ToString(),
				new()
				{
					Expires = environment.IsDevelopment() ? DateTime.Now.AddMinutes(5) : DateTime.Now.AddDays(30).Date,
					Secure = true,
					HttpOnly = true
				}
			);
		}
	}
}
