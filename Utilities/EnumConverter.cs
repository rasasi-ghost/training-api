using System;
using TrainingApi.Models;

namespace TrainingApi.Utilities
{
    public static class EnumConverter
    {
        public static UserRole ParseUserRole(object value)
        {
            if (value == null)
                return UserRole.User;

            // Handle numeric values (0, 1, 2, etc.)
            if (value is long longValue)
                return (UserRole)longValue;

            if (value is int intValue)
                return (UserRole)intValue;

            // Handle string values ("Admin", "Teacher", etc.)
            if (value is string stringValue)
            {
                if (Enum.TryParse<UserRole>(stringValue, true, out UserRole result))
                    return result;
            }

            // Default fallback
            return UserRole.User;
        }

        public static T ParseEnum<T>(object value, T defaultValue) where T : struct, Enum
        {
            if (value == null)
                return defaultValue;

            // Handle numeric values
            if (value is long longValue)
                return (T)(object)(int)longValue;

            if (value is int intValue)
                return (T)(object)intValue;

            // Handle string values
            if (value is string stringValue)
            {
                if (Enum.TryParse<T>(stringValue, true, out T result))
                    return result;
            }

            return defaultValue;
        }
    }
}
