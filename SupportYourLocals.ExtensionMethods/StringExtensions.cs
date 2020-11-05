namespace SupportYourLocals.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string Format(this string value, params object[] args) 
        {
            return string.Format(value, args);
        }

        public static int Compare(this string value, string arg)
        {
            return string.Compare(value, arg);
        }
    }
}
