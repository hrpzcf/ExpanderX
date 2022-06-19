namespace ExpanderX
{
    internal static class Information
    {
        public const int Date = 220611;
        public const int Major = 1;
        public const int Minor = 1;
        public const int Micro = 0;
        public static string VER
        {
            get { return $"{Major}.{Minor}.{Micro}.{Date}"; }
        }
    }
}
