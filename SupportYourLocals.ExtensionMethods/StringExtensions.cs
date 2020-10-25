namespace SupportYourLocals.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string Format(this string value, params object[] args) 
        {
            return string.Format(value, args);
        }
    }
}
