using SoftEEring.Core.Helpers;

namespace LaMinuteReferendum.Models;

public record Survey(string Id, DateOnly Day, string Language, string Question, List<Answer> Answers, bool Deprecated = false)
{
	public static TimeZoneInfo CET = TimeZoneInfo.GetSystemTimeZones().First(tzi => tzi.StandardName == "Central European Standard Time");

	public static string TodaysSurveyKey => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Survey.CET).Date.ToString("yyyy-MM-dd");

}

public record Answer(string Id, string Language, string Text)
{
	public static Answer CreateAnswer(string text, string language = "FR")
	{
		return new Answer(StringHelper.GenerateRandomString(6, includeSpecialChars: false), language, text);
	}
}

public record SurveyAnswer(string Id, string SurveyId, string AnswerId);
