namespace Datahub.LanguageTraining.Data;

public static class SeedingUtils
{
	public static IEnumerable<SeasonRegistrationPeriod> GetSeasonRegistrationPeriodSeeding(int fromYear, int count)
	{
		foreach (var year in Enumerable.Range(fromYear, count))
		{
			var startMonth = 1; // January
			var endMonth = 3;   // March
			foreach (var q in Enumerable.Range(1, 4))
			{
				yield return new()
				{
					Year_NUM = year,
					Quarter_NUM = (byte)q,
					Open_DT = new DateTime(year, startMonth, 17),
					Close_DT = new DateTime(year, endMonth, 2)
				};
				startMonth += 3;
				endMonth += 3;
			}
		}
	}
}