using System;

namespace TrainingApi.Utilities
{
    /// <summary>
    /// Utility class for DateTime operations
    /// </summary>
    public static class DateTimeUtility
    {
        /// <summary>
        /// Ensures a DateTime is in UTC format
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>A UTC DateTime</returns>
        public static DateTime EnsureUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }
            
            return dateTime; // Already UTC
        }
    }
}
