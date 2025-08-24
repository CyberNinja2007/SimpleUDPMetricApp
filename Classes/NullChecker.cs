namespace Classes
{
    public static class NullChecker
    {
        public static bool IsNull(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNull(string[] value)
        {
            return value == null || value.Length == 0;
        }

        public static bool IsNull(double? value)
        {
            return !value.HasValue;
        }
    }
}