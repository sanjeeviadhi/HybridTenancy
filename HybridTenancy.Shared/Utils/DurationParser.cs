using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace HybridTenancy.Shared.Utils
{
    public static class DurationParser
    {
        private static readonly Regex DurationRegex = new Regex(
            @"(\d+)\s*(y|year|years|m|month|months|d|day|days)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static DateTime ParseToDate(string? input, ILogger? logger = null, string defaultDuration = "30d")
        {
            try
            {
                var now = DateTime.UtcNow.Date;
                input = string.IsNullOrWhiteSpace(input) ? defaultDuration.ToLower() : input.ToLower();

                logger?.LogInformation("Parsing duration input: {Input}", input);

                var matches = DurationRegex.Matches(input);
                if (matches.Count == 0)
                {
                    logger?.LogWarning("No matches found in duration input: {Input}", input);
                    throw new ArgumentException($"Invalid duration input: '{input}'");
                }

                int years = 0, months = 0, days = 0;

                foreach (Match match in matches)
                {
                    var value = int.Parse(match.Groups[1].Value);
                    var unit = match.Groups[2].Value;

                    switch (unit)
                    {
                        case "y":
                        case "year":
                        case "years":
                            years += value;
                            break;

                        case "m":
                        case "month":
                        case "months":
                            months += value;
                            break;

                        case "d":
                        case "day":
                        case "days":
                            days += value;
                            break;

                        default:
                            logger?.LogError("Unsupported time unit encountered: {Unit}", unit);
                            throw new ArgumentException($"Unsupported time unit: '{unit}'");
                    }
                }

                var result = now.AddYears(years).AddMonths(months).AddDays(days);

                logger?.LogInformation("Parsed duration result: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred while parsing duration: {Input}", input);
                throw;
            }
        }
    }
}
