namespace LaMinuteReferendum.Models;

public class AppConfiguration
{
	public string DatabaseName { get; set; }
	public string SurveyContainerName { get; init; }
	public string SurveyAnswerContainerName { get; init; }
}
