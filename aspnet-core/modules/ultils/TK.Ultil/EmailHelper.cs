using System.Linq;

namespace System
{
    public static class EmailHelper
    {
        private const string DEFAULT_INTERNAL_EMAIL_ADDRESS_TEMPLATE = "{0}@tkventures.vn";

        /// <summary>
        /// Chuẩn hóa lại phần tiền tố của email
        /// </summary>
        /// <param name="prefix">haihp trong haihp@gmail.com</param>
        /// <returns></returns>
        public static string SanitizeEmailPrefix(string prefix)
        {
            prefix = StringHelper.RemoveSign4VietnameseString(prefix);
            prefix = StringHelper.RemoveSpecialCharacters(prefix);
            return prefix;
        }

        public static string GenerateEmailByFullName(string fullName, int? token = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException($"'{nameof(fullName)}' cannot be null or whitespace.", nameof(fullName));
            }

            string prefix = string.Empty;

            var parts = fullName.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (parts.Length == 1)
            {
                prefix = SanitizeEmailPrefix(parts[0]);
                if (prefix.IsEmpty())
                {
                    throw new Exception("Tên người dùng không phù hợp để sinh email");
                }

                if (prefix.Length == 1)
                {
                    throw new Exception("Độ dài của email quá nhỏ");
                }

                return string.Format(DEFAULT_INTERNAL_EMAIL_ADDRESS_TEMPLATE, prefix).ToLower();
            }

            int count = parts.Length - 1;
            var last = SanitizeEmailPrefix(parts[count]);
            while (last.IsEmpty() && parts.Length >= 0)
            {
                count--;
                last = SanitizeEmailPrefix(parts[count]);
            }

            if (last.IsEmpty())
            {
                throw new Exception("Tên người dùng không phù hợp để sinh email");
            }

            prefix += SanitizeEmailPrefix(last);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = SanitizeEmailPrefix(parts[i]);
                if (part.IsEmpty())
                {
                    continue;
                }
                var firstLetter = part.ToCharArray()[0];
                prefix += firstLetter;
            }

            if (token.HasValue)
            {
                prefix += token;
            }

            return string.Format(DEFAULT_INTERNAL_EMAIL_ADDRESS_TEMPLATE, prefix).ToLower();
        }
    }
}
