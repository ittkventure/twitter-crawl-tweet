using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringHelper
    {
        private static readonly string[] VietnameseSigns = new string[]{
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ" };

        public static bool IsEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNotEmpty(this string source)
        {
            return !IsEmpty(source);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool EqualsIgnoreCase(this string s1, string s2, bool trim = false)
        {
            if (s1 == null && s2 == null)
            {
                return true;
            }

            if (s1 == "" && s2 == "")
            {
                return true;
            }

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }

            var normalize1 = s1.ToLower();
            var normalize2 = s2.ToLower();
            if (trim)
            {
                normalize1 = normalize1.Trim();
                normalize2 = normalize2.Trim();
            }

            return normalize1.Equals(normalize2);
        }

        public static byte[] ToByteArray(this string s1)
        {
            if (s1.IsEmpty())
            {
                return null;
            }

            return Encoding.ASCII.GetBytes(s1);
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        /// <summary>
        /// Loại bỏ dấu của tiếng Việt
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveSign4VietnameseString(string str)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/54991/generating-random-passwords
        /// </summary>
        /// <param name="lowercase"></param>
        /// <param name="uppercase"></param>
        /// <param name="numerics"></param>
        /// <returns></returns>
        public static string GeneratePassword(int lowercase, int uppercase, int numerics, int specialChars)
        {
            string lowers = "abcdefghijklmnopqrstuvwxyz";
            string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string number = "0123456789";
            string specials = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            Random random = new Random();

            string generated = "!";
            for (int i = 1; i <= lowercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    lowers[random.Next(lowers.Length - 1)].ToString()
                );

            for (int i = 1; i <= uppercase; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    uppers[random.Next(uppers.Length - 1)].ToString()
                );

            for (int i = 1; i <= numerics; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    number[random.Next(number.Length - 1)].ToString()
                );

            for (int i = 1; i <= specialChars; i++)
                generated = generated.Insert(
                    random.Next(generated.Length),
                    specials[random.Next(specials.Length - 1)].ToString()
                );

            return generated.Replace("!", string.Empty);
        }

    }
}
