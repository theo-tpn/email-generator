using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Noctus.Infrastructure
{
    public static class StringExtensions
    {
        public static string DecodeEncodedNonAsciiCharacters(this string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var generatedString = new string(Enumerable.Repeat(chars, length - 2)
                .Select(s => s[RandomGenerator.Random.Next(s.Length)]).ToArray());
            return $"{generatedString}{RandomGenerator.Random.Next(99):D2}";
        }
        
        public static string DecodeUtf8(this string value)
        {
            return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(value).ToCharArray(), c => (byte)c));
        }
    }
}
