using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;


namespace Eudora.Net.Data
{
    public static partial class DataValidation
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex Regex_PureNumeric();

        [GeneratedRegex("[^a-zA-Z]+")]
        private static partial Regex Regex_PureAlpha();

        public static bool IsBlank(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) return true;
            return false;
        }

        public static bool IsValidEmailAddress(string value)
        {
            return(new EmailAddressAttribute().IsValid(value));
        }

        public static bool IsPureNumeric(string value)
        {
            //return Double.TryParse(value, out double d);
            return Regex_PureAlpha().IsMatch(value);
        }

        public static bool IsPureAlpha(string value)
        {
            return Regex_PureAlpha().IsMatch(value);
        }

        public static bool IsValidPath(string value)
        {
            if (IsBlank(value)) return false;
            //if (!Directory.Exists(value)) return false;
            if (value.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;
            return true;
        }
    }
}
