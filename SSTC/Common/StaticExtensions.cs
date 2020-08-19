using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Common
{
    public static class StaticExtensions
    {
        public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
        //Culture independent string to nullable double parser. Hooked up to string object. Use with caution. It treats commas and dots equally as floating point separators. Method is smart enough to tell whether string has more than one dot or comma if so it return null.
        public static double? AdvancedParseToNullableDouble(this string text)
        {
            double numberValue;
            char currentDecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            var potentialSeparators = text.ToCharArray().Where(x => (Char.IsLetterOrDigit('.') || Char.IsLetterOrDigit(','))).Select(x => x); 

            if (potentialSeparators.Count() > 1) return null;
            if (potentialSeparators.Count() == 1) text.Replace(potentialSeparators.First(),currentDecimalSeparator);

            if (Double.TryParse(text, out numberValue)) return numberValue;
            else return null;
        }
    }
}
