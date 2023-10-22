using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonReferendum.Pages;

public class AboutModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public AboutModel(ILogger<IndexModel> logger)
    {
        this._logger = logger;
    }

    public void OnGet()
    {
    }
}