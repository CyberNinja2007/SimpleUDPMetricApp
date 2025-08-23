namespace Classes
{
    public static class NullChecker
    {
        public static bool IsNull(string value) => string.IsNullOrEmpty(value);

        public static bool IsNull(string[] value) => value == null || value.Length == 0;

        public static bool IsNull(double? value) => !value.HasValue;
    }
}